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
                
                string base64_image="";
                
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

                
                SE.Message.ObjectDetectorAPIReqMsg reqMsg=new SE.Message.ObjectDetectorAPIReqMsg() {
                    
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
                    Hp = modelParameters.Hp,
                };
                if(modelParameters.Fs != null)
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
                ObjectDetectorAPIResMsg response=null;
                
                try {
                    
                    var apiResponse=ServiceCaller.ApiCaller(reqMsg,modelParameters.BaseUrl+"/"+modelParameters.ModelName,"POST").Result; /* Close for testing purpose */
                    /* var apiResponse="{\"Fs\":[],\"Tid\":\"1\",\"Did\":\"DeviceId_10\",\"Fid\":\"\",\"Ts\":\"2023-11-21,12:42:10.904 PM\",\"Ts_ntp\":\"20231121124210\",\"Inf_ver\":\"1\",\"Msg_ver\":\"1\",\"Rm\":\"Success\",\"Rc\":\"200\",\"Ad\":\"\",\"Ffp\":\"\",\"Ltsize\":\"\",\"Lfp\":\"\",\"Mtp\":[{\"Etime\":\"2023-11-21,12:41:07.686 PM\",\"Src\":\"Grabber\",\"Stime\":\"2023-11-21,12:41:05.127 PM\"},{\"Etime\":\"\",\"Src\":\"FrameProcessor\",\"Stime\":\"2023-11-21,12:42:10.993 PM\"},{\"Etime\":\"2023-11-21,12:52:43.153 PM\",\"Src\":\"ImageGeneration\",\"Stime\":\"2023-11-21,12:52:41.958 PM\"}],\"Obase_64\":[],\"Img_url\":[\"http://localhost/documents/1_DeviceId_11/1001.jpg\",\"http://localhost/documents/1_DeviceId_11/77776688.jpg\"]}"; */
                    
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
                        response.Prompt=reqMsg.Prompt;
                        for (int i = 0; i < response.Fs.Count; i++)
                        {
                            response.Fs[i].TaskType = modelParameters.TaskType;
                        }
                        response.Ad = modelParameters.ExplainerURL;
                        
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
