/*=============================================================================================================== *
 * Copyright 2025 Infosys Ltd.                                                                                    *
 * Use of this source code is governed by Apache License Version 2.0 that can be found in the LICENSE file or at  *
 * http://www.apache.org/licenses/                                                                                *
 * ===============================================================================================================*/
﻿using Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.Common;
using Infosys.Solutions.Ainauto.VideoAnalytics.Services.MaskDetector.Contracts.Message;
using SE = Infosys.Solutions.Ainauto.VideoAnalytics.Services.MaskDetector.Contracts;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.Caching;

namespace Infosys.Solutions.Ainauto.VideoAnalytics.AIModels
{
    public class UniquePersonCount_CentroidTracking : ExecuteBase
    {
        
        public override bool InitializeModel()
        {
            return true;
        }

        
        public override string MakePrediction(Stream st, ModelParameters modelParameters)
        {
            ObjectCache cache = MemoryCache.Default;

            string sstime = DateTime.UtcNow.ToString("yyy-MM-dd,HH:mm:ss.fff tt");

            List<SE.Message.Mtp> MtpData = new List<SE.Message.Mtp>()
                {
                    new SE.Message.Mtp(){ Etime = modelParameters.Etime, Src = modelParameters.Src, Stime=modelParameters.Stime},
                    new SE.Message.Mtp(){ Etime = "", Src = "Frame Processor", Stime=sstime},
                };

#if DEBUG
            using (LogHandler.TraceOperations("TrackingInferenceAPI:MakePrediction", LogHandler.Layer.MaskPrediction, Guid.NewGuid(), null))
            {



                LogHandler.LogUsage(String.Format("TrackingInferenceAPI MakePrediction is getting executed at : {0}", DateTime.UtcNow.ToLongTimeString()), null);
#endif
                string metadata = "";
                st.Position = 0;
                string base64_image = "";
                string Ad = "";
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    st.CopyTo(memoryStream);
                    base64_image = Convert.ToBase64String(memoryStream.ToArray());
                    memoryStream.Dispose();
                }
                



                

                if (modelParameters.FrameNumber == 1)
                {
                    Ad = "";
                    
                }
                else
                {
                    var result = cache.GetValues(new string[] { "Ad" });

                    if (result != null)
                    {
                        foreach (var item in result)
                        {

                            Ad = item.Value.ToString();
                        }
                    }
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
                    Ad = Ad,
                    Base_64 = base64_image,// for yolov7
                    C_threshold = modelParameters.ConfidenceThreshold, // for yolov7

                    Ffp = modelParameters.Ffp,
                    Ltsize = modelParameters.Ltsize,
                    Lfp = modelParameters.Lfp,
                    I_fn = modelParameters.videoFileName,
                    Hp = modelParameters.Hp,
                };

                if (!string.IsNullOrEmpty(modelParameters.Prompt))
                {
                    LogHandler.LogDebug($"Formatting prompt: {modelParameters.Prompt} to list of list", LogHandler.Layer.Business);
                    reqMsg.Prompt = JsonConvert.DeserializeObject<List<List<string>>>(modelParameters.Prompt);
                }
                else
                {
                    reqMsg.Prompt = new List<List<string>>();
                    List<string> list = new List<string>();
                    reqMsg.Prompt.Add(list);
                }

                #region  Commenting old request part for testing new response structure
                /*
               //var response = channel.GetDetectPersonCount_t7(reqMsg);
               //metadata = JsonConvert.SerializeObject(response);

               var apiResponse = ServiceCaller.ApiCaller(reqMsg, baseUrl + "/" + modelname, "POST");

               try
               {
                   var response = JsonConvert.DeserializeObject<List<PersonCountAPIResMsg>>(apiResponse);
                   metadata = JsonConvert.SerializeObject(response);
               }
               */
                #endregion

                ObjectDetectorAPIResMsg response = null;

                 var apiResponse = ServiceCaller.ApiCaller(reqMsg, modelParameters.BaseUrl + "/" + modelParameters.ModelName, "POST").Result;

                #region Testing for new changes for IVA request/response structure
                /*
                var apiResponse = "";
               using (StreamReader r = new StreamReader(@"D:\\I\\TestProject\\TestControl\\iphone\\ObjectDetectionApi\\api_response.json"))
               {
                   apiResponse = r.ReadToEnd();
               }
                */
                #endregion

                try
                {

                    if (!string.IsNullOrEmpty(apiResponse))
                    {
                        
                        response = JsonConvert.DeserializeObject<ObjectDetectorAPIResMsg>(apiResponse);
                        string etime = DateTime.UtcNow.ToString("yyy-MM-dd,HH:mm:ss.fff tt");
                        for (int i = 0; i < response.Mtp.Count; i++)
                        {
                            if (response.Mtp[i].Etime == "")
                            {
                                response.Mtp[i].Etime = etime;
                            }
                        }
                        for (int i = 0; i < response.Fs.Count; i++)
                        {
                            response.Fs[i].TaskType = modelParameters.TaskType;
                        }
                        response.Prompt = reqMsg.Prompt;
                    }
                    var cacheItemPolicy = new CacheItemPolicy
                    {
                        AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(1)
                    };
                    cache.Remove(key: "Ad");
                    cache.Add(new CacheItem("Ad", response.Ad), cacheItemPolicy);
                    metadata = JsonConvert.SerializeObject(response);
                }
                catch (Exception ex)
                {

                }
#if DEBUG
                LogHandler.LogUsage(String.Format("TrackingInferenceAPI MakePrediction finished execution at : {0}", DateTime.UtcNow.ToLongTimeString()), null);
#endif
                return metadata;
#if DEBUG
            }
#endif
        }
    }
}
