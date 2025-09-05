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

using SE = Infosys.Solutions.Ainauto.VideoAnalytics.Services.MaskDetector.Contracts;
using Newtonsoft.Json;
using Infosys.Solutions.Ainauto.VideoAnalytics.BusinessComponent;
using Infosys.Solutions.Ainauto.VideoAnalytics.Services.MaskDetector.Contracts.Message;
using System.Collections.Generic;

namespace Infosys.Solutions.Ainauto.VideoAnalytics.AIModels
{
    public class ServeModelDetectMask_t1 : ExecuteBase
    {
        
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
            using (LogHandler.TraceOperations("ServeModelDetectMask_t1:MakePrediction", LogHandler.Layer.MaskPrediction, Guid.NewGuid(), null))
            {
                LogHandler.LogUsage(String.Format("ServeModelDetectMask_t1 MakePrediction is getting executed at : {0}", DateTime.UtcNow.ToLongTimeString()), null);
#endif
                string metadata = "";
                st.Position = 0;
                string base64_image = "";
                

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    st.CopyTo(memoryStream);
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
                    Base_64 = base64_image,// for yolov7
                    C_threshold = modelParameters.ConfidenceThreshold, // for yolov7

                         Ffp = modelParameters.Ffp,
                    Ltsize = modelParameters.Ltsize,
                    Lfp = modelParameters.Lfp,
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
                string methodName = "Get" + modelName;
                var apiResponse = ServiceCaller.ApiCaller(reqMsg, baseUrl + "/" + modelName,"POST");
                var response = JsonConvert.DeserializeObject<SE.Message.ObjectDetectorAPIResMsg>(apiResponse);
                //var response = Helper.GetMaskPrediction_API(reqMsg, baseUrl + "/" + modelName);
                metadata = JsonConvert.SerializeObject(Helper.RemoveDuplicateRegionsAPI(response, overlapThreshold));\
                */
                #endregion

                string methodName = "Get" + modelParameters.ModelName;
                var apiResponse = ServiceCaller.ApiCaller(reqMsg, modelParameters.BaseUrl + "/" + modelParameters.ModelName, "POST").Result;

                ObjectDetectorAPIResMsg response = null;

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
                }
                metadata = JsonConvert.SerializeObject(response);
#if DEBUG
                LogHandler.LogUsage(String.Format("ServeModelDetectMask_t1 MakePrediction finished execution at : {0}", DateTime.UtcNow.ToLongTimeString()), null);
#endif
                return metadata;
#if DEBUG
            }
#endif
        }

    }
}
