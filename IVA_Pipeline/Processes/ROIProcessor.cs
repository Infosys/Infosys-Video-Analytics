/*=============================================================================================================== *
 * Copyright 2025 Infosys Ltd.                                                                                    *
 * Use of this source code is governed by Apache License Version 2.0 that can be found in the LICENSE file or at  *
 * http://www.apache.org/licenses/                                                                                *
 * ===============================================================================================================*/
using Infosys.Solutions.Ainauto.VideoAnalytics.BusinessComponent;
using Infosys.Solutions.Ainauto.VideoAnalytics.BusinessEntity;
using Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.Common;
using Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.ProcessScheduler.Framework;
using Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.TaskRoute;
using Infosys.Solutions.Ainauto.VideoAnalytics.Resource.DataAccess;
using Infosys.Solutions.Ainauto.VideoAnalytics.Resource.Entity.Queue;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using QueueEntity=Infosys.Solutions.Ainauto.VideoAnalytics.Resource.Entity.Queue;
using DE=Infosys.Solutions.Ainauto.VideoAnalytics.Resource.Entity;
using OpenCvSharp;
using static Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.Common.ApplicationConstants;
using SE=Infosys.Solutions.Ainauto.VideoAnalytics.Services.MaskDetector.Contracts;
using Nest;
using System.Collections.Immutable;
using System.Numerics;
using System.Collections.ObjectModel;

namespace Infosys.Solutions.Ainauto.VideoAnalytics.Processes {
    internal class ROIProcessor:ProcessHandlerBase<QueueEntity.ROIProcessorMetaData> {
        static AppSettings appSettings=Config.AppSettings;
        public string _taskCode;
        public static DeviceDetails deviceDetails;
        public ROIProcessor() {}
        public ROIProcessor(string processId,Dictionary<string,string> arguments) {
            _taskCode=TaskRoute.GetTaskCode(processId,arguments);
            deviceDetails=ConfigHelper.SetDeviceDetails(appSettings.TenantID.ToString(),appSettings.DeviceID,TaskRouteConstants.ROIProcessorCode,arguments);
            if(arguments!=null && arguments.Count>0) {
                string type=arguments[arguments.Keys.First()];
                if(type.ToLower()=="values") {
                    deviceDetails=Helper.UpdateConfigValues(arguments,deviceDetails);
                }
            }
        }

        public override void Dump(QueueEntity.ROIProcessorMetaData message) {            
        }

        public override bool Initialize(MaintenanceMetaData message) {
            appSettings=Config.AppSettings;
            return true;
        }

        public override bool Process(QueueEntity.ROIProcessorMetaData message,int robotId,int runInstanceId,int robotTaskMapId) {
            LogHandler.LogDebug(String.Format("The Process method of ROIProcessor class is getting executed with parameters: ROIProcessor message={0}",JsonConvert.SerializeObject(message)),
            LogHandler.Layer.ROIProcessor,null);
            try {
                string sstime=DateTime.UtcNow.ToString("yyy-MM-dd,HH:mm:ss.fff tt");
                int messageStucktime=deviceDetails.FrameSequencingMessageStuckDuration;
                int messageRetry=deviceDetails.FrameSequencingMessageRetry;  
                DE.Document.Workflow blobImage=null;
                blobImage=BusinessComponent.Helper.DownloadBlob(message.Did,message.Fid,message.Tid,message.Sbu,".jpg");
                for(int i=0;i<=messageRetry;i++) {
                    Thread.Sleep(messageStucktime);
                    blobImage=BusinessComponent.Helper.DownloadBlob(message.Did,message.Fid,message.Tid,message.Sbu,".jpg");
                    if(blobImage!=null) {
                        break;
                    }
                }
                if(blobImage!=null && blobImage.File!=null) {
                    Mat cvImage=null;
                    using(Stream imageStream=blobImage.File) {
                        if(imageStream.Length>0) {
                            using(MemoryStream ms=new MemoryStream()) {
                                imageStream.CopyTo(ms);
                                byte[] imageBytes=ms.ToArray();
                                ms.Dispose();
                                Mat matImage=Cv2.ImDecode(imageBytes,ImreadModes.Color);
                                cvImage=matImage;
                            }
                        }
                    }
                    List<string> base64=new List<string>();
                    if(cvImage!=null) {
                        if(!String.IsNullOrEmpty(deviceDetails.ImageDebugEnabled) && deviceDetails.ImageDebugEnabled.Equals("true",StringComparison.InvariantCultureIgnoreCase)) {
                            if(!String.IsNullOrEmpty(deviceDetails.InputDebugImageFilePath) && Directory.Exists(deviceDetails.InputDebugImageFilePath)) {
                                Cv2.ImWrite(deviceDetails.InputDebugImageFilePath+message.Fid+".jpg",cvImage);
                            }
                        }
                        var roiCoordinates=JsonConvert.DeserializeObject<Dictionary<string,List<List<SE.Message.PointData>>>>(deviceDetails.RoiCoordinates);
                        var keys=roiCoordinates.Keys;
                        int type1=deviceDetails.RoiType;
                        if(type1==1) {
                            int i=1;
                            foreach(var key in keys) {
                                var shapes=roiCoordinates[key];
                                if(key.ToLower()=="polygon") {
                                    foreach(var shape in shapes) {
                                        Mat image=new Mat(cvImage.Height,cvImage.Width,MatType.CV_8UC3,new Scalar(0,0,0));
                                        List<Point> pts=shape.ConvertAll(a=>new Point(a.X,a.Y));
                                        Point[] pointsArr=pts.ToArray();
                                        Cv2.FillPoly(image,new[]{pointsArr},new Scalar(255,255,255));
                                        Cv2.BitwiseAnd(cvImage,image,image);
                                        image=new Mat(image,Cv2.BoundingRect(pointsArr));
                                        Cv2.ImEncode(".jpg", image, out byte[] encodedBytes);
                                        base64.Add(Convert.ToBase64String(encodedBytes));
                                        if(!String.IsNullOrEmpty(deviceDetails.ImageDebugEnabled) && deviceDetails.ImageDebugEnabled.Equals("true",StringComparison.InvariantCultureIgnoreCase)) {
                                            if(!String.IsNullOrEmpty(deviceDetails.RoiDebugImageFilePath) && Directory.Exists(deviceDetails.RoiDebugImageFilePath)) {
                                                Cv2.ImWrite(deviceDetails.RoiDebugImageFilePath+message.Fid+"_"+i+".jpg",image);
                                                i++;
                                            }
                                        }
                                        image.Dispose();
                                    }
                                } 
                                else if(key.ToLower()=="circle") {
                                    foreach(var shape in shapes) {
                                        Mat image=new Mat(cvImage.Height,cvImage.Width,MatType.CV_8UC3,new Scalar(0,0,0));
                                        int x=shape[0].X;
                                        int y=shape[0].Y;
                                        int radius=shape[0].R;
                                        Cv2.Circle(image,new Point(x,y),radius,new Scalar(255,255,255),-1);
                                        Cv2.BitwiseAnd(cvImage,image,image);
                                        image=new Mat(image,new Rect(x-radius,y-radius,2*radius,2*radius));
                                        Cv2.ImEncode(".jpg", image, out byte[] encodedBytes);
                                        base64.Add(Convert.ToBase64String(encodedBytes));
                                        if(!String.IsNullOrEmpty(deviceDetails.ImageDebugEnabled) && deviceDetails.ImageDebugEnabled.Equals("true",StringComparison.InvariantCultureIgnoreCase)) {
                                            if(!String.IsNullOrEmpty(deviceDetails.RoiDebugImageFilePath) && Directory.Exists(deviceDetails.RoiDebugImageFilePath)) {
                                                Cv2.ImWrite(deviceDetails.RoiDebugImageFilePath+message.Fid+"_"+i+".jpg",image);
                                                i++;
                                            }
                                        }
                                        image.Dispose();
                                    }
                                }
                                else if(key.ToLower()=="irregular") {
                                    foreach(var shape in shapes) {
                                        Mat image=new Mat(cvImage.Height,cvImage.Width,MatType.CV_8UC3,new Scalar(0,0,0));
                                        for(int j=0;j<shape.Count;j++) {
                                            int x=shape[j].X;
                                            int y=shape[j].Y;
                                            image.Set(y,x,new Vec3b(255,255,255));
                                        }
                                        Cv2.BitwiseAnd(cvImage,image,image);
                                        List<Point> pts=shape.ConvertAll(a=>new Point(a.X,a.Y));
                                        Point[] pointsArr=pts.ToArray();
                                        image=new Mat(image,Cv2.BoundingRect(pointsArr));
                                        Cv2.ImEncode(".jpg", image, out byte[] encodedBytes);
                                        base64.Add(Convert.ToBase64String(encodedBytes));
                                        if(!String.IsNullOrEmpty(deviceDetails.ImageDebugEnabled) && deviceDetails.ImageDebugEnabled.Equals("true",StringComparison.InvariantCultureIgnoreCase)) {
                                            if(!String.IsNullOrEmpty(deviceDetails.RoiDebugImageFilePath) && Directory.Exists(deviceDetails.RoiDebugImageFilePath)) {
                                                Cv2.ImWrite(deviceDetails.RoiDebugImageFilePath+message.Fid+"_"+i+".jpg",image);
                                                i++;
                                            }
                                        }
                                        image.Dispose();
                                    }
                                } 
                            }
                        }
                        else if(type1==2) {
                            Mat image=new Mat(cvImage.Height,cvImage.Width,MatType.CV_8UC3,new Scalar(0,0,0));
                            foreach(var key in keys) {
                                var shapes=roiCoordinates[key];
                                if(key.ToLower()=="polygon") {
                                    foreach(var shape in shapes) {
                                        List<Point> pts=shape.ConvertAll(a=>new Point(a.X,a.Y));
                                        Cv2.FillPoly(image,new[]{pts.ToArray()},new Scalar(255,255,255));
                                    }
                                } 
                                else if(key.ToLower()=="circle") {
                                    foreach(var shape in shapes) {
                                        Point point=new Point(shape[0].X,shape[0].Y);
                                        int radius=shape[0].R;
                                        Cv2.Circle(image,point,radius,new Scalar(255,255,255),-1);
                                    }
                                } 
                                else if(key.ToLower()=="irregular") {
                                    foreach(var shape in shapes) {
                                        for(int i=0;i<shape.Count;i++) {
                                            int x=shape[i].X;
                                            int y=shape[i].Y;
                                            image.Set(y,x,new Vec3b(255,255,255));
                                        }
                                    }
                                }
                            }
                            Cv2.BitwiseAnd(cvImage,image,image);
                            Cv2.ImEncode(".jpg", image, out byte[] encodedBytes);
                            base64.Add(Convert.ToBase64String(encodedBytes));
                            if(!String.IsNullOrEmpty(deviceDetails.ImageDebugEnabled) && deviceDetails.ImageDebugEnabled.Equals("true",StringComparison.InvariantCultureIgnoreCase)) {
                                if(!String.IsNullOrEmpty(deviceDetails.RoiDebugImageFilePath) && Directory.Exists(deviceDetails.RoiDebugImageFilePath)) {
                                    Cv2.ImWrite(deviceDetails.RoiDebugImageFilePath+message.Fid+".jpg",image);
                                }
                            }
                            image.Dispose();
                        }
                        else if(type1==3) {
                            foreach(var key in keys) {
                                var shapes=roiCoordinates[key];
                                if(key.ToLower()=="polygon") {
                                    foreach(var shape in shapes) {
                                        List<Point> pts=shape.ConvertAll(a=>new Point(a.X,a.Y));
                                        Cv2.FillPoly(cvImage,new[]{pts.ToArray()},new Scalar(1,1,1));
                                    }
                                }
                                else if(key.ToLower()=="circle") {
                                    foreach(var shape in shapes) {
                                        Point point=new Point(shape[0].X,shape[0].Y);
                                        int radius=shape[0].R;
                                        Cv2.Circle(cvImage,point,radius,new Scalar(1,1,1),-1);
                                    }
                                }
                                else if(key.ToLower()=="irregular") {
                                    foreach(var shape in shapes) {
                                        for(int i=0;i<shape.Count;i++) {
                                            int x=shape[i].X;
                                            int y=shape[i].Y;
                                            cvImage.Set(y,x,new Vec3b(1,1,1));
                                        }
                                    }
                                }
                            }
                            Cv2.ImEncode(".jpg", cvImage, out byte[] encodedBytes);
                            base64.Add(Convert.ToBase64String(encodedBytes));
                            if(!String.IsNullOrEmpty(deviceDetails.ImageDebugEnabled) && deviceDetails.ImageDebugEnabled.Equals("true",StringComparison.InvariantCultureIgnoreCase)) {
                                if(!String.IsNullOrEmpty(deviceDetails.RoiDebugImageFilePath) && Directory.Exists(deviceDetails.RoiDebugImageFilePath)) {
                                    Cv2.ImWrite(deviceDetails.RoiDebugImageFilePath+message.Fid+".jpg",cvImage);
                                }
                            }
                        }
                        int frameHeight=cvImage.Height;
                        int frameWidth=cvImage.Width;
                        cvImage.Dispose();
                        string etime=DateTime.UtcNow.ToString("yyy-MM-dd,HH:mm:ss.fff tt");
                        if(message.Mtp==null) {
                            message.Mtp=new List<SE.Message.Mtp>();
                        }
                        message.Mtp.Add(new SE.Message.Mtp() {Etime=message.Etime,Src=message.Src,Stime=message.Stime});
                        message.Mtp.Add(new SE.Message.Mtp() {Etime=etime,Src="ROI Processor",Stime=sstime});
                        TaskRoute taskRoute=new TaskRoute();
                        FrameProcessorMetaData queueEntity=new FrameProcessorMetaData() {
                            Fid=message.Fid,
                            Did=message.Did,
                            Sbu=message.Sbu,
                            Tid=message.Tid,
                            Mod=message.Mod,
                            FeedId=message.FeedId,
                            Fp=message.Fp,
                            Fids=message.Fids,
                            SequenceNumber=message.SequenceNumber,
                            FrameNumber=message.FrameNumber,
                            Mtp=message.Mtp,
                            Msk_img=message.Msk_img, 
                            Rep_img=message.Rep_img,  
                            Ffp=message.Ffp,
                            Ltsize=message.Ltsize,
                            Lfp=message.Lfp,
                            videoFileName=message.videoFileName,
                            Pcd=message.Pcd,
                            Hp=deviceDetails.HyperParameters,
                            Base64=base64,
                            FrameHeight=frameHeight,
                            FrameWidth=frameWidth
                        };
                        var taskList=taskRoute.GetTaskRouteDetails(appSettings.TenantID.ToString(),appSettings.DeviceID,_taskCode,deviceDetails)[_taskCode];
                        foreach(string moduleCode in taskList) {
                            queueEntity.TE=taskRoute.GetTaskRouteDetails(appSettings.TenantID.ToString(),appSettings.DeviceID,moduleCode,deviceDetails);
                            taskRoute.SendMessageToQueue(Config.AppSettings.TenantID.ToString(),Config.AppSettings.DeviceID,moduleCode,queueEntity,deviceDetails);
                        }
                    }
                }
            }
            catch(Exception e) {
                LogHandler.LogError("Error in Process method of ROIProcessor. Exception: {0}, inner exception: {1}, stack trace: {2}",
                LogHandler.Layer.ROIProcessor,e.Message,e.InnerException,e.StackTrace);
            }
            return true;
        }

        private void sendEventMessage(QueueEntity.MaintenanceMetaData message) {
            TaskRoute taskRouter=new TaskRoute();
            TaskRouteMetadata taskRouteMetadata=taskRouter.GetTaskRouteConfig(message.Tid,message.Did,deviceDetails);
            var taskList=taskRouter.GetTaskRouteDetails(message.Tid,message.Did,_taskCode,deviceDetails)[_taskCode];
            if(taskList!=null) {
                foreach(var task in taskList) {
                    taskRouter.SendMessageToQueueWithTask(taskRouteMetadata,_taskCode,message,task);
                }
            }
        }

        public override bool HandleEventMessage(QueueEntity.MaintenanceMetaData message) {
            if(message!=null) {
                string eventType=message.EventType;
                switch(eventType) {
                    case ProcessingStatus.StartOfFile:
                        sendEventMessage(message);
                        break;
                    case ProcessingStatus.EndOfFile:
                        sendEventMessage(message);
                        break;
                }
            }
            return true;
        }
    }
}