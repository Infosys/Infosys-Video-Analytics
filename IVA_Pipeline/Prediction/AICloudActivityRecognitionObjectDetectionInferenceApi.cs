/*=============================================================================================================== *
 * Copyright 2024 Infosys Ltd.                                                                                    *
 * Use of this source code is governed by Apache License Version 2.0 that can be found in the LICENSE file or at  *
 * http://www.apache.org/licenses/                                                                                *
 * ===============================================================================================================*/

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.Common;
using System.IO;
using SE = Infosys.Solutions.Ainauto.VideoAnalytics.Services.MaskDetector.Contracts;
using Newtonsoft.Json;
using Infosys.Solutions.Ainauto.VideoAnalytics.AIModels;
using System.Net.Http;
using Infosys.Solutions.Ainauto.VideoAnalytics.Services.MaskDetector.Contracts.Message;
using System.Net.Http.Headers;
using Newtonsoft.Json.Linq;
using Infosys.Solutions.Ainauto.VideoAnalytics.Resource.Entity.Queue;
using System.Threading;
using System.Diagnostics;
using System.Runtime.Caching;

namespace Infosys.Solutions.Ainauto.VideoAnalytics.AIModels
{
    public class AICloudActivityRecognitionObjectDetectionInferenceApi : ExecuteBase
    {
        ObjectCache cache = MemoryCache.Default;
        CacheItemPolicy policy = new CacheItemPolicy();
        List<PersonCountAPIResMsg> objectDetectorResponse = null;
        string metadata = "";
        double cacheExpiration = 10.0;
        
        public override bool InitializeModel()
        {
            return true;
        }

       

        public override string MakePrediction(Stream stream, ModelParameters modelParameters)
        {
            string sstime = DateTime.UtcNow.ToString("yyy-MM-dd,HH:mm:ss.fff tt");
            List<SE.Message.Mtp> MtpData = new List<SE.Message.Mtp>()
                {
                    new SE.Message.Mtp(){ Etime = modelParameters.Etime, Src = modelParameters.Src, Stime=modelParameters.Stime},
                    new SE.Message.Mtp(){ Etime = "", Src = "Frame Processor", Stime=sstime},
                };

#if DEBUG

            LogHandler.LogUsage(String.Format("TrackingInferenceAPI MakePrediction is getting executed at : {0}", DateTime.UtcNow.ToLongTimeString()), null);
#endif
            string metadata = "";      
            
            string base64_image = "";
          
            using (MemoryStream memoryStream = new MemoryStream())
            {
                stream.CopyTo(memoryStream);
                base64_image = Convert.ToBase64String(memoryStream.ToArray());
                memoryStream.Dispose();
            }
            SE.Message.ObjectDetectorAPIReqMsg reqMsg = new SE.Message.ObjectDetectorAPIReqMsg()
            {

                Did = modelParameters.deviceId,
                Fid = modelParameters.Fid,
                Mtp = MtpData,
                Tid = modelParameters.tId,
                Ts = modelParameters.Ts,
                Ts_ntp = modelParameters.Ts_ntp,
                Msg_ver = modelParameters.Msg_ver,
                Inf_ver = modelParameters.Inf_ver,
                Model = modelParameters.ModelName,
                Per = null,
                Ad = " ",
                Base_64 = base64_image,
                C_threshold = modelParameters.ConfidenceThreshold,
                     Ffp = modelParameters.Ffp,
                Ltsize = modelParameters.Ltsize,
                Lfp = modelParameters.Lfp,
                Msk_img = modelParameters.Msk_img == null ? new List<string>() : modelParameters.Msk_img,
                Rep_img = modelParameters.Rep_img == null ? new List<string>() : modelParameters.Rep_img,
                Prompt = modelParameters.Prompt == null ? new List<List<string>>() : modelParameters.Prompt,
            };

            var response = MakePredictionRequestAsync(reqMsg, Convert.ToDouble(modelParameters.ConfidenceThreshold), modelParameters.BaseUrl, modelParameters.AuthenticationUrl, modelParameters.TokenCacheExpirationTime).GetAwaiter().GetResult();
            
            return response;
            
        }
        
        public async Task<string> MakePredictionRequestAsync(ObjectDetectorAPIReqMsg objectDetectorAPIReqMsgAICloud,
            double confidenceThreshold, string baseUrl, string authenticationUrl, double tokenCacheExpirationTime)
        {
            try
            {
                
                var client = new HttpClient(new HttpClientHandler { UseCookies = false });
                string authenticationToken = "";
                string response = "";
                if (cache[CacheConstants.CacheKeyFormatForToken] != null)
                {
                    authenticationToken = cache[CacheConstants.CacheKeyFormatForToken].ToString();

                }
                else
                {
                    authenticationToken = await GetAuthenticationToken(authenticationUrl);
                    policy.SlidingExpiration = TimeSpan.FromMinutes(tokenCacheExpirationTime);
                    cache.Set(CacheConstants.CacheKeyFormatForToken, authenticationToken, policy);
                }
                using (LogHandler.TraceOperations("AICloudObjectDetectionInferenceAPI:MakePredictionRequestAsync", LogHandler.Layer.Business, Guid.NewGuid(), null))
                {
                    string token = "authservice_session=" + authenticationToken;
                    client.DefaultRequestHeaders.Add("Cookie", token);
                    client.DefaultRequestHeaders.Add("Accept", "*/*");
                    HttpResponseMessage httpresponse;
                    var f = JsonConvert.SerializeObject(objectDetectorAPIReqMsgAICloud);
                    var content = new StringContent(JsonConvert.SerializeObject(objectDetectorAPIReqMsgAICloud), Encoding.UTF8, "application/json");
                    content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                    httpresponse = await client.PostAsync(baseUrl, content);
                    response = await httpresponse.Content.ReadAsStringAsync();
                    return response;
                    
                }
            }
            catch (Exception ex)
            {
                LogHandler.LogError(ex.Message, LogHandler.Layer.Business);
                LogHandler.LogError(ex.InnerException.Message, LogHandler.Layer.Business);
                throw ex;
            }

        }





        public async Task<string> GetAuthenticationToken(string authenticationUrl)
        {
            try
            {
                using (LogHandler.TraceOperations("AICloudObjectDetectionInferenceAPI:GetAuthenticationToken", LogHandler.Layer.Business, Guid.NewGuid(), null))
                {
                    var client = new HttpClient();
                    string response = "";

                    HttpResponseMessage httpresponse;
                    client.DefaultRequestHeaders
              .Accept
              .Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    httpresponse = await client.PostAsync(authenticationUrl, null);
                    response = await httpresponse.Content.ReadAsStringAsync();

                    return response;
                }

            }
            catch (Exception ex)
            {
                LogHandler.LogError(ex.Message, LogHandler.Layer.Business);
                LogHandler.LogError(ex.InnerException.Message, LogHandler.Layer.Business);
                throw ex;
            }

        }

    }
}