/*=============================================================================================================== *
 * Copyright 2024 Infosys Ltd.                                                                                    *
 * Use of this source code is governed by Apache License Version 2.0 that can be found in the LICENSE file or at  *
 * http://www.apache.org/licenses/                                                                                *
 * ===============================================================================================================*/

﻿
using Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.Common;
using System;
using System.IO;
using SC = Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.ServiceClientLibrary;
using SE = Infosys.Solutions.Ainauto.VideoAnalytics.Services.MaskDetector.Contracts;
using Newtonsoft.Json;
using Infosys.Solutions.Ainauto.VideoAnalytics.BusinessComponent;
using Infosys.Solutions.Ainauto.VideoAnalytics.Services.MaskDetector.Contracts.Message;
using System.Reflection;
using static System.Net.WebRequestMethods;
using File = System.IO.File;
using System.Linq;
using MediaToolkit.Model;
using Nest;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Infosys.Solutions.Ainauto.VideoAnalytics.AIModels
{
    public class ObjectDetectionInferenceAPI : ExecuteBase
    {
  
        public override bool InitializeModel()
        {
            return true;
        }

       
        public override string MakePrediction(Stream st,ModelParameters modelParameters) {
            #if DEBUG
            using(LogHandler.TraceOperations("ObjectDetectionInferenceAPI:MakePrediction",LogHandler.Layer.MaskPrediction,Guid.NewGuid(),null)) {
                LogHandler.LogUsage(String.Format("ObjectDetectionInferenceAPI MakePrediction is getting executed at {0}.",DateTime.UtcNow.ToLongTimeString()),null);
                #endif
                string metadata="";
                /* st.Position=0; */
                string base64_image="";
                /* SC.ModelInferenceAPI maskDetector=new SC.ModelInferenceAPI(baseUrl);
                SE.IModelInference channel=maskDetector.ServiceChannel; */
                if(st!=null) {
                    st.Position=0;
                    using(MemoryStream memoryStream=new MemoryStream()) {
                        st.CopyTo(memoryStream);
                        base64_image=Convert.ToBase64String(memoryStream.ToArray());
                        memoryStream.Dispose();
                    }
                }
                if(modelParameters.Pcd != null && modelParameters.Pcd.Length!=0) {
                    base64_image=Convert.ToBase64String(modelParameters.Pcd);
                }
                /* DateTime today=DateTime.Now; */
                string sstime=DateTime.UtcNow.ToString("yyy-MM-dd,HH:mm:ss.fff tt");
                List<SE.Message.Mtp> MtpData=new List<SE.Message.Mtp>() {
                    new SE.Message.Mtp() {Etime=modelParameters.Etime,Src=modelParameters.Src,Stime=modelParameters.Stime},
                    new SE.Message.Mtp() {Etime="",Src="Frame Processor",Stime=sstime},
                };

                #region Commented hardcoded value used for IVA new request/response testing
                /* Dm dmdata=new Dm() {
                    X="1.2",
                    Y="1.4",
                    H="1.5",
                    W="1.6"
                };
                List<Fs> allfs=new List<Fs>() {
                    new Fs() {Lb="1",Dm=dmdata,Uid="3",Nobj="3",Cs="0.5"}
                };
                Per perobj=new Per() {
                    Fid="1",
                    Fs=allfs
                }; */
                #endregion

                /* using(StreamReader r=new StreamReader(@"C:\Users\yoges.govindaraj\IVA\WorkSpace\base64imagesegmentation.txt")) {
                    base64_image=r.ReadToEnd();
                } */
                SE.Message.ObjectDetectorAPIReqMsg reqMsg=new SE.Message.ObjectDetectorAPIReqMsg() {
                    /* base64_image=base64_image,
                    confidence_threshold=confidenceThreshold */
                    Did=modelParameters.deviceId,
                    Fid=modelParameters.Fid,
                    Mtp=MtpData,
                    Tid=modelParameters.tId,
                    Ts=modelParameters.Ts,
                    Ts_ntp=modelParameters.Ts_ntp,
                    Msg_ver=modelParameters.Msg_ver,
                    Inf_ver=modelParameters.Inf_ver,
                    Model=modelParameters.ModelName,
                    Per=null,
                    Ad=" ",
                    Base_64=base64_image,
                    C_threshold=modelParameters.ConfidenceThreshold,
                    Ffp=modelParameters.Ffp,
                    Ltsize=modelParameters.Ltsize,
                    Lfp=modelParameters.Lfp,
                    I_fn=modelParameters.videoFileName,
                    Msk_img = modelParameters.Msk_img == null ? new List<string>() : modelParameters.Msk_img,
                    Rep_img = modelParameters.Rep_img == null ? new List<string>() : modelParameters.Rep_img,
                    Prompt = modelParameters.Prompt == null ? new List<List<string>>() : modelParameters.Prompt,
                };
                ObjectDetectorAPIResMsg response=null;  
                
                try {
                    
                    var apiResponse=ServiceCaller.ApiCaller(reqMsg,modelParameters.BaseUrl+"/"+modelParameters.ModelName,"POST"); /* Close for testing purpose */
                    
                    #region Testing for new changes for IVA request/response structure
                    /* var apiResponse1="";
                    using(StreamReader r=new StreamReader(@"C:\Users\yoges.govindaraj\Desktop\Crowd_Counting_Input.txt")) {
                        apiResponse=r.ReadToEnd();
                        Ad heatMapPoints=null;
                        heatMapPoints=JsonConvert.DeserializeObject<Ad>(apiResponse1);
                    } */
                    #endregion

                    if(!string.IsNullOrEmpty(apiResponse)) {
                        response=JsonConvert.DeserializeObject<ObjectDetectorAPIResMsg>(apiResponse);
                        string etime=DateTime.UtcNow.ToString("yyy-MM-dd,HH:mm:ss.fff tt");
                        for(int i=0;i<response.Mtp.Count;i++) {
                            if(response.Mtp[i].Etime=="") {
                                response.Mtp[i].Etime=etime;
                            }
                        }
                        response.I_fn=modelParameters.videoFileName; /* Appended Video Filename */
                       
                    }
                }
                catch(Exception ex) {
                    if(ex.InnerException==null) {
                        LogHandler.LogError(String.Format("Exception occured in MakePrediction. Error Message: {0}, Exception Trace: {1}",ex.Message,ex.StackTrace),LogHandler.Layer.Business,null);
                    }
                    else {
                        LogHandler.LogError(String.Format("Exception occured in MakePrediction. Error Message: {0}, Inner Exception: {1}, Exception Trace: {2}",ex.Message,ex.InnerException.Message,ex.StackTrace),LogHandler.Layer.Business,null);
                    }
                    throw ex;
                }
                metadata=JsonConvert.SerializeObject(response);
               
                #if DEBUG
                LogHandler.LogUsage(String.Format("ObjectDetectionInferenceAPI MakePrediction finished execution at {0}.",DateTime.UtcNow.ToLongTimeString()),null);
                #endif
                return metadata;
                #if DEBUG
            }
            #endif
        }
    }
}
