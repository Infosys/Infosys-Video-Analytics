/*=============================================================================================================== *
 * Copyright 2025 Infosys Ltd.                                                                                    *
 * Use of this source code is governed by Apache License Version 2.0 that can be found in the LICENSE file or at  *
 * http://www.apache.org/licenses/                                                                                *
 * ===============================================================================================================*/
﻿using System;
using Infosys.Solutions.Ainauto.VideoAnalytics.Resource.Entity.Table;
using Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.ProcessScheduler.Framework;
using QueueEntity=Infosys.Solutions.Ainauto.VideoAnalytics.Resource.Entity.Queue;
using Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.Common;
using System.Runtime.Caching;
using System.Configuration;
using Newtonsoft.Json;
using PH=Infosys.Solutions.Ainauto.VideoAnalytics.BusinessComponent.PcdHandler;
using Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.TaskRoute;

namespace Infosys.Solutions.Ainauto.VideoAnalytics.Processes {
    public class PcdHandlerProcess:ProcessHandlerBase<TableDetails> {
        public string _taskCode;
        public PcdHandlerProcess() { }
        public PcdHandlerProcess(string processId)
        {
            _taskCode = TaskRoute.GetTaskCode(processId);
        }
        public override void Dump(TableDetails message) {
        }
        public override bool Initialize(QueueEntity.MaintenanceMetaData message) {
            if(message==null) {
                ReadFromConfig();
            }
            return true;
        }
        private void ReadFromConfig() {
            PH.ReadFromConfig();
        }
        
        public override bool Process(TableDetails message,int robotId,int runInstanceId,int robotTaskMapId) {
            #if DEBUG
            LogHandler.LogInfo(String.Format(InfoMessages.Method_Execution_Start,"Process","PcdHandlerProcess"),LogHandler.Layer.Business,null);
            LogHandler.LogDebug(String.Format("The Process method of PcdHandlerProcess class is getting executed with parameters: "+
            "message={0}; robotId={1}; runInstanceId={2}; robotTaskMapId={3}",message,robotId,runInstanceId,robotTaskMapId),
            LogHandler.Layer.Business,null);
            #endif
            try {
                using(LogHandler.TraceOperations("PcdHandlerProcess:Process",LogHandler.Layer.Business,Guid.NewGuid(),null)) {
                    PH.PcdHandlerProcess(false);
                    return true;
                }
            }
            catch(Exception exMP) {
                LogHandler.LogError("Exception in PcdHandlerProcess: {0}, stack trace: {1}",LogHandler.Layer.Business,exMP.Message,exMP.StackTrace);
                bool failureLogged=false;
                try {
                    Exception ex=new Exception();
                    bool rethrow=ExceptionHandler.HandleException(exMP,ApplicationConstants.WORKER_EXCEPTION_HANDLING_POLICY,out ex);
                    failureLogged=true;
                    if(rethrow) {
                        throw ex;
                    }
                    else {
                        
                        return true;
                    }
                }
                catch(Exception ex) {
                    LogHandler.LogError(String.Format(ErrorMessages.Exception_Failed,"Process","PcdHandlerProcess"),
                    LogHandler.Layer.Business,null);
                    
                    if(!failureLogged) {
                        LogHandler.LogError(String.Format("Exception occured while handling an exception in PcdHandlerProcess in Process method. Error message: {0}",ex.Message),LogHandler.Layer.Business,null);
                    }
                    return false;
                }
            }
        }
    }
}
