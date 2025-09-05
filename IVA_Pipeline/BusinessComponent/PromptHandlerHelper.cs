/*=============================================================================================================== *
 * Copyright 2025 Infosys Ltd.                                                                                    *
 * Use of this source code is governed by Apache License Version 2.0 that can be found in the LICENSE file or at  *
 * http://www.apache.org/licenses/                                                                                *
 * ===============================================================================================================*/
﻿using Infosys.Solutions.Ainauto.VideoAnalytics.BusinessEntity;
using Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.Common;
using Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.TaskRoute;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.Common.ApplicationConstants;
using DE = Infosys.Solutions.Ainauto.VideoAnalytics.Resource.Entity;
using TR = Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.TaskRoute;
using BE = Infosys.Solutions.Ainauto.VideoAnalytics.BusinessEntity;
using Infosys.Solutions.Ainauto.VideoAnalytics.Services.MaskDetector.Contracts.Data;
using Infosys.Solutions.Ainauto.VideoAnalytics.Services.MaskDetector.Contracts.Message;
using System.Text.RegularExpressions;

namespace Infosys.Solutions.Ainauto.VideoAnalytics.BusinessComponent
{
    public static class PromptHandlerHelper
    {
        public static long instanceCount;
        public static bool onlyPrompt;
        static bool isConsoleMode;
        static bool isUpdated;
        static bool isclosing;
        public static string promptText;
        public static string HyperParameters;
        public static Dictionary<string, string> promptDictionary = new Dictionary<string, string>();
        static TR.TaskRoute taskRouter = new TR.TaskRoute();
        public static Dictionary<string, List<string>> FGRtaskRoute;
        public static Dictionary<string, List<string>> PRHtaskRoute;
        public static string promptTemplate;
        public static string feedKey = "";
        public static Dictionary<string, string> promptKeys = null;
        public static AppSettings appSettings = Config.AppSettings;
        private static DeviceDetails deviceDetails;
        public static string tenantId = appSettings.TenantID.ToString();
        public static string deviceId = appSettings.DeviceID;
        public static string masterId;
        public static string modelName;
        public static string promptInputDirectory;
        public static string maskInputDirectory;
        public static string replaceInputDirectory;
        private static readonly object _lock = new object();
        private static Task _getPrompt;
        private static CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        static DateTime LastFrameGrabbedTime;
        public static string _taskCode;

        static PromptHandlerHelper()
        {
            deviceDetails = ConfigHelper.SetDeviceDetails(tenantId.ToString(), deviceId, CacheConstants.PromptHandler);
            UpdateModelName(deviceDetails.ModelName);
            promptInputDirectory = deviceDetails.PromptInputDirectory;
            maskInputDirectory = deviceDetails.MaskImageDirectory;
            replaceInputDirectory = deviceDetails.ReplaceImageDirectory;
            if (deviceDetails.VideoFeedType == "PROMPT")
            {
                onlyPrompt = true;
                GetPrompt();
            }

        }


        public static void sendEventMessage(string eventType, int totalFrameCount, int frameNumberSendForPredict, int totalMessageSend)
        {

            DE.Queue.MaintenanceMetaData queueEntity = new DE.Queue.MaintenanceMetaData();
            queueEntity.Did = deviceId;
            queueEntity.Tid = tenantId;
            queueEntity.MessageType = ProcessingStatus.EventHandling;
            queueEntity.Timestamp = DateTime.UtcNow;
            queueEntity.ResourceId = deviceId;
            queueEntity.EventType = eventType;
            DE.Queue.FrameInformation frameInformation = new DE.Queue.FrameInformation();
            frameInformation.TID = tenantId;
            frameInformation.DID = deviceId;
            frameInformation.TotalFrameCount = totalFrameCount.ToString();
            frameInformation.LastFrameNumberSendForPrediction = frameNumberSendForPredict.ToString();
            frameInformation.TotalMessageSendForPrediction = totalMessageSend.ToString();
            frameInformation.FeedId = masterId;
            frameInformation.FramesNotSendForRendering = new Dictionary<int, string>();
            queueEntity.Data = JsonConvert.SerializeObject(frameInformation);
            
            var taskList = taskRouter.GetTaskRouteDetails(FrameGrabberHelper.tenantId.ToString(),
                FrameGrabberHelper.deviceId, PromptHandler._taskCode)[PromptHandler._taskCode];

            foreach (string moduleCode in taskList)
            {
                taskRouter.SendMessageToQueue(FrameGrabberHelper.tenantId.ToString(), FrameGrabberHelper.deviceId, moduleCode, queueEntity);
            }
        }

        public static void GetPrompt()
        {
            lock (_lock)
            {
                if(_getPrompt != null && !_getPrompt.IsCompleted)
                {
                    return;
                }
                _getPrompt = Task.Run(() => StartBackgroundTask());
            }
        }

        

        public static Dictionary<string, List<string>> SendMessage<T>(T message, string tenantId, string deviceId, Dictionary<string, List<string>> TE)
        {
            List<string> taskList = TE[PromptHandler._taskCode];
            Dictionary<string, List<string>> te = new Dictionary<string, List<string>>();
            if (taskList != null)
            {
                foreach (var task in taskList)
                {
                    var type = message.GetType();
                    var property = type.GetProperty("TE");
                    te = taskRouter.GetTaskRouteDetails(tenantId, deviceId, task);
                    property.SetValue(message, te);
                    taskRouter.SendMessageToQueueWithTask(TaskRoute.TaskRouteMetaData, PromptHandler._taskCode, message, task);
                }
            }
            return te;
        }

        public static BE.Queue.FrameProcessorMetaData CreateFrameProcessorMessage()
        {
            BE.Queue.FrameProcessorMetaData frameProcessorMessage = new BE.Queue.FrameProcessorMetaData()
            {
                Fid = DateTime.Now.Ticks.ToString(),
                Did = deviceId,
                Sbu = FrameGrabberHelper.storageBaseUrl,
                Tid = tenantId,
                Mod = deviceDetails.PredictionModel.Contains(PromptHandler._taskCode) ? JsonConvert.DeserializeObject<Dictionary<string, string>>(deviceDetails.PredictionModel).GetValueOrDefault(PromptHandler._taskCode) : JsonConvert.DeserializeObject<Dictionary<string, string>>(deviceDetails.PredictionModel).GetValueOrDefault("default"),
                TE = taskRouter.GetTaskRouteDetails(appSettings.TenantID.ToString(), appSettings.DeviceID, PromptHandler._taskCode),
                FeedId = masterId,
                Fids = new List<string>(),
                SequenceNumber = "",
                FrameNumber = "1",
                Stime = DateTime.UtcNow.ToString("yyyy-MM-dd,HH:mm:ss.fff tt"),
                Src = PromptHandler._taskCode,
                Etime = DateTime.UtcNow.ToString("yyyy-MM-dd,HH:mm:ss.fff tt"),
                Prompt = "",
                Ffp = "",
                Ltsize = FrameGrabberHelper.frameToPredict.ToString(),
                Lfp ="1",
                videoFileName = "",
                Pcd = null,
                Hp = !string.IsNullOrEmpty(HyperParameters) ? HyperParameters : deviceDetails.HyperParameters
            };
            return frameProcessorMessage;
        }

        public static string RemoveSpecialCharacters(string input)
        {
            char[] ch = new char[] { '.', '\n', ','};
            foreach(char c in ch)
            {
                if (input.Contains(c))
                {
                    input = input.Replace(c, ' ');
                }
            }
            return input;
        }

        public static string GetTemplate(string promptData)
        {
            
            if (string.IsNullOrEmpty(promptData))
            {
                return promptTemplate;
            }
            string template = InitializeModelTemplates();
            try
            {
                
                List<List<string>> masterTemplate = new List<List<string>>();
                if (template.Contains("PROMPT"))
                {
                    template = template.Replace("PROMPT", "\"" + promptData + "\"");
                }
                else if (template.Contains("[OBJECT]"))
                {
                    
                    List<string> promptList = promptData.Split('.', StringSplitOptions.RemoveEmptyEntries).ToList();
                    for (int i = 0; i < promptList.Count; i++)
                    {
                        List<string> objects = ExtractObjects(promptList[i]);
                        if(objects.Count > 0)
                        {
                            masterTemplate.Add(objects);
                        }
                        else
                        {
                            objects.Add(promptList[i]);
                            masterTemplate.Add(objects);
                        }
                    }
                    template = JsonConvert.SerializeObject(masterTemplate);
                    LogHandler.LogDebug($"Generated prompt template for the model {FrameGrabberHelper.modelName} : {template}", LogHandler.Layer.PromptHandler);
                }
                else
                {
                    List<string> promptList = promptData.Split('.', StringSplitOptions.RemoveEmptyEntries).ToList();
                    masterTemplate.Add(promptList);
                    template = JsonConvert.SerializeObject(masterTemplate);
                }
            }
            catch(Exception ex)
            {
                LogHandler.LogError($"Error in formatting the template {ex.Message}", LogHandler.Layer.PromptHandler);
            }
            return template;
        }

        public static string InitializeModelTemplates()
        {
            string template = "";
            try
            {
                
                LogHandler.LogDebug($"Getting template for the model {modelName}", LogHandler.Layer.PromptHandler);
                List<ModelTemplates> modelTemplates = new List<ModelTemplates>();
                string filePath=Path.Combine(Directory.GetCurrentDirectory(),deviceDetails.PromptTemplatesDirectory);
                string jsonString = File.ReadAllText(filePath);
                modelTemplates = JsonConvert.DeserializeObject<List<ModelTemplates>>(jsonString);
                for (int i = 0; i < modelTemplates.Count; i++)
                {
                    if (modelTemplates[i].ModelName == modelName)
                    {
                        template = modelTemplates[i].ModelTemplate;
                        break;
                    }
                }
            }
            catch(Exception ex)
            {
                LogHandler.LogError($"Exception while pulling template for the model {FrameGrabberHelper.modelName}, Exception: {ex.Message}, Inner Exception: {ex.InnerException}",
                    LogHandler.Layer.PromptHandler);
            }
            return template;
        }


        public static void InitializePromptKeys(string promptData)
        {
            promptKeys = new Dictionary<string, string>();
            promptKeys.Add("PROMPT", promptData);
            promptKeys.Add("OUTPUT", "5");
        }

        public static List<string> ExtractObjects(string promptData)
        {
            List<string> list = new List<string>();
            string pattern = @"\[(.*?)\]";
            foreach (Match match in Regex.Matches(promptData, pattern))
            {
                list.Add(match.Groups[1].Value);
            }
            return list;
        }

        public static void ReadPromptFromFile()
        {
            if (Directory.Exists(promptInputDirectory))
            {
                string file = Directory.GetFiles(promptInputDirectory).FirstOrDefault();
                
                if (!string.IsNullOrEmpty(file))
                {
                    int retry = 3;
                    while (retry > 0)
                    {
                        try
                        {
                            using (var fileStream = new FileStream(file, FileMode.Open, FileAccess.ReadWrite, FileShare.Read))
                            {
                                using (var streamReader = new StreamReader(fileStream, Encoding.UTF8))
                                {
                                    promptText = streamReader.ReadToEnd();
                                    LogHandler.LogDebug($"Prompt {promptText} read from the file {file}", LogHandler.Layer.PromptHandler);
                                    break;
                                }
                            }
                        }
                        catch (IOException)
                        {
                            LogHandler.LogError($"File {file} not accessible, retry count: {retry}", LogHandler.Layer.PromptHandler);
                            retry--;
                            Thread.Sleep(1000);
                        }
                    }
                    File.Delete(file);
                }
            }
            else
            {
                LogHandler.LogDebug($"Prompt input directory does not exists", LogHandler.Layer.PromptHandler);
            }
        }

        public static async Task StartBackgroundTask()
        {
            LogHandler.LogDebug($"Started background task to read prompts", LogHandler.Layer.PromptHandler);
            while (!_cancellationTokenSource.Token.IsCancellationRequested)
            {
                try
                {
                    
                    ReadPromptFromFile();

                    if (!string.IsNullOrEmpty(promptText))
                    {
                        promptTemplate = GetTemplate(promptText);
                        promptText = "";
                    }

                    
                    if (!string.IsNullOrEmpty(promptTemplate))
                    {
                        if (onlyPrompt)
                        {
                            if(deviceDetails.DBEnabled) {
                                GetFeedDetails();
                            }
                            else
                            {
                                masterId = FrameGrabberHelper.GenerateMaterId().ToString();
                                feedKey = tenantId + "_" + deviceId + "_" + masterId;
                            }
                        }
                        if (promptDictionary.ContainsKey(feedKey))
                        {
                            promptDictionary[feedKey] = promptTemplate;
                        }
                        else if (!string.IsNullOrEmpty(feedKey))
                        {
                            promptDictionary.Add(feedKey, promptTemplate);
                        }
                        promptTemplate = "";
                    }

                    
                    if (onlyPrompt && promptDictionary.ContainsKey(feedKey) && promptDictionary.TryGetValue(feedKey, out string prompt))
                    {
                        sendEventMessage(ApplicationConstants.ProcessingStatus.StartOfFile, 0, 0, 0);
                        BE.Queue.FrameProcessorMetaData frameProcessorMetaData = CreateFrameProcessorMessage();
                        frameProcessorMetaData.Prompt = prompt;
                        SendMessage(frameProcessorMetaData, frameProcessorMetaData.Tid, frameProcessorMetaData.Did, frameProcessorMetaData.TE);
                        
                        LastFrameGrabbedTime = DateTime.UtcNow;
                        DateTime LastProcessedTime = DateTime.UtcNow;
                        
                        if(deviceDetails.DBEnabled) {
                            var feedRequest = FrameGrabberHelper.GetFeedRequestWithMasterId(FrameGrabberHelper.MasterId);
                            if (feedRequest != null && feedRequest.RequestId != null)
                            {
                                feedRequest.LastFrameGrabbedTime = LastFrameGrabbedTime;
                                feedRequest.LastFrameId = frameProcessorMetaData.Fid;
                                FrameGrabberHelper.UpdateFeedRequestDetails(feedRequest);
                            }
                            FeedProcessorMasterMsg feedProcessorMaster = FrameGrabberHelper.GetFeedProcessorMasterWithMasterId(Convert.ToInt32(masterId));
                            feedProcessorMaster.FeedProcessorMasterDetail.Status = ProcessingStatus.feedCompletedStatus;

                            feedProcessorMaster.FeedProcessorMasterDetail.ProcessingEndTimeTicks = DateTime.UtcNow.Ticks;
                            if (!isUpdated && FrameGrabberHelper.UpdateAllFeedDetails(feedProcessorMaster))
                                isUpdated = true;
                        }
                        sendEventMessage(ApplicationConstants.ProcessingStatus.EndOfFile, 1, 1, 1);
                        prompt = "";
                        masterId = "";
                        promptDictionary[feedKey] = "";
                        feedKey = "";
                        HyperParameters = "";
                    }
                }
                catch (Exception ex)
                {
                    LogHandler.LogError("Exception occurred in StartProcessPrompt of PromptHandlerHelper, exception: {0}", LogHandler.Layer.PromptHandler, ex.Message);
                }
            }
        }

        public static async Task StopBackgroundTask()
        {
            lock (_lock)
            {
                if(_getPrompt != null && !_getPrompt.IsCompleted)
                {
                    _cancellationTokenSource.Cancel();
                    _getPrompt.Wait();
                    _getPrompt = null;
                    _cancellationTokenSource = new CancellationTokenSource();
                }
            }
        }

        private static void GetFeedDetails()
        {
            try
            {
               
                FeedProcessorMasterMsg feedProcessorMasterMsg = FrameGrabberHelper.GetFeedMasterWithDeviceId(deviceId);   

                LogHandler.LogDebug($"Feed processor master details fetched for the deviceId: {deviceId} is {JsonConvert.SerializeObject(feedProcessorMasterMsg)}", LogHandler.Layer.FrameGrabber);
                
                if (feedProcessorMasterMsg != null && feedProcessorMasterMsg.FeedProcessorMasterDetail?.Status == 0)
                {
                    if (feedProcessorMasterMsg != null && feedProcessorMasterMsg.FeedProcessorMasterDetail != null && feedProcessorMasterMsg.FeedProcessorMasterDetail.FeedProcessorMasterId != 0)
                    {
                        var feedProcessorMasterDetail = feedProcessorMasterMsg.FeedProcessorMasterDetail;
                        feedProcessorMasterDetail = feedProcessorMasterMsg.FeedProcessorMasterDetail;
                        feedProcessorMasterDetail.Status = FrameGrabberHelper.IN_PROGRESS;
                        feedProcessorMasterDetail.FrameProcessedRate = FrameGrabberHelper.lotSize;
                        FrameGrabberHelper.MasterId = feedProcessorMasterDetail.FeedProcessorMasterId;
                        LogHandler.LogDebug($"Master id from DB: {feedProcessorMasterDetail.FeedProcessorMasterId.ToString()}", LogHandler.Layer.FrameGrabber);
                        feedProcessorMasterMsg.FeedProcessorMasterDetail = feedProcessorMasterDetail;
                        FrameGrabberHelper.UpdateAllFeedDetails(feedProcessorMasterMsg);

                        var feedRequest = FrameGrabberHelper.GetFeedRequestWithMasterId(feedProcessorMasterDetail.FeedProcessorMasterId);
                        
                        FrameGrabberHelper.MasterId = feedRequest.FeedProcessorMasterId.Value;
                        masterId = feedRequest.FeedProcessorMasterId.ToString();
                        feedRequest.Status = ProcessingStatus.inProgressStatus;
                        feedRequest.StartFrameProcessedTime = DateTime.UtcNow;
                        feedRequest.ResourceId = FrameGrabberHelper.deviceId;
                        var status = FrameGrabberHelper.UpdateFeedRequestDetails(feedRequest);
                        FrameGrabberHelper.modelName = feedRequest.Model;
                        Media_MetaData_Msg_Req mediaMetaDataMsgReq = new Media_MetaData_Msg_Req();
                        mediaMetaDataMsgReq.MediaMetadataDetails = new Media_Metadata_Details();
                        mediaMetaDataMsgReq.MediaMetadataDetails.FeedProcessorMasterId = feedRequest.FeedProcessorMasterId.Value;
                        mediaMetaDataMsgReq.MediaMetadataDetails.RequestId = feedRequest.RequestId;
                        FrameGrabberHelper.UpdateMediaMetaData(mediaMetaDataMsgReq);
                    }

                }
                else
                {
                    return;
                }
                feedKey = tenantId + "_" + deviceId + "_" + masterId;
            }
            catch (Exception ex)
            {
                LogHandler.LogError("Error in getting feed details from DB, Exception: {0}, Inner exception: {1}, Stack trace: {2}", LogHandler.Layer.PromptHandler, ex.Message, ex.InnerException, ex.StackTrace);
            }
        }

        public static void UpdateModelName(string modelInfo)
        {
            try
            {
                if (deviceDetails.ModelName.Contains("default"))
                {
                    Dictionary<string, string> models = JsonConvert.DeserializeObject<Dictionary<string, string>>(modelInfo);
                    if (models.ContainsKey("default"))
                    {
                        modelName = models["default"];
                    }
                }
            }
            catch (Exception ex)
            {
                LogHandler.LogError("Error in UpdateModelName method in PromptHandlerHelper class, exception: {0}, inner exception: {1}, stack trace: {2}", LogHandler.Layer.PromptHandler, ex.Message, ex.InnerException, ex.StackTrace);
            }
        }
    }
}
