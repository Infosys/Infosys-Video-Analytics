/*=============================================================================================================== *
 * Copyright 2024 Infosys Ltd.                                                                                    *
 * Use of this source code is governed by Apache License Version 2.0 that can be found in the LICENSE file or at  *
 * http://www.apache.org/licenses/                                                                                *
 * ===============================================================================================================*/

﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infosys.Solutions.Ainauto.VideoAnalytics.BusinessEntity;
using Infosys.Solutions.Ainauto.VideoAnalytics.BusinessEntity.Queue;
using Infosys.Solutions.Ainauto.VideoAnalytics.Entity;
using Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.Common;
using Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.ComputerVisionLib;
using Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.TaskRoute;
using Infosys.Solutions.Ainauto.VideoAnalytics.Resource.Entity.Queue;
using Infosys.Solutions.Ainauto.VideoAnalytics.Services.MaskDetector.Contracts.Message;
using Newtonsoft.Json;

namespace Infosys.Solutions.Ainauto.VideoAnalytics.AIModels
{

    public class SingletonComputerVision
    {

        public AutomationFacade automationFacade = null;
        private static SingletonComputerVision instance = null;
        private static readonly object Instancelock = new object();
        public static string modelPath = string.Empty;
        public static string modelLabelPath = string.Empty;
        private static DeviceDetails deviceDetails = null;
        public static ModelParameters modelParameters = new ModelParameters();
        public string[] labels;
        public static SingletonComputerVision GetInstance
        {
            get
            {
#if DEBUG
                LogHandler.LogDebug("Inside Infosys.Solutions.Ainauto.VideoAnalytics.AIModels.SingletonComputerVision GetInstance method called.",LogHandler.Layer.MaskPrediction);
#endif
                if (instance == null)
                {
                    lock (Instancelock)
                    {
                        if (instance == null)
                        {  
                            instance = new SingletonComputerVision(modelPath, modelLabelPath, modelParameters);
                        }
                    }
                }
                return instance;
            }
        }

        private SingletonComputerVision(string modelPath,string modelLabelPath,ModelParameters modelParameters) {
            #if DEBUG
            LogHandler.LogUsage(String.Format("SingletonComputerVision method of FrameProcessor is getting executed at {0}",DateTime.UtcNow.ToLongTimeString()),null);
            LogHandler.LogDebug("Inside Infosys.Solutions.Ainauto.VideoAnalytics.AIModels.SingletonComputerVision automationfacade model initialization called.",LogHandler.Layer.MaskPrediction);
            using (LogHandler.TraceOperations("SingletonComputerVision:SingletonComputerVision",LogHandler.Layer.MaskPrediction,Guid.NewGuid(),null)) {
                #endif
                if(modelPath!="") {
                    automationFacade=new AutomationFacade(modelPath,false,false,"",false,false,false);
                    if(ConfigHelper.Cache!=null)
                        deviceDetails=(DeviceDetails)ConfigHelper.Cache[ConfigHelper.DeviceDetailsCacheKey];
                    /* Setting value from cache */
                    if(deviceDetails==null) {
                        modelParameters.PredictionKey=modelParameters.tId+"_"+modelParameters.deviceId+"_"+modelParameters.keyPrefix;
                        string[] predictionKeyList=modelParameters.PredictionKey.Split("_");
                        deviceDetails=(DeviceDetails)ConfigHelper.SetDeviceDetails(predictionKeyList[0],predictionKeyList[1],predictionKeyList[2]);
                    }
                    automationFacade.FindControlInMultipleControlStates=deviceDetails.TemplateMatching.FindControlInMultipleControlStates;
                    automationFacade.ImageRecognitionTimeout=deviceDetails.TemplateMatching.ImageRecognitionTimeout;
                    automationFacade.UseTrueColorTemplateMatching=deviceDetails.TemplateMatching.UseTrueColorTemplateMatching;
                    automationFacade.ImageMatchConfidenceThreshold=deviceDetails.TemplateMatching.ImageMatchConfidenceThreshold;
                    automationFacade.MultipleScaleTemplateMatching=deviceDetails.TemplateMatching.MultipleScaleTemplateMatching;
                    automationFacade.ImageMatchMaxScaleStepCount=deviceDetails.TemplateMatching.ImageMatchMaxScaleStepCount;
                    automationFacade.ImageMatchScaleStepSize=deviceDetails.TemplateMatching.ImageMatchScaleStepSize;
                    automationFacade.EnableTemplateMatchMapping=deviceDetails.TemplateMatching.EnableTemplateMatchMapping;
                    automationFacade.WaitForever=deviceDetails.TemplateMatching.WaitForever;
                    automationFacade.TemplateMatchMappingBorderThickness=deviceDetails.TemplateMatching.TemplateMatchMappingBorderThickness;
                    automationFacade.MultiRotationTemplateMatching=deviceDetails.TemplateMatching.MultiRotationTemplateMatching;
                    automationFacade.ImageMatchRotationStepAngle=deviceDetails.TemplateMatching.ImageMatchRotationStepAngle;
                }
                else {
                    FaceMaskDetectionValidationException validateException=new FaceMaskDetectionValidationException();
                    List<ValidationError> validateErrs=new List<ValidationError>();
                    ValidationError validationErr=new ValidationError();
                    validationErr.Code=Errors.ErrorCodes.InvalidCharacter_Validation.ToString();
                    validationErr.Description=string.Format(ErrorMessages.Value_NullOrEmpty_Error,Path.GetFileNameWithoutExtension(modelPath),"Workflow.modelPath");
                    validateErrs.Add(validationErr);
                    validateException.Data.Add("ValidationErrors",validateErrs);
                    throw validateException;
                }
                #if DEBUG
            }
            LogHandler.LogUsage(String.Format("SingletonComputerVision method of FrameProcessor finished execution at {0}",DateTime.UtcNow.ToLongTimeString()),null);
            #endif
        }
    }


    public class ComputerVisionInference : ExecuteBase
    {

        SingletonComputerVision modelObject = null;


        public override bool InitializeModel(ModelParameters modeltoInfer /*string modelPath, string modelLabelPath = null*/)
        {

#if DEBUG
            using (LogHandler.TraceOperations("ObjectDetectionOfflineInferenceAPI:InitializeModel", LogHandler.Layer.MaskPrediction, Guid.NewGuid(), null))
            {
#endif
                 
               SingletonComputerVision.modelPath = modeltoInfer.ModelPath;
               SingletonComputerVision.modelLabelPath = modeltoInfer.ModelLabelPath;
                modelObject = SingletonComputerVision.GetInstance;
               
                
                if (modelObject == null)
                    return false;
                else
                    return true;
                
#if DEBUG
            }
#endif
            
            
           
        }

        
        public override string MakePrediction(Stream st,ModelParameters modelParameters) {
            using (LogHandler.TraceOperations("ComputerVisionInference:MakePrediction",LogHandler.Layer.MaskPrediction,Guid.NewGuid(),null)) {
                var sw=Stopwatch.StartNew();
                DateTime stime=DateTime.Now;
                LogHandler.LogInfo("Starting the execution of FindControls",LogHandler.Layer.Business);
                List<Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.ComputerVisionLib.Control> ctrl 
                =modelObject.automationFacade.FindControls(modelParameters.CanonicalPath,st);
                DateTime etime=DateTime.Now;
                sw.Stop();
                var ts=sw.Elapsed;
                LogHandler.LogInfo($"Total execution of FindControls: {ts.TotalMilliseconds}",LogHandler.Layer.Business);
                var predictions=new ObjectDetectorAPIResMsg();
                predictions.Fs=new List<Infosys.Solutions.Ainauto.VideoAnalytics.Services.MaskDetector.Contracts.Message.PersonDetails>();
                var img=System.Drawing.Image.FromStream(st);
                float height=img.Height;
                float width=img.Width;
                foreach(Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.ComputerVisionLib.Control ctr in ctrl) {
                    float x=ctr.ImageReference.CurrentBoundingRectangle.X/width;
                    float y=ctr.ImageReference.CurrentBoundingRectangle.Y/height;
                    float H=ctr.ImageReference.CurrentBoundingRectangle.Height/height;
                    float W=ctr.ImageReference.CurrentBoundingRectangle.Width/width;
                    var personDetails=new Infosys.Solutions.Ainauto.VideoAnalytics.Services.MaskDetector.Contracts.Message.PersonDetails();
                    personDetails.Dm=new();
                    personDetails.Dm.X=x.ToString();
                    personDetails.Dm.Y=y.ToString();
                    personDetails.Dm.H=H.ToString();
                    personDetails.Dm.W=W.ToString();
                    personDetails.Cs=ctr.ImageReference.ConfidenceScore.ToString();
                    personDetails.Lb=ctr.ImageReference.CurrentState.ToString();
                    personDetails.Info=ctr.ImageReference.Angle.ToString();;
                    predictions.Fs.Add(personDetails);
                    /* var predictions=new ObjectDetectorAPIResMessage();
                    predictions.Fs[0]=new Infosys.Solutions.Ainauto.VideoAnalytics.Services.MaskDetector.Contracts.Message.PersonDetails();
                    predictions.Fs[0].Dm=new Infosys.Solutions.Ainauto.VideoAnalytics.Resource.Entity.Queue.BoundingBox();
                    predictions.Fs[0].Dm.X=x.ToString();
                    predictions.Fs[0].Dm.Y=y.ToString();
                    predictions.Fs[0].Dm.W=W.ToString();
                    predictions.Fs[0].Dm.H=H.ToString(); */
                }
                predictions.Mtp=new List<Infosys.Solutions.Ainauto.VideoAnalytics.Services.MaskDetector.Contracts.Message.Mtp>();
                var mtp=new Infosys.Solutions.Ainauto.VideoAnalytics.Services.MaskDetector.Contracts.Message.Mtp();
                mtp.Stime=stime.ToString("yyy-MM-dd,HH:mm:ss.fff tt");
                mtp.Etime=etime.ToString("yyy-MM-dd,HH:mm:ss.fff tt");
                mtp.Src="AutomationFacade[TemplateMatching]";
                predictions.Mtp.Add(mtp);
                predictions.Did=modelParameters.deviceId;
                predictions.Tid=modelParameters.tId;
                predictions.Fid=modelParameters.Fid;
                predictions.Ts=modelParameters.Ts.ToString();
                predictions.Ts_ntp=modelParameters.Ts_ntp;
                predictions.Msg_ver=modelParameters.Msg_ver.ToString();
                /* predictions.Msg_ver="1"; */
                predictions.Inf_ver=modelParameters.Inf_ver.ToString();
                /* predictions.Inf_ver="2"; */
                predictions.Ad=modelParameters.Ad.ToString();
                /* predictions.Ad="Test"; */
                predictions.Rc=200;
                predictions.Rm="success";
                return JsonConvert.SerializeObject(predictions);
                /* var prediction=new Predictions();
                prediction.Dm=new BusinessEntity.Queue.BoundingBox();
                prediction.Dm.X=ctr.ImageReference.CurrentBoundingRectangle.X.ToString();
                prediction.Dm.Y=ctr.ImageReference.CurrentBoundingRectangle.Y.ToString();
                prediction.Dm.H=ctr.ImageReference.CurrentBoundingRectangle.Height.ToString();
                prediction.Dm.W=ctr.ImageReference.CurrentBoundingRectangle.Width.ToString();
                prediction.Lb=ctr.ImageReference.CurrentState.ToString();
                prediction.Cs="";
                predictions.Add(prediction);
                if(modelObject.automationFacade.TemplateMatchesMapScreen!=null) {
                    using(System.Drawing.Image image1=System.Drawing.Image.FromStream(new MemoryStream(modelObject.automationFacade.TemplateMatchesMapScreen))) {
                        image1.Save(@"D:\IVF\FindControlTest\iphone\output.jpg",System.Drawing.Imaging.ImageFormat.Jpeg);  Or png
                    }
                }
                var i1=System.Drawing.Image.FromStream(st);
                i1.Save(@"D:\IVF\FindControlTest\iphone\output1.jpg",System.Drawing.Imaging.ImageFormat.Jpeg);
                }  
                return JsonConvert.SerializeObject(predictions); */
            }
        }
    }
}
