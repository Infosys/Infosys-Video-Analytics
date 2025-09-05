/*=============================================================================================================== *
 * Copyright 2025 Infosys Ltd.                                                                                    *
 * Use of this source code is governed by Apache License Version 2.0 that can be found in the LICENSE file or at  *
 * http://www.apache.org/licenses/                                                                                *
 * ===============================================================================================================*/
/* 
 * © 2012-2013 Infosys Limited, Bangalore, India. All Rights Reserved.
 * Version: 1.0 b
 * Except for any open source software components embedded in this Infosys proprietary software program ("Program"),
 * this Program is protected by copyright laws, international treaties and other pending or existing intellectual
 * property rights in India, the United States and other countries. Except as expressly permitted, any unauthorized
 * reproduction, storage, transmission in any form or by any means (including without limitation electronic, mechanical,
 * printing, photocopying, recording or otherwise), or any distribution of this Program, or any portion of it, may
 * results in severe civil and criminal penalties, and will be prosecuted to the maximum extent possible under the law.
 */

using Infosys.Lif.LegacyCommon;
using Infosys.Lif.LegacyIntegrator;
using Infosys.Lif.LegacyIntegratorService;
using Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.Common;
using Infosys.Solutions.Ainauto.VideoAnalytics.Resource.Entity.Queue;
using Infosys.Solutions.Ainauto.VideoAnalytics.Resource.Entity.Table;
using Newtonsoft.Json;
using System;
using System.Threading;
using static Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.Common.ApplicationConstants;

namespace Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.ProcessScheduler.Framework
{
    public abstract class ProcessHandlerBase<T> : IProcessHandler where T : class
    {

        protected AdapterManager adapterManager = new AdapterManager();
        protected Drive[] drives;
        protected ModeType mode;
        protected string entityName;
        protected string id;
       
        protected int robotId;
        protected int runInstanceId;
        protected int robotTaskMapId;

        public virtual void Start(
            Drive[] drives,
            ModeType mode,
            string entityName,
            string id,
            int robotId, int runInstanceId, int robotTaskMapId)
            {
            
            this.drives = drives;
            this.mode = mode;
            this.entityName = entityName;
            this.id = id;
           
            this.robotId = robotId;
            this.runInstanceId = runInstanceId;
            this.robotTaskMapId = robotTaskMapId;
            
            LogHandler.LogDebug("Mask Detector  Windows Service {0} Process Started", LogHandler.Layer.Infrastructure, id);
            
            
            initiate();
            Initialize(null);
          
            switch (mode)
            {
                case ModeType.Queue:

                    
                    adapterManager.ResponseReceived +=
                        new AdapterManager.AdapterReceiveHandler(adapterManager_ResponseReceived);

                    
                        LogHandler.LogDebug("Mask Detector Windows Service {0} Process receive first message", LogHandler.Layer.Infrastructure, id);
                        try
                        {
                            adapterManager.Receive(this.id);
                        }
                        catch (LegacyException integrationException)
                        {
                           
                            LogHandler.LogError(integrationException.Message,
                                LogHandler.Layer.Business);
                             LogHandler.LogError("Mask Detector Message {0} failed to be processed and exception {1}, stack trace", 
                                 LogHandler.Layer.Infrastructure, id, integrationException.Message, integrationException.StackTrace);

                        }
                        catch (Exception ex)
                        {
                            
                            
                            LogHandler.LogError("Mask Detector Message {0} failed to be processed and General Exception {1}, stack trace",
                                LogHandler.Layer.Infrastructure, id, ex.Message, ex.StackTrace);
                             throw ex;
                        }


                    break;
                   
                case ModeType.Table:

                    LogHandler.LogDebug("Mask Detector Windows Service {0} Process started for Table. Call Process logic.", LogHandler.Layer.Infrastructure, id);

                    if (id == "FrameGrabber" || id == "PromptHandlerProcess" || id=="PcdHandler")
                    {
                        TableDetails jobdetails = new TableDetails();
                        jobdetails.JobName = id;
                        jobdetails.EntityName = entityName;
                        string jsonString = JsonConvert.SerializeObject(jobdetails);
                        T data = Utility.DeserializeFromJSON<T>(jsonString);
                        Process(data, robotId, runInstanceId, robotTaskMapId);
                    }

                    while (true)
                    {
                        try
                        {                    
                            if (id == "R2WJob")
                            {
                                TableDetails jobdetails = new TableDetails();
                                jobdetails.JobName = id;
                                jobdetails.EntityName = entityName;                    
                                string jsonString = JsonConvert.SerializeObject(jobdetails);
                                T data = Utility.DeserializeFromJSON<T>(jsonString);
                                Process(data, robotId, runInstanceId, robotTaskMapId);                                
                            }
                        }
                        catch (Exception e)
                        {
                            LogHandler.LogDebug("Message {0} failed to be processed and exception {1}", LogHandler.Layer.Infrastructure, id, e.Message);
                            LogHandler.LogError("Message {0} failed to be processed and exception {1}", LogHandler.Layer.Infrastructure, id, e.Message);
                            try
                            {
                                Exception ex = new Exception();
                                bool rethrow = ExceptionHandler.HandleException(e, ApplicationConstants.SERVICE_EXCEPTIONHANDLING_POLICY, out ex);

                                if (rethrow)
                                {
                                    throw e;
                                }

                            }
                            catch (Exception ex)
                            {
                                LogHandler.LogDebug("Exception occured and Message {0} failed to be processed and exception {1}", LogHandler.Layer.Infrastructure, id, ex.Message);
                                LogHandler.LogError("Exception occured and Message {0} failed to be processed and exception {1}", LogHandler.Layer.Infrastructure, id, ex.Message);
                            }
                        }
                       
                        Thread.Sleep(30000);
                    }
                    break;
            }
        }


        void adapterManager_ResponseReceived(ReceiveEventArgs eventArgs)
        {
            string message ="";
            string messageId = "";

            try
            {
                LogHandler.LogDebug("Mask Detector Windows Service {0} Process message received", LogHandler.Layer.Infrastructure, id);
                messageId = eventArgs.ResponseDetails["MessageIdentifier"] as string;
                message = eventArgs.ResponseDetails["MessageBody"] as string;

                if (string.IsNullOrWhiteSpace(message) || string.IsNullOrWhiteSpace(messageId))
                {
                    LogHandler.LogWarning("Empty Message was received", LogHandler.Layer.Infrastructure, messageId);
                    return;
                }

                {
                    
                    T qMessage = Utility.DeserializeFromJSON<T>(message);
                    
                    if (qMessage == null)
                    {
                        LogHandler.LogWarning(
                            "Message {0} deserialized was invalid (null)", LogHandler.Layer.Infrastructure, messageId);
                        return;
                    }
                    

                    Dump(qMessage);
                    LogHandler.LogDebug("Message {0} dumped", LogHandler.Layer.Infrastructure, messageId);
                    
                    MaintenanceMetaData maintenanceMetaData = Utility.DeserializeFromJSON<MaintenanceMetaData>(message);
                    string messageType = maintenanceMetaData?.MessageType;
                    switch (messageType)
                    {
                        case ProcessingStatus.Maintenance:
                            if (Initialize(maintenanceMetaData))
                            {
                                adapterManager.Delete(messageId);
                            }
                            else
                            {
                                LogHandler.LogError("Message {0} failed to be processed", LogHandler.Layer.Infrastructure, messageId);
                               
                            }
                            break;
                        case ProcessingStatus.EventHandling:
                            if (HandleEventMessage(maintenanceMetaData))
                            {
                                adapterManager.Delete(messageId);
                            }
                            else
                            {
                                LogHandler.LogError("Message {0} failed to be processed", LogHandler.Layer.Infrastructure, messageId);
                               
                            }
                            break;
                        default:
                            if (Process(qMessage, robotId, runInstanceId, robotTaskMapId))
                            {
                                LogHandler.LogDebug("Message {0} processed succesfully", LogHandler.Layer.Infrastructure, messageId);

                                adapterManager.Delete(messageId);
                                LogHandler.LogDebug("Message {0} deleted succesfully", LogHandler.Layer.Infrastructure, messageId);
                            }
                            else
                            {
                                LogHandler.LogError("Message {0} failed to be processed", LogHandler.Layer.Infrastructure, messageId);
                                
                            }
                            break;
                    }
                    
                        
                }
            }
            catch (Exception difException)
            {
                try
                {
                    Exception ex = new Exception();
                    bool rethrow = ExceptionHandler.HandleException(difException, ApplicationConstants.SERVICE_EXCEPTIONHANDLING_POLICY,out ex);

                    if (rethrow)
                    {
                        throw difException;
                    }
                    else
                    {
                       
                        adapterManager.Delete(messageId);
                        LogHandler.LogDebug("Message {0} deleted succesfully", LogHandler.Layer.Infrastructure, messageId);
                    }

                }
                catch (Exception)
                {
                   
                    LogHandler.LogDebug("Message {0} failed to be processed", LogHandler.Layer.Infrastructure, messageId);
                    LogHandler.LogError("Message {0} failed to be processed", LogHandler.Layer.Infrastructure, messageId);
                }
            }
        }


        
        public abstract void Dump(T message);


        
        public abstract bool Process(T message , int robotId, int runInstanceId, int robotTaskMapId );

       
        public virtual void initiate()
        {

        }


        public abstract bool Initialize(MaintenanceMetaData message);

        public virtual bool HandleEventMessage(MaintenanceMetaData message)
        {
            return true;
        }



    }
}
