/*=============================================================================================================== *
 * Copyright 2025 Infosys Ltd.                                                                                    *
 * Use of this source code is governed by Apache License Version 2.0 that can be found in the LICENSE file or at  *
 * http://www.apache.org/licenses/                                                                                *
 * ===============================================================================================================*/
﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Collections;
using System.Linq;
using System.Text;


using Experimental.System.Messaging;

using System.Threading;
using System.Runtime.Caching;
using Infosys.Lif.LegacyCommon;
using Infosys.Lif.LegacyIntegratorService.Ext;
using Newtonsoft.Json;





namespace Infosys.Lif.Ext
{
    class MSMQAdapterExtenstion : IAdapterExt
    {
        #region private variables and constant
        private const string REGION = "Region";
        private const string TRANSPORT_SECTION = "TransportSection";
        private const string SUCCESSFUL_SENT_MESSAGE = "Message successfully sent to the MSMQ.";
        private const string SUCCESSFUL_RECEIVE_MESSAGE = "Message successfully received from MSMQ.";
        private const string SUCCESSFUL_PEEK_MESSAGE = "Message successfully peeked from MSMQ.";
        private const string PROCESSING_INCOMPLETE = "Processing is incomplete.";
        private const string QUEUE_NOTFOUND = "Queue not found.";
        private const int SUCCESSFUL_STATUS_CODE = 0;
        private const int UNSUCCESSFUL_STATUS_CODE = 1000;
        private const string dateFormat = "dd-MMM-yyyy-HH:mm:ss.fffff";
        private const string MESSAGE_TO_BE_DELETED = "MessageToBeDeleted";

        private Message responseMessage;
        private string response = PROCESSING_INCOMPLETE;
        //private string threadName;
        private MSMQDetails tempMsMQDetails;
        //private string messageToDelete="";
        private List<string> messagesToDelete = new List<string>();

        //the below constant and variable are to be used during send pattren of types- round robin or queue load
        private const string KEY_FOR_LAST_QUEUE_POPULATED = "LastQueuePopulated";
        private int totalQueuesTraversed = 0;
        private int errorCount = 0;
        //Default Max Error Count = 10
        private int maxErrorCount = 5;
        private DateTime lastProcessedTime = DateTime.Now;
        private MessageQueue queueForDelete;
        private Dictionary<string, string> messagesToDeleteForAsync = new Dictionary<string, string>();
        private readonly object messagesToDeleteForAsyncLock = new object();


        #endregion

        private MSMQDetails ValidateTransportName(Infosys.Lif.LegacyIntegratorService.Ext.MSMQq transportSection, string transportName)
        {
            MSMQDetails msMQDetails = null;
            bool isTransportNameExists = false;
            
           
            for (int count = 0; count < transportSection.MSMQDetails.Count; count++)
            {
                msMQDetails = transportSection.MSMQDetails[count] as MSMQDetails;
                if (msMQDetails.TransportName == transportName)
                {
                    isTransportNameExists = true;
                    break;
                }
            }
          
            if (!isTransportNameExists)
            {
                throw new LegacyException(transportName + " is not defined in MSMQDetails section");
            }
            return msMQDetails;
        }
        private string HandleMessage(MSMQOperationType operation, MSMQDetails msMQDetails, string message)
        {
            LifLogHandler.LogDebug("MSMQ Adapter- Handle Message called for operation of type- " + operation.ToString(), LifLogHandler.Layer.IntegrationLayer);
           
            MessageQueue queue = new MessageQueue();
            
            queue.MessageReadPropertyFilter.ArrivedTime = true;

            queue.DefaultPropertiesToSend.Recoverable = true;

            if (msMQDetails.QueueType == MSMQType.Private.ToString())
            {
                if (msMQDetails.ServerName.Contains('.') && msMQDetails.ServerName != ".") 
                    queue.Path = "FormatName:Direct=TCP:" + msMQDetails.ServerName + @"\Private$\" + msMQDetails.QueueName;
                else
                    queue.Path = "FormatName:Direct=OS:" + msMQDetails.ServerName + @"\Private$\" + msMQDetails.QueueName;
               
            }
            else if (msMQDetails.QueueType == MSMQType.Public.ToString())
                queue.Path = msMQDetails.ServerName + @"\" + msMQDetails.QueueName;

           
            ((XmlMessageFormatter)queue.Formatter).TargetTypes = new Type[] { typeof(string) };

            try
            {
                LifLogHandler.LogDebug("MSMQ Adapter (transport- {0})- Send pattern configured- None", LifLogHandler.Layer.IntegrationLayer, msMQDetails.TransportName);
                queue.Send(message, msMQDetails.MessageLabel + "$0");
                response = SUCCESSFUL_SENT_MESSAGE;
                queue.Close();
                errorCount = 0;
            }
            catch (Exception ex)
            {

            }
            
            return response;
        }
        public string Send(System.Collections.Specialized.ListDictionary adapterDetails, string message)
        {
            LifLogHandler.LogDebug("MSMQ Adapter- Send called", LifLogHandler.Layer.IntegrationLayer);

            MSMQq transportSection = null;
            Region regionToBeUsed = null;
            string response = string.Empty;
            try
            {

                if (string.IsNullOrEmpty(message))
                    throw new ArgumentException("Message parameter cannot be Empty", "message");
                foreach (DictionaryEntry items in adapterDetails)
                {
                    if (items.Key.ToString() == REGION)
                    {
                        regionToBeUsed = items.Value as Region;
                    }
                    else if (items.Key.ToString() == TRANSPORT_SECTION)
                    {
                        string summa = JsonConvert.SerializeObject(items.Value);
                        transportSection = JsonConvert.DeserializeObject<MSMQq>(summa);
                    }
                }

                MSMQDetails msMQDetails = ValidateTransportName(transportSection, regionToBeUsed.TransportName);

                #region change
                //if (msMQDetails.is)
                //    response = HandleTransactionalMessage(MSMQOperationType.Send, msMQDetails, message);
                //else
                #endregion

                response = HandleMessage(MSMQOperationType.Send, msMQDetails, message);

            }
            catch (LegacyException exception)
            {
                throw exception;
            }
            catch (Exception exception)
            {
                throw exception;
            }
            return response;
        }

        MSMQDetails IAdapterExt.ValidateTransportName(MSMQq transportSection, string transportName)
        {
            throw new NotImplementedException();
        }

        public string HandleMessage(LegacyIntegratorService.Ext.MSMQOperationType operation, MSMQDetails msMQDetails, string message)
        {
            throw new NotImplementedException();
        }
    }
    enum MSMQOperationType
    {
        Send, Receive, Peek
    }

    enum MessageProcessingStatus
    {
        InProcess, Processed
    }

}
