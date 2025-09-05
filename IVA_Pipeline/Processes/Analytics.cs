/*=============================================================================================================== *
 * Copyright 2025 Infosys Ltd.                                                                                    *
 * Use of this source code is governed by Apache License Version 2.0 that can be found in the LICENSE file or at  *
 * http://www.apache.org/licenses/                                                                                *
 * ===============================================================================================================*/
﻿using Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.ProcessScheduler.Framework;
using QueueEntity = Infosys.Solutions.Ainauto.VideoAnalytics.Resource.Entity.Queue;
using DE = Infosys.Solutions.Ainauto.VideoAnalytics.Resource.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.Common;
using Infosys.Solutions.Ainauto.VideoAnalytics.BusinessComponent;
using AI = Infosys.Solutions.Ainauto.VideoAnalytics.AIModels;
using System.Configuration;
using Newtonsoft.Json;
using System.Drawing;
using Infosys.Solutions.Ainauto.VideoAnalytics.Resource.DataAccess;
using System.Threading;
using Infosys.Solutions.Ainauto.VideoAnalytics.Resource.Entity;
using Infosys.Solutions.Ainauto.VideoAnalytics.Resource.Entity.Framedetail;
using static Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.Common.ApplicationConstants;
using BE = Infosys.Solutions.Ainauto.VideoAnalytics.BusinessEntity.Analytics;
using Infosys.Solutions.Ainauto.VideoAnalytics.BusinessEntity;
using Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.TaskRoute;
using System.Runtime.Caching;
using Infosys.Solutions.Ainauto.VideoAnalytics.Resource.Entity.Queue;
using System.Runtime.InteropServices;

namespace Infosys.Solutions.Ainauto.VideoAnalytics.Processes
{
    public class Analytics : ProcessHandlerBase<QueueEntity.PersonCountQueueMsg>
    {
        FramePredictedClassDetailsDS predictDS = new FramePredictedClassDetailsDS();
        ObjectTrackingdetailsDS objectTrackingDS = new ObjectTrackingdetailsDS();
        PartitionKeyUtility partitionKeyUtility = new PartitionKeyUtility();

        int exceptionCount = 0;
        
        static string  predictionType = string.Empty;

        public string _taskCode;
        public Analytics() { }
        public Analytics(string processId)
        {
            _taskCode = TaskRoute.GetTaskCode(processId);
        }

        public override void Dump(QueueEntity.PersonCountQueueMsg message)
        {}

        public override bool Initialize(QueueEntity.MaintenanceMetaData message)
        {

            if (message == null)
            {
                ReadFromConfig();

            }
            else
            {
                try
                {
                    if (message.EventType != null)
                    {
                        var eventList = message.EventType.Split(',');

                        for (var i = 0; i < eventList.Length; i++)
                        {
                            switch (eventList[i])
                            {
                                case "reload_config":
                                    ReadFromConfig();
                                    break;
                                case "cache_cleanup":
                                    if (message.ResourceId != null)
                                    {
                                        var resourceIdList = message.ResourceId.Split(',');
                                        CacheCleanUp(resourceIdList);
                                    }
                                    else
                                    {
                                        LogHandler.LogError("ResourceId is  null in maintenance message : {0}", LogHandler.Layer.Business, JsonConvert.SerializeObject(message));
                                    }

                                    break;

                            }
                        }
                    }
                    else
                    {
                        LogHandler.LogError("EventType is  null in maintenance message : {0}", LogHandler.Layer.Business, JsonConvert.SerializeObject(message));
                    }
                }
                catch (Exception ex)
                {
                    LogHandler.LogError("Exception in Initialize method of Analytics : {0} ", LogHandler.Layer.Business, ex.Message);
                    return false;
                }


            }
            return true;
        }
            private void CacheCleanUp(string[] resourceIdList)
            {
            ObjectCache cache = MemoryCache.Default;
            for (var j = 0; j < resourceIdList.Length; j++)
            {
                if(cache.Contains(resourceIdList[j])){
                    cache.Remove(resourceIdList[j]);
                  
                }
               
            }

        }


        private void ReadFromConfig()
        {
            AppSettings appSettings=Config.AppSettings;
            DeviceDetails deviceDetails=ConfigHelper.SetDeviceDetails(appSettings.TenantID.ToString(),appSettings.DeviceID,CacheConstants.AnalyticsCode);
            if(deviceDetails.AnalyticsPredictionType!=null) {
                AnalyticsHelper.predictionType=deviceDetails.AnalyticsPredictionType;
                predictionType=deviceDetails.AnalyticsPredictionType;
            }
            

        }


        public override bool Process(QueueEntity.PersonCountQueueMsg message, int robotId, int runInstanceId, int robotTaskMapId)
        {
#if DEBUG
            LogHandler.LogInfo(InfoMessages.Method_Execution_Start,LogHandler.Layer.Business, "Process", "Analytics");
            LogHandler.LogDebug("The Process Method of Ananlytics class getting executed with parameters {0}", LogHandler.Layer.Business, JsonConvert.SerializeObject(message));
#endif            
            TaskRoute taskRouter = new TaskRoute();
            long frameGrabTimeTick = long.Parse(message.Fid);
            DateTime frameGrabTime = new DateTime(frameGrabTimeTick);
            int partitionKey = partitionKeyUtility.generatePartionKey(message.Tid, frameGrabTime);
            try
            {
                using (LogHandler.TraceOperations("Analytics:Process", LogHandler.Layer.Business, Guid.NewGuid(), null))
                {
                    if (!message.TE.ContainsKey(TaskRouteConstants.AnalyticsCode))
                    {
                        LogHandler.LogError("Message is not processed in Analytics for FrameId = {0} ,TenantId = {1}, deviceId = {2} , module = {3}", LogHandler.Layer.Business, message.Fid, message.Tid, message.Did, TaskRouteConstants.AnalyticsCode);
                        return true;
                    }
                   
                        bool status = true;
                        DateTime createdDate = DateTime.UtcNow;
                        DateTime modifiedDate = DateTime.UtcNow;
                        if (message.Fs.Length > 0) 
                        {
                            #region Update Frame master table
                            FrameMasterDS framemasterDS = new FrameMasterDS();
                            BE.FrameMasterPersonCount pc = new BE.FrameMasterPersonCount()
                            {
                                TotalPersonCount = message.Fs.Length,
                                NewPersonCount = message.Fs.Where(f => f.Np == "Yes").ToList().Count
                            };

                            string personCountString = JsonConvert.SerializeObject(pc);
                            string username = UserDetails.userName;
                            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                            {
                                username = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
                            }

                        string predictionStatus = "";
                        if (ApplicationConstants.ProcessingStatus.FailedToPredict.Equals(message.Status))
                        {
                            predictionStatus = message.Status;
                        }
                        else
                        {
                            predictionStatus = FrameDetailsProcessConstants.successStatus;
                        }

                        FrameMaster frameMaster = new FrameMaster()
                        {

                            ResourceId = message.Did,
                            FrameId = message.Fid,
                            FrameGrabTime = frameGrabTime,
                            ClassPredictionCount = personCountString,
                            Status = predictionStatus,
                            PartitionKey = partitionKey,
                            FeedProcessorMasterId = Convert.ToInt32(message.FeedId),
                            CreatedBy = username,
                            CreatedDate = createdDate,
                            //ModifiedBy = System.Security.Principal.WindowsIdentity.GetCurrent().Name,
                            //ModifiedDate = modifiedDate,
                            TenantId = Convert.ToInt32(message.Tid),
                        };
                            //if already record is inserted no need to process the message
                            var frameMasterEntity = framemasterDS.GetRecord(frameMaster);
                            if (frameMasterEntity != null)
                            {
                                string frameMasterMessage = frameMasterEntity.ClassPredictionCount;
                                Dictionary<string, string> frameMasterObj = JsonConvert.DeserializeObject<Dictionary<string, string>>(frameMasterMessage);
                                Dictionary<string, string> personCountObj = JsonConvert.DeserializeObject<Dictionary<string, string>>(personCountString);
                            personCountObj.ToList().Where(p => !frameMasterObj.ContainsKey(p.Key)).ToList().ForEach(x => frameMasterObj.Add(x.Key, x.Value));
                            //personCountObj.ToList().ForEach(x => frameMasterObj.Add(x.Key, x.Value));
                                frameMasterEntity.ClassPredictionCount = JsonConvert.SerializeObject(frameMasterObj);
                                var entity = framemasterDS.UpdatePersonCount(frameMasterEntity);

                                if (entity != null)
                                {
                                    status = true;
#if DEBUG
                                    LogHandler.LogDebug("Updated the FrameMaster table Successfully.", LogHandler.Layer.Business, null);
#endif
                                }
                                else
                                {
                                    status = false;
                                    LogHandler.LogError("Update failed for FrameMaster table.", LogHandler.Layer.Business, null);

                                }
                            }
                            else
                            {
                                framemasterDS.Insert(frameMaster);
                                status = true;
                            }

                            #endregion

                            #region Update Frame MetaData table
                            if (status)
                            {
                                status = AnalyticsHelper.InsertFrameMetaDataDetail(message, partitionKey, frameGrabTime);

                                if (status)
                                {
#if DEBUG
                                        LogHandler.LogDebug("Updated the FrameMetadata table Successfully.", LogHandler.Layer.Business, null);
#endif
                                }
                                else
                                {
#if DEBUG
                                        LogHandler.LogError("Update failed for FrameMetadata table.", LogHandler.Layer.Business, null);
#endif
                                }
                            }

                        #endregion

                        #region Update Frame Predicted table and Object Tracking table
                            if (status)
                            {
                                int seqId = 1;
                                DateTime frameProcessedTime = DateTime.UtcNow;
                                var regions = AnalyticsHelper.GetRegions(message.Fid, message.Did, Convert.ToInt32(message.Tid)).ToList();

                                if (regions.Count == 0)
                                    LogHandler.LogDebug($"There are no regions found for FrameId: {message.Fid}, DeviceID: {message.Did} and TenantId: {message.Tid}", LogHandler.Layer.Business, null);


                            FrameRendererMetadata frameRendererMetadata = new FrameRendererMetadata();
                            frameRendererMetadata.Did = message.Did;
                            frameRendererMetadata.Tid = message.Tid;
                            frameRendererMetadata.Fid = message.Fid;
                            frameRendererMetadata.FeedId = message.FeedId;
                            frameRendererMetadata.SequenceNumber = message.SequenceNumber;
                            frameRendererMetadata.FrameNumber = message.FrameNumber;

                            frameRendererMetadata.Fs = new Predictions[message.Fs.Length + regions.Count];
                            for (var i = 0; i < message.Fs.Length; i++ )
                            {
                                frameRendererMetadata.Fs[i] = new Predictions();
                                frameRendererMetadata.Fs[i].Cs = message.Fs[i]?.Cs;
                                if (message.Fs[i]?.Dm != null)
                                {
                                    frameRendererMetadata.Fs[i].Dm = new BoundingBox();
                                    frameRendererMetadata.Fs[i].Dm.H = message.Fs[i]?.Dm.H;
                                    frameRendererMetadata.Fs[i].Dm.W = message.Fs[i]?.Dm.W;
                                    frameRendererMetadata.Fs[i].Dm.X = message.Fs[i]?.Dm.X;
                                    frameRendererMetadata.Fs[i].Dm.Y = message.Fs[i]?.Dm.Y;
                                }
                                
                                frameRendererMetadata.Fs[i].Pid = message.Fs[i].Pid;
                                frameRendererMetadata.Fs[i].Np = message.Fs[i].Np;
                            }
                            
                            int j = message.Fs.Length;
                            foreach (var reg in regions)
                            {
                                frameRendererMetadata.Fs[j] = new Predictions();
                                if (reg?.Region != null)
                                {
                                    frameRendererMetadata.Fs[j].Cs = reg.PredictedClass;
                                    frameRendererMetadata.Fs[j].Dm = JsonConvert.DeserializeObject<QueueEntity.BoundingBox>(reg.Region);
                                    frameRendererMetadata.Fs[j].Lb = reg.PredictedClass;
                                    // frameRendererMetadata.Fs[j].Np = message.Fs[i].Np;
                                }

                                j++;
                            }
                            TaskRouteMetadata taskRouteMetadata = taskRouter.GetTaskRouteConfig(frameRendererMetadata.Tid, frameRendererMetadata.Did);
                            List<string> taskList = message.TE[TaskRouteConstants.AnalyticsCode];
                            if (taskList != null)
                            {
                                foreach (var task in taskList)
                                {
                                    Dictionary<string, List<string>> te = new Dictionary<string, List<string>>();
                                    te = taskRouter.GetTaskRouteDetails(frameRendererMetadata.Tid, frameRendererMetadata.Did, task);
                                    frameRendererMetadata.TE = te;
                                    taskRouter.SendMessageToQueueWithTask(taskRouteMetadata, TaskRouteConstants.AnalyticsCode, frameRendererMetadata, task);
                                }
                            }

                            foreach (var face in message.Fs)
                                {
#if DEBUG
                                        LogHandler.LogDebug("Inserting into the frame_predicted_class_details table.", LogHandler.Layer.Business, null);
#endif

                                    var insRes = predictDS.Insert(new DE.Framedetail.FramePredictedClassDetail()
                                    {
                                        ResourceId = message.Did,
                                        FrameId = message.Fid,
                                        FrameGrabTime = frameGrabTime,
                                        PredictedClassSequenceId = seqId++,
                                        FrameProcessedTime = frameProcessedTime.Ticks,
                                        FrameProcessedDateTime = frameProcessedTime,
                                        Region = JsonConvert.SerializeObject(face.Dm),
                                        PredictedClass = face.Pid,
                                        ConfidenceScore = Convert.ToDouble(face.Cs),
                                        PartitionKey = partitionKey,
                                        CreatedBy = username,
                                        CreatedDate = DateTime.UtcNow,
                                        TenantId = Convert.ToInt32(message.Tid),
                                        PredictionType=predictionType
                                        //ConfigurationManager.AppSettings["AnalyticsPredictionType"] //"UniquePerson"
                                    });
                                
                                if (face.Dm != null)
                                    {
                                        if (regions.Count > 0)
                                        {
                                            foreach (var reg in regions)
                                            {
                                                if (reg?.Region != "")
                                                {
                                                    var r = JsonConvert.DeserializeObject<QueueEntity.BoundingBox>(reg.Region);
                                                    float overlapPercent = AI.Helper.IntersectionOverUnion(new RectangleF(float.Parse(r.X), float.Parse(r.Y), float.Parse(r.W), float.Parse(r.H)),
                                                                                new RectangleF(float.Parse(face.Dm.X), float.Parse(face.Dm.Y), float.Parse(face.Dm.W), float.Parse(face.Dm.H)));
                                                DeviceDetails configDetails = ConfigHelper.SetDeviceDetails(frameRendererMetadata.Tid, frameRendererMetadata.Did, CacheConstants.AnalyticsCode);

                                                var upOverlapThreshold = configDetails.OverlapThreshold;
                                                
                                                if (overlapPercent >= upOverlapThreshold)
                                                    {
                                                        if (insRes != null)
                                                        {
#if DEBUG
                                                                LogHandler.LogDebug("Inserting into the object_tracking_details table.", LogHandler.Layer.Business, null);
#endif
                                                            var otdInsRes = objectTrackingDS.Insert(new DE.Framedetail.ObjectTrackingDetail()
                                                            {
                                                                TenantId = Convert.ToInt32(message.Tid),
                                                                DeviceId = message.Did,
                                                                FrameId = message.Fid,
                                                                ObjectDetectionId = reg.Id,
                                                                ObjectTrackingId = insRes.Id,
                                                                FrameGrabTime = Convert.ToDateTime(reg.FrameGrabTime)
                                                            });

                                                            if (otdInsRes == null)
                                                            {
                                                                LogHandler.LogError("Error while inserting into ObjectTrackingDetail table", LogHandler.Layer.Business, null);
                                                            }
                                                        }
                                                        else
                                                            LogHandler.LogError("Error while inserting into FramePredictionClassDetails table", LogHandler.Layer.Business, null);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }

                            }

                            #endregion
                        }
                        return status;
                    
                   
                   
                }
                
            }
            catch (Exception exMP)
            {

                LogHandler.LogError("Exception in Analytics : {0} , trace : {1}", LogHandler.Layer.Business, exMP.Message, exMP.StackTrace);
                bool failureLogged = false;
                exceptionCount++;

                try
                {
                    Exception ex = new Exception();
                    bool rethrow = ExceptionHandler.HandleException(exMP, ApplicationConstants.WORKER_EXCEPTION_HANDLING_POLICY, out ex);
                    failureLogged = true;

                    if (rethrow)
                    {
                        throw ex;
                    }
                    else
                    {
                        //Set as a succesfull operation as the message was invalid since an equivalent presentation entity was
                        //not found in the database. This could be a rogue transaction.
                        //returning a true since the message has been sent with invalid presentation id and has to be deleted
                        //to avoid further processing
                       // if (exceptionCount == exceptionCount_threshold )
                        //    return false;
                         return true;
                    }
                }
                catch (Exception ex)
                {
                    LogHandler.LogError("Exception in Analytics : {0} , trace : {1}", LogHandler.Layer.Business, ex.Message, ex.StackTrace);
                    //Any messages which would have to indicate to the worker process that the transaction has failed
                    // and the messahe should be retried
                    //Request  processing failed
                    if (!failureLogged)
                    {
                        LogHandler.LogDebug(String.Format("Exception Occured while handling an exception. error message: {0}", ex.Message), LogHandler.Layer.Business, null);
                    }

                  //  if (exceptionCount == exceptionCount_threshold)
                    //    return false;
                     return true;
                }
            }
        }

    }
}
