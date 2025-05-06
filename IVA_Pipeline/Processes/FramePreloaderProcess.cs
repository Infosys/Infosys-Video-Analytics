/*=============================================================================================================== *
 * Copyright 2025 Infosys Ltd.                                                                                    *
 * Use of this source code is governed by Apache License Version 2.0 that can be found in the LICENSE file or at  *
 * http://www.apache.org/licenses/                                                                                *
 * ===============================================================================================================*/
﻿
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.ProcessScheduler.Framework;
using QueueEntity = Infosys.Solutions.Ainauto.VideoAnalytics.Resource.Entity.Queue;
using BE = Infosys.Solutions.Ainauto.VideoAnalytics.BusinessEntity;
using Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.Common;
using Infosys.Solutions.Ainauto.VideoAnalytics.BusinessEntity;
using Infosys.Solutions.Ainauto.VideoAnalytics.Resource.Entity.Document;
using Infosys.Solutions.Ainauto.VideoAnalytics.BusinessComponent.Translator;

using SC = Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.ServiceClientLibrary;
using System.Runtime.Caching;
using Helper = Infosys.Solutions.Ainauto.VideoAnalytics.BusinessComponent.Helper;
using System.Configuration;
using static Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.Common.ApplicationConstants;
using Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.TaskRoute;
using Infosys.Solutions.Ainauto.VideoAnalytics.Services.MaskDetector.Contracts.Message;
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.Core;

namespace Infosys.Solutions.Ainauto.VideoAnalytics.Processes

{
    public class FramePreloaderProcess : ProcessHandlerBase<QueueEntity.FramePreloaderMetadata>
    {

        ObjectCache cache = MemoryCache.Default;
        CacheItemPolicy policy = new CacheItemPolicy();
        CacheItemPolicy framePolicy = new CacheItemPolicy();
        SC.MaskDetector maskDetector = new SC.MaskDetector();
        static double cacheExpiration = 1.0; 

        public string _taskCode;
        public FramePreloaderProcess() { }
        public FramePreloaderProcess(string processId)
        {
            _taskCode = TaskRoute.GetTaskCode(processId);
        }

        public override void Dump(QueueEntity.FramePreloaderMetadata message)
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
            if (ConfigurationManager.AppSettings["FrameCacheSlidingExpirationInMins"] != null)
            {
                cacheExpiration = Convert.ToDouble(ConfigurationManager.AppSettings["FrameCacheSlidingExpirationInMins"]);
            }
        }


        public override bool Process(QueueEntity.FramePreloaderMetadata message, int robotId, int runInstanceId, int robotTaskMapId)
        {
#if DEBUG
            LogHandler.LogInfo(String.Format(InfoMessages.Method_Execution_Start, "Process", "FramePreloaderProcess"), LogHandler.Layer.Business, null);
            LogHandler.LogDebug(String.Format("The Process Method of FramePreloaderProcess class is getting executed with parameters : FrameProcessor message={0}; robotId={1};runInstanceId={2}; robotTaskMapId={3}", JsonConvert.SerializeObject(message), robotId, runInstanceId, robotTaskMapId),
              LogHandler.Layer.Business, null);
#endif
            TaskRoute taskRouter = new TaskRoute();

            try
            {
              
                using (LogHandler.TraceOperations("FramePreloaderProcess:Process", LogHandler.Layer.Business, Guid.NewGuid(), null))
                {
                    if (!message.TE.ContainsKey(TaskRouteConstants.PreLoaderCode))
                    {
                        LogHandler.LogError("Message is not processed in FramePreloaderProcess for FrameId = {0} ,TenantId = {1}, deviceId = {2} , module = {3}, message ={4}", LogHandler.Layer.Business, message.Fid, message.Tid, message.Did, TaskRouteConstants.PreLoaderCode, JsonConvert.SerializeObject(message));
                        return true;
                    }



                    
                    FrameRendererEntityTranslator entityTranslator = new FrameRendererEntityTranslator();
                    BE.FramePreloaderData framePreLoaderData = entityTranslator.FramePreLoaderTranslator(message);
                    string deviceId = framePreLoaderData.DID;
                    DeviceDetails deviceDetails = ConfigHelper.SetDeviceDetails(framePreLoaderData.TID, deviceId, CacheConstants.FramePreloaderCode);
                  
                   
                    bool downLoadLot = deviceDetails.DownLoadLot;
                    string baseUrl = deviceDetails.BaseUrl;



                   


                    DownLoadFrames(framePreLoaderData, baseUrl, downLoadLot);
#if DEBUG
                    LogHandler.LogInfo(String.Format(InfoMessages.Method_Execution_End, "Process", "FramePreloaderProcess"), LogHandler.Layer.Business, null);
#endif
                    
                   
                    return true;
                }
            }
            catch (Exception exMP)
            {
                LogHandler.LogError("Exception in FramePreloaderProcess : {0}  for FrameId = {1} and DeviceId = {2}", LogHandler.Layer.Business, exMP.Message, message.Fid,message.Did);
                LogHandler.LogDebug(String.Format("Exception occured in Process method of FramePreloaderProcess class for FrameId = {0}", message.Fid), LogHandler.Layer.Business, null);
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
                    LogHandler.LogError(String.Format(ErrorMessages.Exception_Failed, "Process", "FramePreloaderProcess"),
                            LogHandler.Layer.Business, null);
                   
                    if (!failureLogged)
                    {
                        LogHandler.LogError(String.Format("Exception Occured while handling an exception in FramePreloaderProcess in Process method. Error message: {0}", ex.Message), LogHandler.Layer.Business, null);
                    }
#if DEBUG
                    LogHandler.LogInfo(String.Format(InfoMessages.Method_Execution_End, "Process", "FramePreloaderProcess"), LogHandler.Layer.Business, null);
#endif
                    return false;
                }
            }
        }




        private void DownLoadFrames(FramePreloaderData framePreloaderData, string baseUrl, bool downLoadZip)
        {
#if DEBUG
            LogHandler.LogInfo(String.Format(InfoMessages.Method_Execution_Start, "DownLoadFrames", "FramePreloaderProcess"), LogHandler.Layer.Business, null);
            using (LogHandler.TraceOperations("FramePreloaderProcess:DownLoadFrames", LogHandler.Layer.Business, Guid.NewGuid()))
            {
#endif

                Workflow workflow;
                string cacheKey = framePreloaderData.TID + framePreloaderData.DID + framePreloaderData.FID;
                if (downLoadZip)
                {
                    workflow = Helper.DownloadBlob(framePreloaderData.DID, framePreloaderData.FID, framePreloaderData.TID, baseUrl, ApplicationConstants.FileExtensions.zip);
                    if (workflow != null)
                    {
                        Dictionary<string, Stream> frameDict = UnZipToMemory(workflow.File);

                      
                        framePolicy.SlidingExpiration = TimeSpan.FromMinutes(cacheExpiration);
                        cache.Set(cacheKey, frameDict, framePolicy);
                    }
                        
                }
                else
                {
                    workflow = Helper.DownloadBlob(framePreloaderData.DID, framePreloaderData.FID, framePreloaderData.TID, baseUrl, ApplicationConstants.FileExtensions.jpg);
                    if (workflow != null)
                    {
                        Stream imageStream = workflow.File;
                        if (imageStream.Length > 0)
                        {
                            Dictionary<string, Stream> frameDict = new Dictionary<string, Stream>();
                            frameDict.Add(framePreloaderData.FID + ApplicationConstants.FileExtensions.jpg, imageStream);
                            framePolicy.SlidingExpiration = TimeSpan.FromMinutes(cacheExpiration);
                            cache.Set(cacheKey, frameDict, framePolicy);
                        }
                    }
                }
#if DEBUG
                LogHandler.LogInfo(String.Format(InfoMessages.Method_Execution_End, "DownLoadFrames", "FramePreloaderProcess"), LogHandler.Layer.Business, null);
            }

#endif
        }

        private static Dictionary<string, Stream> UnZipToMemory(Stream zipStream)
        {
            var result = new Dictionary<string, Stream>();
            

            if (zipStream.Length > 0)
            {

                ZipInputStream zipInputStream = new ZipInputStream(zipStream);
                ZipEntry zipEntry = zipInputStream.GetNextEntry();

                while (zipEntry != null)
                {
                    MemoryStream data = new MemoryStream();
                    String entryFileName = zipEntry.Name;
                   

                    byte[] buffer = new byte[zipEntry.Size];

                    

                    StreamUtils.Copy(zipInputStream, data, buffer);
                    result.Add(zipEntry.Name, data);

                    zipEntry = zipInputStream.GetNextEntry();
                }

            }

            return result;
        }
                                    
    }

}

