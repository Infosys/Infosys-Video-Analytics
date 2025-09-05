/*=============================================================================================================== *
 * Copyright 2025 Infosys Ltd.                                                                                    *
 * Use of this source code is governed by Apache License Version 2.0 that can be found in the LICENSE file or at  *
 * http://www.apache.org/licenses/                                                                                *
 * ===============================================================================================================*/
using Google.Protobuf;
using Infosys.Solutions.Ainauto.VideoAnalytics.BusinessEntity;
using Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.Common;
using Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.TaskRoute;
using Infosys.Solutions.Ainauto.VideoAnalytics.Resource.Entity.Queue;
using Infosys.Solutions.Ainauto.VideoAnalytics.Services.MaskDetector.Contracts.Message;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Caching;
using System.Text;
using System.Threading.Tasks;
using SE = Infosys.Solutions.Ainauto.VideoAnalytics.Services.MaskDetector.Contracts;
namespace Infosys.Solutions.Ainauto.VideoAnalytics.AIModels
{
    class ActivityRecognitionObjectDetectionInferenceApi : ExecuteBase
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
                
                string base64_image = "";
                
                string Ad = "";
                
                List<Per> pcart = new List<Per>();
          
                Per pc2 = new Per();
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    st.CopyTo(memoryStream);
                    base64_image = Convert.ToBase64String(memoryStream.ToArray());
                    memoryStream.Dispose();
                }


                if (modelParameters.FrameNumber == 1)
                {
                    Ad = "";
                    pcart = null;
                }
                else
                {
                    var result = cache.GetValues(new string[] { "Ad" });
                    var respcart = cache.GetValues(new string[] { "Per" });
                    if (result != null)
                    {
                        foreach (var item in result)
                        {
                            
                            Ad = item.Value.ToString();                            
                        }

                        foreach (var item1 in respcart)
                        {
                            pc2 = (Per)item1.Value;

                           
                            pcart.Add(pc2);
                            
                        }
                    }

                    //                    Ad = pcart.Ad;
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
                    Per = pcart,
                    Ad = Ad,
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

                

                var apiResponse = ServiceCaller.ApiCaller(reqMsg, modelParameters.BaseUrl + "/" + modelParameters.ModelName, "POST").Result; //close for testing purpose of new response

                #region Testing for New IVA request/response structure
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
                    response.Prompt = reqMsg.Prompt;
                    for (int i = 0; i < response.Fs.Count; i++)
                    {
                        response.Fs[i].TaskType = modelParameters.TaskType;
                    }
                    
                }
                

                Per pobj = new Per();
                pobj.Fid = response.Fid;
                pobj.Fs = new CartPredictions[response.Fs.Count];
                for (var i = 0; i < response.Fs.Count; i++)
                {

                    pobj.Fs[i] = new();

                    if (response.Fs[i].Dm.X != null || response.Fs[i].Dm.Y != null || response.Fs[i].Dm.W != null || response.Fs[i].Dm.H != null)
                    {
                        pobj.Fs[i].Dm = response.Fs[i].Dm;
                        //map other properties
                        pobj.Fs[i].Dm.X = response.Fs[i].Dm.X;
                        pobj.Fs[i].Dm.Y = response.Fs[i].Dm.Y;
                        pobj.Fs[i].Dm.W = response.Fs[i].Dm.W;
                        pobj.Fs[i].Dm.H = response.Fs[i].Dm.H;
                    }

                    //Add Kp value
                    if (response.Fs[i].Kp != null)
                    {
                        pobj.Fs[i].Kp = response.Fs[i].Kp;
                    }

                    pobj.Fs[i].Info = response.Fs[i].Info;
                    pobj.Fs[i].Cs = response.Fs[i].Cs;
                    pobj.Fs[i].NoObj = response.Fs[i].Nobj;
                    pobj.Fs[i].Uid = response.Fs[i].Uid;
                    pobj.Fs[i].Lb = response.Fs[i].Lb;

                }
                

                var cacheItemPolicy = new CacheItemPolicy
                {
                    AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(1)
                };
                cache.Remove(key:"Ad");
                cache.Remove(key: "Per");
                cache.Add(new CacheItem("Ad", response.Ad), cacheItemPolicy);
             
                cache.Add(new CacheItem("Per", pobj), cacheItemPolicy);

                
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
