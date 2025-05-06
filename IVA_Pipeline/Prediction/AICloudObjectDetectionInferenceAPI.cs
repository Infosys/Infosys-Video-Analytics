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
using System.Runtime.Caching;
//using static Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.Common.ExtentionMethodsClass;

namespace Infosys.Solutions.Ainauto.VideoAnalytics.AIModels
{
    public class AICloudObjectDetectionInferenceAPI : ExecuteBase
    {
        ObjectCache cache = MemoryCache.Default;
        CacheItemPolicy policy = new CacheItemPolicy();
        ObjectDetectorAPIResMsg objectDetectorResponse = null;
        string metadata = "";
        double cacheExpiration = 10.0;
        /// Initialize instance of ml model
        public override bool InitializeModel()
        {
            return true;
        }

        

        public override string MakePrediction(Stream st, ModelParameters modelParameters)
        {
            string sstime = DateTime.UtcNow.ToString("yyy-MM-dd,HH:mm:ss.fff tt");
            List<SE.Message.Mtp> MtpData = new List<SE.Message.Mtp>()
                {
                    new SE.Message.Mtp(){ Etime = modelParameters.Etime, Src = modelParameters.Src, Stime=modelParameters.Stime},
                    new SE.Message.Mtp(){ Etime = "", Src = "Frame Processor", Stime=sstime},
                };

#if DEBUG
            using (LogHandler.TraceOperations("AICloudObjectDetectionInferenceAPI:MakePrediction", LogHandler.Layer.MaskPrediction, Guid.NewGuid(), null))
            {
                LogHandler.LogUsage(String.Format("AICloudObjectDetectionInferenceAPI MakePrediction is getting executed at : {0}", DateTime.UtcNow.ToLongTimeString()), null);
#endif
                string base64_image = ""; 
                                          
                if (st != null)
                {
                    st.Position = 0;
                    byte[] image_byte_array;
                    



                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        st.CopyTo(memoryStream);
                        base64_image = Convert.ToBase64String(memoryStream.ToArray());
                        memoryStream.Dispose();
                    }
                }
                    
                ObjectDetectorAPIReqMsgAICloud reqMsg = new ObjectDetectorAPIReqMsgAICloud();
                reqMsg.instances = new List<ObjectDetectorImageBytesMsgAICloud>();
                ObjectDetectorImageBytesMsgAICloud imgBytesMsg = new ObjectDetectorImageBytesMsgAICloud();



                SE.Message.ObjectDetectorAPIReqMsg objectDetectorAPIReqMsg = new SE.Message.ObjectDetectorAPIReqMsg()
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
                    Base_64 = base64_image,// for yolov7
                    C_threshold = modelParameters.ConfidenceThreshold, // for yolov7  
                    Ffp = modelParameters.Ffp,
                    Ltsize = modelParameters.Ltsize,
                    Lfp = modelParameters.Lfp,
                    Msk_img = modelParameters.Msk_img == null ? new List<string>() : modelParameters.Msk_img,
                    Rep_img = modelParameters.Rep_img == null ? new List<string>() : modelParameters.Rep_img,
                    I_fn = modelParameters.videoFileName,
                    Hp = modelParameters.Hp

                    
                };
                
                    objectDetectorAPIReqMsg.Prompt = new List<List<string>>();
                    List<string> list = new List<string>();
                    list.Add(modelParameters.Prompt);
                    objectDetectorAPIReqMsg.Prompt.Add(list);
                
                imgBytesMsg.image_bytes = objectDetectorAPIReqMsg;
                reqMsg.instances.Add(imgBytesMsg);
                MakePredictionRequestAsync(reqMsg, Convert.ToDouble(modelParameters.ConfidenceThreshold), modelParameters.BaseUrl, modelParameters.AuthenticationUrl, modelParameters.Host, modelParameters.TokenCacheExpirationTime).GetAwaiter().GetResult();
                metadata = JsonConvert.SerializeObject(Helper.RemoveDuplicateRegionsAPI(objectDetectorResponse, modelParameters.OverlapThreshold));
#if DEBUG
                LogHandler.LogUsage(String.Format("AICloudObjectDetectionInferenceAPI MakePrediction finished execution at : {0}", DateTime.UtcNow.ToLongTimeString()), null);
#endif
                return metadata;
#if DEBUG
            }
#endif
        }
        public async Task MakePredictionRequestAsync(ObjectDetectorAPIReqMsgAICloud objectDetectorAPIReqMsgAICloud, double confidenceThreshold, string baseUrl, string authenticationUrl,
            string host, double tokenCacheExpirationTime)
        {
            try
            {
                // to add cookies refer https://d-fens.ch/2016/12/27/howto-set-cookie-header-on-defaultrequestheaders-of-httpclient/
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
                    if (!string.IsNullOrEmpty(host))
                        client.DefaultRequestHeaders.Add("Host", host);
                    string token = "authservice_session=" + authenticationToken;
                    client.DefaultRequestHeaders.Add("Cookie", token);
                    client.DefaultRequestHeaders.Add("Accept", "*/*");
                    HttpResponseMessage httpresponse;
                    
                    string jsonprompt = objectDetectorAPIReqMsgAICloud.instances[0].image_bytes.Prompt[0][0];
                    string entireobject = JsonConvert.SerializeObject(objectDetectorAPIReqMsgAICloud);
                    
                    var content = new StringContent(jsonprompt, Encoding.UTF8, "application/json");
                    content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                    httpresponse = await client.PostAsync(baseUrl, content);
                    response = await httpresponse.Content.ReadAsStringAsync();
                    response = response.Replace("predictedBase64Image", "Obase_64");
                    objectDetectorResponse = (ObjectDetectorAPIResMsg)JsonConvert.DeserializeObject<ObjectDetectorAPIResMsg>(response);
                    objectDetectorResponse.Fs = new List<SE.Message.PersonDetails> { new SE.Message.PersonDetails() };
                    objectDetectorResponse.Tid = objectDetectorAPIReqMsgAICloud.instances[0].image_bytes.Tid;
                    objectDetectorResponse.Did = objectDetectorAPIReqMsgAICloud.instances[0].image_bytes.Did;
                    objectDetectorResponse.Fid = objectDetectorAPIReqMsgAICloud.instances[0].image_bytes.Fid;
                    objectDetectorResponse.Prompt = objectDetectorAPIReqMsgAICloud.instances[0].image_bytes.Prompt;
                    objectDetectorResponse.Hp = objectDetectorAPIReqMsgAICloud.instances[0].image_bytes.Hp;
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
            
            if(!string.IsNullOrEmpty(authenticationUrl))
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
            return "";
        }

    }

}
