/*=============================================================================================================== *
 * Copyright 2025 Infosys Ltd.                                                                                    *
 * Use of this source code is governed by Apache License Version 2.0 that can be found in the LICENSE file or at  *
 * http://www.apache.org/licenses/                                                                                *
 * ===============================================================================================================*/
﻿using System;
using Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.ProcessScheduler.Framework;
using QueueEntity = Infosys.Solutions.Ainauto.VideoAnalytics.Resource.Entity.Queue;
using Infosys.Solutions.Ainauto.VideoAnalytics.Resource.DataAccess;
using Newtonsoft.Json;
using Infosys.Solutions.Ainauto.VideoAnalytics.Resource.Entity;
using Infosys.Solutions.Ainauto.VideoAnalytics.BusinessComponent.Translator;
using BE = Infosys.Solutions.Ainauto.VideoAnalytics.BusinessEntity;
using Infosys.Solutions.Ainauto.VideoAnalytics.BusinessComponent;
using System.Collections.Generic;
using System.Linq;
using System.Configuration;
using System.Runtime.Caching;
using System.Data.SqlClient;
using System.Diagnostics;
using Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.TaskRoute;
using Infosys.Solutions.Ainauto.VideoAnalytics.Resource.Entity.Framedetail;
using System.Runtime.InteropServices;
using Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.Common;
using static Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.Common.ApplicationConstants;
using Infosys.Solutions.Ainauto.VideoAnalytics.BusinessEntity.Queue;
using Nest;
using SE = Infosys.Solutions.Ainauto.VideoAnalytics.Services.MaskDetector.Contracts;
namespace Infosys.Solutions.Ainauto.VideoAnalytics.Processes
{
    public   class SensorDataCollectorProcess : ProcessHandlerBase<QueueEntity.SensorMetaData>
    {
       
        ObjectCache cache = MemoryCache.Default;
        CacheItemPolicy policy = new CacheItemPolicy();
        double cacheExpiration = 1.0; 
        Stopwatch processStopWatch = new Stopwatch();
        string counterInstanceName = "";
        static string predictionType = "";
        static double frameCacheSlidingExpirationInMins = 10;
        static private Dictionary<string, bool> allFrameReceived = new Dictionary<string, bool>();
        static private Dictionary<string, int> receivedFrameCountDetails = new Dictionary<string, int>();
        static private Dictionary<string, int> lastFrameNumberSendForPredictDetails = new Dictionary<string, int>();
        static private Dictionary<string, int> totalFrameCountDetails = new Dictionary<string, int>();
        static private Dictionary<string, int> totalFrameSendForPredictDetails = new Dictionary<string, int>();

        public string _taskCode;
        public SensorDataCollectorProcess() { }
        public SensorDataCollectorProcess(string processId)
        {
            _taskCode = TaskRoute.GetTaskCode(processId);
        }

        public override void Dump(QueueEntity.SensorMetaData message)
        {

        }
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
                    LogHandler.LogError("Exception in Initialize method of FramePreloaderMetadata : {0} ", LogHandler.Layer.Business, ex.Message);
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
                if (cache.Contains(resourceIdList[j]))
                {
                    cache.Remove(resourceIdList[j]);

                }

            }

        }


        private void ReadFromConfig()
        {
            
            AppSettings appSettings = Config.AppSettings;
            BE.DeviceDetails deviceDetails=ConfigHelper.SetDeviceDetails(appSettings.TenantID.ToString(),appSettings.DeviceID,CacheConstants.SensorDataCollectorProcess);
            if (ConfigurationManager.AppSettings["FrameCacheSlidingExpirationInMins"] != null)
            {
                frameCacheSlidingExpirationInMins = Convert.ToDouble(System.Configuration.ConfigurationManager.AppSettings["FrameCacheSlidingExpirationInMins"]);
            }
           
            if(deviceDetails.PredictionType!=null) {
               
                predictionType=deviceDetails.PredictionType;
            }


        }




        public override bool Process(QueueEntity.SensorMetaData message, int robotId, int runInstanceId, int robotTaskMapId)
        {
                      processStopWatch.Reset();
                       processStopWatch.Start();
                      counterInstanceName = message.Tid + "_" + message.Did;
                      TaskRoute taskRouter = new TaskRoute();
#if DEBUG
            LogHandler.LogDebug("counterInstanceName in SensorDataCollector: {0}", LogHandler.Layer.Business, counterInstanceName);

            LogHandler.LogInfo(String.Format(InfoMessages.Method_Execution_Start, "Process", "SensorDataCollectorProcess"), LogHandler.Layer.Business, null);
            LogHandler.LogDebug(String.Format("The Process Method of SensorDataCollector class is getting executed with parameters :  message={0}; robotId={1};runInstanceId={2}; robotTaskMapId={3}", JsonConvert.SerializeObject(message), robotId, runInstanceId, robotTaskMapId),
                LogHandler.Layer.Business, null);
#endif
            try
            {
                #region
                
                string feedKey = message.Tid + FrameRendererKey.UnderScore + message.Did + FrameRendererKey.UnderScore;
                
                int lastFrameNumberSendForPredict = -1;
                int totalFrameCount = -1;
                int framecount = 0;
                int totalMessageCount = 0;
                
                #region Code Need to remove
                

                
                #endregion
                #endregion

                using (LogHandler.TraceOperations("FrameDetailsProcess:Process", LogHandler.Layer.Business, Guid.NewGuid(), null))
                {
                   
                    string sstime = DateTime.UtcNow.ToString("yyy-MM-dd,HH:mm:ss.fff tt");
                    List<SE.Message.Mtp> MtpData = new List<SE.Message.Mtp>() {
                    
                    new  SE.Message.Mtp() {Etime=" ",Src="Sensor Data Collector",Stime=sstime},
                };


                    cacheExpiration = frameCacheSlidingExpirationInMins;
                    Random rand = new Random();
                    string feedprocessormasterid = Convert.ToString( rand.Next());
                    string cacheKey = CacheConstants.SensorDataCollectorProcess + message.Tid + message.Did;
                    
                    SensorDataEntityTranslator dataCollectorEntityTranslator = new SensorDataEntityTranslator();
                    BE.Queue.SensorMetaData frameProcessor = dataCollectorEntityTranslator.SensorDataCollectorTranslator(message);
                    string machineName = System.Environment.MachineName;
                    DateTime createdDate = DateTime.UtcNow;
                    DateTime modifiedDate = DateTime.UtcNow;
                    PartitionKeyUtility partitionKeyUtility = new PartitionKeyUtility();
                    long frameGrabTimeTick = long.Parse(feedprocessormasterid);

                    DateTime frameGrabTime = createdDate;
                    int parttitionKey = partitionKeyUtility.generatePartionKey(frameProcessor.Tid, frameGrabTime);
                    string username = UserDetails.userName;
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    {
                        username = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
                    }
           
                     
                    FrameMetaDataDS frameMetaDataDS = new FrameMetaDataDS();

                    FrameMetadatum metadataEntity = new FrameMetadatum()
                    
                    {
                        PartitionKey = parttitionKey,
                        MachineName = machineName,
                        ResourceId = frameProcessor.Did,
                         FrameId = DateTime.UtcNow.ToString("yyyyMMddHHmmss"),
                        CreatedDate = createdDate,
                        TenantId = Convert.ToInt32(frameProcessor.Tid),
                        FrameGrabTime = frameGrabTime,
                        CreatedBy = username,
                        PredictionType = predictionType,
                        FeedProcessorMasterId = Convert.ToInt32(feedprocessormasterid),
                        MetaData = JsonConvert.SerializeObject(message.Metadata)  


                    };
                    
                    Dictionary<string, string> resultEntity = new Dictionary<string, string>();
                    bool hasPrediction = false;
                    
                   
                    
                    string facedetails = String.Empty;



                   
                    facedetails = JsonConvert.SerializeObject(resultEntity);
                    FrameMasterDS framemasterDS = new FrameMasterDS();

                    string status = "";
                    if (ApplicationConstants.ProcessingStatus.FailedToPredict.Equals(frameProcessor.Status))
                    {
                        status = frameProcessor.Status;
                    }
                    else
                    {
                        status = message.Status;
                    }

                    FrameMaster frameMaster = new FrameMaster()
                   
                    {

                        ResourceId = frameProcessor.Did,
                        FrameId = DateTime.UtcNow.ToString("yyyyMMddHHmmss"),
                        FrameGrabTime = frameGrabTime,
                        ClassPredictionCount = facedetails,
                        Status = status,
                        PartitionKey = parttitionKey,
                        CreatedBy = username,
                        CreatedDate = createdDate,
                        FeedProcessorMasterId = Convert.ToInt32(feedprocessormasterid),
                        
                        TenantId = Convert.ToInt32(frameProcessor.Tid),
                        FileName = "SensorData",
                        Mtp= JsonConvert.SerializeObject(MtpData)
                    };



                    
                    if (framemasterDS.GetRecord(frameMaster) != null)
                    {

                        string frameMasterMessage = frameMaster.ClassPredictionCount;
                        Dictionary<string, string> frameMasterObj = JsonConvert.DeserializeObject<Dictionary<string, string>>(frameMasterMessage);
                        Dictionary<string, string> personCountObj = JsonConvert.DeserializeObject<Dictionary<string, string>>(facedetails);

                        

                        personCountObj.ToList().Where(p => !frameMasterObj.ContainsKey(p.Key)).ToList().ForEach(x => frameMasterObj.Add(x.Key, x.Value));
                        
                        frameMaster.ClassPredictionCount = JsonConvert.SerializeObject(frameMasterObj);

                        var entity = framemasterDS.UpdatePersonCount(frameMaster);

                        if (entity != null)
                        {
#if DEBUG
                            LogHandler.LogDebug("Updated the FrameMaster table Successfully.", LogHandler.Layer.Business, null);
#endif
                        }
                        else
                        {
#if DEBUG
                            LogHandler.LogError("Update failed for FrameMaster table.", LogHandler.Layer.Business, null);
#endif
                        }

                        return true;
                    }

                    framemasterDS.Insert(frameMaster);

                    policy.SlidingExpiration = TimeSpan.FromMinutes(cacheExpiration);
                    cache.Set(cacheKey, DataCollectionStatus.frameInserted, policy);
                    
                      frameMetaDataDS.Insert(metadataEntity);
                   


                    LogHandler.LogInfo(String.Format(InfoMessages.Method_Execution_End, "Process", "FrameDetailsProcess"), LogHandler.Layer.Business, null);
                    processStopWatch.Stop();
                    

                    if (allFrameReceived[feedKey])
                   
                    {
                   

                    }

                    return true;
                }

            }

            catch (DuplicateRecordException DuplicateEx)
            {
#if DEBUG
                LogHandler.LogDebug("Duplicate Key in frame_metadata for deviceID:{0}, FrameId: {1}. message :{2}", LogHandler.Layer.Business, message.Did, "", DuplicateEx.Message);
#endif
                return true;
            }
           
       
            catch (Exception exMP)
            {
                LogHandler.LogError("Exception in FrameDetailsProcess : {0}", LogHandler.Layer.Business, exMP.Message);
                bool failureLogged = false;
                

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
                       
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    LogHandler.LogError(String.Format(ErrorMessages.Exception_Failed, "Process", "FrameDetailsProcess"),
                            LogHandler.Layer.Business, null);
                   
                    if (!failureLogged)
                    {
                        LogHandler.LogError(String.Format("Exception Occured while handling an exception in FrameDetailProcess in Process method. error message: {0}", ex.Message), LogHandler.Layer.Business, null);
                    }

                    return false;
                }
            }
            return true;
        }

        /**
         * Parse the frame metadata and insert into frame_predicted_class_details 
         * */
        private void InsertPredictedClassDetails(BE.Queue.FrameCollectorMetadata frameProcessor, int partitionKey)
        {
#if DEBUG
            LogHandler.LogInfo(String.Format(InfoMessages.Method_Execution_Start, "InsertPredictedClassDetails", "FrameDetailsProcess"), LogHandler.Layer.Business, null);
            LogHandler.LogDebug(String.Format("The InsertPredictedClassDetails Method of FrameDetailsProcess class is getting executed with parameters :" +
                "  frameProcessor={0}; partitionKey={1};", JsonConvert.SerializeObject(frameProcessor), partitionKey),
                LogHandler.Layer.Business, null);
#endif
            try
            {
#if DEBUG
                using (LogHandler.TraceOperations("FrameDetailsProcess:InsertPredictedClassDetails", LogHandler.Layer.Business, Guid.NewGuid(), null))
                {
#endif
                    string resourceId = frameProcessor.Did;
                    string frameId = frameProcessor.Fid;
                    int tenantId = Convert.ToInt32(frameProcessor.Tid);
                    DateTime createdDate = DateTime.UtcNow;
                    string createdBy = UserDetails.userName;
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    {
                        createdBy = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
                    }
                    long frameprocessedtime = 0;
                    if (frameProcessor.Pts != null)
                    {
                        frameprocessedtime = long.Parse(frameProcessor.Pts);
                    }
                    DateTime frameprocesseddatetime = new DateTime(frameprocessedtime);
                    string region = null;
                    int predictedclasssequenceId = 0;
                    long frameGrabTimeTick = long.Parse(frameProcessor.Fid);
                    DateTime frameGrabTime = new DateTime(frameGrabTimeTick);

                    foreach (BE.Queue.Predictions prediction in frameProcessor.Fs)
                    {
                        FramePredictedClassDetail framePredictedClassDetails = new FramePredictedClassDetail();
                        
                        if (prediction.Dm != null)
                        {
                            region = JsonConvert.SerializeObject(prediction.Dm);

                        }
                        else
                        {
                            region = "";
                        }
                        double confidence_score = ConvertToDouble(prediction.Cs);
                        string predictedclass = prediction.Lb;
                        predictedclasssequenceId++;
                        framePredictedClassDetails.Region = region;
                        framePredictedClassDetails.ConfidenceScore = confidence_score;
                        framePredictedClassDetails.FrameId = frameId;
                        framePredictedClassDetails.ResourceId = resourceId;
                        framePredictedClassDetails.FrameGrabTime = frameGrabTime;
                        framePredictedClassDetails.TenantId = tenantId;
                        framePredictedClassDetails.PredictedClass = predictedclass;
                        framePredictedClassDetails.PartitionKey = partitionKey;
                        framePredictedClassDetails.CreatedDate = createdDate;
                        framePredictedClassDetails.CreatedBy = createdBy;
                        framePredictedClassDetails.PredictedClassSequenceId = predictedclasssequenceId;
                        framePredictedClassDetails.FrameProcessedTime = frameprocessedtime;
                        framePredictedClassDetails.FrameProcessedDateTime = frameprocesseddatetime;
                        framePredictedClassDetails.PredictionType = predictionType;
                        framePredictedClassDetails.FeedProcessorMasterId = Convert.ToInt32(frameProcessor.FeedId);
                        FramePredictedClassDetailsDS framePredictedClassDetailsDS = new FramePredictedClassDetailsDS();
                        using (LogHandler.TraceOperations("FrameDetailsProcess:Insert into frame_predicted_class_details", LogHandler.Layer.Business, Guid.NewGuid(), null))
                        {
                            framePredictedClassDetailsDS.Insert(framePredictedClassDetails);
                        }

                    }
                    FeedRequestDS feedRequestDS = new FeedRequestDS();
                    FeedRequest feedRequest = feedRequestDS.GetOneWithMasterId(Convert.ToInt32(frameProcessor.FeedId));
                    if (feedRequest != null)
                    {
                        feedRequest.LastFrameProcessedTime = frameprocesseddatetime;
                        feedRequestDS.Update(feedRequest);
                    }
#if DEBUG
                    LogHandler.LogInfo(String.Format(InfoMessages.Method_Execution_End, "InsertPredictedClassDetails", "FrameDetailsProcess"), LogHandler.Layer.Business, null);
                }
#endif
            }

            catch (Exception ex)
            {
                LogHandler.LogError("Exception in FrameDetailsProcess class and InsertPredictedClassDetails method  : {0}", LogHandler.Layer.Business, ex.Message);
                throw ex;
            }

        }

        private double ConvertToDouble(string str)
        {
            double d;
            if (str != null && Double.TryParse(str, out d))
                return d;
            else
                return 0;
        }


        public override bool HandleEventMessage(QueueEntity.MaintenanceMetaData message)
        {
            if (message != null)
            {

                try
                {
                    string eventType = message.EventType;
                    switch (eventType)
                    {
                        case ProcessingStatus.StartOfFile:
                            if (message != null)
                            {

                               
                                HandleStartOfFile(message);


                            }
                            break;
                        case ProcessingStatus.EndOfFile:
                            setEndOfFrameDetails(message);
                            break;
                    }
                }
                catch (Exception exp)
                {
                    LogHandler.LogError("Exception occured in FrameDetailProcess HandleEventMessage {0} , Exception {1}",
                        LogHandler.Layer.Business, JsonConvert.SerializeObject(message), exp.Message);
                    return false;
                }
            }
            return true;
        }

        
        private void HandleStartOfFile(QueueEntity.MaintenanceMetaData message)
        {
            QueueEntity.FrameInformation frameInformation = JsonConvert.DeserializeObject<QueueEntity.FrameInformation>(message.Data);
            if (frameInformation != null)
            {
                string feedKey = frameInformation.TID + FrameRendererKey.UnderScore + frameInformation.DID + FrameRendererKey.UnderScore + frameInformation.FeedId;
               
                receivedFrameCountDetails.Add(feedKey, 0);
                allFrameReceived.Add(feedKey, false);


            }


        }
        private void setEndOfFrameDetails(QueueEntity.MaintenanceMetaData message)
        {
            if (message != null && message.Data != null)
            {
                QueueEntity.FrameInformation frameInformation = JsonConvert.DeserializeObject<QueueEntity.FrameInformation>(message.Data);
                if (frameInformation != null)
                {
                    string feedKey = frameInformation.TID + FrameRendererKey.UnderScore + frameInformation.DID + FrameRendererKey.UnderScore + frameInformation.FeedId;
                    int lastFrameNumberSendForPredict = int.Parse(frameInformation.LastFrameNumberSendForPrediction);
                    int totalFrameCount = int.Parse(frameInformation.TotalFrameCount);
                    int totalMessage = int.Parse(frameInformation.TotalMessageSendForPrediction);
                    lastFrameNumberSendForPredictDetails.Add(feedKey, lastFrameNumberSendForPredict);
                    totalFrameCountDetails.Add(feedKey, totalFrameCount);
                    totalFrameSendForPredictDetails.Add(feedKey, totalMessage);
                    int count = 0;
                    if (receivedFrameCountDetails.ContainsKey(feedKey))
                    {
                        count = receivedFrameCountDetails[feedKey];
                    }
                    if (count == totalMessage)
                    {
                        int feedId = int.Parse(frameInformation.FeedId);
                        HandleEndOfFile(feedId);
                    }
                }
            }
        }


        private void HandleEndOfFile(int feedId)
        {
            
            FeedProcessorMasterDS feedProcessorMasterDS = new FeedProcessorMasterDS();
            
            FeedRequestDS feedRequestDS = new FeedRequestDS();
            var feedRequest = feedRequestDS.GetOneWithMasterId(feedId);
            if (feedRequest != null)
            {
               
                if (feedRequest.LastFrameId != null && feedRequest.LastFrameGrabbedTime != null && feedRequest.LastFrameProcessedTime != null)
                {
                    feedRequest.Status = ProcessingStatus.inProgressStatus;
                   


                    feedRequestDS.Update(feedRequest);
                    
                    var feedProcessorMaster = feedProcessorMasterDS.GetOneWithMasterId(feedId);
                    FrameMasterDS frameMasterDs = new FrameMasterDS();
                    feedProcessorMaster.FeedProcessorMasterId = feedId;
                    feedProcessorMaster.TotalFrameProcessed = frameMasterDs.GetCount(feedId);
                    if (feedRequest.LastFrameProcessedTime != null && feedRequest.StartFrameProcessedTime != null)
                    {
                        feedProcessorMaster.TimeTaken = ((DateTime)feedRequest.LastFrameProcessedTime - (DateTime)feedRequest.StartFrameProcessedTime).TotalSeconds;
                    }
                    feedProcessorMasterDS.Update(feedProcessorMaster);
                }
            }
        }

       

        }
}
