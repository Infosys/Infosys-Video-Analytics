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
using SE = Infosys.Solutions.Ainauto.VideoAnalytics.Services.MaskDetector.Contracts.Message;
namespace Infosys.Solutions.Ainauto.VideoAnalytics.Processes
{ 
    public class FrameExplainerDetailsProcess : ProcessHandlerBase<QueueEntity.FrameCollectorMetadata>
    {
        //int i = 15;
        ObjectCache cache = MemoryCache.Default;
        CacheItemPolicy policy = new CacheItemPolicy();
        double cacheExpiration = 1.0; //default expiration time
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
        public FrameExplainerDetailsProcess() { }
        public FrameExplainerDetailsProcess(string processId)
        {
            _taskCode = TaskRoute.GetTaskCode(processId);
        }
        static int batchcount = 0;
        public override void Dump(QueueEntity.FrameCollectorMetadata message)
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
            //Added App settings to get the predicted model type
            AppSettings appSettings = Config.AppSettings;
            BE.DeviceDetails deviceDetails=ConfigHelper.SetDeviceDetails(appSettings.TenantID.ToString(),appSettings.DeviceID,CacheConstants.FrameExplainerDataCollector);
            if (ConfigurationManager.AppSettings["FrameCacheSlidingExpirationInMins"] != null)
            {
                frameCacheSlidingExpirationInMins = Convert.ToDouble(System.Configuration.ConfigurationManager.AppSettings["FrameCacheSlidingExpirationInMins"]);
            }
            //   if (ConfigurationManager.AppSettings["PredictionType"] != null)
            if(deviceDetails.PredictionType!=null) {
                /* predictionType=System.Configuration.ConfigurationManager.AppSettings["PredictionType"]; */
                predictionType=deviceDetails.PredictionType;
            }


        }




        public override bool Process(QueueEntity.FrameCollectorMetadata message, int robotId, int runInstanceId, int robotTaskMapId)
        {
            processStopWatch.Reset();
            processStopWatch.Start();
            counterInstanceName = message.Tid + "_" + message.Did;
            TaskRoute taskRouter = new TaskRoute();
#if DEBUG
            LogHandler.LogDebug("counterInstanceName in frameProcessor: {0}", LogHandler.Layer.Business, counterInstanceName);

            LogHandler.LogInfo(String.Format(InfoMessages.Method_Execution_Start, "Process", "FrameDetailsProcess"), LogHandler.Layer.Business, null);
            LogHandler.LogDebug(String.Format("The Process Method of FrameDetailsProcess class is getting executed with parameters :  message={0}; robotId={1};runInstanceId={2}; robotTaskMapId={3}", JsonConvert.SerializeObject(message), robotId, runInstanceId, robotTaskMapId),
                LogHandler.Layer.Business, null);
#endif
            try
            {
                #region
                //Restoring code to track frames predicted for updating DB as per demo portal
                string feedKey = message.Tid + FrameRendererKey.UnderScore + message.Did + FrameRendererKey.UnderScore;// + message.FeedId;

                  long frameNumber = long.Parse(message.Fids[0]);
           //     long frameNumber = int.Parse(DateTime.UtcNow.ToString("yyyyMMdd"));
                int lastFrameNumberSendForPredict = -1;
                int totalFrameCount = -1;
                int framecount = 0;
                int totalMessageCount = 0;
                //    LogHandler.LogError("DC Process feedKey: {0}", LogHandler.Layer.Business, feedKey);

                if (receivedFrameCountDetails.ContainsKey(feedKey))
                {
                    framecount = receivedFrameCountDetails[feedKey];
                    framecount++;
                    receivedFrameCountDetails[feedKey] = framecount;
                }
                else
                {
                    receivedFrameCountDetails.Add(feedKey, 0);
                }
                if (lastFrameNumberSendForPredictDetails.ContainsKey(feedKey))
                {
                    lastFrameNumberSendForPredict = lastFrameNumberSendForPredictDetails[feedKey];
                }
                if (totalFrameCountDetails.ContainsKey(feedKey))
                {
                    totalFrameCount = totalFrameCountDetails[feedKey];
                }

                if (totalFrameSendForPredictDetails.ContainsKey(feedKey))
                {
                    totalMessageCount = totalFrameSendForPredictDetails[feedKey];
                }
                //if (frameNumber == lastFrameNumberSendForPredict || frameNumber == totalFrameCount)
                //{
                //    lastFrameTransferDetails[feedKey] = true;
                //}

                // LogHandler.LogError("DC framecount : {0} ,lastFrameNumberSendForPredict : {1} ,totalFrameCount : {2} ", LogHandler.Layer.Business, framecount, totalMessageCount, totalFrameCount);
                if (framecount >= totalMessageCount && totalMessageCount > 0)
                {
                    //LogHandler.LogError(" allFrameReceived DC framecount : {0} ,totalMessageCount : {1} ,totalFrameCount : {2} ", LogHandler.Layer.Business, framecount, totalMessageCount, totalFrameCount);
                    if (allFrameReceived.ContainsKey(feedKey))
                    {
                        allFrameReceived[feedKey] = true;
                    }
                    else
                    {
                        allFrameReceived.Add(feedKey, true);
                    }
                }
                #endregion

                using (LogHandler.TraceOperations("FrameExplainerDetailsProcess:Process", LogHandler.Layer.Business, Guid.NewGuid(), null))
                {
                    //if (!message.TE.ContainsKey(TaskRouteConstants.FrameExplainerDataCollector))
                    //{
                    //    LogHandler.LogError("Message is not processed in FrameDetailsProcess for FrameId = {0} ,TenantId = {1}, deviceId = {2} , module = {3}", LogHandler.Layer.Business, message.Fid, message.Tid, message.Did, TaskRouteConstants.FrameCollectorCode);
                    //    return true;
                    //}

                    // LogHandler.CollectPerformanceMetric(ApplicationConstants.DCPerfMonCategories.CategoryName,
                    //  ApplicationConstants.DCPerfMonCounters.FramesReceivedCount, counterInstanceName, 1, false, false);

                    cacheExpiration = frameCacheSlidingExpirationInMins;
                    string sstime = DateTime.UtcNow.ToString("yyy-MM-dd,HH:mm:ss.fff tt");
                    message.Mtp.Add(new SE.Mtp() { Etime = "", Src = "XDCO", Stime = sstime });
                    string cacheKey = CacheConstants.FrameCollectorCode + message.Tid + message.Did + frameNumber;
                    string alreadyInserted = (string)cache[cacheKey];
                    //if already record is inserted no need to process the message
                    if (alreadyInserted != null && alreadyInserted == DataCollectionStatus.frameInserted)
                    {
                        return true;
                    }
                    //DE.Queue.FrameExplainerModeMetaData deReceivedPersonCountMessage = new DE.Queue.FrameExplainerModeMetaData();
                    //deReceivedPersonCountMessage = BE.FaceMaskTranslator.FaceMaskExplainerBEToDE(metadata, message);
                    DataCollectorEntityTranslator dataCollectorEntityTranslator = new DataCollectorEntityTranslator();
                    BE.Queue.FrameExplainerModeMetaData frameProcessor = new BE.Queue.FrameExplainerModeMetaData();
                    frameProcessor= dataCollectorEntityTranslator.FaceMaskExplainerBEToDE(message);
                    frameProcessor.FeedId= DateTime.UtcNow.ToString("yyyyMMmmss");
                    string machineName = System.Environment.MachineName;
                    DateTime createdDate = DateTime.UtcNow;
                    DateTime modifiedDate = DateTime.UtcNow;
                    PartitionKeyUtility partitionKeyUtility = new PartitionKeyUtility();
                    long frameGrabTimeTick = long.Parse(frameProcessor.Fids[0]);//getting error hereDateTime.UtcNow.ToString("yyyyMMddHHmmss")
              //      long frameGrabTimeTick = long.Parse(frameProcessor.Fid);
                    DateTime frameGrabTime = new DateTime(frameGrabTimeTick);
                    int parttitionKey = partitionKeyUtility.generatePartionKey(frameProcessor.Tid, frameGrabTime);
                    string username = UserDetails.userName;
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    {
                        username = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
                    }

                    //Inserting the framemetadata into FrameMetadata Table 
                    FrameMetaDataDS frameMetaDataDS = new FrameMetaDataDS();

                    FrameMetadatum metadataEntity = new FrameMetadatum()
                    //frame_metadata metadataEntity = new frame_metadata()
                    {
                        PartitionKey = parttitionKey,
                        MachineName = machineName,
                        ResourceId = frameProcessor.Did,
                    FrameId = frameProcessor.Fids[0],
                   //  FrameId=Convert.ToString( frameNumber),
                        CreatedDate = createdDate,
                        TenantId = Convert.ToInt32(frameProcessor.Tid),
                        FrameGrabTime = frameGrabTime,
                        CreatedBy = username,
                        PredictionType = predictionType,
                        FeedProcessorMasterId = Convert.ToInt32(frameProcessor.FeedId),
                        MetaData = JsonConvert.SerializeObject(frameProcessor) // full object should be stored as string 


                    };
                    //Calculating the Mask count and nomask count and inserting into FrameMaster Table
                    BE.Queue.Predictions[] BEPredArr = frameProcessor.Fs;
                    Dictionary<string, string> resultEntity = new Dictionary<string, string>();
                    bool hasPrediction = false;
                    if (BEPredArr != null)
                    {
                        foreach (BE.Queue.Predictions predictions in BEPredArr)
                        {
                            hasPrediction = true;
                            if (resultEntity.ContainsKey(predictions.Lb))
                            {
                                string count = resultEntity[predictions.Lb];
                                if (count != null)
                                {
                                    int value = Convert.ToInt32(count);
                                    value++;
                                    resultEntity[predictions.Lb] = value.ToString();

                                }
                                else
                                {
                                    int value = 1;
                                    resultEntity[predictions.Lb] = value.ToString();

                                }

                            }
                            else
                            {

                                int value = 1;
                                resultEntity.Add(predictions.Lb, value.ToString());
                            }

                        }
                    }
                    string facedetails = String.Empty;



                    // resultEntity.Add(FrameDetailsProcessConstants.nomaskClassType, noMaskCount.ToString());
                    facedetails = JsonConvert.SerializeObject(resultEntity);
                    FrameMasterDS framemasterDS = new FrameMasterDS();

                    string status = "";
                    if (ApplicationConstants.ProcessingStatus.FailedToPredict.Equals(frameProcessor.Status))
                    {
                        status = frameProcessor.Status;
                    }
                    else
                    {
                        status = FrameDetailsProcessConstants.successStatus;
                    }

                    FrameMaster frameMaster = new FrameMaster()
                    // frame_master frameMaster = new frame_master()
                    {

                        ResourceId = frameProcessor.Did,
                        FrameId =Convert.ToString( frameProcessor.Fids[0]),
                        FrameGrabTime = frameGrabTime,
                        ClassPredictionCount = facedetails,
                        Status = status,
                        PartitionKey = parttitionKey,
                        CreatedBy = username,
                        CreatedDate = createdDate,
                        FeedProcessorMasterId = Convert.ToInt32(frameProcessor.FeedId),
                        //ModifiedBy = System.Security.Principal.WindowsIdentity.GetCurrent().Name,
                        //ModifiedDate = modifiedDate,
                        TenantId = Convert.ToInt32(frameProcessor.Tid),
                        FileName = frameProcessor.videoFileName
                    };



                    //if already record is inserted no need to process the message
                    if (framemasterDS.GetRecord(frameMaster) != null)
                    {

                        string frameMasterMessage = frameMaster.ClassPredictionCount;
                        Dictionary<string, string> frameMasterObj = JsonConvert.DeserializeObject<Dictionary<string, string>>(frameMasterMessage);
                        Dictionary<string, string> personCountObj = JsonConvert.DeserializeObject<Dictionary<string, string>>(facedetails);

                        //Dictionary<string, string> personCountObjtmp = JsonConvert.DeserializeObject<Dictionary<string, string>>(facedetails);
                        //var toUpdate = personCountObj
                        //               .Where(c => !personCountObjtmp.ContainsKey(c.Key))
                        //               .Select(c => new KeyValuePair<string, string>(c.Key, c.Value.ToString()));
                        //toUpdate.ToList().ForEach(x => frameMasterObj.Add(x.Key, x.Value));

                        personCountObj.ToList().Where(p => !frameMasterObj.ContainsKey(p.Key)).ToList().ForEach(x => frameMasterObj.Add(x.Key, x.Value));
                        //personCountObj.ToList().ForEach(x => frameMasterObj.Add(x.Key, x.Value));
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
                        batchcount += 1;                
#if DEBUG
                    LogHandler.LogDebug("counterInstanceName in FrameExplainerDetailsProcess Started Processing Frames: {0}", LogHandler.Layer.Business, counterInstanceName);
                    LogHandler.LogInfo(String.Format(InfoMessages.Method_Execution_Start, "Process", "FrameExplainerDetailsProcess"), LogHandler.Layer.Business, null);
                    LogHandler.LogDebug(String.Format("Receiving Frames from Kafaka Queue {0}:{1} :  message={0}; robotId={1};runInstanceId={2}; robotTaskMapId={3}", JsonConvert.SerializeObject(message.Fids[0]), batchcount, runInstanceId, robotTaskMapId),
                    LogHandler.Layer.Business, null);
#endif             

                    LogHandler.LogInfo(String.Format(InfoMessages.Method_Execution_End, "Process", "FrameExplainerDetailsProcess"), LogHandler.Layer.Business, null);
                    processStopWatch.Stop();                    
                    return true;
                }
            }
            catch (DuplicateRecordException DuplicateEx)
            {
#if DEBUG
                LogHandler.LogDebug("Duplicate Key in frame_metadata for deviceID:{0}, FrameId: {1}. message :{2}", LogHandler.Layer.Business, message.Did, message.Fid, DuplicateEx.Message);
#endif
                return true;
            }
            catch (Exception exMP)
            {
                LogHandler.LogError("Exception in FrameDetailsProcess : {0}", LogHandler.Layer.Business, exMP.Message);
                //LogHandler.LogDebug(String.Format("Exception occured in Process method of FrameDetailsProcess class"), LogHandler.Layer.Business, null);
                bool failureLogged = false;
                // LogHandler.CollectPerformanceMetric(ApplicationConstants.DCPerfMonCategories.CategoryName, ApplicationConstants.DCPerfMonCounters.TotalErrors,
                //      counterInstanceName, 1, false, false);

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
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    LogHandler.LogError(String.Format(ErrorMessages.Exception_Failed, "Process", "FrameDetailsProcess"),
                            LogHandler.Layer.Business, null);
                    //Any messages which would have to indicate to the worker process that the transaction has failed
                    // and the messahe should be retried
                    //MetricProcessing Request  processing failed
                    if (!failureLogged)
                    {
                        LogHandler.LogError(String.Format("Exception Occured while handling an exception in FrameDetailProcess in Process method. error message: {0}", ex.Message), LogHandler.Layer.Business, null);
                    }

                    return false;
                }
            }

        }

        /**
         * Parse the frame metadata and insert into frame_predicted_class_details 
         * */
        private void InsertPredictedClassDetails(BE.Queue.FrameExplainerModeMetaData frameProcessor, int partitionKey)
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
                        //frame_predicted_class_details framePredictedClassDetails = new frame_predicted_class_details();
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
            //  LogHandler.LogError("DC HandleEventMessage , message : {0}", LogHandler.Layer.Business,JsonConvert.SerializeObject(message));
            if (message != null)
            {

                try
                {
                    //LogHandler.LogError("FrameRendererProcess HandleEventMessage {0}", LogHandler.Layer.Business, JsonConvert.SerializeObject(message));
                    string eventType = message.EventType;
                    switch (eventType)
                    {
                        case ProcessingStatus.StartOfFile:
                            if (message != null)
                            {

                                //resetAllFrameSequenceVariables();
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

        // Initialize All the variables for each video feed based and call the TriggerTransferFrame to handle the transportframe
        private void HandleStartOfFile(QueueEntity.MaintenanceMetaData message)
        {
            //  LogHandler.LogError("DC HandleStartOfFile: message : {0}", LogHandler.Layer.Business, JsonConvert.SerializeObject(message));
            QueueEntity.FrameInformation frameInformation = JsonConvert.DeserializeObject<QueueEntity.FrameInformation>(message.Data);
            if (frameInformation != null)
            {
                string feedKey = frameInformation.TID + FrameRendererKey.UnderScore + frameInformation.DID + FrameRendererKey.UnderScore + frameInformation.FeedId;
                //frameMessageDetails.Add(feedKey, new Dictionary<int, TransportFrameDetails>());
                //  sequenceNumberQueueDetails.Add(feedKey, new Queue());
                //  frameTransferCountDetails.Add(feedKey, 0);
                //  lastFrameTransferDetails.Add(feedKey, false);
                receivedFrameCountDetails.Add(feedKey, 0);
                allFrameReceived.Add(feedKey, false);


            }


        }
        private void setEndOfFrameDetails(QueueEntity.MaintenanceMetaData message)
        {
            if (message != null && message.Data != null)
            {
                //  LogHandler.LogError("DC setEndOfFrameDetails : message : {0}", LogHandler.Layer.Business,JsonConvert.SerializeObject(message));
                QueueEntity.FrameInformation frameInformation = JsonConvert.DeserializeObject<QueueEntity.FrameInformation>(message.Data);
                if (frameInformation != null)
                {
                    string feedKey = frameInformation.TID + FrameRendererKey.UnderScore + frameInformation.DID + FrameRendererKey.UnderScore + frameInformation.FeedId;
                    //   LogHandler.LogError("DC setEndOfFrameDetails feedKey: {0}", LogHandler.Layer.Business, feedKey);
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
            //  LogHandler.LogError("DC HandleEndOfFile processing last frame in fc", LogHandler.Layer.Business);
            // FeedRequestDS feedRequestMasterDS = new FeedRequestDS();
            FeedProcessorMasterDS feedProcessorMasterDS = new FeedProcessorMasterDS();
            // Feed_Request feed_Request_entity = new Feed_Request();
            //int feedId = Convert.ToInt32(message.FeedId);
            FeedRequestDS feedRequestDS = new FeedRequestDS();
            var feedRequest = feedRequestDS.GetOneWithMasterId(feedId);
            if (feedRequest != null)
            {
                // feedRequest.LastFrameProcessedTime = DateTime.UtcNow;
                if (feedRequest.LastFrameId != null && feedRequest.LastFrameGrabbedTime != null && feedRequest.LastFrameProcessedTime != null)
                {
                    feedRequest.Status = ProcessingStatus.inProgressStatus;
                    //switch (feedRequest.Status)
                    //{
                    //    case ProcessingStatus.FrameRendererCompletedStatus:
                    //        feedRequest.Status = ProcessingStatus.completedStatus;
                    //        break;
                    //    default:
                    //        feedRequest.Status = ProcessingStatus.DataCollectorCompletedStatus;
                    //        break;

                    //}


                    feedRequestDS.Update(feedRequest);
                    //updating total time taken  in feed_processor_master table
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
