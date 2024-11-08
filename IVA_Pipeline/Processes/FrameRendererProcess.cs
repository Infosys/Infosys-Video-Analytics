/*=============================================================================================================== *
 * Copyright 2024 Infosys Ltd.                                                                                    *
 * Use of this source code is governed by Apache License Version 2.0 that can be found in the LICENSE file or at  *
 * http://www.apache.org/licenses/                                                                                *
 * ===============================================================================================================*/

﻿using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;

using Infosys.Solutions.Ainauto.VideoAnalytics.BusinessEntity;
using Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.Common;
using Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.ProcessScheduler.Framework;
using Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.TaskRoute;
using Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.TCPSChannelCommunication;
using Infosys.Solutions.Ainauto.VideoAnalytics.Resource.DataAccess;
using Infosys.Solutions.Ainauto.VideoAnalytics.Resource.Entity.Document;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.Caching;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using static Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.Common.ApplicationConstants;

using Helper = Infosys.Solutions.Ainauto.VideoAnalytics.BusinessComponent.Helper;
using QueueEntity = Infosys.Solutions.Ainauto.VideoAnalytics.Resource.Entity.Queue;
using SC = Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.ServiceClientLibrary;
using SE = Infosys.Solutions.Ainauto.VideoAnalytics.Services.MaskDetector.Contracts;

using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.CvEnum;
using Infosys.Solutions.Ainauto.VideoAnalytics.Resource.Entity.VideoAnalytics;
using Nest;
using Infosys.Solutions.Ainauto.VideoAnalytics.Services.MaskDetector.Contracts.Message;
using Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.PythonLoader;
using System.Drawing.Imaging;
 
using FGH = Infosys.Solutions.Ainauto.VideoAnalytics.BusinessComponent;
using Infosys.Solutions.Ainauto.VideoAnalytics.Resource.DataAccess.Document;

namespace Infosys.Solutions.Ainauto.VideoAnalytics.Processes
{
    public class FrameRendererProcess : ProcessHandlerBase<QueueEntity.FrameRendererMetadata>
    {
         
        #region Attribute
        static Dictionary<string, LocalDataStoreSlot> tlsSlots = new Dictionary<string, LocalDataStoreSlot>();
        ClientConnectHost clientConnect;
        [ThreadStatic] static ClientConnectHost staticClientConnection;
        private readonly object clientConnectLock = new object();
        string processId = string.Empty;
        string storageBaseUrl = string.Empty;
        string ipAddress = string.Empty;
        int port;
        Dictionary<string, ClientConnectHost> clientDictionary = new Dictionary<string, ClientConnectHost>();
        static MemoryCache cache = MemoryCache.Default;
        static CacheItemPolicy policy = new CacheItemPolicy();
        SC.MaskDetector maskDetector = new SC.MaskDetector();
        string debugImageFilePath;
        string ffmpegExeFile;
        string isImageDebugEnabled;
        bool enablePing;
        string frameId;
        int penThickness;
        string maskPenColor;
        string noMaskPenColor;
        int retryCount = 5;
        int waitingTime = 10;
        int labelFontSize;
        string labelFontStyle;
        int labelHeight;
        string labelFontColor;
        string deviceId;
        Stopwatch transportStopWatch = new Stopwatch();
        Stopwatch predictedFrameStopWatch = new Stopwatch();
        Stopwatch lotStopWatch = new Stopwatch();
        Stopwatch processStopWatch = new Stopwatch();
        string counterInstanceName = "";
        string boxColor;
        string predictionClassType;
        string predictionModel;
       
        private Dictionary<string, Queue> frameQueueDetails = new Dictionary<string, Queue>();
        Queue frameQueue = new Queue();
        bool canInitiateTransferFrame = true;
        private readonly object clientLock = new object();
        int frameRendererWaitTimeForTransport = 100;
      
        bool isSharedResource = false;
        private readonly object ffmpegLock = new object();

        int eofCount = 1;
        
        private Dictionary<string, Dictionary<int, List<TransportFrameDetails>>> frameMessageDetails =
           new Dictionary<string, Dictionary<int, List<TransportFrameDetails>>>();
        private Dictionary<string, Queue> sequenceNumberQueueDetails = new Dictionary<string, Queue>();
        private Dictionary<string, int> frameTransferCountDetails = new Dictionary<string, int>();
        private Dictionary<string, bool> lastFrameTransferDetails = new Dictionary<string, bool>();
        private Dictionary<string, bool> allFrameReceived = new Dictionary<string, bool>();
        private Dictionary<string, int> receivedFrameCountDetails = new Dictionary<string, int>();
        private Dictionary<string, int> lastFrameNumberSendForPredictDetails = new Dictionary<string, int>();
        private Dictionary<string, int> totalFrameCountDetails = new Dictionary<string, int>();
        private Dictionary<string, int> totalFrameSendForPredictDetails = new Dictionary<string, int>();
        private Dictionary<string, Dictionary<int, string>> framesNotSendForRendering = new Dictionary<string, Dictionary<int, string>>();
        private Dictionary<string, bool> ClientStatus = new Dictionary<string, bool>();
        private Dictionary<string, int> SeqNumberAfterClientActive = new Dictionary<string, int>();
        private Queue<QueueEntity.FrameInformation> nextFeedToProcess = new Queue<QueueEntity.FrameInformation>();
        private Task previousTask = null;
        static string eofFilePath = null;
        static bool isDbEnabled = true;
       
        Queue messsageQueue = new Queue();

        static private Dictionary<int, QueueEntity.FrameRendererMetadata> messages = new Dictionary<int, QueueEntity.FrameRendererMetadata>();

        static int frameRendererWaitTimeForSequencing = 10;
       
        private readonly object frameCountLock = new object();
        private const string UnderScore = "_";

        int x, y, w, h = 0;

        string isRenderImageEnabled;
        string RenderImageFilePath;
        int i = 0;
        Dictionary<int, string> colornames = new Dictionary<int, string>();
        string backGroundColor;
        int RendererRectanglePointX;
        int RendererRectanglePointY;
        int RendererLabelPointX;
        int RendererLabelPointY;
        int RendererRectangleHeight;
        string RendererPredictCartListBackgroundColor;

        public override void Dump(QueueEntity.FrameRendererMetadata message)
        {

        }
        public override bool Initialize(QueueEntity.MaintenanceMetaData message)
        {
            if (message == null)
            {
                ReadFromConfig();
                
                
                foreach (KnownColor kc in Enum.GetValues(typeof(KnownColor)))   
                {
                    Color known = Color.FromKnownColor(kc);
                    colornames.Add(i, known.Name);
                    i++;
                }

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
                    LogHandler.LogError("Exception in Initialize method of FrameRendererProcess : {0} ", LogHandler.Layer.Business, ex.Message);
                    return false;
                }


            }
            return true;
        }
        private void CacheCleanUp(string[] resourceIdList)
        {
            
            staticClientConnection = null;
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
            var appSettings = Config.AppSettings;
            frameRendererWaitTimeForTransport = appSettings.FrameRenderer_WaitTimeForTransportms;
         
            isSharedResource = appSettings.FrameRender_IsSharedResource;
            debugImageFilePath = appSettings.DebugImageFilePath;
            isImageDebugEnabled = appSettings.ImageDebugEnabled;
            enablePing = appSettings.EnablePing;
            retryCount = appSettings.ClientConnectionRetryCount;
            waitingTime = appSettings.ClientConnectionWaitingTime;
            ffmpegExeFile = appSettings.FfmpegExeFile;
            eofCount = appSettings.FrameRenderer_EOF_Count;
            eofFilePath = appSettings.FrameRenderer_EOF_File_Path;
            isDbEnabled = appSettings.DBEnabled;
            isRenderImageEnabled = appSettings.RenderImageEnabled;
            RenderImageFilePath = appSettings.RenderImageFilePath;

        }

        public override bool Process(QueueEntity.FrameRendererMetadata message,int robotId,int runInstanceId,int robotTaskMapId) {
            
            bool isCompleted=true;
            DeviceDetails deviceDetails=ConfigHelper.SetDeviceDetails(message.Tid,message.Did,CacheConstants.FrameRendererCode);
            int initialCollectionBufferingSize=deviceDetails.InitialCollectionBufferingSize;
          
            int framecount=0;
            int totalMessageCount=-1;
            try {
                if(message!=null) {
                    
                    string feedKey=GenerateFeedKey(message.Tid,message.Did,message.FeedId);
                    if(receivedFrameCountDetails.ContainsKey(feedKey)) {
                        framecount=receivedFrameCountDetails[feedKey];
                        framecount++;
                        receivedFrameCountDetails[feedKey]=framecount;
                    }
                    else {
                        receivedFrameCountDetails.Add(feedKey,0);
                    }
                    
                    if(totalFrameSendForPredictDetails.ContainsKey(feedKey)) {
                        totalMessageCount=totalFrameSendForPredictDetails[feedKey];
                    }
                    if(totalMessageCount>0 && framecount>=totalMessageCount) {
                        if(allFrameReceived.ContainsKey(feedKey)) {
                            allFrameReceived[feedKey]=true;
                        }
                        else {
                            allFrameReceived.Add(feedKey,true);
                        }
                    }
                    if(framecount<initialCollectionBufferingSize) {
                        messsageQueue.Enqueue(message);
                    }
                    else {
                        while(messsageQueue.Count>0) {
                            QueueEntity.FrameRendererMetadata frameMessage=(QueueEntity.FrameRendererMetadata)messsageQueue.Dequeue();
                            ProcessMessage(frameMessage);
                        }
                        ProcessMessage(message);
                    }
                }
            }
            catch(Exception exMP) {
                LogHandler.LogError(String.Format("Exception occured in FrameRendererProcess in Process method."+
                "Error message: {0}",exMP.Message),LogHandler.Layer.Business,null);
                try {
                    Exception ex=new Exception();
                    bool rethrow=ExceptionHandler.HandleException(exMP,ApplicationConstants.FRAMERENDERER_HANDLING_POLICY,out ex);
                    if(rethrow) {
                        throw ex;
                    }
                    else {
                       
                        return true;
                    }
                }
                catch(Exception ex) {
                    LogHandler.LogError(String.Format("Exception occured while handling an exception in FrameRendererProcess in Process method."+
                    "Error message: {0}",ex.Message),LogHandler.Layer.Business,null);
                    return false;
                }
            }
            return isCompleted;
        }

        private byte[] ReadFromLocalFileStore()
        {
            byte[] fileData = null;
            string eof_cacheKey = CacheConstants.FrameRendererEOF;
            fileData = (byte[])cache[eof_cacheKey];

            if (fileData == null)
            {

                if (File.Exists(eofFilePath))
                {
                    fileData = File.ReadAllBytes(eofFilePath);
                    cache.Set(eof_cacheKey, fileData, policy);
                 



                }
            }


            return fileData;
        }
         
        
        public bool ProcessMessage(QueueEntity.FrameRendererMetadata frameRendererData) {
            transportStopWatch.Reset();
            lotStopWatch.Reset();
            predictedFrameStopWatch.Reset();
            processStopWatch.Reset();
            processStopWatch.Start();
            string feedKey=GenerateFeedKey(frameRendererData.Tid,frameRendererData.Did,frameRendererData.FeedId);
            bool isClientActive=true;
            DeviceDetails deviceDetails=ConfigHelper.SetDeviceDetails(frameRendererData.Tid,frameRendererData.Did,CacheConstants.FrameRendererCode);
            bool deleteFramesFromBlob=deviceDetails.DeleteFramesFromBlob;
            bool isEnableLots=deviceDetails.EnableLots;
            String baseUrl=deviceDetails.BaseUrl;
            String fileExtenstion=String.Empty;
            bool blobDeleted=false;
            #if DEBUG
            LogHandler.LogDebug("FrameRendererProcess counterInstanceName in FrameProcessor: {0}",LogHandler.Layer.Business,counterInstanceName);
            LogHandler.LogInfo(String.Format(InfoMessages.Method_Execution_Start,"ProcessData","FrameRendererProcess"),LogHandler.Layer.Business,null);
            LogHandler.LogDebug(String.Format("The Process method of FrameRendererProcess class is getting executed with parameters: FrameProcessor message={0}",JsonConvert.SerializeObject(frameRendererData)),
            LogHandler.Layer.Business,null);
            #endif
            /* LogHandler.LogError(String.Format("The ProcessMessage method of FrameRendererProcess class is getting executed with parameters: FrameProcessor message={0}",JsonConvert.SerializeObject(frameRendererData)),
            LogHandler.Layer.Business,null); */
            try {
                using(LogHandler.TraceOperations("FrameRenderer:ProcessMessage",LogHandler.Layer.Business,Guid.NewGuid(),null)) {
                    /* if(frameRendererData.Fid==ProcessingStatus.EndOfFile)
                        return true; */
                    if(!frameRendererData.TE.ContainsKey(TaskRouteConstants.FrameRendererCode)) {
                        LogHandler.LogError("Message is not processed in FrameRenderer for frameId={0}, tenantId={1}, deviceId={2}, module={3}, message={4}",LogHandler.Layer.Business,frameRendererData.Fid,frameRendererData.Tid,frameRendererData.Did,TaskRouteConstants.FrameProcessorCode,JsonConvert.SerializeObject(frameRendererData));
                        return true;
                    }
                    frameId=frameRendererData.Fid;
                    deviceId=frameRendererData.Did;
                    counterInstanceName=frameRendererData.Tid+"_"+deviceId;
                    /* DeviceDetails deviceDetails=SetDeviceDetails(frameRendererData.Tid,deviceId); */
                    bool EnforceFrameSequencing=deviceDetails.EnforceFrameSequencing;
                    /* If the client viewer is configured for device */
                    if(deviceDetails.VideoStreamingOption==ProcessingStatus.CLIENT_VIEWER) {
                        isClientActive=Helper.getClientStatus(frameRendererData.Did,frameRendererData.Tid);
                        /* If client viewer is active and prevoiusly inactive, in that case we need to set new SequenceNumber to handle sequencing */
                        if(isClientActive) {
                            if(ClientStatus.ContainsKey(feedKey) && !ClientStatus[feedKey]) {
                                if(SeqNumberAfterClientActive.ContainsKey(feedKey)) {
                                    SeqNumberAfterClientActive[feedKey]=int.Parse(frameRendererData.SequenceNumber);
                                }
                                else {
                                    SeqNumberAfterClientActive.Add(feedKey,int.Parse(frameRendererData.SequenceNumber));
                                }
                            }
                        }
                        if(ClientStatus.ContainsKey(feedKey)) {
                            ClientStatus[feedKey]=isClientActive;
                        }
                        else {
                            ClientStatus.Add(feedKey,isClientActive);
                        }
                    }
                    /* Added to input data type as image
                    if(deviceDetails.VideoStreamingOption==ProcessingStatus.IMAGE) {
                        predictedFrameStopWatch.Start();
                        Translate from data entity to business entity
                        FrameRendererEntityTranslator entityTranslator=new FrameRendererEntityTranslator();
                        BE.FrameRendererData frameRendererData1=entityTranslator.FrameRendererTranslator(message);                     
                        string previousFrameIdKey=frameRendererData.Did+"_previousFrameId";
                        long currentFrameId=long.Parse(frameRendererData.Fid);
                        long previousFrameId=0;
                        if(cache[previousFrameIdKey]!=null) {
                            previousFrameId=(long)cache[previousFrameIdKey];
                        }
                        if(EnforceFrameSequencing || currentFrameId>previousFrameId) {
                            PostProcessFrame execute as seperate async thread to improve the throughput of FrameRendererProcess
                            #if DEBUG
                            LogHandler.LogDebug("FrameRendererProcess - Before calling to PostProcessFrame and Thread Id: {0}",
                            LogHandler.Layer.Business,Thread.CurrentThread.ManagedThreadId);
                            #endif
                            Task.Run(()=> {
                                try {
                                    #if DEBUG
                                    LogHandler.LogDebug("FrameRendererProcess - Calling to PostProcessFrame and Thread Id: {0} ",
                                    LogHandler.Layer.Business,Thread.CurrentThread.ManagedThreadId);
                                    #endif
                                    PostProcessFrameImage(frameRendererData,deviceDetails);
                                }
                                catch(Exception ex) {
                                    if(frameTransferCountDetails.ContainsKey(feedKey)) {
                                        int count=frameTransferCountDetails[feedKey];
                                        count++;
                                        frameTransferCountDetails[feedKey]=count;
                                    }
                                    LogHandler.LogError("FrameRendererProcess failed message: {0}",LogHandler.Layer.Business,
                                    JsonConvert.SerializeObject(frameRendererData));
                                    LogHandler.LogError("Exception occured while calling PostProcessFrame of FrameRendererProcess error message: {0}, exception trace: {1} ",
                                    LogHandler.Layer.Business,ex.Message,ex.StackTrace);
                                    throw ex;
                                }
                            });
                            LogHandler.LogInfo("Message got processed",LogHandler.Layer.Business);
                            cache.Set(previousFrameIdKey,currentFrameId,policy);
                            InitiateTransferFrame should call only one time at the begin of the method and keep execute as background thread
                            #if DEBUG
                            LogHandler.LogDebug("FrameRendererProcess - canInitiateTransferFrame value: {0} and Thread Id: {1}",
                            LogHandler.Layer.Business,canInitiateTransferFrame,Thread.CurrentThread.ManagedThreadId);
                            #endif
                            if(canInitiateTransferFrame) {
                                Task.Run(()=> {
                                    InitiateTransferFrame();
                                });
                            }
                            canInitiateTransferFrame=false;
                        }
                        else {
                            if(frameTransferCountDetails.ContainsKey(feedKey)) {
                                int count=frameTransferCountDetails[feedKey];
                                count++;
                                frameTransferCountDetails[feedKey]=count;
                            }
                            LogHandler.LogError("FrameRendererProcess - Frame with frameId: {0} is not processed as the current frame tick ({1}) is less than previous frame tick ({2})",LogHandler.Layer.Business,frameId,currentFrameId,previousFrameId);
                        }
                    } 
                    Check client status */
                    if (deviceDetails.VideoStreamingOption==ProcessingStatus.FFMPEG || isClientActive) {
                        predictedFrameStopWatch.Start();
                        /* Translate from data entity to business entity
                        FrameRendererEntityTranslator entityTranslator=new FrameRendererEntityTranslator();
                        BE.FrameRendererData frameRendererData1=entityTranslator.FrameRendererTranslator(message); */ 
                        string previousFrameIdKey=frameRendererData.Did+"_previousFrameId";
                        long currentFrameId=0;
                        if(frameRendererData.Fid!="") {
                            currentFrameId=long.Parse(frameRendererData.Fid);
                        }
                        long previousFrameId=0;
                        if(cache[previousFrameIdKey]!=null) {
                            previousFrameId=(long)cache[previousFrameIdKey];
                        }
                        string outputImage=deviceDetails.OutputImage.ToLower();
                        if(EnforceFrameSequencing || currentFrameId>previousFrameId || outputImage=="yes") {
                            /* PostProcessFrame execute as seperate async thread to improve the throughput of FrameRendererProcess */
                            #if DEBUG
                            LogHandler.LogDebug("FrameRendererProcess - Before calling to PostProcessFrame and Thread Id: {0}",
                            LogHandler.Layer.Business,Thread.CurrentThread.ManagedThreadId);
                            #endif
                            Task.Run(()=> {
                                try {
                                    #if DEBUG
                                    LogHandler.LogDebug("FrameRendererProcess - Calling to PostProcessFrame and Thread Id: {0}",
                                    LogHandler.Layer.Business,Thread.CurrentThread.ManagedThreadId);
                                    #endif
                                    PostProcessFrame(frameRendererData,deviceDetails);
                                }
                                catch(Exception ex) {
                                    if(frameTransferCountDetails.ContainsKey(feedKey)) {
                                        int count=frameTransferCountDetails[feedKey];
                                        count++;
                                        frameTransferCountDetails[feedKey]=count;

                                    }
                                    LogHandler.LogError("FrameRendererProcess failed message: {0}",LogHandler.Layer.Business,
                                    JsonConvert.SerializeObject(frameRendererData));
                                    LogHandler.LogError("Exception occured while calling PostProcessFrame of FrameRendererProcess error message: {0}, exception trace: {1}",
                                    LogHandler.Layer.Business,ex.Message,ex.StackTrace);
                                    /* throw ex; */
                                }
                            });
                            /* LogHandler.LogInfo("Message got processed",LogHandler.Layer.Business); */
                            cache.Set(previousFrameIdKey,currentFrameId,policy);
                            /* InitiateTransferFrame should call only one time at the begin of the method and keep execute as background thread */
                            #if DEBUG
                            LogHandler.LogDebug("FrameRendererProcess - canInitiateTransferFrame value: {0} and ManagedThreadId: {1}",
                            LogHandler.Layer.Business,canInitiateTransferFrame,Thread.CurrentThread.ManagedThreadId);
                            #endif
                            /* if(canInitiateTransferFrame) {
                                Task.Run(()=> {
                                    InitiateTransferFrame();
                                });
                            }
                            canInitiateTransferFrame=false; */
                        }
                        else {
                            if(frameTransferCountDetails.ContainsKey(feedKey)) {
                                int count=frameTransferCountDetails[feedKey];
                                count++;
                                frameTransferCountDetails[feedKey]=count;
                            }
                            LogHandler.LogError("FrameRendererProcess - Frame with frameId: {0} is not processed as the current frame tick ({1}) is less than previous frame tick ({2})",LogHandler.Layer.Business,frameId,currentFrameId,previousFrameId);
                        }
                    }
                    else {
                        if(frameTransferCountDetails.ContainsKey(feedKey)) {
                            int count=frameTransferCountDetails[feedKey];
                            count++;
                            frameTransferCountDetails[feedKey]=count;
                        }
                        if(framesNotSendForRendering.ContainsKey(feedKey)) {
                            Dictionary<int,string> framesNotSendForClientViewer=(Dictionary<int,string>)framesNotSendForRendering[feedKey];
                            int seqNumber=int.Parse(frameRendererData.SequenceNumber);
                            if(!framesNotSendForClientViewer.ContainsKey(seqNumber)) {
                                framesNotSendForClientViewer.Add(seqNumber,frameRendererData.Fid);
                            }
                        }
                        /* bool deleteFramesFromBlob=deviceDetails.DeleteFramesFromBlob;
                        bool isEnableLots=deviceDetails.EnableLots;
                        String baseUrl=deviceDetails.BaseUrl;
                        String fileExtenstion=String.Empty;
                        if(isEnableLots) {
                            fileExtenstion=ApplicationConstants.FileExtensions.zip;
                        }
                        else {
                            fileExtenstion=ApplicationConstants.FileExtensions.jpg;
                        }
                        if(deleteFramesFromBlob) {
                            Helper.DeleteBlob(frameRendererData.Did,frameRendererData.Fid,frameRendererData.Tid,baseUrl,fileExtenstion);
                        } */
                        DeleteBlobForDisplayAllFrames(deviceDetails,frameRendererData);
                        blobDeleted=true;
                        throw new ClientInactiveException(String.Format("Client - {0} is inactive, no frames are processed",ipAddress));
                    }
                    processStopWatch.Stop();
                    #if DEBUG
                    LogHandler.LogInfo(String.Format(InfoMessages.Method_Execution_End,"Process","FrameRendererProcess"),LogHandler.Layer.Business,null);
                    #endif
                    return true;
                }
            }
            catch(Exception exMP) {
                if(!blobDeleted) {
                    DeleteBlobForDisplayAllFrames(deviceDetails,frameRendererData);
                }
                LogHandler.LogError("Exception in FrameRendererProcess: {0}",LogHandler.Layer.Business,exMP.Message);
                if(exMP is ClientInactiveException || exMP is ClientDisconnectedException || exMP is ClientNotConnectedException) {
                    #if DEBUG
                    LogHandler.LogDebug("Exception in FrameRendererProcess: {0} for frameId: {1} and deviceId: {2}. Exception trace: {3}",LogHandler.Layer.Business,exMP.Message,frameId,deviceId,exMP.StackTrace);
                    #endif
                }
                else {
                    LogHandler.LogError("Exception in FrameRendererProcess: {0} for frameId: {1} and deviceId: {2}. Exception trace: {3}",LogHandler.Layer.Business,exMP.Message,frameId,deviceId,exMP.StackTrace);
                }
                /* LogHandler.LogDebug(String.Format("Exception occured in ProcessData method of FrameRendererProcess class"),LogHandler.Layer.Business,null); */
                bool failureLogged=false;
                try {
                    Exception ex=new Exception();
                    bool rethrow=ExceptionHandler.HandleException(exMP,ApplicationConstants.FRAMERENDERER_HANDLING_POLICY,out ex);
                    failureLogged=true;
                    if(rethrow) {
                        throw ex;
                    }
                    else {
                        /* Set as a successful operation as the message was invalid since an equivalent presentation entity was
                        not found in the database. This could be a rogue transaction.
                        Returning a true since the message has been sent with invalid presentation id and has to be deleted
                        to avoid further processingtiple */
                        return true;
                    }
                }
                catch (Exception ex) {
                    LogHandler.LogError(String.Format(ErrorMessages.Exception_Failed,"ProcessMessage","FrameRendererProcess"),
                    LogHandler.Layer.Business,null);
                    /* Any messages which would have to indicate to the worker process that the transaction has failed
                    and the message should be retried
                    MetricProcessing request failed */
                    if(!failureLogged) {
                        LogHandler.LogError(String.Format("Exception occured while handling an exception in FrameRendererProcess in ProcessMessage method. Error message: {0}",ex.Message),LogHandler.Layer.Business,null);
                    }
                    #if DEBUG
                    LogHandler.LogInfo(String.Format(InfoMessages.Method_Execution_End,"ProcessMessage","FrameRendererProcess"),LogHandler.Layer.Business,null);
                    #endif
                    return false;
                }
            }
        }
        #endregion

        private void DeleteBlobForDisplayAllFrames(DeviceDetails deviceDetails, QueueEntity.FrameRendererMetadata frameRendererData)
        {
            string fileExtenstion;
            if (deviceDetails.EnableLots)
            {
                fileExtenstion = ApplicationConstants.FileExtensions.zip;
            }
            else
            {
                fileExtenstion = ApplicationConstants.FileExtensions.jpg;
            }

            if (deviceDetails.DeleteFramesFromBlob)
            {
                if (deviceDetails.DisplayAllFrames)
                {
                    foreach (var frameiD in frameRendererData.Fids)
                    {

                        Helper.DeleteBlob(frameRendererData.Did, frameiD, frameRendererData.Tid, deviceDetails.BaseUrl, fileExtenstion);

                    }
                }
                else
                {
                    Helper.DeleteBlob(frameRendererData.Did, frameRendererData.Fid, frameRendererData.Tid, deviceDetails.BaseUrl, fileExtenstion);
                }

            }
        }
        private int getNextSeqNumber(int previousSeqNumber, int frameToPredict, int maxSequenceNumber)
        {
            int nextSequenceNumber = 0;
            switch (previousSeqNumber)
            {
                case 0:
                    previousSeqNumber = 1;
                    nextSequenceNumber = 1;
                    break;
                default:
                    nextSequenceNumber = previousSeqNumber + frameToPredict;
                    if (nextSequenceNumber >= maxSequenceNumber)
                    {
                        nextSequenceNumber = 1;
                    }
                    break;
            }

            return nextSequenceNumber;
        }

        private int resetpreviousSeqNumber(int sequenceNumber, int maxSequenceNumber)
        {
            int previousSeqNumber = 0;
            if (sequenceNumber >= maxSequenceNumber)
            {
                previousSeqNumber = 0;
            }
            else
            {
                previousSeqNumber = sequenceNumber;
            }
            return previousSeqNumber;
        }


        public void sendData(byte[] data, string deviceId, int tenantId, string ipaddress, int port, bool isFirstFrame)
        {

            transportStopWatch.Start();
#if DEBUG
            LogHandler.LogDebug("FrameRendererProcess Ping is successful", LogHandler.Layer.Business);
#endif
            clientConnect.Send(data);
            transportStopWatch.Stop();

#if DEBUG
            LogHandler.LogDebug("FrameRendererProcess Frame is sent", LogHandler.Layer.Business);
#endif

            if (isFirstFrame)
            {
                predictedFrameStopWatch.Stop();


            }
            else
            {

                lotStopWatch.Stop();


            }


        }


        public void TransportFrameWithffmpeg(TransportFrameDetails transportFrameDetails)
        {
            Process proc;
           proc = IntialiseFfmpeg(transportFrameDetails.FfmpegArguments, transportFrameDetails.TenantId.ToString(), transportFrameDetails.DeviceId, Convert.ToInt32(transportFrameDetails.FeedId));
            try
            {

               using (var ms = new MemoryStream(transportFrameDetails.Data))
                {
                    ms.WriteTo(proc.StandardInput.BaseStream);
                   ms.Dispose();
                }


            }
            catch (Exception ex)
            {
                LogHandler.LogError("Exception in TransportFrameWithffmpeg : {0}", LogHandler.Layer.Business, ex.Message);
                throw ex;
            }

        }

        public void TransportFrame(byte[] data, string deviceId, int tenantId, string ipaddress, int port, bool isFirstFrame)
        {
#if DEBUG
            LogHandler.LogInfo(String.Format(InfoMessages.Method_Execution_Start, "TransportFrame", "FrameRendererProcess"), LogHandler.Layer.Business, null);
            LogHandler.LogInfo("Transporting Frame to IpAddress ={0}. port = {1}", LogHandler.Layer.Business, ipaddress, port);
#endif
            
            bool allDevice = processId.Contains("ALL");
            string clientConnectKey = deviceId + "_clientConnect";
            int currentRetry = 0;
#if DEBUG
            using (LogHandler.TraceOperations("FrameRendererProcess:TransportFrame", LogHandler.Layer.Business, Guid.NewGuid()))
            {
#endif
                while (true)
                {
                    CreateClientConnect(ipaddress, port, allDevice, clientConnectKey);

                    try
                    {
                        if (enablePing)
                        {

                            byte[] pingMsg = Encoding.ASCII.GetBytes("ping");
                            
                            if (clientConnect.Send(pingMsg))
                            {

                                sendData(data, deviceId, tenantId, ipaddress, port, isFirstFrame);
                            }
                            break;
                        }
                        else
                        {
                            
                            sendData(data, deviceId, tenantId, ipaddress, port, isFirstFrame);
                            break;
                        }
                    }
                    catch (ClientNotConnectedException ex)
                    {
                        currentRetry++;
                        if (currentRetry > retryCount)
                        {
                            
                            updateClientStatus(deviceId, tenantId, FrameRendererKey.clientInactive);

                            clientConnect.WaitClientThreadToStop();
                            switch (allDevice)
                            {
                                case true:
                                    cache.Remove(clientConnectKey);
                                    break;
                                case false:
                                    staticClientConnection = null;
                                    break;
                            }
                            clientConnect = null;
                            
                            throw ex;
                        }
                        if (currentRetry <= retryCount)
                        {
                            Thread.Sleep(waitingTime);

                        }
                    }
                    catch (Exception ex)
                    {
                        currentRetry++;
                        if (ex is IOException || ex is ClientDisconnectedException)
                        {
                            if (currentRetry > retryCount)
                            {
                                
                                updateClientStatus(deviceId, tenantId, FrameRendererKey.clientInactive);
                                clientConnect.WaitClientThreadToStop();
                                switch (allDevice)
                                {
                                    case true:
                                        cache.Remove(clientConnectKey);
                                        break;
                                    case false:
                                        staticClientConnection = null;
                                        break;
                                }
                                clientConnect = null;
                                
                                throw ex;
                            }
                            if (currentRetry <= retryCount)
                            {
                                clientConnect.WaitClientThreadToStop();
                                clientConnect = null;
                                clientConnect = StartClient(ipaddress, port);


                                switch (allDevice)
                                {
                                    case true:
                                        cache.Set(clientConnectKey, clientConnect, policy);
                                        break;
                                    case false:
                                        staticClientConnection = clientConnect;
                                        break;
                                }
                                


                            }
                        }
                        else
                        {
#if DEBUG
                            LogHandler.LogInfo(String.Format(InfoMessages.Method_Execution_End, "TransportFrame", "FrameRendererProcess"), LogHandler.Layer.Business, null);
#endif
                            throw ex;
                        }


                    }


                }
#if DEBUG
                LogHandler.LogInfo(String.Format(InfoMessages.Method_Execution_End, "TransportFrame", "FrameRendererProcess"), LogHandler.Layer.Business, null);

            }
#endif
        }

        private void CreateClientConnect(string ipaddress, int port, bool allDevice, string clientConnectKey)
        {
#if DEBUG
            using (LogHandler.TraceOperations("FrameRendererProcess:CreateClientConnect", LogHandler.Layer.Business, Guid.NewGuid()))
            {
#endif
                switch (allDevice)
                {

                    case false:

                        lock (clientConnectLock)
                        {

                            if (staticClientConnection == null)
                            {
#if DEBUG
                                LogHandler.LogDebug("FrameRendererProcess creating new connection for {0}, ipaddress {1}, port {2}", LogHandler.Layer.Business, processId, ipaddress, port);
#endif

                                staticClientConnection = StartClient(ipaddress, port);
                            }
                            else
                            {
#if DEBUG
                                LogHandler.LogDebug("FrameRendererProcess reusing client connection for {0}, ipaddress {1}, port {2}", LogHandler.Layer.Business, processId, ipaddress, port);
#endif
                            }

                        }


                        clientConnect = staticClientConnection;

                        break;
                    case true:

                        lock (clientConnectLock)
                        {
                            if (cache[clientConnectKey] != null)
                            {

#if DEBUG
                                LogHandler.LogDebug("FrameRendererProcess reusing client connection for {0} from Cache", LogHandler.Layer.Business, processId);
#endif

                                clientConnect = (ClientConnectHost)cache[clientConnectKey];

                            }
                            else
                            {


#if DEBUG
                                LogHandler.LogDebug("FrameRendererProcess created new connection for {0}, ipaddress {1}, port {2}", LogHandler.Layer.Business, processId, ipaddress, port);
#endif
                                clientConnect = StartClient(ipaddress, port);
                                
                                cache.Set(clientConnectKey, clientConnect, policy);


                            }
                        }
                        
                        break;
                }
#if DEBUG
            }
#endif
        }

        public void updateClientStatus(string deviceId, int tenantId, string status)
        {
#if DEBUG
            using (LogHandler.TraceOperations("FrameRendererProcess:updateClientStatus", LogHandler.Layer.Business, Guid.NewGuid()))
            {
#endif
                
                if (!isDbEnabled)
                {
                    return;
                }
                SE.Message.UpdateResourceAttributeReqMsg data = new SE.Message.UpdateResourceAttributeReqMsg()
                {
                    AttributeName = FrameRendererKey.clientActivationAttribute,
                    AttributeValue = status,
                    ResourceId = deviceId,
                    TenantId = tenantId
                };

               
                var uri = String.Format($"{Config.AppSettings.ConfigWebApi}Configuration/UpdateResourceAttribute");
                var req = JsonConvert.SerializeObject(data);
                string apiResponse = ServiceCaller.ServiceCall(req, uri, "PUT");
                var response = JsonConvert.DeserializeObject<SE.Message.UpdateResourceAttributeResMsg>(apiResponse);

#if DEBUG
                LogHandler.LogInfo(String.Format(InfoMessages.Method_Execution_End, "updateClientStatus", "FrameRendererProcess"), LogHandler.Layer.Business, null);
            }
#endif
        }

        private void TriggerTransferFrame(QueueEntity.FrameInformation frameInformation)
        {
            string feedKey = GenerateFeedKey(frameInformation.TID, frameInformation.DID, frameInformation.FeedId);
#if DEBUG
            LogHandler.LogDebug("FrameRendererProcess - TriggerTransferFrame is called for process Id {0} ", LogHandler.Layer.Business, feedKey);
#endif
            DeviceDetails deviceDetails = ConfigHelper.SetDeviceDetails(frameInformation.TID, frameInformation.DID, CacheConstants.FrameRendererCode);
            bool EnforceFrameSequencing = deviceDetails.EnforceFrameSequencing;
            int frameToPredict = deviceDetails.FrameToPredict;
            int maxSequenceNumber = deviceDetails.MaxSequenceNumber;
            int initialCollectionBufferingSize = deviceDetails.InitialCollectionBufferingSize;
            int frameSequencingMessageStuckDuration = deviceDetails.FrameSequencingMessageStuckDuration;
            int frameSequencingMessageRetry = deviceDetails.FrameSequencingMessageRetry;
            int transportSequencingBufferingSize = deviceDetails.TransportSequencingBufferingSize;
            bool isAllFrameReceived = false;
            bool isAllFrameReceivedForTransfer = false;
            int previousSeqNumber = 0;
            int frameCount = 0;
            Dictionary<int, List<TransportFrameDetails>> transferFrameMessage = (Dictionary<int, List<TransportFrameDetails>>)frameMessageDetails[feedKey];
            Queue seqNumberQueue = (Queue)sequenceNumberQueueDetails[feedKey];
            Dictionary<int, string> framesNotSendForClientViewer = null;
            List<int> sequenceNumberList = new List<int>();
            int count = 0;
            bool canTransferFrame = true;
            bool isLastFrameTransferred = false;
            int lastFrameNumberSendForPredict = -1;
            int totalFrameCount = -1;
            int totalMessageCount = -1;
            int totalFrameprocessed = -1;
            int tid = int.Parse(frameInformation.TID);
            string ffmpegArgument = deviceDetails.FfmpegArguments;
            string deviceId = frameInformation.DID;
            int feedId = int.Parse(frameInformation.FeedId);
            string ipAddress = deviceDetails.IpAddress;
            int port = deviceDetails.Port;
            int videoStreamingOption = deviceDetails.VideoStreamingOption;
            bool isFrameReceivedForTransfer = false;
            int currentRetry = 0;
            int currentMaxSeqNumber = 0;
            TransportSequenceDetails transportSequenceDetails = new TransportSequenceDetails();
            List<int> skippedSequenceNumbers = new List<int>();
            bool isClientActive = true;
            bool isCleanedUpAlready = false;
            int seqNumber=0;
            while (canTransferFrame)
            {
                try
                {
                    int queueCount = 0;
                    int messageCount = 0;

                    if (frameTransferCountDetails.ContainsKey(feedKey))
                    {
                        frameCount = frameTransferCountDetails[feedKey];

                    }
                    if (lastFrameTransferDetails.ContainsKey(feedKey))
                    {
                        isLastFrameTransferred = lastFrameTransferDetails[feedKey];
                    }
                    if (allFrameReceived.ContainsKey(feedKey))
                    {
                        isAllFrameReceived = allFrameReceived[feedKey];
                    }

                    if (totalFrameSendForPredictDetails.ContainsKey(feedKey))
                    {
                        totalMessageCount = totalFrameSendForPredictDetails[feedKey];
                    }
                    if (lastFrameNumberSendForPredictDetails.ContainsKey(feedKey))
                    {
                        lastFrameNumberSendForPredict = lastFrameNumberSendForPredictDetails[feedKey];
                    }
                    if (totalFrameCountDetails.ContainsKey(feedKey))
                    {
                        totalFrameCount = totalFrameCountDetails[feedKey];
                        if (totalFrameCount > 0)
                        {
                            totalFrameprocessed = (totalFrameCount / deviceDetails.FrameToPredict) * deviceDetails.FrameToPredict;
                        }


                    }
                    if (framesNotSendForRendering.ContainsKey(feedKey))
                    {
                        framesNotSendForClientViewer =
                                    (Dictionary<int, string>)framesNotSendForRendering[feedKey];
                    }
                    if (isLastFrameTransferred)
                    {
                        canTransferFrame = false;
                        break;
                    }
                    
                    if (totalMessageCount > 0 && frameCount >= totalMessageCount)
                    {
                        
                        isAllFrameReceivedForTransfer = true;

                    }
                    else if (!EnforceFrameSequencing && isAllFrameReceived)
                    {
                        isAllFrameReceivedForTransfer = true;
                    }
                    if (ClientStatus.ContainsKey(feedKey))
                    {
                        isClientActive = ClientStatus[feedKey];
                    }
                    
                    if (deviceDetails.VideoStreamingOption == ProcessingStatus.CLIENT_VIEWER && !isClientActive)
                    {
                        cleanUpFrames(transferFrameMessage, deviceDetails);
                        if (seqNumberQueue != null)
                        {
                            seqNumberQueue.Clear();
                        }
                        isCleanedUpAlready = true;
                    }
                    
                    if (isCleanedUpAlready && SeqNumberAfterClientActive.ContainsKey(feedKey))
                    {
                        int seqNo = SeqNumberAfterClientActive[feedKey];
                        if (seqNo > 0)
                        {
                            previousSeqNumber = seqNo - 1;
                        }
                    }
                     
                    if (seqNumberQueue.Count != 0)         
                    {
                        queueCount = seqNumberQueue.Count;
                    }
                  
                    if (transferFrameMessage != null)
                    {
                        
                        foreach (int skippedSeqNumber in skippedSequenceNumbers)
                        {
                            if (sequenceNumberList.Contains(skippedSeqNumber))
                            {
                                sequenceNumberList.Remove(skippedSeqNumber);
                            }
                            if (transferFrameMessage.ContainsKey(skippedSeqNumber))
                            {
                                
                                transferFrameMessage.Remove(skippedSeqNumber);
                                
                            }
                        }
                        messageCount = transferFrameMessage.Count;
                    }
                  
                    if (queueCount > 0 )                  
                    {
                        try
                        {
#if DEBUG
                            LogHandler.LogDebug("FrameRendererProcess - frameQueue count {0}  ",
                                LogHandler.Layer.Business, queueCount);
#endif  
                          
                            currentRetry = 0;
                            count++;
                            


                            
                            if (deviceDetails.VideoStreamingOption == 2)
                            {
                                if (seqNumberQueue.ToArray().Length > 0)
                                {
                                   
                                    if ( seqNumberQueue.Contains(2)==true)
                                    {
                                         
                                            seqNumber = (int)seqNumberQueue.Dequeue();
                                    }
                                    else
                                    {
                                        seqNumber = 1;
                                    }
                                   

                                }
                            }
                            else
                            {
                                seqNumber = (int)seqNumberQueue.Dequeue();
                            }

                          
                            sequenceNumberList.Add(seqNumber);

                            isFrameReceivedForTransfer = true;
                            
                            if (seqNumber > currentMaxSeqNumber)
                            {
                                currentMaxSeqNumber = seqNumber;
                                
                            }
                            

                            
                            

                            
                            if (count >= transportSequencingBufferingSize || isAllFrameReceivedForTransfer)
                            {
                                count = 0;
                                switch (EnforceFrameSequencing)
                                {
                                    case true:
                                        transportSequenceDetails.PreviousSeqNumber = previousSeqNumber;
                                        transportSequenceDetails.LastFrameNumberSendForPredict = lastFrameNumberSendForPredict;
                                        transportSequenceDetails.TotalFrameCount = totalFrameCount;
                                        transportSequenceDetails.TotalFrameprocessed = totalFrameprocessed;
                                        transportSequenceDetails.CurrentMaxSeqNumber = currentMaxSeqNumber;
                                        transportSequenceDetails.SkippedSequenceNumbers = skippedSequenceNumbers;
                                        transportSequenceDetails.FrameToPredict = frameToPredict;
                                        transportSequenceDetails.MaxSequenceNumber = maxSequenceNumber;
                                        transportSequenceDetails.FrameSequencingMessageStuckDuration = frameSequencingMessageStuckDuration;
                                        transportSequenceDetails.FrameSequencingMessageRetry = frameSequencingMessageRetry;

                                        transportSequenceDetails = HandleFrameForSequencing(sequenceNumberList, transferFrameMessage,
                                                 framesNotSendForClientViewer, transportSequenceDetails);

                                        bool isNextSeqBeyondLastFrame = transportSequenceDetails.IsNextSeqBeyondLastFrame;
                                        previousSeqNumber = transportSequenceDetails.PreviousSeqNumber;
                                        skippedSequenceNumbers = transportSequenceDetails.SkippedSequenceNumbers;

                                        if (isNextSeqBeyondLastFrame)
                                        {
                                            
                                            transferFrameMessage.Clear();
                                            seqNumberQueue.Clear();
                                            sequenceNumberList.Clear();
                                            canTransferFrame = false;
                                        }

                                        break;
                                    case false:
                                        foreach (int i in sequenceNumberList)
                                        {
                                            if (transferFrameMessage.ContainsKey(i))
                                            {
                                                List<TransportFrameDetails> frameDetails = transferFrameMessage[i];
                                                if (frameDetails != null)
                                                {
                                                    
                                                    transferFrameMessage.Remove(i);
                                                    
                                                    TransportFrame(frameDetails, totalFrameprocessed);
                                                    frameDetails = null;
                                                }
                                            }

                                        }
                                        sequenceNumberList.Clear();
                                        break;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            LogHandler.LogError("Exception in  triggertransferframe  : {0},strace : {1}", LogHandler.Layer.Business, ex.Message, ex.StackTrace);                      
                            throw ex;
                        }

                    }
                    else if (messageCount > 0 && isAllFrameReceivedForTransfer)
                    {
                        currentRetry = 0;
                        
                        switch (EnforceFrameSequencing)
                        {
                            case true:

                                transportSequenceDetails.PreviousSeqNumber = previousSeqNumber;
                                transportSequenceDetails.LastFrameNumberSendForPredict = lastFrameNumberSendForPredict;
                                transportSequenceDetails.TotalFrameCount = totalFrameCount;
                                transportSequenceDetails.TotalFrameprocessed = totalFrameprocessed;
                                transportSequenceDetails.CurrentMaxSeqNumber = currentMaxSeqNumber;
                                transportSequenceDetails.SkippedSequenceNumbers = skippedSequenceNumbers;
                                transportSequenceDetails.FrameToPredict = frameToPredict;
                                transportSequenceDetails.MaxSequenceNumber = maxSequenceNumber;
                                transportSequenceDetails.FrameSequencingMessageStuckDuration = frameSequencingMessageStuckDuration;
                                transportSequenceDetails.FrameSequencingMessageRetry = frameSequencingMessageRetry;

                                transportSequenceDetails = HandleReminingFrameForTransferSequencing(transferFrameMessage,
                                    framesNotSendForClientViewer, transportSequenceDetails);

                                previousSeqNumber = transportSequenceDetails.PreviousSeqNumber;
                                skippedSequenceNumbers = transportSequenceDetails.SkippedSequenceNumbers;
                                bool isNextSeqBeyondLastFrame = transportSequenceDetails.IsNextSeqBeyondLastFrame;

                                if (isNextSeqBeyondLastFrame)
                                {
                                    
                                    transferFrameMessage.Clear();
                                    seqNumberQueue.Clear();
                                    sequenceNumberList.Clear();
                                    canTransferFrame = false;
                                }

                                break;
                            case false:
                                foreach (var details in transferFrameMessage)
                                {
                                    if (details.Value != null)
                                    {
                                        TransportFrame(details.Value, totalFrameprocessed);
                                    }
                                }
                                break;
                        }
                        transferFrameMessage.Clear();
                        seqNumberQueue.Clear();
                        sequenceNumberList.Clear();
                        canTransferFrame = false;
                    }
                    else
                    {
#if DEBUG
                        LogHandler.LogDebug("FrameRendererProcess - No data to transfer and wait time {0} ",
                            LogHandler.Layer.Business, frameRendererWaitTimeForTransport);
#endif
                        


                        if (lastFrameNumberSendForPredict > 0 && totalFrameCount > 0 && isFrameReceivedForTransfer && messageCount <= 0)
                        {


                            Thread.Sleep(frameSequencingMessageStuckDuration);
                            currentRetry++;

                            

                            if (frameSequencingMessageRetry <= currentRetry)
                            {
                                break;
                            }
                        }
                        else
                        {
                            

                            currentRetry = 0;
                            Thread.Sleep(frameSequencingMessageStuckDuration);
                        }
                        

                    }

                }
                catch (Exception ex)
                {
                    LogHandler.LogError("Exception in  triggertransferframe  : {0},strace : {1}", LogHandler.Layer.Business, ex.Message, ex.StackTrace);
                 
                }
            }
            
            if (lastFrameTransferDetails.Count > 0)
            {
            if (feedKey != null && !lastFrameTransferDetails[feedKey])
                {

                    HandlingVideoCompletion(tid, deviceId, ffmpegArgument, feedId, ipAddress, port, videoStreamingOption);
                }
            }

            transferFrameMessage.Clear();
            seqNumberQueue.Clear();
            ClearDictionaries(feedKey);

        }

        private void ClearDictionaries(string feedkey)
        {
            if (frameMessageDetails.ContainsKey(feedkey))
                frameMessageDetails[feedkey].Clear();
            if (sequenceNumberQueueDetails.ContainsKey(feedkey))
                sequenceNumberQueueDetails[feedkey].Clear();
            if (frameTransferCountDetails.ContainsKey(feedkey))
                frameTransferCountDetails.Remove(feedkey);
            if (lastFrameTransferDetails.ContainsKey(feedkey))
                lastFrameTransferDetails.Remove(feedkey);
            if (receivedFrameCountDetails.ContainsKey(feedkey))
                receivedFrameCountDetails.Remove(feedkey);
            if (allFrameReceived.ContainsKey(feedkey))
                allFrameReceived.Remove(feedkey);
            if (lastFrameNumberSendForPredictDetails.ContainsKey(feedkey))
                lastFrameNumberSendForPredictDetails.Remove(feedkey);
            if (totalFrameCountDetails.ContainsKey(feedkey))
                totalFrameCountDetails.Remove(feedkey);
            if (totalFrameSendForPredictDetails.ContainsKey(feedkey))
                totalFrameSendForPredictDetails.Remove(feedkey);
            if (framesNotSendForRendering.ContainsKey(feedkey))
                framesNotSendForRendering[feedkey].Clear();
            if (ClientStatus.ContainsKey(feedkey))
                ClientStatus.Remove(feedkey);
            if (SeqNumberAfterClientActive.ContainsKey(feedkey))
                SeqNumberAfterClientActive.Remove(feedkey);
        }


        private TransportSequenceDetails HandleReminingFrameForTransferSequencing(Dictionary<int, List<TransportFrameDetails>> transferFrameframeDetail,
            Dictionary<int, string> framesNotSendForClientViewer, TransportSequenceDetails transportSequenceDetails)
        {
            int currentRetry = 0;
            int previousSeqNumber = transportSequenceDetails.PreviousSeqNumber;
            int lastFrameNumberSendForPredict = transportSequenceDetails.LastFrameNumberSendForPredict;
            int totalFrameCount = transportSequenceDetails.TotalFrameCount;
            int totalFrameprocessed = transportSequenceDetails.TotalFrameprocessed;
            int currentMaxSeqNumber = transportSequenceDetails.CurrentMaxSeqNumber;
            List<int> skippedSequenceNumbers = transportSequenceDetails.SkippedSequenceNumbers;
            int frameToPredict = transportSequenceDetails.FrameToPredict;
            int maxSequenceNumber = transportSequenceDetails.MaxSequenceNumber;
            int frameSequencingMessageStuckDuration = transportSequenceDetails.FrameSequencingMessageStuckDuration;
            int frameSequencingMessageRetry = transportSequenceDetails.FrameSequencingMessageRetry;
            bool isNextSeqBeyondLastFrame = false;

            while (transferFrameframeDetail.Count > 0)
            {

                int nextSeqNumber = getNextSeqNumber(previousSeqNumber, frameToPredict, maxSequenceNumber);
                
                if (nextSeqNumber > currentMaxSeqNumber)
                {
                    
                    break;
                }
                
                if (lastFrameNumberSendForPredict > 0 && totalFrameCount > 0)
                {
                    if (nextSeqNumber > lastFrameNumberSendForPredict || nextSeqNumber > totalFrameCount)
                    {
                        
                        isNextSeqBeyondLastFrame = true;
                        break;
                    }
                }
                if (transferFrameframeDetail.ContainsKey(nextSeqNumber))
                {
                    List<TransportFrameDetails> frameDetail = transferFrameframeDetail[nextSeqNumber];
                    if (frameDetail != null)
                    {
                        try
                        {
                            
                            transferFrameframeDetail.Remove(nextSeqNumber);
                            
                            previousSeqNumber = resetpreviousSeqNumber(nextSeqNumber, maxSequenceNumber);
                            TransportFrame(frameDetail, totalFrameprocessed);
                            frameDetail = null;
                        }
                        catch (Exception exp)
                        {
                            LogHandler.LogError(String.Format(ErrorMessages.Exception_Failed, "HandleReminingFrameForTransferSequencing", "FrameRendererProcess"),
                            LogHandler.Layer.Business, exp.Message);
                            throw exp;
                        }

                    }
                }
                else if (framesNotSendForClientViewer != null && framesNotSendForClientViewer.ContainsKey(nextSeqNumber))
                {
                    previousSeqNumber = resetpreviousSeqNumber(nextSeqNumber, maxSequenceNumber);
                }
                else
                {
                                        
                    Thread.Sleep(frameSequencingMessageStuckDuration);
                    currentRetry++;
                    if (frameSequencingMessageRetry <= currentRetry)
                    {
                        currentRetry = 0;
                        
                        skippedSequenceNumbers.Add(nextSeqNumber);
                        previousSeqNumber = resetpreviousSeqNumber(nextSeqNumber, maxSequenceNumber);
                    }
                }


            }
            transportSequenceDetails.PreviousSeqNumber = previousSeqNumber;
            transportSequenceDetails.IsNextSeqBeyondLastFrame = isNextSeqBeyondLastFrame;
            transportSequenceDetails.SkippedSequenceNumbers = skippedSequenceNumbers;
            return transportSequenceDetails;

        }


        private TransportSequenceDetails HandleFrameForSequencing(List<int> seqNumberList, Dictionary<int, List<TransportFrameDetails>> transferFrameMessage,
             Dictionary<int, string> framesNotSendForClientViewer, TransportSequenceDetails transportSequenceDetails)
        {


            int size = seqNumberList.Count;
            int currentRetry = 0;
            bool isNextSeqBeyondLastFrame = false;
            int previousSeqNumber = transportSequenceDetails.PreviousSeqNumber;
            int lastFrameNumberSendForPredict = transportSequenceDetails.LastFrameNumberSendForPredict;
            int totalFrameCount = transportSequenceDetails.TotalFrameCount;
            int totalFrameprocessed = transportSequenceDetails.TotalFrameprocessed;
            int currentMaxSeqNumber = transportSequenceDetails.CurrentMaxSeqNumber;
            List<int> skippedSequenceNumbers = transportSequenceDetails.SkippedSequenceNumbers;
            int frameToPredict = transportSequenceDetails.FrameToPredict;
            int maxSequenceNumber = transportSequenceDetails.MaxSequenceNumber;
            int frameSequencingMessageStuckDuration = transportSequenceDetails.FrameSequencingMessageStuckDuration;
            int frameSequencingMessageRetry = transportSequenceDetails.FrameSequencingMessageRetry;

            for (int i = 1; i <= size; i++)
            {

                
                int nextSeqNumber = getNextSeqNumber(previousSeqNumber, frameToPredict, maxSequenceNumber);
                

                
                if (nextSeqNumber > currentMaxSeqNumber)
                {
                    
                    break;
                }

                if (lastFrameNumberSendForPredict > 0 && totalFrameCount > 0)
                {
                    
                    if (nextSeqNumber > lastFrameNumberSendForPredict || nextSeqNumber > totalFrameCount)
                    {
                        
                        isNextSeqBeyondLastFrame = true;
                        break;
                    }
                }
                if (transferFrameMessage.ContainsKey(nextSeqNumber))
                {
                    List<TransportFrameDetails> frameDetails = transferFrameMessage[nextSeqNumber];
                    if (frameDetails != null)
                    {
                        try
                        {
                            
                            transferFrameMessage.Remove(nextSeqNumber);
                            
                            if (seqNumberList.Contains(nextSeqNumber))
                            {
                                seqNumberList.Remove(nextSeqNumber);
                            }
                            previousSeqNumber = resetpreviousSeqNumber(nextSeqNumber, maxSequenceNumber);
                            TransportFrame(frameDetails, totalFrameprocessed);
                            frameDetails = null;
                        }
                        catch (Exception exp)
                        {
                            LogHandler.LogError(String.Format(ErrorMessages.Exception_Failed, "HandleFrameForSequencing", "FrameRendererProcess"),
                            LogHandler.Layer.Business, exp.Message);
                            throw exp;
                        }

                    }
                }
                else if (framesNotSendForClientViewer != null && framesNotSendForClientViewer.ContainsKey(nextSeqNumber))
                {
                    previousSeqNumber = resetpreviousSeqNumber(nextSeqNumber, maxSequenceNumber);
                }
                else
                {
                                        
                    Thread.Sleep(frameSequencingMessageStuckDuration);
                    currentRetry++;
                    if (frameSequencingMessageRetry <= currentRetry)
                    {
                        currentRetry = 0;
                        skippedSequenceNumbers.Add(nextSeqNumber);
                        previousSeqNumber = resetpreviousSeqNumber(nextSeqNumber, maxSequenceNumber);
                    }
                }
            }
            
            transportSequenceDetails.PreviousSeqNumber = previousSeqNumber;
            transportSequenceDetails.IsNextSeqBeyondLastFrame = isNextSeqBeyondLastFrame;
            transportSequenceDetails.SkippedSequenceNumbers = skippedSequenceNumbers;
            return transportSequenceDetails;
        }

        private void HandleEndOfFile(int feedId, string deviceId, int tId)
        {
            
            string ffmpegCacheKey = string.Format(CacheConstants.CacheKeyFormat, CacheConstants.CacheKeyFormatForFfmpegIntialise, tId, deviceId, feedId);
            string eof_cacheKey = CacheConstants.FrameRendererEOF;
            Process proc;

            lock (ffmpegLock)
            {
                proc = (Process)cache[ffmpegCacheKey];



                if (proc != null)
                {
                    
                    proc.Kill();
                    proc.WaitForExit();
                    proc.Dispose();
                    
                    proc = null;
                }
                cache.Remove(ffmpegCacheKey);
                cache.Remove(eof_cacheKey);

                
                Helper.UpdateCompletedFeedRequestDetails(feedId, ProcessingStatus.FrameRendererCompletedStatus);
                

            }
            DeviceDetails deviceDetails = ConfigHelper.SetDeviceDetails(tId.ToString(), deviceId, CacheConstants.FrameRendererCode);
            if(deviceDetails.BackgroundChange.ToLower() == "yes")
            {
                string intialiseFfmpegCacheKey = string.Format(CacheConstants.CacheKeyFormat, CacheConstants.CacheKeyFormatForFfmpegIntialise, tId, deviceId, feedId);
                Process ffmpegBackgroundChange;
                lock (ffmpegLock)
                {
                    ffmpegBackgroundChange = (Process)cache[intialiseFfmpegCacheKey];

                    if (ffmpegBackgroundChange == null)
                    {
                        ffmpegBackgroundChange = new Process();
                        ffmpegBackgroundChange.StartInfo.FileName = ffmpegExeFile;
                        ffmpegBackgroundChange.StartInfo.Arguments = deviceDetails.FfmpegforBackgroundChange;
                        ffmpegBackgroundChange.StartInfo.UseShellExecute = false;
                        ffmpegBackgroundChange.StartInfo.RedirectStandardInput = true;
                        ffmpegBackgroundChange.StartInfo.RedirectStandardOutput = true;
                        ffmpegBackgroundChange.Start();
                        
                        Thread.Sleep(200);
                        cache.Set(intialiseFfmpegCacheKey, ffmpegBackgroundChange, policy);
                        ffmpegBackgroundChange.WaitForExit();
                        ffmpegBackgroundChange.Kill();
                        ffmpegBackgroundChange.Dispose();
                        cache.Remove(ffmpegCacheKey);
                        cache.Remove(eof_cacheKey);
                    }
                }
            }
        }

        

        private void TransportFrame(List<TransportFrameDetails> transportFrameDetailsList, int totalFrameprocessed)
        {
            try
            {

                string feedKey = null;
                string deviceId = null;
                string ffmpegArgument = "";
                int tid = 0;
                int videoStreamingOption = -1;
                int feedId = -1;
                int totalMessageCount = -1;
                string ipAddress = "";
                int port = 0;
                
                foreach (TransportFrameDetails transportFrameDetails in transportFrameDetailsList)
                {
                    if (transportFrameDetails != null)
                    {
                        Stopwatch transportProcessStopWatch = new Stopwatch();
                        transportProcessStopWatch.Reset();
                        transportProcessStopWatch.Start();

                        feedKey = transportFrameDetails.TenantId + UnderScore + transportFrameDetails.DeviceId + UnderScore + transportFrameDetails.FeedId;
                        deviceId = transportFrameDetails.DeviceId;
                        feedId = int.Parse(transportFrameDetails.FeedId);
                        tid = transportFrameDetails.TenantId;
                        videoStreamingOption = transportFrameDetails.VideoStreamingOption;
                        ffmpegArgument = transportFrameDetails.FfmpegArguments;
                        port = transportFrameDetails.Port;
                        ipAddress = transportFrameDetails.IpAddress;
                        int frameNumber = int.Parse(transportFrameDetails.FrameNumber);
                        int lastFrameNumberSendForPredict = -1;
                        int totalFrameCount = -1;
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

                        

                        if (lastFrameNumberSendForPredict > 0 && totalFrameCount > 0 && totalFrameprocessed > 0)
                        {

                            if (frameNumber >= lastFrameNumberSendForPredict || frameNumber >= totalFrameCount || frameNumber >= totalFrameprocessed)
                            {

                                
                                lastFrameTransferDetails[feedKey] = true;
                            }

                        }

                        
                        switch (transportFrameDetails.VideoStreamingOption)
                        {

                            case 1:
                                TransportFrameWithffmpeg(transportFrameDetails);

                                break;
                            case 2:
                                break;
                            default:

                                TransportFrame(transportFrameDetails.Data, transportFrameDetails.DeviceId, transportFrameDetails.TenantId,
                                transportFrameDetails.IpAddress, transportFrameDetails.Port, transportFrameDetails.IsFirstFrame);
                                
                                break;
                        }
                        if(transportFrameDetails.Data !=null)
                        {
                            Array.Clear(transportFrameDetails.Data, 0, transportFrameDetails.Data.Length);
                            transportFrameDetails.Data = null;
                        }
                        

                        transportProcessStopWatch.Stop();


                    }
                   
                }
                if (lastFrameTransferDetails.Count > 0)
                {
                    if (feedKey != null && lastFrameTransferDetails[feedKey])
                    {

                        HandlingVideoCompletion(tid, deviceId, ffmpegArgument, feedId, ipAddress, port, videoStreamingOption);
                    }
                }


            }
            catch (Exception ex)
            {

                LogHandler.LogError(String.Format("Exception Occured while calling TransportFrame of FrameRendererProcess" +
                    " error message: {0}, exception trace {1} ", ex.Message, ex.StackTrace), LogHandler.Layer.Business, null);
                
            }
        }


        private void HandlingVideoCompletion(int tid, string deviceId, string ffmpegArgument, int feedId, string ipAddress, int port, int videoStreamingOption)
        {
            
            TransportFrameDetails transportFrameDetails = new TransportFrameDetails();
            transportFrameDetails.TenantId = tid;
            transportFrameDetails.DeviceId = deviceId;
            transportFrameDetails.FfmpegArguments = ffmpegArgument;
            transportFrameDetails.Data = ReadFromLocalFileStore();
            transportFrameDetails.IpAddress = ipAddress;
            transportFrameDetails.Port = port;
            transportFrameDetails.IsFirstFrame = false;
            transportFrameDetails.VideoStreamingOption = videoStreamingOption;
            if (transportFrameDetails.Data != null)
            {
                

                switch (transportFrameDetails.VideoStreamingOption)
                {

                    case 1:

                        for (int i = 0; i < eofCount; i++)
                        {
                            TransportFrameWithffmpeg(transportFrameDetails);
                        }

                        break;
                    default:
                        FrameDetails frameDetails = new FrameDetails();
                        frameDetails.DeviceId = deviceId;
                        frameDetails.TenantId = tid.ToString();
                        frameDetails.IsFirstFrame = false;
                        frameDetails.Objects = new List<ObjectDetails>();
                        frameDetails.FrameId = DateTime.UtcNow.Ticks.ToString();
                        frameDetails.Frame = transportFrameDetails.Data;
                        transportFrameDetails.Data = SerializeFrameDetails(frameDetails);
                        TransportFrame(transportFrameDetails.Data, transportFrameDetails.DeviceId, transportFrameDetails.TenantId,
                        transportFrameDetails.IpAddress, transportFrameDetails.Port, transportFrameDetails.IsFirstFrame);
                        break;
                }


                
            }
            
            HandleEndOfFile(feedId, deviceId, tid);
        }



        
        


       

        

        

        

       

        
        public void PostProcessFrame(QueueEntity.FrameRendererMetadata frameRendererData, DeviceDetails deviceDetails)
        {
#if DEBUG
            using (LogHandler.TraceOperations("FrameRendererProcess:PostProcessFrame", LogHandler.Layer.Business, Guid.NewGuid()))
            {


                LogHandler.LogInfo(String.Format(InfoMessages.Method_Execution_Start, "PostProcessFrame", "FrameRendererProcess"), LogHandler.Layer.Business, null);
                LogHandler.LogInfo("Executing  PostProcessFrame in FrameRenderer", LogHandler.Layer.Business);
#endif
                Stopwatch postProcessStopWatch = new Stopwatch();
                postProcessStopWatch.Reset();
                postProcessStopWatch.Start();
                List<string> deviceIdList = new List<string>();
                
                List<byte[]> dataList = new List<byte[]>();
                string deviceId = frameRendererData.Did;
                string baseUrl = deviceDetails.BaseUrl;
                predictionModel = deviceDetails.PredictionModel;
                bool enableLot = deviceDetails.EnableLots;
                string ipaddress = deviceDetails.IpAddress;
                port = deviceDetails.Port;
                penThickness = deviceDetails.PenThickness;
                maskPenColor = deviceDetails.MaskPenColor;
                noMaskPenColor = deviceDetails.NoMaskPenColor;
                boxColor = deviceDetails.BoxColor;
                int tenantId = Convert.ToInt32(frameRendererData.Tid);
                labelFontSize = deviceDetails.LabelFontSize;
                labelFontStyle = deviceDetails.LabelFontStyle;
                labelHeight = deviceDetails.LabelHeight;
                labelFontColor = deviceDetails.LabelFontColor;
                predictionClassType = deviceDetails.PredictionClassType;
                int videoStreamingOption = deviceDetails.VideoStreamingOption;
                string ffmpegArguments = deviceDetails.FfmpegArguments;
                backGroundColor=deviceDetails.BackGroundColor;
                RendererRectanglePointX = deviceDetails.RendererRectanglePointX;
                RendererRectanglePointY = deviceDetails.RendererRectanglePointY;
                RendererLabelPointX = deviceDetails.RendererLabelPointX;
                RendererLabelPointY = deviceDetails.RendererLabelPointY;
                RendererRectangleHeight = deviceDetails.RendererRectangleHeight;
                RendererPredictCartListBackgroundColor= deviceDetails.RendererPredictCartListBackgroundColor;

#if DEBUG
                LogHandler.LogDebug(String.Format("The PostProcessFrame Method of FrameRendererProcess class is getting executed with parameters : DeviceId ={0}, lotSize = {1} ,  frameId = {2} , ipAddress = {3} , port = {4} , tenantId = {5} ; ",
                    deviceId, enableLot, frameId, ipaddress, port, frameRendererData.Tid),
                  LogHandler.Layer.Business, null);
#endif
                try
                {
                    ProcessPreLoadedImage(frameRendererData, baseUrl, predictionModel, deviceDetails);

                }
                catch (Exception ex)
                {
                    LogHandler.LogError("Exception occured while calling ProcessPreLoadedImage , exception message:{0},exception trace:{1}", LogHandler.Layer.Business, ex.Message, ex.StackTrace);
                    throw ex;
                }
                postProcessStopWatch.Stop();

#if DEBUG
                LogHandler.LogInfo(String.Format(InfoMessages.Method_Execution_End, "PostProcessFrame", "FrameRendererProcess"), LogHandler.Layer.Business, null);
            }
#endif
        }

        
        
       

       

        private Process IntialiseFfmpeg(string arguments, string tId, string deviceId, int feedId)
        {
            
            string intialiseFfmpegCacheKey = string.Format(CacheConstants.CacheKeyFormat, CacheConstants.CacheKeyFormatForFfmpegIntialise, tId, deviceId, feedId);
            Process proc;
            lock (ffmpegLock)
            {
                proc = (Process)cache[intialiseFfmpegCacheKey];

                if (proc == null)
                {
                  
                    proc = new Process();
                    proc.StartInfo.FileName = ffmpegExeFile;
                    
                      proc.StartInfo.Arguments = arguments;
              
                    proc.StartInfo.UseShellExecute = false;
                    proc.StartInfo.RedirectStandardInput = true;
                    proc.StartInfo.RedirectStandardOutput = true;
                    proc.Start();

 
                    
                    Thread.Sleep(200);
                    cache.Set(intialiseFfmpegCacheKey, proc, policy);
                }
            }
            return proc;
        }
        
        private void ProcessPreLoadedImage(QueueEntity.FrameRendererMetadata frameRendererData,string baseUrl,string modelName,DeviceDetails deviceDetails) {
            try {
                #if DEBUG
                LogHandler.LogInfo(String.Format(InfoMessages.Method_Execution_Start,"ProcessPreLoadedImage","FrameRendererProcess"),LogHandler.Layer.Business,null);
                using(LogHandler.TraceOperations("FrameRendererProcess:ProcessPreLoadedImage",LogHandler.Layer.Business,Guid.NewGuid())) {
                    #endif
                    FrameDetails frameDetails=new FrameDetails();
                    List<TransportFrameDetails> transportFrameDetailsList=new List<TransportFrameDetails>();
                    string cacheKey=frameRendererData.Tid+frameRendererData.Did+frameRendererData.Fid;
                    lotStopWatch.Start();
                    int seqNumber=int.Parse(frameRendererData.SequenceNumber);
                    string feedKey=GenerateFeedKey(frameRendererData.Tid,frameRendererData.Did,frameRendererData.FeedId);
                    Dictionary<string,Stream> frameDict=(Dictionary<string,Stream>)cache[cacheKey];
                    Dictionary<string,Stream> frameDict1=(Dictionary<string,Stream>)cache[cacheKey];
                    string outputImage=deviceDetails.OutputImage.ToLower();
                    if(outputImage=="yes") {   
                        if(frameRendererData.Obase_64.Count!=0) {
                            foreach(var base64 in frameRendererData.Obase_64) {
                                byte[] imageBytes=Convert.FromBase64String(base64);
                                var ms=new MemoryStream(imageBytes,0,imageBytes.Length);
                                if(frameDict==null) {
                                    frameDict=new Dictionary<string,Stream>();
                                }
                                frameDict.Add(DateTime.UtcNow.Ticks.ToString()+ApplicationConstants.FileExtensions.jpg,ms); /* Creating fid for multi image output and adding it to the dict - sajid */
                            }
                        }
                        else if(frameRendererData.Img_url.Count!=0) {
                            foreach(var imageUrl in frameRendererData.Img_url) {
                                var uri=new Uri(imageUrl);
                                string storageBaseUrl=uri.Scheme+"://"+uri.DnsSafeHost;
                                frameId=uri.Segments[3].Replace(".jpg","");
                                var index=uri.Segments[2].IndexOf("_");
                                string tId=uri.Segments[2].Substring(0,index);
                                deviceId=uri.Segments[2].Substring(index+1,uri.Segments[2].Length-(index+1)).Replace("/","");
                                var blobImage=Helper.DownloadBlob(deviceId,frameId,tId,storageBaseUrl,ApplicationConstants.FileExtensions.jpg);
                                Stream stream=blobImage.File;
                                if(frameDict==null) {
                                    frameDict=new Dictionary<string,Stream>();
                                }
                                frameDict.Add(DateTime.UtcNow.Ticks.ToString()+ApplicationConstants.FileExtensions.jpg,stream);
                            }
                        }
                        else {
                            return;
                        }
                    }
                    if(frameDict==null) {
                        #if DEBUG
                        LogHandler.LogDebug(String.Format("FrameRendererProcess no frame detail available in cache and downloading it, frameId: {0}",frameId),
                        LogHandler.Layer.Business,null);
                        #endif
                        frameDict=DownLoadFrames(frameRendererData,deviceDetails);
                    }
                    int tenantId=Convert.ToInt32(frameRendererData.Tid);
                    if(frameDict!=null) {
                        try {
                            bool isFirstFrame=true;
                            List<string> keyList=new List<string>(frameDict.Keys);
                            keyList=keyList.OrderBy(x=>getFrameId(x)).ToList();
                            byte[] data;
                            foreach(var key in keyList) {
                                using(Stream imageStream=frameDict[key]) {
                                    if(imageStream.Length>0) {
                                        Image<Bgr,byte> emguImage;
                                        using(MemoryStream ms=new MemoryStream()) {
                                            imageStream.CopyTo(ms);
                                            byte[] imageBytes=ms.ToArray();
                                            ms.Dispose();
                                            Mat matImage=new Mat();
                                            CvInvoke.Imdecode(imageBytes,ImreadModes.Color,matImage);
                                            emguImage=matImage.ToImage<Bgr,byte>();
                                        }
                                        frameRendererData.Fid=key.Replace(".jpg","");
                                        /* frameDetails=ProcessData(emguImage,frameRendererData,modelName,
                                        deviceDetails.SharedBlobStorage,deviceDetails.KpSkeleton); */
                                        frameDetails=ProcessData(emguImage,frameRendererData,modelName,
                                        deviceDetails);
                                        if(deviceDetails.HeatMap.ToUpper()=="YES") {
                                            frameDict1=DownLoadFrames(frameRendererData,deviceDetails);
                                            using(Stream imageStream1=frameDict1[key]) {
                                                if(imageStream1.Length>0) {
                                                    /* Image<Bgr,byte> emguImage1; */
                                                    using(MemoryStream ms1=new MemoryStream()) {
                                                        imageStream1.CopyTo(ms1);
                                                        byte[] imageBytes=ms1.ToArray();
                                                        ms1.Dispose();
                                                        Mat matImage1=new Mat();
                                                        CvInvoke.Imdecode(imageBytes,ImreadModes.Color,matImage1);
                                                        emguImage=matImage1.ToImage<Bgr,byte>();
                                                        frameDetails.Frame=emguImage.ToJpegData();
                                                        /* emguImage1.Save(RenderImageFilePath+frameRendererData.Fid+ApplicationConstants.FileExtensions.jpg); */
                                                    }
                                                }
                                            }
                                        }
                                        if(frameDetails==null) {
                                            return;
                                        }
                                        Mat dest=new Mat();
                                        if(modelName.ToLower()=="templatematching") {
                                            Mat matImage2=new Mat();
                                            CvInvoke.Imdecode(frameDetails.Frame,ImreadModes.Color,matImage2);
                                            /* matImage2.Save(@"C:\Users\riya.sharma03\Downloads\outputvideos\test6.jpg"); */
                                            emguImage=matImage2.ToImage<Bgr,byte>();
                                        }
                                        frameDetails.IsFirstFrame=isFirstFrame;
                                        if(deviceDetails.VideoStreamingOption==2) {
                                            if(outputImage=="yes" && isRenderImageEnabled!=null && isRenderImageEnabled.Equals("true",
                                            StringComparison.InvariantCultureIgnoreCase)) {   
                                                if(RenderImageFilePath!=null && Directory.Exists(RenderImageFilePath)) {
                                                    emguImage.Save(RenderImageFilePath+frameRendererData.Fid+ApplicationConstants.FileExtensions.jpg);
                                                }
                                            }
                                            /* Console.WriteLine("Hitted save");
                                            Save the processed image into local file path for debug purpose */
                                            if(isFirstFrame && isRenderImageEnabled!=null && isRenderImageEnabled.Equals("true",
                                            StringComparison.InvariantCultureIgnoreCase)) {
                                                if(RenderImageFilePath!=null && Directory.Exists(RenderImageFilePath)) {
                                                    emguImage.Save(RenderImageFilePath+frameRendererData.Fid+ApplicationConstants.FileExtensions.jpg);
                                                }
                                            }
                                            /* emguImage.Dispose();
                                            emguImage=null; */
                                        }
                                        /* else if(deviceDetails.VideoStreamingOption==2 && deviceDetails.VideoFeedType=="IMAGE") {
                                        Save the processed image into local file path for debug purpose */
                                        if(isFirstFrame && isImageDebugEnabled!=null && isImageDebugEnabled.Equals("true",
                                        StringComparison.InvariantCultureIgnoreCase)) {
                                            if(debugImageFilePath!=null && Directory.Exists(debugImageFilePath)) {
                                                emguImage.Save(debugImageFilePath+frameRendererData.Fid+ApplicationConstants.FileExtensions.jpg);
                                            }
                                        }
                                        emguImage.Dispose();
                                        emguImage=null;
                                        /* } */
                                    }
                                    TransportFrameDetails transportFrameDetails;
                                    switch(deviceDetails.VideoStreamingOption)  {
                                        /* There are two VideoStreamingOptions: 0 and 1
                                        Incase of VideoStreamingOptions is 0, tcp connection is established and frames are sent along with other details for analytics purpose
                                        Incase of VideoStreamingOptions is 1, ffmpeg is intialised and only frames are sent */
                                        case 1:
                                            data=frameDetails.Frame;
                                            break;
                                        default:
                                            data=SerializeFrameDetails(frameDetails);
                                            break;
                                    }
                                    /* if(deviceDetails.VideoStreamingOption!=2) { */
                                    transportFrameDetails=new TransportFrameDetails {
                                        Data=data,
                                        DeviceId=frameRendererData.Did,
                                        TenantId=tenantId,
                                        IpAddress=deviceDetails.IpAddress,
                                        Port=deviceDetails.Port,
                                        IsFirstFrame=isFirstFrame,
                                        FfmpegArguments=deviceDetails.FfmpegArguments,
                                        Fid=frameRendererData.Fid,
                                        VideoStreamingOption=deviceDetails.VideoStreamingOption,
                                        SequenceNumber=frameRendererData.SequenceNumber,
                                        FeedId=frameRendererData.FeedId,
                                        FrameNumber=frameRendererData.FrameNumber
                                    };
                                    transportFrameDetailsList.Add(transportFrameDetails);
                                    /* } */
                                    #if DEBUG
                                    LogHandler.LogDebug("FrameRendererProcess - after pushing transportFrameDetails - frameQueue count: {0} and thread id: {1}",
                                    LogHandler.Layer.Business,frameQueue.Count,Thread.CurrentThread.ManagedThreadId);
                                    #endif
                                    isFirstFrame=false;
                                    imageStream.Dispose();
                                    frameDict.Remove(key);
                                }
                            }
                        }
                        catch(Exception ex) {
                            LogHandler.LogError("Constructing transportFrameDetailsList, error message: {0}, stack trace: {1}",
                            LogHandler.Layer.Business,ex.Message,ex.StackTrace);
                            throw ex;
                        }
                        lock(frameCountLock) {
                            try {
                                Dictionary<int,List<TransportFrameDetails>> frameMessage=null;
                                IntializeDictionaries(feedKey);
                                frameMessage=(Dictionary<int,List<TransportFrameDetails>>)frameMessageDetails[feedKey];
                                Queue seqNumberQueue=null;
                                seqNumberQueue=(Queue)sequenceNumberQueueDetails[feedKey];
                                int count=frameTransferCountDetails[feedKey];
                                count++;
                                frameTransferCountDetails[feedKey]=count;
                                if(frameMessage!=null) {
                                    /* If sequence number already exist in the frameMessage and update new frameDetails, if not add it to the message */
                                    if(!frameMessage.ContainsKey(seqNumber)) {
                                        frameMessage.Add(seqNumber,transportFrameDetailsList);
                                    }
                                    else {
                                        frameMessage[seqNumber]=transportFrameDetailsList;
                                    }
                                }
                                if(deviceDetails.VideoStreamingOption != 2)
                                {
                                    if (seqNumberQueue != null)
                                    {
                                        seqNumberQueue.Enqueue(seqNumber);
                                    }
                                }
                            }
                            catch(Exception ex) {
                                LogHandler.LogError("Exception in frameCountLock, exception message: {0}, inner message: {1}, exception trace: {2}, inner exception trace",
                                LogHandler.Layer.Business,ex.Message,ex?.InnerException?.Message,ex.StackTrace);
                                throw ex;
                            }
                        }
                    }
                    else {
                        #if DEBUG
                        LogHandler.LogDebug(String.Format("FrameRendererProcess no frame detail available to process FrameRendererProcess class, frameId: {0}",
                        frameId), LogHandler.Layer.Business,null);
                        #endif
                    }
                    #if DEBUG
                    LogHandler.LogInfo(String.Format(InfoMessages.Method_Execution_End,"ProcessPreLoadedImage",
                    "FrameRendererProcess"),LogHandler.Layer.Business,null);
                }
                #endif
            }
            catch(Exception ex) {
                if(ex.InnerException!=null) {
                    LogHandler.LogError("Exception occured in ProcessPreLoadedImage of FrameRendererProcess, "+
                    "exception message: {0}, inner message: {1}, exception trace: {2}, inner exception trace: {3}",
                    LogHandler.Layer.Business,ex.Message,ex.InnerException.Message,ex.StackTrace,ex.InnerException.StackTrace);
                }
                else {
                    LogHandler.LogError("Exception occured in ProcessPreLoadedImage of FrameRendererProcess, error message: {0}, exception trace: {1}",
                    LogHandler.Layer.Business,ex.Message,ex.StackTrace);
                }
                throw ex;
            }
        }


        private void IntializeDictionaries(string feedKey)
        {
            if (!frameMessageDetails.ContainsKey(feedKey))
                frameMessageDetails.Add(feedKey, new Dictionary<int, List<TransportFrameDetails>>());
            if (!sequenceNumberQueueDetails.ContainsKey(feedKey))
                sequenceNumberQueueDetails.Add(feedKey, new Queue());
            if (!frameTransferCountDetails.ContainsKey(feedKey))
                frameTransferCountDetails.Add(feedKey, 0);
            if (!lastFrameTransferDetails.ContainsKey(feedKey))
                lastFrameTransferDetails.Add(feedKey, false);
            if (!receivedFrameCountDetails.ContainsKey(feedKey))
                receivedFrameCountDetails.Add(feedKey, 0);
            if (!allFrameReceived.ContainsKey(feedKey))
                allFrameReceived.Add(feedKey, false);
            if (!framesNotSendForRendering.ContainsKey(feedKey))
                framesNotSendForRendering.Add(feedKey, new Dictionary<int, string>());
            if (!ClientStatus.ContainsKey(feedKey))
                ClientStatus.Add(feedKey, true);
            if (!SeqNumberAfterClientActive.ContainsKey(feedKey))
                SeqNumberAfterClientActive.Add(feedKey, 0);
        }

        


        

        

        
        public BigInteger getFrameId(string file)
        {
            file = file.Replace(".jpg", "");
            BigInteger frameId = BigInteger.Parse(file);
            return frameId;
        }

        

        
        

        

       

        private FrameDetails ProcessData(Image<Bgr,byte> image,QueueEntity.FrameRendererMetadata frameRendererData,string modelName,DeviceDetails deviceDetails) {
            /* var response=JObject.Parse(Kpskeleton);
            string[] values=Kpskeleton.Split(']');
            JsonConvert.DeserializeObject(Kpskeleton);
            foreach(var v in Kpskeleton) {
                Console.WriteLine(v);
            } */
            #if DEBUG
            LogHandler.LogDebug(String.Format("The ProcessData method of FrameRendererProcess class is getting executed with parameters: frameRendererData message={0}",
            JsonConvert.SerializeObject(frameRendererData)),LogHandler.Layer.Business,null);
            LogHandler.LogInfo(String.Format(InfoMessages.Method_Execution_Start,"ProcessData",
            "FrameRendererProcess"),LogHandler.Layer.Business,null);
            using(LogHandler.TraceOperations("FrameRendererProcess:ProcessData",LogHandler.Layer.Business,Guid.NewGuid())) {
                #endif
                FrameDetails frameDetails=null;
                try {
                    frameDetails=new FrameDetails();
                    frameDetails.Objects=new List<ObjectDetails>();
                    frameDetails.TenantId=frameRendererData.Tid;
                    frameDetails.DeviceId=frameRendererData.Did;
                    frameDetails.FrameId=frameRendererData.Fid;
                    /* frameDetails.Ad=frameRendererData.Ad; */
                    int frameWidth=image.Width;
                    int frameHeight=image.Height;
                    List<QueueEntity.Predictions> ObjectList=new List<QueueEntity.Predictions>();
                    if(frameRendererData.Fs!=null && frameRendererData.Fs.Length!=0) {
                        foreach(QueueEntity.Predictions obj in frameRendererData.Fs) {
                            ObjectDetails objectDetails=new ObjectDetails();
                            if(obj.Lb==null)  {
                                obj.Lb=obj.Pid;
                            }
                            ObjectList.Add(obj);
                            objectDetails.Class=obj.Lb;
                            #if DEBUG
                            using(LogHandler.TraceOperations("FrameRendererProcess:double.Parse",
                            LogHandler.Layer.Business,Guid.NewGuid())) {
                                #endif
                                if(obj.Cs!=null) {
                                    double score=0;
                                    double.TryParse(obj.Cs,out score);
                                    objectDetails.ConfidenceScore=score;
                                }
                                else {
                                    objectDetails.ConfidenceScore=0;
                                }
                                #if DEBUG
                            }
                            #endif
                            frameDetails.Objects.Add(objectDetails);
                        }
                        string outputImage=deviceDetails.OutputImage.ToLower();
                        if(outputImage!="yes") {
                            image=ProcessImage(ObjectList,image,frameWidth,frameHeight,modelName,
                            frameRendererData.Info,deviceDetails,frameRendererData.Ad,frameDetails);
                            /* image.Save(@"C:\Users\riya.sharma03\Downloads\outputvideos\test5.jpg"); */
                        }
                        /* image=ProcessImage(ObjectList,image,frameWidth,frameHeight,modelName,
                        frameRendererData.Class,frameRendererData.Info,deviceDetails,frameRendererData.Ad); */
                    }
                    /* Added to test heatmap
                    else if(!string.IsNullOrEmpty(frameRendererData.Ad)) {
                        image=ProcessImage(ObjectList,image,frameWidth,frameHeight,modelName,
                        frameRendererData.Info,deviceDetails,frameRendererData.Ad);
                    } */
                    else if(!string.IsNullOrEmpty(frameRendererData.Class)) {
                        ObjectDetails objectDetails=new ObjectDetails();
                        objectDetails.Class=frameRendererData.Class;
                        frameDetails.Objects.Add(objectDetails);
                    }
                    #if DEBUG
                    LogHandler.LogInfo(String.Format(InfoMessages.Method_Execution_End,"ProcessData",
                    "FrameRendererProcess"),LogHandler.Layer.Business,null);
                    #endif
                }
                catch(Exception ex) {
                    LogHandler.LogError("Exception in ProcessData method of FrameRenderer process, message: {0}, stack trace: {1}",
                    LogHandler.Layer.Business,ex.Message,ex.StackTrace);
                    throw ex;
                    /* Console.WriteLine(ex.Message); */
                }
                /* image.Save("bef.jpg"); */
                frameDetails.Frame=image.ToJpegData();
                /* image.Save("tafter2.jpg"); */
                return frameDetails;
                #if DEBUG
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
       


        

        

        
        
       

        
        

        



        

        

        private Image<Bgr,byte> ProcessImage(List<QueueEntity.Predictions> objectList,Image<Bgr,byte> image,int frameWidth,
        int frameHeight,string modelName,string info,DeviceDetails deviceDetails,string Ad,FrameDetails frameDetails) {
            #if DEBUG
            LogHandler.LogDebug(String.Format("The ProcessImage method of FrameRendererProcess class is getting executed with parameters: face message={0}",
            JsonConvert.SerializeObject(objectList)),LogHandler.Layer.Business,null);
            LogHandler.LogInfo(String.Format(InfoMessages.Method_Execution_Start,"ProcessImage",
            "FrameRendererProcess"),LogHandler.Layer.Business,null);
            using(LogHandler.TraceOperations("FrameRendererProcess:ProcessImage",LogHandler.Layer.Business,Guid.NewGuid())) {
                #endif
                /* BE.BoundingBoxData box=new BE.BoundingBoxData();
                box=face.DM; */
                Rectangle rectangle=new Rectangle();
                Rectangle rectangle1=new Rectangle();
                MCvScalar color=new MCvScalar();
                #region Added background color from Device.json
                Color clBgColor=Color.FromName(backGroundColor);
                Color clBgColorPredictCartList=Color.FromName(RendererPredictCartListBackgroundColor);
                #endregion
                string label="";

                 
                if (deviceDetails.SharedBlobStorage && deviceDetails.PredictCart.ToLower()=="yes")
                {
                    
                    if (Ad != null)
                    {
                        #region Added the code as per new IVA schema starts 
                        JObject jObjects = JObject.Parse(Ad);
                        var outCome = jObjects["Outcome"].ToString();
                        var Obj = jObjects["Obj"].ToString();
                        #endregion
                        rectangle = new Rectangle(10, 10, 700, 35);
                        
                        color = new MCvScalar(clBgColor.R, clBgColor.G, clBgColor.B);
                        CvInvoke.Rectangle(image, rectangle, color, penThickness);
                        CvInvoke.Rectangle(image, rectangle, color, -1);
                        rectangle = new Rectangle(10, 50, 200, 115);
                        
                        color = new MCvScalar(clBgColorPredictCartList.R,clBgColorPredictCartList.G,clBgColorPredictCartList.B);
                        CvInvoke.Rectangle(image, rectangle, color, penThickness);
                        CvInvoke.Rectangle(image, rectangle, color, -1);
                        int width = 30;
                        Point point1 = new Point(10, width);
                        color = new MCvScalar(255, 255, 255);
                        
                        CvInvoke.PutText(image, outCome, point1, FontFace.HersheySimplex, deviceDetails.RendererFontScale, color, deviceDetails.RendererFontThickness);

                        color = new MCvScalar(0, 0, 0);
                        
                        JObject jObjects1 = JObject.Parse(Obj);

                        foreach (var jObject in jObjects1)
                        {
                            string message = jObject.Key + "  :  " + jObject.Value;
                            width = width + 30;
                            Point point2 = new Point(10, width);
                            
                            CvInvoke.PutText(image, message, point2, FontFace.HersheySimplex, deviceDetails.RendererFontScale, color, deviceDetails.RendererFontThickness);
                        }
                    }
                }
                if (deviceDetails.Mplug.ToLower() == "yes")
                {
                    
                    rectangle = new Rectangle(RendererRectanglePointX, RendererRectanglePointY, frameWidth, RendererRectangleHeight);
                    
                    color = new MCvScalar(clBgColor.R, clBgColor.G, clBgColor.B);
                    CvInvoke.Rectangle(image, rectangle, color, penThickness);
                    CvInvoke.Rectangle(image, rectangle, color, -1);
                    
                    Point point = new Point(RendererLabelPointX, RendererLabelPointY);
                    
                    Color color3 = Color.FromName(labelFontColor);
                    color = new MCvScalar(color3.B, color3.G, color3.R);
                    CvInvoke.PutText(image, Ad, point, FontFace.HersheySimplex, deviceDetails.RendererFontScale, color);

                }
                else if (deviceDetails.BackgroundChange.ToLower() == "yes")
                {
                    
                }
                else if (deviceDetails.SegmentRendering.ToLower() == "yes")
                {
                    Bgr bpcColor = new Bgr();
                    Bgr tpcColor = new Bgr();
                    Dictionary<string, string> segmentColors = JsonConvert.DeserializeObject<Dictionary<string, string>>(deviceDetails.SegmentColors);
                    Dictionary<string, string> labelColors = JsonConvert.DeserializeObject<Dictionary<string, string>>(deviceDetails.LabelColor);
                    for (int obj = 0; obj < objectList.Count; obj++)
                    {
                        List<List<float>> bpc = objectList[obj].Bpc;
                        List<List<float>> tpc = objectList[obj].Tpc;
                        int h = image.Height;
                        int w = image.Width;

                        if (deviceDetails.PanopticSegmentation == "yes")
                        {
                            for (int i = 0; i < tpc.Count; i++)
                                {
                                    int x = (int)Math.Round(tpc[i][0] * w);
                                    int y = (int)Math.Round(tpc[i][1] * h);
                                    string pickColor = segmentColors[(obj+1).ToString()];
                                    Color objectColor = System.Drawing.Color.FromName(pickColor);
                                    tpcColor = new Bgr(objectColor);
                                    image[y, x] = tpcColor;
                                }
                        }
                        else
                        {
                            for (int i = 0; i < tpc.Count; i++)
                            {
                                int x = (int)Math.Round(tpc[i][0] * w);
                                int y = (int)Math.Round(tpc[i][1] * h);
                                
                                string pickColor = labelColors[objectList[obj].Lb];
                                Color objectColor = System.Drawing.Color.FromName(pickColor);
                                tpcColor = new Bgr(objectColor);
                                image[y, x] = tpcColor;
                            }
                        }
                        for (int i = 0; i < bpc.Count; i++)
                        {
                            int x = (int)Math.Round(bpc[i][0] * w);
                            int y = (int)Math.Round(bpc[i][1] * h);
                            bpcColor = image[y, x];
                            bpcColor.Red = 255;
                            image[y, x] = bpcColor;
                        }
                    }
                }
                
                else if(deviceDetails.SpeedDetection.ToLower() == "yes")
                {
                    MCvScalar lbColor = new Bgr(255, 255, 255).MCvScalar;
                    
                    double fontScale = 1;
                    int thickness = 3;

                    
                    Point position = new Point(10, 30);

                    
                    CvInvoke.PutText(image, objectList[0].Lb + "mph", position, FontFace.HersheySimplex, fontScale, lbColor, thickness);
                }
                else if (deviceDetails.PosePointRendering.ToLower() == "yes")
                {
                    if (objectList != null)
                    {
                        int objectListCount = objectList.Count;
                        for (int i = 0; i < objectListCount; i++)
                        {
                            var keypoints = objectList[i].Kp.Count;
                            for (int j = 1; j < keypoints; j++)
                            {
                                var point1 = Convert.ToInt32(objectList[i].Kp[j][0] * image.Width);
                                var point2 = Convert.ToInt32(objectList[i].Kp[j][1] * image.Height);
                                CvInvoke.Circle(image, new Point(point1, point2), 4, new MCvScalar(0, 0, 255), -1);
                            }
                        }

                        for (int i = 0; i < objectListCount; i++)
                        {
                            var keypoints = objectList[i].Kp.Count;
                            for (int j = 1; j < keypoints; j++)
                            {
                                for (int z = 1; z < deviceDetails.KpSkeleton.Count; z++)
                                {
                                    var Kskeletonpoint1 = deviceDetails.KpSkeleton[z][0];
                                    var Kskeletonpoint2 = deviceDetails.KpSkeleton[z][1];
                                    var kppoint1 = Convert.ToInt32(objectList[i].Kp[Kskeletonpoint1][0] * image.Width);
                                    var kppoint2 = Convert.ToInt32(objectList[i].Kp[Kskeletonpoint1][1] * image.Height);
                                    var kppoint3 = Convert.ToInt32(objectList[i].Kp[Kskeletonpoint2][0] * image.Width);
                                    var kppoint4 = Convert.ToInt32(objectList[i].Kp[Kskeletonpoint2][1] * image.Height);
                                    CvInvoke.Line(image, new Point(kppoint1, kppoint2), new Point(kppoint3, kppoint4), new MCvScalar(0, 255, 0), 2);

                                }
                            }
                        }
                    }
                }
                else if (deviceDetails.ClassificationRendering.ToLower() == "yes")
                {
                    
                    if (objectList != null)
                    {
                       
                      rectangle = new Rectangle(RendererRectanglePointX,RendererRectanglePointY, frameWidth, RendererRectangleHeight);
                        color = new MCvScalar(clBgColor.B, clBgColor.G, clBgColor.R);
                        
                        CvInvoke.Rectangle(image, rectangle, color, -1);
                       
                        Color color3 = Color.FromName(labelFontColor);
                        color = new MCvScalar(color3.B, color3.G, color3.R);
                        int x = RendererLabelPointX;
                        int y = RendererLabelPointY;                        
                        double cs;
                        for (int i = 0; i < objectList.Count; i++)
                        {
                            if (objectList[i].Cs != "")
                            {
                                cs = Convert.ToDouble(objectList[i].Cs);
                                cs = Math.Round(cs, 2);
                                y += 10;
                                Point point = new Point(x, y);
                                CvInvoke.PutText(image, objectList[i].Lb + ", " + cs, point, FontFace.HersheySimplex, deviceDetails.RendererFontScale, color,deviceDetails.RendererFontThickness);
                            }
                            else
                            {
                                y += 10;
                                Point point = new Point(x, y);
                                CvInvoke.PutText(image, objectList[i].Lb, point, FontFace.HersheySimplex, deviceDetails.RendererFontScale, color, deviceDetails.RendererFontThickness);
                            }
                        }
                    }
                }
             
             else if(deviceDetails.Tracking.ToLower() == "yes")
                {
                     Color color2 = new Color();
                  
                    for (var i = 0; i < objectList.Count; i++)
                    {
                        QueueEntity.Predictions face = objectList[i];
                        QueueEntity.BoundingBox box = face.Dm;
                        if (face.Uid != null)
                        {
                            
                            label = face.Uid;
                        }
                        else
                        {
                            label = face.Pid.ToLower();
                        }
                        switch (label.Contains("mask"))
                        {
                            case true:
                                if (label.StartsWith("mask"))
                                {
                                    color2 = Color.FromName(maskPenColor);
                                    if (label.Contains("2"))
                                    {
                                        /*
                                        LogHandler.CollectPerformanceMetric(ApplicationConstants.FRPerfMonCategories.CategoryName,
                                        ApplicationConstants.FRPerfMonCounters.Mask2ObjectDetected,
                                        counterInstanceName,1,false,false);
                                        */
                                    }
                                    else if (label.Contains("1"))
                                    {
                                        /*
                                        LogHandler.CollectPerformanceMetric(ApplicationConstants.FRPerfMonCategories.CategoryName, 
                                        ApplicationConstants.FRPerfMonCounters.Mask1ObjectDetected,
                                        counterInstanceName,1,false,false);
                                        */
                                    }
                                    label = FrameRendererKey.maskLabel;
                                }
                                else if (label.StartsWith("no"))
                                {
                                    color2 = Color.FromName(noMaskPenColor);
                                    label = FrameRendererKey.noMaskLabel;
                                    /*
                                    LogHandler.CollectPerformanceMetric(ApplicationConstants.FRPerfMonCategories.CategoryName, 
                                    ApplicationConstants.FRPerfMonCounters.NoMaskObjectDetected,
                                    counterInstanceName,1,false,false);
                                    */
                                }
                                color = new MCvScalar(color2.B, color2.G, color2.R);
                                break;
                            case false:
                                JObject colorJson = JObject.Parse(boxColor);
                                string pencolor;
                                if (colorJson[label] != null)
                                {
                                    pencolor = colorJson[label].ToString();
                                    color2 = Color.FromName(pencolor);
                                }
                                else if (label != null && int.TryParse(label, out int value))
                                {
                                    try
                                    {
                                        pencolor = colornames[Convert.ToInt32(label)].ToString();
                                        color2 = Color.FromName(pencolor);
                                    }
                                    catch(Exception ex)
                                    {
                                        
                                        LogHandler.LogError("Error in rendering, Exception : {0}\nStackTrace : {1}", LogHandler.Layer.Business, ex.Message, ex.StackTrace);
                                    }
                                }
                                else
                                {
                                    pencolor = string.Empty;
                                    if (face.Np != null)
                                    {
                                        if (face.Np.ToLower() == "yes")
                                        {
                                            pencolor = colorJson["new_person"].ToString();
                                            color2 = Color.FromName(pencolor);
                                        }
                                        else
                                        {
                                            pencolor = colorJson["default"].ToString();
                                            color2 = Color.FromName(pencolor);
                                        }
                                    }
                                    else
                                    {
                                        pencolor = colorJson["default"].ToString();
                                        color2 = Color.FromName(pencolor);
                                    }
                                }
                                color = new MCvScalar(color2.B, color2.G, color2.R);
                                string counterName;
                                if (predictionClassType == "dynamic")
                                {
                                    counterName = string.Format(ApplicationConstants.FRPerfMonCounters.CounterTemplate,
                                    predictionModel);
                                }
                                else
                                {
                                    counterName = string.Format(ApplicationConstants.FRPerfMonCounters.CounterTemplate,
                                    label);
                                }
                                /*
                                LogHandler.CollectPerformanceMetric(ApplicationConstants.FRPerfMonCategories.CategoryName, 
                                counterName,counterInstanceName,1,false,false);
                                */
                                break;
                        }
                        /*
                        LogHandler.CollectPerformanceMetric(ApplicationConstants.FRPerfMonCategories.CategoryName, 
                        ApplicationConstants.FRPerfMonCounters.TotalObjectDetected,
                        counterInstanceName,1,false,false);
                        */
                        if (box != null)
                        {
                            int x = Convert.ToInt32(Math.Round(float.Parse(box.X) * image.Width));
                            int y = Convert.ToInt32(Math.Round(float.Parse(box.Y) * image.Height));
                            int w = Convert.ToInt32(Math.Round(float.Parse(box.W) * image.Width));
                            int h = Convert.ToInt32(Math.Round(float.Parse(box.H) * image.Height));
#if DEBUG
                            LogHandler.LogInfo(String.Format("v0:{0} v1:{1} v2:{2}", color.V0,
                            color.V1, color.V2), LogHandler.Layer.Business, null);
#endif
                            rectangle = new Rectangle(x, y, w, h);
                            CvInvoke.Rectangle(image, rectangle, color, penThickness);
                            /*
                            image.Data.SetValue((byte)105,new int[]{0,0,0});
                            image.Data.SetValue((byte)105,new int[]{0,0,1});
                            */
                            rectangle = new Rectangle(x, y - labelHeight, w, labelHeight);
                            CvInvoke.Rectangle(image, rectangle, color, penThickness);
                            CvInvoke.Rectangle(image, rectangle, color, -1);
                            Point point3 = new Point(x, y - labelHeight + 15);
                            Color color3 = Color.FromName(labelFontColor);
                            color = new MCvScalar(color3.B, color3.G, color3.R);
                            CvInvoke.PutText(image, label, point3, FontFace.HersheySimplex, deviceDetails.RendererFontScale, color, deviceDetails.RendererFontThickness);
                            /*
                            image.Data.SetValue((byte)35,new int[]{0,0,0});
                            image.Data.SetValue((byte)35,new int[]{0,0,2});
                            */
                        }

                    }
                }
              else if (deviceDetails.CrowdCounting.ToLower() == "yes")
                {                  
                    Color color2 = new Color();
                    
                    QueueEntity.Predictions face = objectList[0];
                    if (face.Lb != null)
                    {
                        
                        label = face.Lb + ":" + face.Info;
                    }
#if DEBUG
                    LogHandler.LogInfo(String.Format("v0:{0} v1:{1} v2:{2}", color.V0,
                        color.V1, color.V2), LogHandler.Layer.Business, null);
#endif
                    
                    Point point3 = new Point(RendererLabelPointX,RendererLabelPointY);
                    Color color3 = Color.FromName(labelFontColor);
                    color = new MCvScalar(color3.B, color3.G, color3.R);
                    CvInvoke.PutText(image, label, point3, FontFace.HersheySimplex, deviceDetails.RendererFontScale, color, deviceDetails.RendererFontThickness);
            
                    for (var z = 0; z < face.Tpc.Count; z++)
                    {
                        var point1 = Convert.ToInt32(face.Tpc[z][0] * image.Width);
                        var point2 = Convert.ToInt32(face.Tpc[z][1] * image.Height);
                       CvInvoke.Circle(image, new Point(point1, point2), 4, new MCvScalar(0, 0, 255), -1);
                      
                    }
                    string ext = new ImageFormatConverter().ConvertToString(image.ToJpegData());
                }
                else if (deviceDetails.HeatMap.ToLower() == "yes")
                {
                    try
                    {
                        QueueEntity.Predictions face = objectList[0];
#if DEBUG
                        LogHandler.LogInfo(String.Format("v0:{0} v1:{1} v2:{2}", color.V0,
                            color.V1, color.V2), LogHandler.Layer.Business, null);
#endif
                        var x = new int[face.Tpc.Count];
                        var y = new int[face.Tpc.Count];
                        for (var z = 0; z < face.Tpc.Count; z++)
                        {
                            var point1 = Convert.ToInt32(face.Tpc[z][0] * image.Width);
                            var point2 = Convert.ToInt32(face.Tpc[z][1] * image.Height);
                            x[z] = point1;
                            y[z] = point2;
                        }
                        string base64_image = "";

                        using (MemoryStream MyMemoryStream = new MemoryStream())
                        {
                            Image MySystemImage = image.ToBitmap();
                            MySystemImage.Save(MyMemoryStream, ImageFormat.Jpeg);
                            
                            base64_image = Convert.ToBase64String(MyMemoryStream.ToArray());
                            MyMemoryStream.Dispose();
                        }
                        SE.Message.CrowdCounting reqMsg = new SE.Message.CrowdCounting()
                        {
                            x = x,
                            y = y,
                            Base_64 = base64_image
                        };
                        PythonNet pNet = new PythonNet();
                        pNet = PythonNet.GetInstance;
                        var val = "";
                       
                        val = "";

                        string base64_return_image = "";
                            base64_return_image=val.ToString();
                        byte[] bytes = Convert.FromBase64String(base64_return_image);

                        System.IO.Stream imageStream = new MemoryStream();
                        using (MemoryStream ms = new MemoryStream())
                        {
                            imageStream.CopyTo(ms);
                            byte[] imageBytes = bytes.ToArray();

                            FGH.FrameGrabberHelper.CompressImage(imageBytes, ms, 100).CopyTo(ms);
                            FGH.FrameGrabberHelper.UploadToBlob(ms, frameDetails.FrameId + ApplicationConstants.FileExtensions.jpg);
                            ms.Dispose();
                        }

                    }
                    catch(Exception ex)
                    {
                        throw ex;
                    }
                    



                }
              
                else {
                    Color color2=new Color();
                    for(var i=0;i<objectList.Count;i++) {
                        QueueEntity.Predictions face=objectList[i];
                        QueueEntity.BoundingBox box=face.Dm;
                        if(face.Lb!=null) {
                            label=face.Lb.ToLower();                            
                        }
                        else {
                            label=face.Pid.ToLower();
                        }
                        switch(label.Contains("mask")) {
                            case true:
                                if(label.StartsWith("mask")) {
                                    color2=Color.FromName(maskPenColor);
                                    if(label.Contains("2")) {
                                        /* LogHandler.CollectPerformanceMetric(ApplicationConstants.FRPerfMonCategories.CategoryName,
                                        ApplicationConstants.FRPerfMonCounters.Mask2ObjectDetected,
                                        counterInstanceName,1,false,false) */
                                    }
                                    else if(label.Contains("1")) {
                                        /* LogHandler.CollectPerformanceMetric(ApplicationConstants.FRPerfMonCategories.CategoryName, 
                                        ApplicationConstants.FRPerfMonCounters.Mask1ObjectDetected,
                                        counterInstanceName,1,false,false); */
                                    }
                                    label=FrameRendererKey.maskLabel;
                                }
                                else if(label.StartsWith("no")) {
                                    color2=Color.FromName(noMaskPenColor);
                                    label=FrameRendererKey.noMaskLabel;
                                    /* LogHandler.CollectPerformanceMetric(ApplicationConstants.FRPerfMonCategories.CategoryName, 
                                    ApplicationConstants.FRPerfMonCounters.NoMaskObjectDetected,
                                    counterInstanceName,1,false,false); */
                                }
                                color=new MCvScalar(color2.B,color2.G,color2.R);
                                break;
                            case false:
                                JObject colorJson=JObject.Parse(boxColor);
                                string pencolor;
                                if(colorJson[label]!=null) {
                                    pencolor=colorJson[label].ToString();
                                    color2=Color.FromName(pencolor);
                                }
                                else {
                                    pencolor=string.Empty;
                                    if(face.Np!=null) {
                                        if(face.Np.ToLower()=="yes") {
                                            pencolor=colorJson["new_person"].ToString();
                                            color2=Color.FromName(pencolor);
                                        }
                                        else {
                                            pencolor=colorJson["default"].ToString();
                                            color2=Color.FromName(pencolor);
                                        }
                                    }
                                    else {
                                        pencolor=colorJson["default"].ToString();
                                        color2=Color.FromName(pencolor);
                                    }
                                }
                                color=new MCvScalar(color2.B,color2.G,color2.R);
                                string counterName;
                                if(predictionClassType=="dynamic") {
                                    counterName=string.Format(ApplicationConstants.FRPerfMonCounters.CounterTemplate,
                                    predictionModel);
                                }
                                else {
                                    counterName=string.Format(ApplicationConstants.FRPerfMonCounters.CounterTemplate,
                                    label);
                                }
                               
                                break;
                        }
                       
                        if(box!=null) {
                            int x=Convert.ToInt32(Math.Round(float.Parse(box.X)*image.Width));
                            int y=Convert.ToInt32(Math.Round(float.Parse(box.Y)*image.Height));
                            int w=Convert.ToInt32(Math.Round(float.Parse(box.W)*image.Width));
                            int h=Convert.ToInt32(Math.Round(float.Parse(box.H)*image.Height));
                            #if DEBUG
                            LogHandler.LogInfo(String.Format("v0:{0} v1:{1} v2:{2}",color.V0,
                            color.V1,color.V2),LogHandler.Layer.Business,null);
                            #endif
                            rectangle=new Rectangle(x,y,w,h);
                           
                            Bgr background=new Bgr(0,0,0);
                            Size size1=image.Size;
                            if(modelName.ToLower()=="templatematching") {
                                double angle = Convert.ToDouble(face.Info);
                                image =image.Rotate(angle,background,false);
                                
                            }
                            CvInvoke.Rectangle(image,rectangle,color,penThickness);
                          
                            rectangle=new Rectangle(x,y-labelHeight,w,labelHeight);
                            CvInvoke.Rectangle(image,rectangle,color,penThickness);
                            CvInvoke.Rectangle(image,rectangle,color,-1);
                            Point point3=new Point(x,y-labelHeight+12);
                            Color color3=Color.FromName(labelFontColor);
                            color=new MCvScalar(color3.B,color3.G,color3.R);
                            CvInvoke.PutText(image,label,point3,FontFace.HersheySimplex,deviceDetails.RendererFontScale,color,deviceDetails.RendererFontThickness);
                            if(modelName.ToLower()=="templatematching") {
                                double angle = Convert.ToDouble(face.Info);
                                image =image.Rotate(-angle,background,false);
                               
                                Size size2=image.Size;
                                int x1=(size2.Width-size1.Width)/2;
                                int y1=(size2.Height-size1.Height)/2;
                                Rectangle roi=new Rectangle(x1,y1,size1.Width,size1.Height);
                                image.ROI=roi;
                              
                            }
                           
                        }
                    }
                }
                #if DEBUG
                LogHandler.LogInfo(String.Format(InfoMessages.Method_Execution_End,"ProcessImage",
                "FrameRendererProcess"),LogHandler.Layer.Business,null);
                #endif
                return image;
                #if DEBUG
            }
            #endif
        }






        private byte[] SerializeFrameDetails(FrameDetails frameDetails)
        {
#if DEBUG
            LogHandler.LogDebug(String.Format("The SerializeFrameDetails Method of FrameRendererProcess class is getting executed with parameters : frameDetails message={0}; ", JsonConvert.SerializeObject(frameDetails)),
           LogHandler.Layer.Business, null);
#endif
            byte[] result = null;
            
            string json = JsonConvert.SerializeObject(frameDetails);
            
            Byte[] data = System.Text.Encoding.ASCII.GetBytes(json);

          
            int byteSize = data.Length;
            int n = 10 - byteSize.ToString().Length;

            string byteSizeString = byteSize.ToString().PadLeft(10, '0');
            byte[] sizeBytes = Encoding.ASCII.GetBytes(byteSizeString);
            result = JoinArrays(sizeBytes, data);
            return result;

        }

        private byte[] JoinArrays(byte[] totalData, byte[] tempData)
        {
            byte[] outputBytes = new byte[totalData.Length + tempData.Length];
            Buffer.BlockCopy(totalData, 0, outputBytes, 0, totalData.Length);
            Buffer.BlockCopy(tempData, 0, outputBytes, totalData.Length, tempData.Length);
            return outputBytes;
        }

        public bool DoesNamedDataSlotsExist(string name)
        {
            lock (tlsSlots)
            {
                return tlsSlots.ContainsKey(name);
            }


        }

        public LocalDataStoreSlot AllocateNamedDataSlot(string name)
        {
            lock (tlsSlots)
            {
                LocalDataStoreSlot slot = null;
                if (tlsSlots.TryGetValue(name, out slot))
                    return slot;


                slot = Thread.GetNamedDataSlot(name);
                tlsSlots[name] = slot;
                return slot;
            }
        }

        public ClientConnectHost StartClient(string ipAddress, int port)
        {
            return ClientConnect(ipAddress, port);
        }

        private ClientConnectHost ClientConnect(string Host, int Port)
        {
            clientConnect = new ClientConnectHost(Host, Port);

            clientConnect.RunClientThread();

            return clientConnect;

        }

        private Dictionary<string, Stream> DownLoadFrames(QueueEntity.FrameRendererMetadata frameRendererData, DeviceDetails deviceDetails)
        {
            Dictionary<string, Stream> frameDict = null;

            try
            {

                bool deleteFramesFromBlob = deviceDetails.DeleteFramesFromBlob;

#if DEBUG
                LogHandler.LogInfo(String.Format(InfoMessages.Method_Execution_Start, "DownLoadFrames", "FrameRendererProcess"), LogHandler.Layer.Business, null);
                using (LogHandler.TraceOperations("FrameRendererProcess:DownLoadFrames", LogHandler.Layer.Business, Guid.NewGuid()))
                {
                    LogHandler.LogDebug("FrameRendererProcess DownLoadFrames and downLoadZip: {0}", LogHandler.Layer.Business, deviceDetails.DownLoadLot);
#endif
                    Workflow workflow;

                    if (deviceDetails.DownLoadLot)
                    {
                        workflow = DownLoadLot(frameRendererData, deviceDetails);
                        if (workflow != null)
                        {
                            frameDict = UnZipToMemory(workflow.File);
                            if (deleteFramesFromBlob)
                            {
                                Helper.DeleteBlob(frameRendererData.Did, frameRendererData.Fid, frameRendererData.Tid, deviceDetails.BaseUrl, ApplicationConstants.FileExtensions.zip);
                            }

                        }
                        else
                        {
#if DEBUG
                            LogHandler.LogDebug("FrameRendererProcess DownLoadFrames no zip to download for {0}. " +
                            "The document is for device Id {1} for company with Id {2} ", LogHandler.Layer.Business,
                            frameRendererData.Fid, frameRendererData.Did, frameRendererData.Tid);
#endif
                        }

                    }
                    else if (TaskRouteDS.IsMemoryDoc() && deviceDetails.DisplayAllFrames)
                    {
                        List<string> fileNameList = new List<string>();
                        fileNameList.Add(frameRendererData.Fid);
                        if (deviceDetails.FrameToPredict > 1)
                        {
                            fileNameList = frameRendererData.Fids;
                        }

                        frameDict = DownloadAllImages(frameRendererData, fileNameList, deviceDetails, deleteFramesFromBlob);
                    }

                    else
                    {
                        workflow = Helper.DownloadBlob(frameRendererData.Did, frameRendererData.Fid, frameRendererData.Tid, deviceDetails.BaseUrl, ApplicationConstants.FileExtensions.jpg);
                        if (workflow != null)
                        {
                            Stream imageStream = workflow.File;
                            if (imageStream.Length > 0)
                            {
                                frameDict = new Dictionary<string, Stream>();
                                string imageName = frameRendererData.Fid + ApplicationConstants.FileExtensions.jpg;
                                if (!frameDict.ContainsKey(imageName))
                                {
                                    frameDict.Add(imageName, imageStream);
                                }
                            }
                            if (deleteFramesFromBlob)
                            {
                                Helper.DeleteBlob(frameRendererData.Did, frameRendererData.Fid, frameRendererData.Tid, deviceDetails.BaseUrl, ApplicationConstants.FileExtensions.jpg);
                            }
                        }
                        else
                        {
#if DEBUG
                            LogHandler.LogDebug("FrameRendererProcess DownLoadFrames no frame to download for {0}. " +
                            "The document is for device Id {1} for company with Id {2} ", LogHandler.Layer.Business,
                            frameRendererData.Fid, frameRendererData.Did, frameRendererData.Tid);
#endif
                        }
                    }
#if DEBUG
                    LogHandler.LogInfo(String.Format(InfoMessages.Method_Execution_End, "DownLoadFrames", "FrameRendererProcess"), LogHandler.Layer.Business, null);

                }
#endif
            }
            catch (Exception ex)
            {
                LogHandler.LogError("Exception in DownLoadFrames, exception message {0}, strace : {1} ", LogHandler.Layer.Business, ex.Message, ex.StackTrace);
                throw ex;
            }
            return frameDict;
        }
        private Dictionary<string, Stream> DownloadAllImages(QueueEntity.FrameRendererMetadata frameRendererData, List<string> fileNameList, DeviceDetails deviceDetails, bool deleteFramesFromBlob)
        {
            Dictionary<string, Stream> frameDict = new Dictionary<string, Stream>();
            int messageStucktime = deviceDetails.FrameSequencingMessageStuckDuration;
            int messageRetry = deviceDetails.FrameSequencingMessageRetry;
            foreach (var fileName in fileNameList)
            {
                var workflow = Helper.DownloadBlob(frameRendererData.Did, fileName, frameRendererData.Tid, deviceDetails.BaseUrl, ApplicationConstants.FileExtensions.jpg);
                if (workflow == null)
                {
                    for (int i = 0; i <= messageRetry; i++)
                    {
                        Thread.Sleep(messageStucktime);
                        workflow = Helper.DownloadBlob(frameRendererData.Did, fileName, frameRendererData.Tid, deviceDetails.BaseUrl, ApplicationConstants.FileExtensions.jpg);
                        if (workflow != null)
                        {
                            break;
                        }
                    }
                }
                if (workflow != null)
                {
                    Stream imageStream = workflow.File;
                    if (imageStream.Length > 0)
                    {

                        string fileNameWithExtension = fileName + ApplicationConstants.FileExtensions.jpg;
                        if (!frameDict.ContainsKey(fileNameWithExtension))
                        {
                            frameDict.Add(fileNameWithExtension, imageStream);
                        }
                    }
                    if (deleteFramesFromBlob)
                    {
                        Helper.DeleteBlob(frameRendererData.Did, fileName, frameRendererData.Tid, deviceDetails.BaseUrl, ApplicationConstants.FileExtensions.jpg);
                    }
                }
                else
                {
#if DEBUG
                    LogHandler.LogDebug("FrameRendererProcess DownLoadFrames no frame to download for {0}. " +
                    "The document is for device Id {1} for company with Id {2} ", LogHandler.Layer.Business,
                    fileName, frameRendererData.Did, frameRendererData.Tid);
#endif
                }
            }
            return frameDict;
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
                    LogHandler.LogError("Exception occured in FrameRendererProcess HandleEventMessage {0} , Exception {1}",
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
                string feedKey = GenerateFeedKey(frameInformation.TID, frameInformation.DID, frameInformation.FeedId);
                IntializeDictionaries(feedKey);
                
                DeviceDetails deviceDetails = ConfigHelper.SetDeviceDetails(frameInformation.TID, frameInformation.DID, CacheConstants.FrameRendererCode);
                cleanUpStreamingFolder(deviceDetails.StreamingPath);
             
   
               
                    Task.Run(() =>
                    {
                        TriggerTransferFrame(frameInformation);
                    });
                

            }


        }

        private static string GenerateFeedKey(string tid, string did, string feedId)
        {
            return tid + UnderScore + did + UnderScore + feedId;
        }

        private void setEndOfFrameDetails(QueueEntity.MaintenanceMetaData message)
        {
            
            if (message != null && message.Data != null)
            {
                QueueEntity.FrameInformation frameInformation = JsonConvert.DeserializeObject<QueueEntity.FrameInformation>(message.Data);
                if (frameInformation != null)
                {
                    string feedKey = GenerateFeedKey(frameInformation.TID, frameInformation.DID, frameInformation.FeedId);
                    int lastFrameNumberSendForPredict = int.Parse(frameInformation.LastFrameNumberSendForPrediction);
                    int totalFrameCount = int.Parse(frameInformation.TotalFrameCount);
                    int totalMessage = int.Parse(frameInformation.TotalMessageSendForPrediction);
                    Dictionary<int, string> framesNotSend = frameInformation.FramesNotSendForRendering;
                    
                    if (framesNotSend != null)
                    {
                        totalMessage = totalMessage - framesNotSend.Count;
                    }

                    if (!lastFrameNumberSendForPredictDetails.ContainsKey(feedKey))
                    {
                        lastFrameNumberSendForPredictDetails.Add(feedKey, lastFrameNumberSendForPredict);
                    }

                    if (!totalFrameCountDetails.ContainsKey(feedKey))
                    {
                        totalFrameCountDetails.Add(feedKey, totalFrameCount);
                    }
                    if (!totalFrameSendForPredictDetails.ContainsKey(feedKey))
                    {
                        totalFrameSendForPredictDetails.Add(feedKey, totalMessage);
                    }
                 

                    if (framesNotSend != null)
                    {
                        if (framesNotSendForRendering.ContainsKey(feedKey))
                        {
                            Dictionary<int, string> framesNotSendForClientViewer =
                                (Dictionary<int, string>)framesNotSendForRendering[feedKey];

                            foreach (var details in framesNotSend)
                            {
                                if (!framesNotSendForClientViewer.ContainsKey(details.Key))
                                {
                                    framesNotSendForClientViewer.Add(details.Key, details.Value);
                                }
                            }
                        }

                    }
                }
            }
        }

        private void cleanUpStreamingFolder(string streamingPath)
        {
            System.IO.DirectoryInfo dir = new DirectoryInfo(streamingPath);
            if (Directory.Exists(streamingPath))
            {
                foreach (FileInfo fileinfo in dir.GetFiles())
                {
                    fileinfo.Delete();
                }


            }
        }

        private void cleanUpFrames(Dictionary<int, List<TransportFrameDetails>> transferFrameMessage, DeviceDetails deviceDetails)
        {
            if (transferFrameMessage != null)
            {
                bool deleteFramesFromBlob = deviceDetails.DeleteFramesFromBlob;
                bool isEnableLots = deviceDetails.EnableLots;
                String baseUrl = deviceDetails.BaseUrl;
                String fileExtenstion = String.Empty;
                if (isEnableLots)
                {
                    fileExtenstion = ApplicationConstants.FileExtensions.zip;
                }
                else
                {
                    fileExtenstion = ApplicationConstants.FileExtensions.jpg;
                }
                foreach (var details in transferFrameMessage)
                {
                    List<TransportFrameDetails> frameDetailsList = details.Value;
                    if (frameDetailsList != null)
                    {
                        foreach (TransportFrameDetails frameDetail in frameDetailsList)
                        {
                            if (deleteFramesFromBlob)
                            {
                                Helper.DeleteBlob(frameDetail.DeviceId, frameDetail.Fid, frameDetail.TenantId.ToString(), baseUrl, fileExtenstion);
                            }
                        }
                    }
                }
                transferFrameMessage.Clear();
            }
        }

       
        private Workflow DownLoadLot(QueueEntity.FrameRendererMetadata frameRendererData, DeviceDetails deviceDetails)
        {
            Workflow workflow = null;
            string deviceId = frameRendererData.Did;
            string frameId = frameRendererData.Fid;
            string tenantId = frameRendererData.Tid;
            string storageBaseURL = deviceDetails.BaseUrl;
            string fileExtension = String.Empty;
            int messageStucktime = deviceDetails.FrameSequencingMessageStuckDuration;
            int messageRetry = deviceDetails.FrameSequencingMessageRetry;
            if (deviceDetails.DownLoadLot)
            {
                fileExtension = ApplicationConstants.FileExtensions.zip;
            }
            else
            {
                fileExtension = ApplicationConstants.FileExtensions.jpg;
            }
            workflow = Helper.DownloadBlob(deviceId, frameId, tenantId, storageBaseURL, fileExtension);
            if (workflow != null)
            {
                return workflow;
            }
            else
            {
                for (int i = 0; i <= messageRetry; i++)
                {
                    Thread.Sleep(messageStucktime);
                    workflow = Helper.DownloadBlob(deviceId, frameId, tenantId, storageBaseURL, fileExtension);
                    if (workflow != null)
                    {
                        return workflow;
                    }
                }
                if (workflow == null)
                {
                    LogHandler.LogError("Not able to download lot even after retry messageRetry : {0}", LogHandler.Layer.Business, messageRetry);
                }
            }
            return workflow;
        }


        public void PostProcessFrameImage(QueueEntity.FrameRendererMetadata frameRendererData, DeviceDetails deviceDetails)
        {
#if DEBUG
            using (LogHandler.TraceOperations("FrameRendererProcess:PostProcessFrame", LogHandler.Layer.Business, Guid.NewGuid()))
            {


                LogHandler.LogInfo(String.Format(InfoMessages.Method_Execution_Start, "PostProcessFrame", "FrameRendererProcess"), LogHandler.Layer.Business, null);
                LogHandler.LogInfo("Executing  PostProcessFrame in FrameRenderer", LogHandler.Layer.Business);
#endif
                Stopwatch postProcessStopWatch = new Stopwatch();
                postProcessStopWatch.Reset();
                postProcessStopWatch.Start();
                List<string> deviceIdList = new List<string>();
              
                List<byte[]> dataList = new List<byte[]>();
                string deviceId = frameRendererData.Did;
                string baseUrl = deviceDetails.BaseUrl;
                predictionModel = deviceDetails.PredictionModel;
                bool enableLot = deviceDetails.EnableLots;
                string ipaddress = deviceDetails.IpAddress;
                port = deviceDetails.Port;
                penThickness = deviceDetails.PenThickness;
                maskPenColor = deviceDetails.MaskPenColor;
                noMaskPenColor = deviceDetails.NoMaskPenColor;
                boxColor = deviceDetails.BoxColor;
                int tenantId = Convert.ToInt32(frameRendererData.Tid);
                labelFontSize = deviceDetails.LabelFontSize;
                labelFontStyle = deviceDetails.LabelFontStyle;
                labelHeight = deviceDetails.LabelHeight;
                labelFontColor = deviceDetails.LabelFontColor;
                predictionClassType = deviceDetails.PredictionClassType;
                int videoStreamingOption = deviceDetails.VideoStreamingOption;
                string ffmpegArguments = deviceDetails.FfmpegArguments;


#if DEBUG
                LogHandler.LogDebug(String.Format("The PostProcessFrame Method of FrameRendererProcess class is getting executed with parameters : DeviceId ={0}, lotSize = {1} ,  frameId = {2} , ipAddress = {3} , port = {4} , tenantId = {5} ; ",
                    deviceId, enableLot, frameId, ipaddress, port, frameRendererData.Tid),
                  LogHandler.Layer.Business, null);
#endif
                try
                {
                    ProcessPreLoadedFrameImage(frameRendererData, baseUrl, predictionModel, deviceDetails);

                }
                catch (Exception ex)
                {
                    LogHandler.LogError("Exception occured while calling ProcessPreLoadedImage , exception message:{0},exception trace:{1}", LogHandler.Layer.Business, ex.Message, ex.StackTrace);
                    throw ex;
                }
                postProcessStopWatch.Stop();

#if DEBUG
                LogHandler.LogInfo(String.Format(InfoMessages.Method_Execution_End, "PostProcessFrame", "FrameRendererProcess"), LogHandler.Layer.Business, null);
            }
#endif
        }

        
        private void ProcessPreLoadedFrameImage(QueueEntity.FrameRendererMetadata frameRendererData, string baseUrl,
        string modelName, DeviceDetails deviceDetails)
        {
            try
            {
#if DEBUG
                LogHandler.LogInfo(String.Format(InfoMessages.Method_Execution_Start, "ProcessPreLoadedImage",
                "FrameRendererProcess"), LogHandler.Layer.Business, null);
                using (LogHandler.TraceOperations("FrameRendererProcess:ProcessPreLoadedImage",
                LogHandler.Layer.Business, Guid.NewGuid()))
                {
#endif
                    FrameDetails frameDetails = new FrameDetails();
                    List<TransportFrameDetails> transportFrameDetailsList = new List<TransportFrameDetails>();
                    string cacheKey = frameRendererData.Tid + frameRendererData.Did + frameRendererData.Fid;
                    lotStopWatch.Start();
                    int seqNumber = int.Parse(frameRendererData.SequenceNumber);
                    string feedKey = GenerateFeedKey(frameRendererData.Tid, frameRendererData.Did,
                    frameRendererData.FeedId);
                    Dictionary<string, Stream> frameDict = (Dictionary<string, Stream>)cache[cacheKey];
                    if (frameDict == null)
                    {
#if DEBUG
                        LogHandler.LogDebug(String.Format("FrameRendererProcess no frame detail available in cache and downdloading it: FrameId={0}", frameId),
                        LogHandler.Layer.Business, null);
#endif
                        frameDict = DownLoadFrames(frameRendererData, deviceDetails);
                    }
                    int tenantId = Convert.ToInt32(frameRendererData.Tid);
                    if (frameDict != null)
                    {
                        try
                        {
                            bool isFirstFrame = true;
                            List<string> keyList = new List<string>(frameDict.Keys);
                            keyList = keyList.OrderBy(x => getFrameId(x)).ToList();
                            byte[] data;
                            foreach (var key in keyList)
                            {
                                using (Stream imageStream = frameDict[key])
                                {
                                    if (imageStream.Length > 0)
                                    {
                                        Image<Bgr, byte> emguImage;
                                        using (MemoryStream ms = new MemoryStream())
                                        {
                                            imageStream.CopyTo(ms);
                                            byte[] imageBytes = ms.ToArray();
                                            ms.Dispose();
                                            Mat matImage = new Mat();
                                            CvInvoke.Imdecode(imageBytes, ImreadModes.Color, matImage);
                                            emguImage = matImage.ToImage<Bgr, byte>();
                                        }
                                        frameRendererData.Fid = key.Replace(".jpg", "");
                                       

                                        frameDetails = ProcessData(emguImage, frameRendererData, modelName,
                                        deviceDetails);

                                        if (frameDetails == null)
                                        {
                                            return;
                                        }
                                        frameDetails.IsFirstFrame = isFirstFrame;

                                        /*Save the processed image into local file path for debug purpose*/
                                        if (isFirstFrame && isImageDebugEnabled != null && isImageDebugEnabled.Equals("true",
                                        StringComparison.InvariantCultureIgnoreCase))
                                        {
                                            if (debugImageFilePath != null && Directory.Exists(debugImageFilePath))
                                            {
                                                emguImage.Save(debugImageFilePath + frameRendererData.Fid + ApplicationConstants.FileExtensions.jpg);
                                            }
                                        }
                                        emguImage.Dispose();
                                        emguImage = null;

                                    }
                             
#if DEBUG
                                    LogHandler.LogDebug("FrameRendererProcess - after pushing transportFrameDetails - frameQueue count {0} and thread id {1}",
                                    LogHandler.Layer.Business, frameQueue.Count, Thread.CurrentThread.ManagedThreadId);
#endif
                                    isFirstFrame = false;
                                    imageStream.Dispose();
                                    frameDict.Remove(key);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            LogHandler.LogError("Constructing transportFrameDetailsList, error message: {0} , stack trace: {1}",
                            LogHandler.Layer.Business, ex.Message, ex.StackTrace);
                            throw ex;
                        }
                        lock (frameCountLock)
                        {
                            try
                            {
                                Dictionary<int, List<TransportFrameDetails>> frameMessage = null;
                                IntializeDictionaries(feedKey);
                                frameMessage = (Dictionary<int, List<TransportFrameDetails>>)frameMessageDetails[feedKey];
                                Queue seqNumberQueue = null;
                                seqNumberQueue = (Queue)sequenceNumberQueueDetails[feedKey];
                                int count = frameTransferCountDetails[feedKey];
                                count++;
                                frameTransferCountDetails[feedKey] = count;
                                if (frameMessage != null)
                                {
                                    /* If sequence number already exist in the frameMessage and update new frameDetails, if not add it to the message */
                                    if (!frameMessage.ContainsKey(seqNumber))
                                    {
                                        frameMessage.Add(seqNumber, transportFrameDetailsList);
                                    }
                                    else
                                    {
                                        frameMessage[seqNumber] = transportFrameDetailsList;
                                    }
                                }
                                if (seqNumberQueue != null)
                                {
                                    seqNumberQueue.Enqueue(seqNumber);
                                }
                            }
                            catch (Exception ex)
                            {
                                LogHandler.LogError("Exception in frameCountLock, exception message:{0}, inner message:{1}, exception trace:{2}, inner exception trace",
                                LogHandler.Layer.Business, ex.Message, ex?.InnerException?.Message, ex.StackTrace);
                                throw ex;
                            }
                        }
                    }
                    else
                    {
#if DEBUG
                        LogHandler.LogDebug(String.Format("FrameRendererProcess no frame detail available to process FrameRendererProcess class: frameId={0}",
                        frameId), LogHandler.Layer.Business, null);
#endif
                    }
#if DEBUG
                    LogHandler.LogInfo(String.Format(InfoMessages.Method_Execution_End, "ProcessPreLoadedImage",
                    "FrameRendererProcess"), LogHandler.Layer.Business, null);
                }
#endif
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                {
                    LogHandler.LogError("Exception occured in ProcessPreLoadedImage of FrameRendererProcess, " +
                    "exception message:{0}, inner message:{1}, exception trace:{2}, inner exception trace:{3}",
                    LogHandler.Layer.Business, ex.Message, ex.InnerException.Message, ex.StackTrace, ex.InnerException.StackTrace);
                }
                else
                {
                    LogHandler.LogError("Exception occured in ProcessPreLoadedImage of FrameRendererProcess, error message:{0}, exception trace:{1}",
                    LogHandler.Layer.Business, ex.Message, ex.StackTrace);
                }
                throw ex;
            }
        }
    }

}






