/*=============================================================================================================== *
 * Copyright 2024 Infosys Ltd.                                                                                    *
 * Use of this source code is governed by Apache License Version 2.0 that can be found in the LICENSE file or at  *
 * http://www.apache.org/licenses/                                                                                *
 * ===============================================================================================================*/


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
                st.Position = 0;
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
                    Base_64 = base64_image,
                    C_threshold = modelParameters.ConfidenceThreshold, 

                    Ffp = modelParameters.Ffp,
                    Ltsize = modelParameters.Ltsize,
                    Lfp = modelParameters.Lfp,
                    Msk_img = modelParameters.Msk_img == null ? new List<string>() : modelParameters.Msk_img,
                    Rep_img = modelParameters.Rep_img == null ? new List<string>() : modelParameters.Rep_img,
                    Prompt = modelParameters.Prompt == null ? new List<List<string>>() : modelParameters.Prompt,

                };

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

                    if (!string.IsNullOrEmpty(respose))
                    {
                        response = JsonConvert.DeserializeObject<ObjectDetectorAPIResMsg>(respose);

                        string etime = DateTime.UtcNow.ToString("yyy-MM-dd,HH:mm:ss.fff tt");
                        for (int i = 0; i < response.Mtp.Count; i++)
                        {
                            if (response.Mtp[i].Etime == "")
                            {
                                response.Mtp[i].Etime = etime;
                            }
                        }
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
