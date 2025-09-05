/*=============================================================================================================== *
 * Copyright 2025 Infosys Ltd.                                                                                    *
 * Use of this source code is governed by Apache License Version 2.0 that can be found in the LICENSE file or at  *
 * http://www.apache.org/licenses/                                                                                *
 * ===============================================================================================================*/
﻿#region namespace
using System;
using Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.ProcessScheduler.Framework;
using QueueEntity = Infosys.Solutions.Ainauto.VideoAnalytics.Resource.Entity.Queue;
using Infosys.Solutions.Ainauto.VideoAnalytics.Resource.DataAccess;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Configuration;
using System.Runtime.Caching;
using System.Diagnostics;
using Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.TaskRoute;
using Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.Common;
using static Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.Common.ApplicationConstants;
using SE = Infosys.Solutions.Ainauto.VideoAnalytics.Services.MaskDetector.Contracts;
using Infosys.Solutions.Ainauto.VideoAnalytics.BusinessEntity;
using Nest;
using Infosys.Solutions.Ainauto.VideoAnalytics.Services.MaskDetector.Contracts.Message;
using System.Xml.Linq;
using System.Linq;
using Newtonsoft.Json.Linq;
using BE = Infosys.Solutions.Ainauto.VideoAnalytics.BusinessEntity;
using DE = Infosys.Solutions.Ainauto.VideoAnalytics.Resource.Entity;
using Elasticsearch.Net;
using System.Threading.Tasks;
using System.Data;
using System.Reflection;
using Infosys.Solutions.Ainauto.VideoAnalytics.Resource.Entity.Queue;
using DataAccess;
using Infosys.Solutions.Ainauto.VideoAnalytics.Resource.Entity.VideoAnalytics;
#endregion

namespace Infosys.Solutions.Ainauto.VideoAnalytics.Processes
{
    public class FrameExplainerProcess : ProcessHandlerBase<QueueEntity.FrameExplainerModeMetaData>
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
        static private string explainerApiVersion;
        static private string explainerToRun;
        AppSettings appSettings = Config.AppSettings;
        static private string explainerUrl;
      static  List<string> imageList = new List<string>();
        static int batchSize;
        static private string blobUrl;
        static List<string> frameIdList = new List<string>();
        static private string templateName;
        static public bool xaiStatus = false;
        List<bool> xaiResults = new List<bool>();
        static int batchcount=0;
        TaskRoute taskRouter = new TaskRoute();

        public string _taskCode;
        public FrameExplainerProcess() { }
        public FrameExplainerProcess(string processId)
        {
            _taskCode = TaskRoute.GetTaskCode(processId);
        }

        public override void Dump(QueueEntity.FrameExplainerModeMetaData message)
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
            
            DeviceDetails deviceDetails=ConfigHelper.SetDeviceDetails(appSettings.TenantID.ToString(),appSettings.DeviceID,CacheConstants.FrameExplainer);
           
            elasticStoreIndexName=deviceDetails.ElasticStoreIndexName;
            if (ConfigurationManager.AppSettings["FrameCacheSlidingExpirationInMins"] != null)
            {
                frameCacheSlidingExpirationInMins = Convert.ToDouble(System.Configuration.ConfigurationManager.AppSettings["FrameCacheSlidingExpirationInMins"]);
            }
           
            if(deviceDetails.PredictionType!=null) {
                
                predictionType=deviceDetails.PredictionType;
            }
            DeviceDetails response = ConfigHelper.SetDeviceDetails(appSettings.TenantID.ToString(), appSettings.DeviceID, CacheConstants.FrameGrabberCode);
            explainerApiVersion = response.XaiApiVersion;
            explainerToRun = response.XaiToRun;
            explainerUrl = response.XaiModel;
            batchSize = response.XaiBatchSize;
            blobUrl = response.StorageBaseUrl;
            templateName = response.XaiTemplateName;
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
        public override bool Process(QueueEntity.FrameExplainerModeMetaData message, int robotId, int runInstanceId, int robotTaskMapId)
        {    
            processStopWatch.Reset();
            processStopWatch.Start();
            counterInstanceName = message.Tid + "_" + message.Did;
            string metadata = string.Empty;      

            TaskRoute taskRouter = new TaskRoute();
#if DEBUG
            LogHandler.LogDebug("counterInstanceName in frameexplainermanger: {0}", LogHandler.Layer.Business, counterInstanceName);

            LogHandler.LogInfo(String.Format(InfoMessages.Method_Execution_Start, "Process", "FrameexplainerProcess"), LogHandler.Layer.Business, null);
            LogHandler.LogDebug(String.Format("The Process Method of FrameExplainerProcess class is getting executed with parameters :  message={0}; robotId={1};runInstanceId={2}; robotTaskMapId={3}", JsonConvert.SerializeObject(message), robotId, runInstanceId, robotTaskMapId),
                LogHandler.Layer.Business, null);
#endif

            try
            {
                TemplateMasterDS templatemasterDS = new TemplateMasterDS();
                List<TemplateDetails> datalist = new List<TemplateDetails>();
                TemplateDetails templateMaster = new TemplateDetails()
                {

                    TemplateName = templateName

                };

                datalist = templatemasterDS.GetAll(templateMaster).ToList();
#if DEBUG
                LogHandler.LogDebug("counterInstanceName in Frame Explainer Process: {0}", LogHandler.Layer.Business, counterInstanceName);
                LogHandler.LogInfo(String.Format(InfoMessages.Method_Execution_Start, "Process", "FrameexplainerProcess"), LogHandler.Layer.Business, null);
                LogHandler.LogDebug(String.Format("The Process Method of FrameExplainerProcess class is getting executed with parameters to identity the Frame Details :  message={0}; robotId={1};runInstanceId={2}; robotTaskMapId={3}", JsonConvert.SerializeObject(message.Ffp), robotId, runInstanceId, robotTaskMapId),
                LogHandler.Layer.Business, null);
#endif
                List<Attributes> xai_attributes = new List<Attributes>();
                List<TemplateAttributes> xai_ConditionMapping = new List<TemplateAttributes>();
                string techniquetobeused = string.Empty;
                string compareValue = string.Empty;
                bool attributeComparisonEnabled = false;
                string attributeComparisonValue = string.Empty;
                foreach (var item in datalist)
                {
                    string attributeValue = item.AttributeValue;
                    if (item.AttributeName == XaiConstantsAttributes.Cs.ToString())
                    {
                        Type type = typeof(QueueEntity.FrameExplainerModeMetaData);
                        PropertyInfo propertyName = type.GetProperty(item.AttributeName);
                        compareValue = "";
                        string Name = XaiConstantsAttributes.Cs.ToString();
                        TemplateAttributes data = new TemplateAttributes
                        {
                            AttributeName = Name,
                            AttributeValue = item.AttributeValue,
                            AttributeCondition = item.Operator,
                            CompareAttributeValue = compareValue
                        };
                        bool alreadyExists = xai_ConditionMapping.Any(x => x.AttributeName == data.AttributeName && x.AttributeCondition == data.AttributeCondition && x.AttributeValue == data.AttributeValue);
                        if (!alreadyExists == true)
                            xai_ConditionMapping.Add(data);
                    }
                    else if (item.AttributeName == XaiConstantsAttributes.Lb.ToString())
                    {
                        Type type = typeof(QueueEntity.FrameExplainerModeMetaData);
                        PropertyInfo propertyName = type.GetProperty(item.AttributeName);
                        compareValue = "";
                        string Name = XaiConstantsAttributes.Lb.ToString();
                        TemplateAttributes data = new TemplateAttributes
                        {
                            AttributeName = Name,
                            AttributeValue = item.AttributeValue,
                            AttributeCondition = item.Operator,
                            CompareAttributeValue = compareValue
                        };
                        bool alreadyExists = xai_ConditionMapping.Any(x => x.AttributeName == data.AttributeName && x.AttributeCondition == data.AttributeCondition && x.AttributeValue == data.AttributeValue);
                        if (!alreadyExists == true)
                            xai_ConditionMapping.Add(data);
                    }
                    else if (item.AttributeName == XaiConstantsAttributes.AttributeComparison.ToString() && item.AttributeValue == XaiConstantsAttributes.Yes.ToString())
                    {
                        attributeComparisonEnabled = true;
                        attributeComparisonValue = item.Operator;

                    }
                    else if (item.AttributeName == XaiConstantsAttributes.AttributeComparison.ToString() && item.AttributeValue == XaiConstantsAttributes.No.ToString())
                    {
                        attributeComparisonEnabled = false;

                    }
                    else
                    {
                        Type type = typeof(QueueEntity.FrameExplainerModeMetaData);
                        PropertyInfo propertyName = type.GetProperty(item.AttributeName);
                        compareValue = (string)propertyName.GetValue(item.AttributeName);
                        string Name = propertyName.Name;
                        TemplateAttributes data = new TemplateAttributes
                        {

                            AttributeName = Name,
                            AttributeValue = item.AttributeValue,
                            AttributeCondition = item.Operator,
                            CompareAttributeValue = compareValue


                        };
                        bool alreadyExists = xai_ConditionMapping.Any(x => x.AttributeName == data.AttributeName && x.AttributeCondition == data.AttributeCondition && x.AttributeValue == data.AttributeValue);
                        if (!alreadyExists == true)
                            xai_ConditionMapping.Add(data);
                    }
                }


                string sstime = DateTime.UtcNow.ToString("yyy-MM-dd,HH:mm:ss.fff tt");

                Double ccScore = Convert.ToDouble(message.Fs[0].Cs);
                string explainerURL = GetModelUrl(explainerUrl);
                string blobUrlFullPath = blobUrl + "Documents" + "/" + message.Tid + "_" + message.Did + "/" + message.Fid + ".jpg";
                List<SE.Message.Mtp> MtpData = new List<SE.Message.Mtp>();
                List<string> ExplainerToRun = new List<string>();

                dynamic myObject = JValue.Parse(explainerToRun);
                foreach (dynamic ename in myObject)
                {
                    ExplainerToRun.Add(Convert.ToString(ename));
                }
                foreach (var item in message.Mtp)
                {
                    MtpData.Add(new SE.Message.Mtp() { Etime = item.Etime, Src = item.Src, Stime = item.Stime });
                };

                if (attributeComparisonEnabled == true)
                {
                    foreach (var item in xai_ConditionMapping)
                    {
                        if (item.AttributeName == XaiConstantsAttributes.Ad.ToString())
                        {
                            attributeComparisonEnabled = EqualMapping(message, item.AttributeCondition, item.AttributeValue, imageList, frameIdList, blobUrlFullPath, message.Ad);
                            xaiResults.Add(attributeComparisonEnabled);

                        }
                        
                        else if (item.AttributeName == XaiConstantsAttributes.Did.ToString())
                        {
                            attributeComparisonEnabled = EqualMapping(message, item.AttributeCondition, item.AttributeValue, imageList, frameIdList, blobUrlFullPath, message.Did);
                            xaiResults.Add(attributeComparisonEnabled);

                        }
                        else if (item.AttributeName == XaiConstantsAttributes.FrameNumber.ToString())
                        {
                            attributeComparisonEnabled = Mapping(message, item.AttributeCondition, item.AttributeValue, imageList, frameIdList, blobUrlFullPath, message.FrameNumber);
                            xaiResults.Add(attributeComparisonEnabled);

                        }
                        else if (item.AttributeName == XaiConstantsAttributes.Cs.ToString())
                        {
                            for (int i = 0; i < message.Fs.Count(); i++)
                            {
                                attributeComparisonEnabled = Mapping(message, item.AttributeCondition, item.AttributeValue, imageList, frameIdList, blobUrlFullPath, message.Fs[i].Cs);
                            }
                            bool alreadyExists = xaiResults.Any(x => x == attributeComparisonEnabled);
                            if (!alreadyExists == true)
                                xaiResults.Add(attributeComparisonEnabled);
                          
                        }
                        else if (item.AttributeName == XaiConstantsAttributes.Lb.ToString())
                        {
                            for (int i = 0; i < message.Fs.Count(); i++)
                            {
                                attributeComparisonEnabled = EqualMapping(message, item.AttributeCondition, item.AttributeValue, imageList, frameIdList, blobUrlFullPath, message.Fs[i].Lb);
                            }
                            bool alreadyExists = xaiResults.Any(x => x == attributeComparisonEnabled);
                            if (!alreadyExists == true)
                                xaiResults.Add(attributeComparisonEnabled);

                        }
                    }

                    switch (attributeComparisonValue)
                    {
                        case "||":
                            if (xaiResults[0] || xaiResults[1])
                            {
                                xaiStatus = true;
                            }
                            break;
                        case "&&":
                            if (xaiResults[0] && xaiResults[1])
                            {
                                xaiStatus = true;
                            }
                            break;
                    }

                }
                else
                {
                    foreach (var item in xai_ConditionMapping)
                    {
                        if (item.AttributeName == XaiConstantsAttributes.Ad.ToString())
                        {
                            xaiStatus = EqualMapping(message, item.AttributeCondition, item.AttributeValue, imageList, frameIdList, blobUrlFullPath, message.Ad);

                        }
                       
                        else if (item.AttributeName == XaiConstantsAttributes.Did.ToString())
                        {
                            xaiStatus = EqualMapping(message, item.AttributeCondition, item.AttributeValue, imageList, frameIdList, blobUrlFullPath, message.Did);

                        }
                        else if (item.AttributeName == XaiConstantsAttributes.FrameNumber.ToString())
                        {
                            xaiStatus = Mapping(message, item.AttributeCondition, item.AttributeValue, imageList, frameIdList, blobUrlFullPath, message.FrameNumber);

                        }
                        else if (item.AttributeName == XaiConstantsAttributes.Cs.ToString())
                        {
                            for (int i = 0; i < message.Fs.Count(); i++)
                            {
                                xaiStatus = Mapping(message, item.AttributeCondition, item.AttributeValue, imageList, frameIdList, blobUrlFullPath, message.Fs[i].Cs);
                            }
                        }
                        else if (item.AttributeName == XaiConstantsAttributes.Lb.ToString())
                        {
                            for (int i = 0; i < message.Fs.Count(); i++)
                            {
                                xaiStatus = EqualMapping(message, item.AttributeCondition, item.AttributeValue, imageList, frameIdList, blobUrlFullPath, message.Fs[i].Lb);
                            }


                        }
                    }

                }
                if (xaiStatus == true)
                {
                    AddImagetolist(message.Fid, imageList, frameIdList, blobUrlFullPath);
                   
                    string estime = DateTime.UtcNow.ToString("yyy-MM-dd,HH:mm:ss.fff tt");
                    MtpData.Add(new SE.Message.Mtp() { Etime = estime, Src = "Explainer Node", Stime = sstime });
                }
                if (imageList.Count == batchSize)
                {
                    batchcount += 1;

#if DEBUG
   
                    LogHandler.LogDebug("counterInstanceName in Frame Explainer Process Started Processing Frames: {0}", LogHandler.Layer.Business, counterInstanceName);
                    LogHandler.LogInfo(String.Format(InfoMessages.Method_Execution_Start, "Process", "FrameexplainerProcess"), LogHandler.Layer.Business, null);
                    LogHandler.LogDebug(String.Format("The Process Method of FrameExplainerProcess Started Sending batch of messages to Kafaka :  message={0}; robotId={1};runInstanceId={2}; robotTaskMapId={3}", JsonConvert.SerializeObject(batchcount), robotId, runInstanceId, robotTaskMapId),
                    LogHandler.Layer.Business, null);
#endif
                    SE.Message.ObjectDetectorAPIReqMsgExp reqMsg = new SE.Message.ObjectDetectorAPIReqMsgExp()
                  
                    {
                        Did = message.Did,
                        Fid = frameIdList,
                        Mtp = MtpData,
                        Tid = message.Tid,
                        Ts = DateTime.UtcNow.ToString("yyyy-MM-dd,HH:mm:ss.fff tt"),
                        Ts_ntp = DateTime.UtcNow.ToString("yyyyMMddHHmmss"),
                        Msg_ver = "",
                        Inf_ver = "",
                        Model = explainerUrl,
                        Per = null,
                        Ad = "",
                        Base_64 = imageList,
                        C_threshold = message.Fs[0].Cs,
                        Ffp = message.Ffp,
                        Ltsize = "",
                        Lfp = message.Lfp,
                        Exp_api_ver = explainerApiVersion,
                        Explainers_to_run = ExplainerToRun,
                        I_fn = message.videoFileName,
                        Msk_img = new List<string>(),
                        Rep_img = new List<string>(),
                        Prompt = new List<List<string>>(),
                        Explainer_url = message.Ad
                    };



                    var input = JsonConvert.SerializeObject(reqMsg);

                    sendMessage(reqMsg, input);
#if DEBUG
                    LogHandler.LogDebug("counterInstanceName in Frame Explainer Process Started Processing Frames: {0}", LogHandler.Layer.Business, counterInstanceName);
                    LogHandler.LogInfo(String.Format(InfoMessages.Method_Execution_Start, "Process", "FrameexplainerProcess"), LogHandler.Layer.Business, null);
                    LogHandler.LogDebug(String.Format("The Process Method of FrameExplainerProcess is Last Frame :  message={0}; robotId={1};runInstanceId={2}; robotTaskMapId={3}", JsonConvert.SerializeObject(message.Lfp), robotId, runInstanceId, robotTaskMapId),
                    LogHandler.Layer.Business, null);
#endif

                    imageList = new List<string>();
                    frameIdList = new List<string>();
                }
                else if (message.Lfp == "1")
                {
                    
                    batchcount += 1;
                    
                    SE.Message.ObjectDetectorAPIReqMsgExp reqMsg = new SE.Message.ObjectDetectorAPIReqMsgExp()
                  
                    {
                        
                        Did = message.Did,
                        Fid = frameIdList, 
                        Mtp = MtpData,
                        Tid = message.Tid,
                        Ts = DateTime.UtcNow.ToString("yyyy-MM-dd,HH:mm:ss.fff tt"),
                        Ts_ntp = DateTime.UtcNow.ToString("yyyyMMddHHmmss"),
                        Msg_ver = "",
                        Inf_ver = "",
                        Model = explainerUrl,
                        Per = null, 
                        Ad = "",
                        Base_64 = imageList,
                        C_threshold = message.Fs[0].Cs,
                        Ffp = message.Ffp,
                        Ltsize = "",
                        Lfp = message.Lfp,
                        Exp_api_ver = explainerApiVersion,
                        Explainers_to_run = ExplainerToRun,
                        I_fn = message.videoFileName,
                        Msk_img = new List<string>(),
                        Rep_img = new List<string>(),
                        Prompt = new List<List<string>>(),
                        Explainer_url = message.Ad

                    };
                    var input = JsonConvert.SerializeObject(reqMsg);
                    ObjectDetectorAPIResMsgExp response = null;
                    DE.Queue.FrameExplainerModeMetaData deReceivedPersonCountMessage = new DE.Queue.FrameExplainerModeMetaData();
                    deReceivedPersonCountMessage = BE.FaceMaskTranslator.FaceMaskExplainerBEToDE(metadata, message);
                    sendMessage(reqMsg, input);
#if DEBUG
                    LogHandler.LogDebug("counterInstanceName in Frame Explainer Process Started Processing Frames: {0}", LogHandler.Layer.Business, counterInstanceName);
                    LogHandler.LogInfo(String.Format(InfoMessages.Method_Execution_Start, "Process", "FrameexplainerProcess"), LogHandler.Layer.Business, null);
                    LogHandler.LogDebug(String.Format("The Process Method of FrameExplainerProcess is Last Frame :  message={0}; robotId={1};runInstanceId={2}; robotTaskMapId={3}", JsonConvert.SerializeObject(message.Lfp), robotId, runInstanceId, robotTaskMapId),
                    LogHandler.Layer.Business, null);
#endif

                    imageList = new List<string>();
                    frameIdList = new List<string>();
                }
            }
            catch (Exception ex)
            {
                LogHandler.LogError(String.Format(ErrorMessages.Exception_Failed, "Process", "FrameExplainerProcess"),

                        LogHandler.Layer.Business, null);
                LogHandler.LogError(String.Format("Exception Occured while handling an exception in FrameExplainerProcess in Process method. error message: {0}", ex.Message), LogHandler.Layer.Business, null);
            }

            return true;
        }
        public static bool EqualMapping(QueueEntity.FrameExplainerModeMetaData message, string Condition, string value, List<string> imageList, List<string> frameIdList, string blobpath, string leftValue)
        {
            bool frameCondition = false;
            switch (Condition)
            {
                case "=":
                    if (leftValue == value)
                    {
                        frameCondition = true;         
                    }
                    break;
                case "&&":

                    string[] valueList = value.Split(",");
                    foreach (var val in valueList)
                    {
                        frameCondition = FrameCondition(Condition, leftValue, val);
                        if (frameCondition == true)
                            continue;
                        else
                            frameCondition = false;
                        break;

                    }

                    if (frameCondition)
                    {
                        frameCondition = true;     
                    }
                    break;
                case "||":

                    string[] List = value.Split(",");
                    foreach (var val in List)
                    {
                        frameCondition = FrameCondition(Condition, leftValue, val);
                        if (frameCondition == true)
                            break;
                        else
                            frameCondition = false;
                        continue;

                    }
                    if (frameCondition)
                    {
                        frameCondition = true;                       
                    }
                    break;

            }
            return frameCondition;

        }

        public static bool AddImagetolist(string Fid, List<string> imageList, List<string> frameIdList, string blobpath)
        {
            if (imageList.Count < batchSize)
            {             
                imageList.Add(blobpath);
                frameIdList.Add(Fid);
            }
            else if (imageList.Count == batchSize)
            {
                imageList = new List<string>();
                frameIdList = new List<string>();
                imageList.Add(blobpath);
                frameIdList.Add(Fid);
            }
            return true;
        }
        public static bool FrameCondition(string Condition, string Lvalue, string Rvalue)
        {
            bool frameCondition;
            frameCondition = String.Equals(Lvalue, Rvalue);
            return frameCondition;
        }
        public static bool Mapping(QueueEntity.FrameExplainerModeMetaData message, string Condition, string value, List<string> imageList, List<string> frameIdList, string blobpath, string Lvalue)
        {
            bool frameCondition = true;
            switch (Condition)
            {
                case "=":
                    Double equalOperatorLeftValue = Convert.ToDouble(Lvalue);
                    Double equalOperatorRightValue = Convert.ToDouble(value);
                    if (equalOperatorLeftValue == equalOperatorRightValue)
                    {
                        frameCondition = true;                      
                    }                 
                    break;
                case ">":
                    Double greaterThanOperatorLeftValue = Convert.ToDouble(Lvalue);
                    Double greaterThanOperatorRightValue = Convert.ToDouble(value);
                    if (greaterThanOperatorLeftValue > greaterThanOperatorRightValue)
                    {
                        frameCondition = true;                        
                    }   
                    break;
                case "<":
                    Double lessThanOperatorLeftValue = Convert.ToDouble(Lvalue);
                    Double lessThanOperatorRightValue = Convert.ToDouble(value);
                    if (lessThanOperatorLeftValue < lessThanOperatorRightValue)
                    {
                        frameCondition = true;                        
                    }
                    break;
                case "&&":

                    string[] valueList = value.Split(",");
                    foreach (var val in valueList)
                    {
                        frameCondition = FrameCondition(Condition, Lvalue, val);
                        if (frameCondition == true)
                            continue;
                        else
                            frameCondition = false;
                        break;

                    }

                    if (frameCondition)
                    {
                        frameCondition = true;                       
                    }
                    break;
                case "||":

                    string[] List = value.Split(",");
                    foreach (var val in List)
                    {
                        frameCondition = FrameCondition(Condition, Lvalue, val);
                        if (frameCondition == true)
                            continue;
                        else
                            frameCondition = false;
                    }

                    if (frameCondition)
                    {
                        frameCondition = true;
                    }
                    break;
            }
            return frameCondition;

        }
        public static bool AttributeMapping(QueueEntity.FrameExplainerModeMetaData message, string Condition, string value, List<string> imageList, List<string> frameIdList, string blobpath, string Lvalue)
        {
            bool frameCondition = true;
            switch (Condition)
            {
                case "=":
                    Double equalOperatorLeftValue = Convert.ToDouble(Lvalue);
                    Double equalOperatorRightValue = Convert.ToDouble(value);
                    if (equalOperatorLeftValue == equalOperatorRightValue)
                    {
                        if (imageList.Count < batchSize)
                        {                        
                            imageList.Add(blobpath);
                            frameIdList.Add(message.Fid);
                        }
                        else if (imageList.Count == batchSize)
                        {
                            imageList = new List<string>();
                            frameIdList = new List<string>();

                            imageList.Add(blobpath);
                            frameIdList.Add(message.Fid);
                        }
                    }
                    break;
                case ">":
                    Double greaterThanOperatorLeftValue = Convert.ToDouble(Lvalue);
                    Double greaterThanOperatorRightValue = Convert.ToDouble(value);
                    if (greaterThanOperatorLeftValue > greaterThanOperatorRightValue)
                    {
                        if (imageList.Count < batchSize)
                        {   
                            imageList.Add(blobpath);
                            frameIdList.Add(message.Fid);
                        }
                        else if (imageList.Count == batchSize)
                        {
                            imageList = new List<string>();
                            frameIdList = new List<string>();

                            imageList.Add(blobpath);
                            frameIdList.Add(message.Fid);
                        }
                    }
                    break;
                case "<":
                    Double lessThanOperatorLeftValue = Convert.ToDouble(Lvalue);
                    Double lessThanOperatorRightValue = Convert.ToDouble(value);
                    if (lessThanOperatorLeftValue < lessThanOperatorRightValue)
                    {
                        if (imageList.Count < batchSize)
                        {
                            imageList.Add(blobpath);
                            frameIdList.Add(message.Fid);
                        }
                        else if (imageList.Count == batchSize)
                        {
                            imageList = new List<string>();
                            frameIdList = new List<string>();

                            imageList.Add(blobpath);
                            frameIdList.Add(message.Fid);
                        }
                    }
                    break;
                case "&&":

                    string[] valueList = value.Split(",");
                    foreach (var val in valueList)
                    {
                        frameCondition = FrameCondition(Condition, Lvalue, val);
                        if (frameCondition == true)
                            continue;
                        else
                            frameCondition = false;
                        break;

                    }

                    if (frameCondition)
                    {
                        if (imageList.Count < batchSize)
                        {
                            imageList.Add(blobpath);
                            frameIdList.Add(message.Fid);
                        }
                        else if (imageList.Count == batchSize)
                        {
                            imageList = new List<string>();
                            frameIdList = new List<string>();

                            imageList.Add(blobpath);
                            frameIdList.Add(message.Fid);
                        }
                    }
                    break;
                case "||":

                    string[] List = value.Split(",");
                    foreach (var val in List)
                    {
                        frameCondition = FrameCondition(Condition, Lvalue, val);
                        if (frameCondition == true)
                            continue;
                        else
                            frameCondition = false;
                    }

                    if (frameCondition)
                    {
                        if (imageList.Count < batchSize)
                        {
                            imageList.Add(blobpath);
                            frameIdList.Add(message.Fid);
                        }
                        else if (imageList.Count == batchSize)
                        {
                            imageList = new List<string>();
                            frameIdList = new List<string>();

                            imageList.Add(blobpath);
                            frameIdList.Add(message.Fid);
                        }
                    }
                    break;
            }
            return true;

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

      

        private string GetModelUrl(string modelType)
        {
#if DEBUG
            using (LogHandler.TraceOperations("ModelInferenceManager:GetModelType", LogHandler.Layer.MaskPrediction, Guid.NewGuid(), null))
            {
#endif
                string modelUrl = "";

                XElement root = XElement.Load(AppDomain.CurrentDomain.BaseDirectory + GetModelXmlPath());


                var objModelUrl = from modeltype in root.Elements("Type")
                                  where modeltype.Attribute("key").Value.ToLower().Contains(modelType.ToLower())
                                  select modeltype.Attribute("modelUrl")?.Value;

                if (objModelUrl.FirstOrDefault() != null)
                    modelUrl = objModelUrl.FirstOrDefault().ToString();

                return modelUrl;
#if DEBUG
            }
#endif
        }

        private string GetModelXmlPath()
        {
            string xmlFilePath = "";
            if (!String.IsNullOrEmpty(appSettings.ModelXmlFilePath))
            {
                xmlFilePath = appSettings.ModelXmlFilePath;
            }
            else
            {
                xmlFilePath = @"XML/ModelType.xml";
            }
            return xmlFilePath;
        }
  
      private void sendMessage(ObjectDetectorAPIReqMsgExp deReceivedPersonCountMessage,string input)
        {
            
            TaskRouteMetadata taskRouteMetadata = taskRouter.GetTaskRouteConfig(deReceivedPersonCountMessage.Tid, deReceivedPersonCountMessage.Did);
            string task = TaskRouteConstants.ExplainerModelPredictor;
            taskRouter.SendMessageToQueueWithTask(taskRouteMetadata, TaskRouteConstants.FrameExplainerNode, input, task);
        }
    }
}
