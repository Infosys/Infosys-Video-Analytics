/*=============================================================================================================== *
 * Copyright 2025 Infosys Ltd.                                                                                    *
 * Use of this source code is governed by Apache License Version 2.0 that can be found in the LICENSE file or at  *
 * http://www.apache.org/licenses/                                                                                *
 * ===============================================================================================================*/
﻿/*
 *© 2019 Infosys Limited, Bangalore, India. All Rights Reserved. Infosys believes the information in this document is accurate as of its publication date; such information is subject to change without notice. Infosys acknowledges the proprietary rights of other companies to the trademarks, product names and such other intellectual property rights mentioned in this document. Except as expressly permitted, neither this document nor any part of it may be reproduced, stored in a retrieval system, or transmitted in any form or by any means, electronic, mechanical, printing, photocopying, recording or otherwise, without the prior permission of Infosys Limited and/or any named intellectual property rights holders under this document.   
 * 
 * © 2019 INFOSYS LIMITED. CONFIDENTIAL AND PROPRIETARY 
 */

using Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.Common;
using System;
using System.IO;
//using SC = Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.ServiceClientLibrary;
using SE = Infosys.Solutions.Ainauto.VideoAnalytics.Services.MaskDetector.Contracts;
using Newtonsoft.Json;
using Infosys.Solutions.Ainauto.VideoAnalytics.BusinessComponent;
using Infosys.Solutions.Ainauto.VideoAnalytics.BusinessEntity.Queue;
using Infosys.Solutions.Ainauto.VideoAnalytics.Services.MaskDetector.Contracts.Message;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Runtime.Caching;
using System.Collections;
using Infosys.Solutions.Ainauto.VideoAnalytics.BusinessEntity;

namespace Infosys.Solutions.Ainauto.VideoAnalytics.AIModels
{
    public class TrackingInferenceAPI : ExecuteBase
    {
        static List<Per> perValue = new List<Per>();
        /// Initialize instance of ml model
        public override bool InitializeModel()
        {
            return true;
        }

        /// Call prediction method
        public override string MakePrediction(Stream st, ModelParameters modelParameters)
        {
            string sstime = DateTime.UtcNow.ToString("yyy-MM-dd,HH:mm:ss.fff tt");

            List<SE.Message.Mtp> MtpData = new List<SE.Message.Mtp>()
                {
                    new SE.Message.Mtp(){ Etime = modelParameters.Etime, Src = modelParameters.Src, Stime=modelParameters.Stime},
                    new SE.Message.Mtp(){ Etime = "", Src = "Tracking Inference", Stime=sstime},
                };
#if DEBUG
            using (LogHandler.TraceOperations("TrackingInferenceAPI:MakePrediction", LogHandler.Layer.MaskPrediction, Guid.NewGuid(), null))
            {



                LogHandler.LogUsage(String.Format("TrackingInferenceAPI MakePrediction is getting executed at : {0}", DateTime.UtcNow.ToLongTimeString()), null);
#endif
                string metadata = "";
                string base64_image = "";
                
                ObjectCache cache = MemoryCache.Default;
                string cacheKey = CacheConstants.UniquePersonCode + modelParameters.tId + modelParameters.deviceId;
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    st.CopyTo(memoryStream);
                    base64_image = Convert.ToBase64String(memoryStream.ToArray());
                    memoryStream.Dispose();
                }
                



                




                
                if (modelParameters.FrameNumber > 1)
                {
                    Queue result = (Queue)cache.Get(cacheKey);

                    if (result != null)
                    {
                        while (result.Count > 0)
                        {
                            if (perValue.Count == Convert.ToInt32(FrameGrabberHelper.deviceDetails.PreviousFrameCount))
                            {
                                perValue.RemoveAt(0);
                    }
                            perValue.Add(JsonConvert.DeserializeObject<Per>(result.Dequeue().ToString()));
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
                    Per = perValue,
                    Ad = " ",
                    Base_64 = base64_image, /* For yolov7 */
                    C_threshold = modelParameters.ConfidenceThreshold, // for yolov7

                    Ffp = modelParameters.Ffp,
                    Ltsize = modelParameters.Ltsize,
                    Lfp = modelParameters.Lfp,
                    I_fn = modelParameters.videoFileName,
                    Hp = modelParameters.Hp
                };
                if (modelParameters.Fs != null)
                {
                    reqMsg.Fs = new();
                    reqMsg.Fs.AddRange(modelParameters.Fs);
                }
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

                

                #region  Commenting old response part for testing new response structure
                /*
                var apiResponse = ServiceCaller.ApiCaller(reqMsg, baseUrl +"/"+ modelname, "POST");

                var response = JsonConvert.DeserializeObject<List<PersonCountAPIResMsg>>(apiResponse);
                metadata = JsonConvert.SerializeObject(response);
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
                metadata = JsonConvert.SerializeObject(response);


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
