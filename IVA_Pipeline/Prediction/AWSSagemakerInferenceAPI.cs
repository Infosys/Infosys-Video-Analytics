/*=============================================================================================================== *
 * Copyright 2025 Infosys Ltd.                                                                                    *
 * Use of this source code is governed by Apache License Version 2.0 that can be found in the LICENSE file or at  *
 * http://www.apache.org/licenses/                                                                                *
 * ===============================================================================================================*/
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.Common;
using System;
using System.IO;
//using SC = Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.ServiceClientLibrary;
using SE = Infosys.Solutions.Ainauto.VideoAnalytics.Services.MaskDetector.Contracts;
using Newtonsoft.Json;
using Infosys.Solutions.Ainauto.VideoAnalytics.AIModels;
using System.Net.Http;
using Infosys.Solutions.Ainauto.VideoAnalytics.Services.MaskDetector.Contracts.Message;
using System.Net.Http.Headers;
using Newtonsoft.Json.Linq;
using Infosys.Solutions.Ainauto.VideoAnalytics.Resource.Entity.Queue;
using System.Threading;
//using Infosys.Solutions.Ainauto.VideoAnalytics.Entity;
using System.Diagnostics;
using Infosys.Solutions.Ainauto.VideoAnalytics.Entity;
using Amazon;
using Amazon.Runtime;
using Amazon.Runtime.Internal;
using Amazon.SageMaker;
using Amazon.SageMaker.Model;
using Amazon.SageMakerRuntime;
using Amazon.SageMakerRuntime.Model;

using RestSharp.Serialization.Json;
using System.Text.Json;
using Nest;

using Amazon.Runtime.CredentialManagement;
using Amazon.SecurityToken;
using Amazon.SecurityToken.Model;
using Elasticsearch.Net;



namespace Infosys.Solutions.Ainauto.VideoAnalytics.AIModels
{
    public class AWSSagemakerInferenceAPI : ExecuteBase
    {
        ObjectDetectorAPIResMsg objectDetectorResponse = null;
        private AmazonSageMakerRuntimeClient smRuntimeClient;
        private AWSCredentials sessionCredentials;
        
        public override bool InitializeModel()
        {
            return true;
        }

        public override string MakePrediction(Stream stream, ModelParameters modelParameters)
        {
            string sstime = DateTime.UtcNow.ToString("yyy-MM-dd,HH:mm:ss.fff tt");
            string base64_image = ConvertStreamToBase64String(stream);
            List<SE.Message.Mtp> MtpData = new List<SE.Message.Mtp>()
                {
                    new SE.Message.Mtp(){ Etime = modelParameters.Etime, Src = modelParameters.Src, Stime=modelParameters.Stime},
                    new SE.Message.Mtp(){ Etime = "", Src = "Frame Processor", Stime=sstime},
                };
#if DEBUG
            using (LogHandler.TraceOperations("AWSSagemakerInferenceAPI:MakePrediction", LogHandler.Layer.MaskPrediction, Guid.NewGuid(), null))
            {

                LogHandler.LogUsage(String.Format("AWSSagemakerInferenceAPI MakePrediction is getting executed at : {0}", DateTime.UtcNow.ToLongTimeString()), null);
#endif

                string metadata = MakePredictionRequestAsync(base64_image, modelParameters).GetAwaiter().GetResult();


#if DEBUG
                LogHandler.LogUsage(String.Format("AWSSagemakerInferenceAPI MakePrediction finished execution at : {0}", DateTime.UtcNow.ToLongTimeString()), null);
#endif
                return metadata;
#if DEBUG
            }
#endif
        }

        public async Task<string> MakePredictionRequestAsync(string base64, ModelParameters modelParameters)
        {
            try
            {
                ObjectDetectorAPIResMsg response = null; // 
                var region = RegionEndpoint.GetBySystemName("us-east-1");
                if (smRuntimeClient == null || sessionCredentials == null)
                {
                    Console.WriteLine("Model loading started");
                    

                    smRuntimeClient = new
                         AmazonSageMakerRuntimeClient(modelParameters.AWSAccessKey,
                         modelParameters.AWSSecretKey, modelParameters.AWSSessionToken, region);
                    Console.WriteLine("Model loading SageMaker client initiated");
                }
                
                var sagemakerClient = new
                           AmazonSageMakerRuntimeClient(modelParameters.AWSAccessKey,
                           modelParameters.AWSSecretKey,
                           modelParameters.AWSSessionToken,
                region);

                

                object json_req = new
                {
                    base64 = base64
                };
                string js = JsonConvert.SerializeObject(json_req);
                
                var request = new InvokeEndpointRequest
                {
                    EndpointName = modelParameters.AWSEndpointName,
                    ContentType = "application/json",
                    Body = new MemoryStream(Encoding.UTF8.GetBytes(js)),

                };
                
                var apiresponse = await sagemakerClient.InvokeEndpointAsync(request);
                string result = Encoding.UTF8.GetString(apiresponse.Body.ToArray());
                response = JsonConvert.DeserializeObject<ObjectDetectorAPIResMsg>(result);
                string etime = DateTime.UtcNow.ToString("yyy-MM-dd,HH:mm:ss.fff tt");

                for (int i = 0; i < response.Mtp.Count; i++)
                {
                    if (response.Mtp[i].Etime == "")
                    {
                        response.Mtp[i].Etime = etime;
                    }
                }
                string metadata= JsonConvert.SerializeObject(response);
                
                return metadata;
            }
            catch (Exception ex)
            {
                LogHandler.LogError(ex.Message, LogHandler.Layer.Business);
                LogHandler.LogError(ex.InnerException.Message, LogHandler.Layer.Business);
                throw ex;
            }
        }

        static string ConvertStreamToBase64String(Stream stream)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                stream.CopyTo(memoryStream); // Copy the data from the Stream to the MemoryStream

                
                string base64String = Convert.ToBase64String(memoryStream.ToArray());
                return base64String;
            }
        }
        private async Task<AWSCredentials> GetSessionCredentialsAsync(string accessKey,
            string secretKey)
        {
            using (var stsClient = new AmazonSecurityTokenServiceClient(accessKey, secretKey))
            {
                var getSessionTokenRequest = new GetSessionTokenRequest();
                var getSessionTokenResponse = await stsClient.GetSessionTokenAsync(getSessionTokenRequest);
                return getSessionTokenResponse.Credentials;
            }
        }


        private async Task<InvokeEndpointResponse> InvokeEndpointWithRetryAsync(InvokeEndpointRequest request)
        {
            try
            {
                return await smRuntimeClient.InvokeEndpointAsync(request);
            }
            catch (AmazonSageMakerException ex)
            {
                if (ex.ErrorCode.Equals("ExpiredTokenException", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine("Token expired. Refreshing token and retrying...");
                    var creds = sessionCredentials.GetCredentials();
                    sessionCredentials = GetSessionCredentialsAsync(creds.AccessKey, creds.SecretKey).Result;
                    smRuntimeClient = new AmazonSageMakerRuntimeClient(sessionCredentials, smRuntimeClient.Config.RegionEndpoint);
                    return await InvokeEndpointWithRetryAsync(request);
                }

                throw ex;
            }
        }

    }

}
