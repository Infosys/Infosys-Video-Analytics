/*=============================================================================================================== *
 * Copyright 2024 Infosys Ltd.                                                                                    *
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

using Infosys.Solutions.Ainauto.VideoAnalytics.Resource.DataAccess.ElasticSearch;

using DE = Infosys.Solutions.Ainauto.VideoAnalytics.Resource.Entity.Index;
using Infosys.Solutions.Ainauto.VideoAnalytics.BusinessEntity;
using Infosys.Solutions.Ainauto.VideoAnalytics.Resource.Entity.VideoAnalytics;
using Infosys.Solutions.Ainauto.VideoAnalytics.Resource.Entity.Queue;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Xml.Linq;
using System.Threading.Tasks;

namespace Infosys.Solutions.Ainauto.VideoAnalytics.Processes
{
    public class FrameElasticSearchProcess : ProcessHandlerBase<QueueEntity.FrameElasticSearchMetaData>
    {
        ObjectCache cache = MemoryCache.Default;
        CacheItemPolicy policy = new CacheItemPolicy();
        double cacheExpiration = 1.0; 
        Stopwatch processStopWatch = new Stopwatch();
        string counterInstanceName = "";
        static string predictionType = "";
        static string elasticStoreIndexName = "";
        static double frameCacheSlidingExpirationInMins = 10;
        static private Dictionary<string, bool> allFrameReceived = new Dictionary<string, bool>();
        static private Dictionary<string, int> receivedFrameCountDetails = new Dictionary<string, int>();
        static private Dictionary<string, int> lastFrameNumberSendForPredictDetails = new Dictionary<string, int>();
        static private Dictionary<string, int> totalFrameCountDetails = new Dictionary<string, int>();
        static private Dictionary<string, int> totalFrameSendForPredictDetails = new Dictionary<string, int>();
        public override void Dump(QueueEntity.FrameElasticSearchMetaData message)
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

        private void ReadFromConfig()
        {
            
            AppSettings appSettings = Config.AppSettings;
            
            elasticStoreIndexName = appSettings.ElasticStoreIndexName;
            if (ConfigurationManager.AppSettings["FrameCacheSlidingExpirationInMins"] != null)
            {
                frameCacheSlidingExpirationInMins = Convert.ToDouble(System.Configuration.ConfigurationManager.AppSettings["FrameCacheSlidingExpirationInMins"]);
            }
            
            if (appSettings.PredictionType != null)
            {
                
                predictionType = appSettings.PredictionType;
            }

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

        public override bool Process(QueueEntity.FrameElasticSearchMetaData message, int robotId, int runInstanceId, int robotTaskMapId)
        {
            processStopWatch.Reset();
            processStopWatch.Start();
            counterInstanceName = message.Tid + "_" + message.Did;
            DeviceDetails response = ConfigHelper.SetDeviceDetails(message.Tid, message.Did, CacheConstants.FrameElasticSearch);

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
                string feedKey = message.Tid + FrameRendererKey.UnderScore + message.Did + FrameRendererKey.UnderScore + message.FeedId;
                int frameNumber = int.Parse(message.FrameNumber);
                int lastFrameNumberSendForPredict = -1;
                int totalFrameCount = -1;
                int framecount = 0;
                int totalMessageCount = 0;
                

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
               

                if (framecount >= totalMessageCount && totalMessageCount > 0)
                {
                    
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

                using (LogHandler.TraceOperations("FrameElasticProcess:Process", LogHandler.Layer.Business, Guid.NewGuid(), null))
                {
                    if (!message.TE.ContainsKey(TaskRouteConstants.FrameElasticSearch))
                    {
                        LogHandler.LogError("Message is not processed in FrameElasticProcess for FrameId = {0} ,TenantId = {1}, deviceId = {2} , module = {3}", LogHandler.Layer.Business, message.Fid, message.Tid, message.Did, TaskRouteConstants.FrameCollectorCode);
                        return true;
                    }

                    

                    cacheExpiration = frameCacheSlidingExpirationInMins;

                    string cacheKey = CacheConstants.FrameCollectorCode + message.Tid + message.Did + message.Fid;
                    string alreadyInserted = (string)cache[cacheKey];
                    
                    if (alreadyInserted != null && alreadyInserted == DataCollectionStatus.frameInserted)
                    {
                        return true;
                    }
                    if (response.EnableElasticStore.ToLower() == "no")
                    {
                        ElasticSearchEntityTranslator elasticSearchCollectorEntityTranslator = new ElasticSearchEntityTranslator();
                        DE.FrameElasticSearchMetadata frameElasticSearch = elasticSearchCollectorEntityTranslator.DataCollectorTranslator(message);
                        string machineName = System.Environment.MachineName;
                        DateTime createdDate = DateTime.UtcNow;
                        DateTime modifiedDate = DateTime.UtcNow;
                        PartitionKeyUtility partitionKeyUtility = new PartitionKeyUtility();
                        long frameGrabTimeTick = long.Parse(frameElasticSearch.Fid);

                        DateTime frameGrabTime = new DateTime(frameGrabTimeTick);
                        int parttitionKey = partitionKeyUtility.generatePartionKey(frameElasticSearch.Tid, frameGrabTime);
                        string username = UserDetails.userName;
                        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                        {
                            username = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
                        }

                         
                        FrameMetaDataDS frameMetaDataDS = new FrameMetaDataDS();

                        FrameMetaDataActionDS frameMetaDataActionDS = new FrameMetaDataActionDS();

                        
                        DE.Predictions[] BEPredArr = frameElasticSearch.Fs;

                        DE.FrameMetaData frameMetaData = new DE.FrameMetaData()
                        
                        {

                            Did = frameElasticSearch.Did,
                            FeedId = frameElasticSearch.FeedId,
                            Fid = frameElasticSearch.Fid,
                            I_fn = frameElasticSearch.FileName,
                            FrameNumber = frameElasticSearch.FrameNumber,
                            Fs = frameElasticSearch.Fs,
                            PredictionType = predictionType,
                            Pts = frameElasticSearch.Pts,
                            SequenceNumber = frameElasticSearch.SequenceNumber,
                            Status = frameElasticSearch.Status,
                            Tid = Convert.ToInt32(frameElasticSearch.Tid),
                            Mtp = frameElasticSearch.Mtp,
                            CreatedBy = username,
                            CreatedDate = createdDate,
                            ModifiedBy = username,
                            ModifiedDate = modifiedDate


                        };

                        
                        frameMetaDataActionDS.Insert(frameMetaData, elasticStoreIndexName);  
                        return true;
                    }
                    

                    

                    

                    policy.SlidingExpiration = TimeSpan.FromMinutes(cacheExpiration);
                    cache.Set(cacheKey, DataCollectionStatus.frameInserted, policy);
                    


                    LogHandler.LogInfo(String.Format(InfoMessages.Method_Execution_End, "Process", "FrameElasticProcess"), LogHandler.Layer.Business, null);
                    processStopWatch.Stop();
                    

                    if (allFrameReceived[feedKey])
                    
                    {
                        HandleEndOfFile(Convert.ToInt32(message.FeedId));

                    }

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
                    LogHandler.LogError("Exception occured in FrameElasticSearch HandleEventMessage {0} , Exception {1}",
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

        
        public static async Task SaveElasticSearch(QueueEntity.FrameRendererMetadata message, string Raw_base64_image, string Rendered_base64_image, string esLabels, string modelName)
        {
            await Task.Run(() =>
            {
                ObjectCache cache = MemoryCache.Default;
                ElasticSearchEntityTranslator elasticSearchCollectorEntityTranslator = new ElasticSearchEntityTranslator();
                DE.FrameElasticSearchMetadata frameElasticSearch = elasticSearchCollectorEntityTranslator.DataCollectorTranslatorRenderer(message, Raw_base64_image, Rendered_base64_image);
                string machineName = System.Environment.MachineName;
                DateTime createdDate = DateTime.UtcNow;
                DateTime modifiedDate = DateTime.UtcNow;
                string username = UserDetails.userName;
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    username = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
                }
                int length = frameElasticSearch.Fs.Length;
                FrameMetaDataActionDS frameMetaDataActionDS = new FrameMetaDataActionDS();
                DE.Predictions[] BEPredArr = new DE.Predictions[length]; 
                int k = 0;
                int j = 0;
                var lbllist = JsonConvert.DeserializeObject<List<string>>(esLabels);
                
                List<string> lstValueExists = new List<string>();
                foreach (var item in lbllist)
                {
                    for (j = 0; j < frameElasticSearch.Fs.Count(); j++)
                    {
                        if (frameElasticSearch.Fs[j].Lb == item)
                        {
                            bool temp = lstValueExists.Contains(item);
                            
                            if (temp == false)
                            {
                                DE.Predictions BEPred = new DE.Predictions();
                                DE.BoundingBox boundingBox = new DE.BoundingBox();
                                if (frameElasticSearch.Fs[j].Dm != null)
                                {
                                    boundingBox.X = frameElasticSearch.Fs[j].Dm.X;
                                    boundingBox.Y = frameElasticSearch.Fs[j].Dm.Y;
                                    boundingBox.W = frameElasticSearch.Fs[j].Dm.W;
                                    boundingBox.H = frameElasticSearch.Fs[j].Dm.H;
                                    
                                    BEPred.Dm = boundingBox;
                                }
                                else
                                {
                                    BEPred.Dm = null;
                                }
                                BEPred.Cs = frameElasticSearch.Fs[j].Cs;
                                BEPred.Lb = frameElasticSearch.Fs[j].Lb;

                                

                                BEPred.Info = frameElasticSearch.Fs[j].Info;
                                BEPred.Kp = frameElasticSearch.Fs[j].Kp;
                                BEPred.Tpc = frameElasticSearch.Fs[j].Tpc;
                                BEPred.Bpc = frameElasticSearch.Fs[j].Bpc;
                                BEPredArr[k] = BEPred;
                                lstValueExists.Add(BEPred.Lb);
                                
                                k++;
                            }
                        }
                        else
                        {
                            DE.Predictions BEPred = new DE.Predictions();
                            DE.BoundingBox boundingBox = new DE.BoundingBox();
                            if (frameElasticSearch.Fs[j].Dm != null)
                            {
                                boundingBox.X = frameElasticSearch.Fs[j].Dm.X;
                                boundingBox.Y = frameElasticSearch.Fs[j].Dm.Y;
                                boundingBox.W = frameElasticSearch.Fs[j].Dm.W;
                                boundingBox.H = frameElasticSearch.Fs[j].Dm.H;
                                
                                BEPred.Dm = boundingBox;
                            }
                            else
                            {
                                BEPred.Dm = null;
                            }
                            BEPred.Cs = frameElasticSearch.Fs[j].Cs;
                            BEPred.Lb = frameElasticSearch.Fs[j].Lb;

                            

                            BEPred.Info = frameElasticSearch.Fs[j].Info;
                            BEPred.Kp = frameElasticSearch.Fs[j].Kp;
                            BEPred.Tpc = frameElasticSearch.Fs[j].Tpc;
                            BEPred.Bpc = frameElasticSearch.Fs[j].Bpc;
                            BEPredArr[k] = BEPred;
                            lstValueExists.Add(BEPred.Lb);

                            k++;
                        }
                    }
                }
                BEPredArr = BEPredArr.Where(c => c != null).ToArray();

                DE.FrameMetaData frameMetaData = new DE.FrameMetaData()
                
                {

                    Did = frameElasticSearch.Did,
                    FeedId = frameElasticSearch.FeedId,
                    Fid = frameElasticSearch.Fid,
                    I_fn = frameElasticSearch.FileName,
                    FrameNumber = frameElasticSearch.FrameNumber,
                    
                    Fs = BEPredArr,
                    PredictionType = modelName,
                    Pts = frameElasticSearch.Pts,
                    SequenceNumber = frameElasticSearch.SequenceNumber,
                    Status = frameElasticSearch.Status,
                    Tid = Convert.ToInt32(frameElasticSearch.Tid),
                    Mtp = frameElasticSearch.Mtp,
                    CreatedBy = username,
                    CreatedDate = createdDate,
                    ModifiedBy = username,
                    ModifiedDate = modifiedDate,
                    Raw_base64_image = frameElasticSearch.Raw_base64_image,
                    Rendered_base64_image = frameElasticSearch.Rendered_base64_image
                };
                var result = frameMetaDataActionDS.Insert(frameMetaData, elasticStoreIndexName);
                
                return result;
            });
        }

        public static bool SaveElasticStore(QueueEntity.FrameRendererMetadata message, string Raw_base64_image, string Rendered_base64_image, string esLabels, string modelName)
        {
            ElasticSearchEntityTranslator elasticSearchCollectorEntityTranslator = new ElasticSearchEntityTranslator();
            DE.FrameElasticSearchMetadata frameElasticSearch = elasticSearchCollectorEntityTranslator.DataCollectorTranslatorRenderer(message, Raw_base64_image, Rendered_base64_image);
            string machineName = System.Environment.MachineName;
            DateTime createdDate = DateTime.UtcNow;
            DateTime modifiedDate = DateTime.UtcNow;
            string username = UserDetails.userName;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                username = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
            }
            int length = frameElasticSearch.Fs.Length;
            FrameMetaDataActionDS frameMetaDataActionDS = new FrameMetaDataActionDS();
            DE.Predictions[] BEPredArr = new DE.Predictions[length]; 
            int k = 0;
            int j = 0;
            DE.FrameMetaData frameMetaData = new DE.FrameMetaData()
            
            {

                Did = frameElasticSearch.Did,
                FeedId = frameElasticSearch.FeedId,
                Fid = frameElasticSearch.Fid,
                I_fn = frameElasticSearch.FileName,
                FrameNumber = frameElasticSearch.FrameNumber,
                Fs = frameElasticSearch.Fs,
                
                PredictionType = modelName,
                Pts = frameElasticSearch.Pts,
                SequenceNumber = frameElasticSearch.SequenceNumber,
                Status = frameElasticSearch.Status,
                Tid = Convert.ToInt32(frameElasticSearch.Tid),
                Mtp = frameElasticSearch.Mtp,
                CreatedBy = username,
                CreatedDate = createdDate,
                ModifiedBy = username,
                ModifiedDate = modifiedDate,
                Raw_base64_image = frameElasticSearch.Raw_base64_image,
                Rendered_base64_image = frameElasticSearch.Rendered_base64_image
            };
            var result = frameMetaDataActionDS.Insert(frameMetaData, elasticStoreIndexName);
            return result;
        }
    }
}
