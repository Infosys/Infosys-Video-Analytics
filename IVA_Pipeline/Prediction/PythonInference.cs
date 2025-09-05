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
using SC = Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.ServiceClientLibrary;
using SE = Infosys.Solutions.Ainauto.VideoAnalytics.Services.MaskDetector.Contracts;
using Newtonsoft.Json;
using Infosys.Solutions.Ainauto.VideoAnalytics.BusinessComponent;
using Infosys.Solutions.Ainauto.VideoAnalytics.Services.MaskDetector.Contracts.Message;
using System.Reflection;
using Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.PythonLoader;
using System.Collections.Generic;
using Python.Runtime;
using System.Collections;
using System.Runtime.Caching;
using System.Linq;

namespace Infosys.Solutions.Ainauto.VideoAnalytics.AIModels
{
    public class PythonInference : ExecuteBase
    {
        static List<Per> perValue = new List<Per>();
        
        PythonNet pNet;
        
        public override bool InitializeModel()
        {
            try
            {

                pNet = new PythonNet();
                
                pNet = PythonNet.GetInstance;

                if (pNet == null)
                    return false;
                else
                    return true;
            }
            catch (Exception ex)
            {

                throw ex;
            }


        }

        
        public override string MakePrediction(Stream st, ModelParameters modelParameters)
        {
            
            string sstime = DateTime.UtcNow.ToString("yyy-MM-dd,HH:mm:ss.fff tt");

            List<SE.Message.Mtp> MtpData = new List<SE.Message.Mtp>()
                {
                    new SE.Message.Mtp(){ Etime = modelParameters.Etime, Src = modelParameters.Src, Stime=modelParameters.Stime},
                    new SE.Message.Mtp(){ Etime = "", Src = "Python Invocation", Stime=sstime},
                };
#if DEBUG
            using (LogHandler.TraceOperations("PythonInference:MakePrediction", LogHandler.Layer.MaskPrediction, Guid.NewGuid(), null))
            {
                LogHandler.LogUsage(String.Format("PythonInference MakePrediction is getting executed at : {0}", DateTime.UtcNow.ToLongTimeString()), null);
#endif
                string metadata = "";
                string base64_image = ""; //Changed As Per New IVA Request : Yoges Govindaraj
                ObjectCache cache = MemoryCache.Default;
                string cacheKey = CacheConstants.UniquePersonCode + modelParameters.tId + modelParameters.deviceId;
                string lastFrameStatus = null;
                


                if (st != null)
                {
                    st.Position = 0;
                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        st.CopyTo(memoryStream);
                        base64_image = Convert.ToBase64String(memoryStream.ToArray());
                        memoryStream.Dispose();
                    }
                }
                if (modelParameters.Pcd != null && modelParameters.Pcd.Length != 0)
                {
                    base64_image = Convert.ToBase64String(modelParameters.Pcd);
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
                    Base_64 = base64_image,// for yolov7
                    C_threshold = modelParameters.ConfidenceThreshold, 

                    Ffp = modelParameters.Ffp,
                    Ltsize = modelParameters.Ltsize,
                    Lfp = modelParameters.Lfp,
                    Msk_img = modelParameters.Msk_img == null ? new List<string>() : modelParameters.Msk_img,
                    Rep_img = modelParameters.Rep_img == null ? new List<string>() : modelParameters.Rep_img,
                    I_fn = modelParameters.videoFileName,
                    Hp = modelParameters.Hp,

                    
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

                ObjectDetectorAPIResMsg response = null;
                
                try
                {
                    

                    #region close for testing purpose of new response
                    /* 
                    var apiResponse = pNet.Inference(JsonConvert.SerializeObject(reqMsg) , modelName);
                    string respose = Convert.ToString(apiResponse);
                    */
                    #endregion

                    #region close for testing purpose of new response
                    /*
                    var apiResponse = "";
                    using (StreamReader r = new StreamReader(@"D:\\I\\TestProject\\TestControl\\iphone\\ObjectDetectionApi\\api_response.json"))
                    {
                        apiResponse = r.ReadToEnd();
                    }
                    */
                    #endregion
                    
                    var apiResponse = pNet.Inference(JsonConvert.SerializeObject(reqMsg), modelParameters.ModelName);
                    
                    string respose = Convert.ToString(apiResponse);
                    if (reqMsg.Lfp == "1")
                    {
                        lastFrameStatus = reqMsg.Lfp;
                        perValue = new List<Per>();
                        
                    }

                    if (!string.IsNullOrEmpty(respose))
                    {
                        response = JsonConvert.DeserializeObject<ObjectDetectorAPIResMsg>(respose);
                        response.Prompt = reqMsg.Prompt;
                        //   Console.WriteLine("Response from Payload{0}:{1}", response.Fid, JsonConvert.SerializeObject(response.Fs));
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
                        response.I_fn = modelParameters.videoFileName; /* Appended Video Filename */
                        
                    }
                }
                catch (Exception ex)
                {
                    if (ex.InnerException == null)
                    {
                        LogHandler.LogError(String.Format("Exception Occured in MakePrediction error message: {0} , exception trace {1}", ex.Message, ex.StackTrace), LogHandler.Layer.Business, null);

                    }
                    else
                    {
                        LogHandler.LogError(String.Format("Exception Occured in MakePrediction error message: {0} , inner Exception : {1}, exception trace {2}", ex.Message, ex.InnerException.Message, ex.StackTrace), LogHandler.Layer.Business, null);
                    }
                    throw ex;
                }

                
                metadata = JsonConvert.SerializeObject(response);
#if DEBUG
                LogHandler.LogUsage(String.Format("PythonInference MakePrediction finished execution at : {0}", DateTime.UtcNow.ToLongTimeString()), null);
#endif
                return metadata;
#if DEBUG
            }
#endif
        }

    }
}
