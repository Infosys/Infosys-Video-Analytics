/*=============================================================================================================== *
 * Copyright 2025 Infosys Ltd.                                                                                    *
 * Use of this source code is governed by Apache License Version 2.0 that can be found in the LICENSE file or at  *
 * http://www.apache.org/licenses/                                                                                *
 * ===============================================================================================================*/
using Infosys.Lif.LegacyCommon;
using Infosys.Lif.LegacyIntegratorService;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Infosys.Lif
{
    public class MemoryQueueAdapter : IAdapter

    {


        private const string REGION = "Region";
        private const string TRANSPORT_SECTION = "TransportSection";
        private const string SUCCESSFUL_SENT_MESSAGE = "Message successfully sent to the Memory Queue.";
        private const string SUCCESSFUL_RECEIVE_MESSAGE = "Message successfully received from Memory Queue.";
        private const string SUCCESSFUL_PEEK_MESSAGE = "Message successfully peeked from Memory Queue.";
        private const string PROCESSING_INCOMPLETE = "Processing is incomplete.";
        private const string QUEUE_NOTFOUND = "Queue not found.";
        private const int SUCCESSFUL_STATUS_CODE = 0;
        private const int UNSUCCESSFUL_STATUS_CODE = 1000;
        private const string dateFormat = "dd-MMM-yyyy-HH:mm:ss.fffff";
        private const string MESSAGE_TO_BE_DELETED = "MessageToBeDeleted";
        private const string LI_FILENAME = "LiSettings.json";
        private const string LI_CONFIGURATION = "LISettings";
        private const string ZERO = "$0";


        private string response = PROCESSING_INCOMPLETE;

        public event ReceiveHandler Received;

        static private Dictionary<string, ConcurrentQueue<MemoryQueueMessage>> queueDetails = new Dictionary<string, ConcurrentQueue<MemoryQueueMessage>>();
        static private ConcurrentDictionary<string, string> messagesToDelete = new ConcurrentDictionary<string, string>();
        private readonly object messagesToDeleteForLock = new object();

             
        static MemoryQueueAdapter()
        {
            LifLogHandler.LogDebug("MemoryQueue Adapter- MemoryQueueAdapter static constructor executed", LifLogHandler.Layer.IntegrationLayer);

            LISettings liSettings = new LISettings();
            var appconfig = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json").Build();
            var liSettingPath = appconfig.GetSection("LISettings").GetSection("Path").Value;
            var config = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile(liSettingPath).Build();
            config.Bind(LI_CONFIGURATION, liSettings);

           
            MemoryQueue queueConfiguration = liSettings.MemoryQueue;
            if (queueConfiguration != null)
            {
                foreach (var details in queueConfiguration.MemoryQueueDetails)
                {
                    string queueName = details.QueueName;
                    if (!string.IsNullOrEmpty(queueName))
                    {
                        if (!queueDetails.ContainsKey(queueName))
                        {
                            lock (queueDetails)
                            {
                                queueDetails.Add(queueName, new ConcurrentQueue<MemoryQueueMessage>());

                            }
                        }

                    }
                    if (!string.IsNullOrEmpty(details.SecondaryQueues))
                    {
                        string[] alternateQueues = details.SecondaryQueues.Split(';');
                        for (int i = 0; i < alternateQueues.Length; i++)
                        {
                            string secondaryQueueName = alternateQueues[i];
                            if (!queueDetails.ContainsKey(secondaryQueueName))
                            {
                                lock (queueDetails)
                                {
                                    queueDetails.Add(secondaryQueueName, new ConcurrentQueue<MemoryQueueMessage>());
                                }
                            }
                        }
                    }
                }
            }
        }
        enum MemoryQueueOperationType
        {
            Send, Receive, Peek
        }

        public bool Delete(ListDictionary messageDetails)
        {
            bool response = true;
            string messageId = messageDetails["MessageIdentifier"].ToString();
            LifLogHandler.LogDebug("MemoryQueue Adapter- Delete called for message with Id {0}",
                   LifLogHandler.Layer.IntegrationLayer, messageId);
            
            messagesToDelete.TryAdd(messageId, MESSAGE_TO_BE_DELETED);
            
            return response;
        }

        public void Receive(ListDictionary adapterDetails)
        {
            LifLogHandler.LogDebug("MemoryQueue Adapter- Receive called", LifLogHandler.Layer.IntegrationLayer);

            MemoryQueue transportSection = null;
            Region regionToBeUsed = null;
            string response = string.Empty;
            try
            {
                foreach (DictionaryEntry items in adapterDetails)
                {
                    if (items.Key.ToString() == REGION)
                    {
                        regionToBeUsed = items.Value as Region;
                    }
                    else if (items.Key.ToString() == TRANSPORT_SECTION)
                    {
                        transportSection = items.Value as MemoryQueue;
                    }
                }

                MemoryQueueDetails memoryQueueDetails = ValidateTransportName(transportSection, regionToBeUsed.TransportName);
                string queueName = memoryQueueDetails.QueueName;
                ConcurrentQueue<MemoryQueueMessage> queue = getQueueDetails(queueName);
                if (queue != null)
                {
                    if (memoryQueueDetails.QueueReadingType == MSMQReadType.Receive.ToString())
                    {
                        Thread receiveOphandler = new Thread((ThreadStart)async delegate { await HandleMessage(MemoryQueueOperationType.Receive, memoryQueueDetails, null, queue); });
                        receiveOphandler.Start();
                    }
                }
                else
                {
                    LifLogHandler.LogError("MemoryQueue Adapter- Receive FAILED, because inMemoryQueue does not exit for {0}",
                   LifLogHandler.Layer.IntegrationLayer, queueName);
                }

            }
            catch (LegacyException exception)
            {
                throw exception;
            }
            catch (Exception exception)
            {
                throw exception;
            }

        }

        public string Send(ListDictionary adapterDetails, string message)
        {
            LifLogHandler.LogDebug("MemoryQueue Adapter- Send called", LifLogHandler.Layer.IntegrationLayer);

            MemoryQueue transportSection = null;
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
                        transportSection = items.Value as MemoryQueue;
                    }
                }

                MemoryQueueDetails memoryQueueDetails = ValidateTransportName(transportSection, regionToBeUsed.TransportName);
                string queueName = memoryQueueDetails.QueueName;
                string label = memoryQueueDetails.MessageLabel + ZERO;
                ConcurrentQueue<MemoryQueueMessage> queue = getQueueDetails(queueName);
                if (queue != null && message !=null)
                {
                    MemoryQueueMessage queueMessage = ConstructMessage(message, label);
                    response = HandleMessage(MemoryQueueOperationType.Send, memoryQueueDetails, queueMessage, queue).Result;
                } else
                {
                    LifLogHandler.LogError("MemoryQueue Adapter- Send FAILED, because inMemoryQueue does not exit for {0}",
                   LifLogHandler.Layer.IntegrationLayer, queueName);
                }

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

        private MemoryQueueMessage ConstructMessage(string msg,string label)
        {
            MemoryQueueMessage message = new MemoryQueueMessage();
            message.Body = msg;
            message.Id = Guid.NewGuid().ToString();
            message.Label = label;
            return message;
        }

      

        private ConcurrentQueue<MemoryQueueMessage> getQueueDetails(string queueName)
        {
            ConcurrentQueue<MemoryQueueMessage> queue = null;
            if (queueDetails.ContainsKey(queueName))
            {
                queue = (ConcurrentQueue<MemoryQueueMessage>)queueDetails[queueName];
            }else
            {
                throw new LegacyException(queueName + " Either Queue is not defined in MemoryQueueDetails section or not created in Memory");
            }
            return queue;

        }

     
        private MemoryQueueDetails ValidateTransportName(MemoryQueue transportSection, string transportName)
        {
            MemoryQueueDetails memoryQueueDetails = null;
            bool isTransportNameExists = false;
           
            for (int count = 0; count < transportSection.MemoryQueueDetails.Count; count++)
            {
                memoryQueueDetails = transportSection.MemoryQueueDetails[count] as MemoryQueueDetails;
                if (memoryQueueDetails.TransportName == transportName)
                {
                    isTransportNameExists = true;
                    break;
                }
            }
        
            if (!isTransportNameExists)
            {
                throw new LegacyException(transportName + " is not defined in MemoryQueueDetails section");
            }
            return memoryQueueDetails;
        }

       
        private ReceiveEventArgs ConstructResponse(string msg,string newMessageId)
        {
            ReceiveEventArgs args = new ReceiveEventArgs();
            args.ResponseDetails = new ListDictionary();
            if (msg != null)
            {
                args.ResponseDetails.Add("MessageBody", msg);
            }
            args.ResponseDetails.Add("MessageIdentifier", newMessageId);
            args.ResponseDetails.Add("Status", response);
           
            if (response == SUCCESSFUL_PEEK_MESSAGE || response == SUCCESSFUL_RECEIVE_MESSAGE)
                args.ResponseDetails.Add("StatusCode", SUCCESSFUL_STATUS_CODE);
            else
                args.ResponseDetails.Add("StatusCode", UNSUCCESSFUL_STATUS_CODE);
            return args;
        }

        private async Task<string> HandleMessage(MemoryQueueOperationType operation, MemoryQueueDetails memoryQueueDetails, MemoryQueueMessage message,
            ConcurrentQueue<MemoryQueueMessage> queue)
        {
            LifLogHandler.LogDebug("MemoryQueue Adapter- Handle Message called for operation of type- " + operation.ToString(), LifLogHandler.Layer.IntegrationLayer);

            try
            {
                switch (operation)
                {
                    case MemoryQueueOperationType.Send:
                         
                        Enum.TryParse<MSMQSendPattern>(memoryQueueDetails.SendPattern,true, out MSMQSendPattern sendPattern);
                      
                        switch (sendPattern)
                        {
                            case MSMQSendPattern.None:
                                LifLogHandler.LogDebug("MemoryQueue Adapter (transport- {0})- Send pattern configured- None",
                                    LifLogHandler.Layer.IntegrationLayer, memoryQueueDetails.TransportName);
                                queue.Enqueue(message);
                                response = SUCCESSFUL_SENT_MESSAGE;
                                break;
                            case MSMQSendPattern.RoundRobin:
                                LifLogHandler.LogDebug("MemoryQueue Adapter (RoundRobin, transport- {0})- Send pattern configured- RoundRobin",
                                    LifLogHandler.Layer.IntegrationLayer, memoryQueueDetails.TransportName);
                                queue.Enqueue(message);
                                response = SUCCESSFUL_SENT_MESSAGE;
                                break;
                            case MSMQSendPattern.QueueLoad:
                                LifLogHandler.LogDebug("MemoryQueue Adapter (QueueLoad, transport- {0})- Send pattern configured- QueueLoad",
                                    LifLogHandler.Layer.IntegrationLayer, memoryQueueDetails.TransportName);
                                queue.Enqueue(message);
                                response = SUCCESSFUL_SENT_MESSAGE;
                                break;
                            case MSMQSendPattern.BroadCast:
                                LifLogHandler.LogDebug("MemoryQueue Adapter (transport- {0})- Send pattern configured- BroadCast",
                                    LifLogHandler.Layer.IntegrationLayer, memoryQueueDetails.TransportName);
                                queue.Enqueue(message);
                                BroadCastMessageToSecondaryQueues(memoryQueueDetails, message);
                                response = SUCCESSFUL_SENT_MESSAGE;
                                break;
                            default:
                                LifLogHandler.LogDebug("MemoryQueue Adapter- Right Send pattern configuration is missing, hence considering - None",
                                    LifLogHandler.Layer.IntegrationLayer);
                                queue.Enqueue(message);
                                response = SUCCESSFUL_SENT_MESSAGE;
                                break;
                        }
                        break;
                    case MemoryQueueOperationType.Receive:
                        if (memoryQueueDetails.QueueReadingMode == MSMQReadMode.Async.ToString())
                        {
                            try
                            {
                                LifLogHandler.LogDebug("MemoryQueue Adapter- Receive in ASYNC mode is requested", LifLogHandler.Layer.IntegrationLayer);
                                string queueName = memoryQueueDetails.QueueName;
                                while (true)
                                {

                                    if (queue.Count == 0)
                                    {
                                        Thread.Sleep(memoryQueueDetails.PollingRestDuration);
                                        continue;
                                    }

                                   
                                    await Parallel.ForEachAsync(queue, new ParallelOptions { MaxDegreeOfParallelism = memoryQueueDetails.ParallelProcessingLimit }, async (msg, _) =>
                                    {
                                        await Task.Run(() => ProcessReceivedMessage(null, memoryQueueDetails, queue));
                                    });

                                  
                                    if (!memoryQueueDetails.ContinueToReceive)
                                        break;
                                    Thread.Sleep(memoryQueueDetails.PollingRestDuration);

                                }
                               
                            }
                            catch (Exception ex)
                            {
                                LifLogHandler.LogError("MemoryQueue Adapter- Receive in ASYNC mode, unexpected Exception occured. " +
                                    " Queue Name {0} , Exception Message: {1} and Exception StackTrace: {2}",
                                    LifLogHandler.Layer.IntegrationLayer,  memoryQueueDetails.QueueName, ex.Message, ex.StackTrace);
                                throw ex;
                            }

                        }
                        else if (memoryQueueDetails.QueueReadingMode == MSMQReadMode.Sync.ToString())
                        {
                            LifLogHandler.LogDebug("MemoryQueue Adapter- Receive in SYNC mode is requested", LifLogHandler.Layer.IntegrationLayer);
                            string queueName = memoryQueueDetails.QueueName;
                            while (true)
                            {
                                MemoryQueueMessage messageDetails = null;
                                bool hasMessage = queue.TryDequeue(out messageDetails);
                                if (hasMessage && messageDetails != null)
                                {
                                    response = SUCCESSFUL_RECEIVE_MESSAGE;
                                    ProcessReceivedMessage(messageDetails, memoryQueueDetails, queue);
                                }

                                if (!memoryQueueDetails.ContinueToReceive)
                                    break;
                                Thread.Sleep(memoryQueueDetails.PollingRestDuration);

                            }

                        }
                        break;
                    case MemoryQueueOperationType.Peek:
                        if (memoryQueueDetails.QueueReadingMode == MSMQReadMode.Async.ToString())
                        {
                            response = SUCCESSFUL_PEEK_MESSAGE;

                        }
                        else if (memoryQueueDetails.QueueReadingMode == MSMQReadMode.Sync.ToString())
                        {
                            response = SUCCESSFUL_PEEK_MESSAGE;
                        }
                        break;
                }

            }
            catch (Exception ex)
            {
                response = ex.Message;
                if (ex.InnerException != null)
                    response = response + ".Inner Exception- " + ex.InnerException.Message;
                LifLogHandler.LogError("MemoryQueue Adapter- HandleMessage(outer) FAILED for- {0}. Reason- {1}. Stack Trace : {2} ",
                    LifLogHandler.Layer.IntegrationLayer, operation.ToString(), response, ex.StackTrace);
            }
            return response;
        }

      
        private void ProcessReceivedMessage(MemoryQueueMessage queueMessage, MemoryQueueDetails memoryQueueDetails, ConcurrentQueue<MemoryQueueMessage> queue)
        {
            bool isMessageToBeResend = false;
            if(queueMessage == null)
            {
                if (!queue.TryDequeue(out queueMessage))
                {
                    return;
                }
            }
            string queueName = memoryQueueDetails.QueueName;
            try
            {
                ReceiveEventArgs args = ConstructResponse(queueMessage.Body, queueMessage.Id);
                if (Received != null)
                {
                    Received(args);
                }
            }
            catch (Exception ex)
            {
                isMessageToBeResend = true;
                      
                LifLogHandler.LogError("MemoryQueue Adapter ProcessReceivedMessage method FAILED for " +
                    " QueueName {0}. Exception Message: {1} and Exception StackTrace: {2}",
                LifLogHandler.Layer.IntegrationLayer, queueName, ex.Message, ex.StackTrace);

            }

            

            if (messagesToDelete.ContainsKey(queueMessage.Id))
            {
                LifLogHandler.LogDebug("MemoryQueue Adapter ProcessReceivedMessage- Message remvoed from messagesToDelete" +
                    " QueueName {0},Message Id {1}",
                    LifLogHandler.Layer.IntegrationLayer, queueName, queueMessage.Id);
                string deletedMessage = string.Empty;
                    messagesToDelete.TryRemove(queueMessage.Id,out deletedMessage);
               
            }
            else
            {
                LifLogHandler.LogDebug("MemoryQueue Adapter ProcessReceivedMessage- Message not exist in messagesToDelete" +
                    " QueueName {0},Message Id {1}",
                    LifLogHandler.Layer.IntegrationLayer, queueName, queueMessage.Id);
            
                isMessageToBeResend = true;
            }
           

            if (isMessageToBeResend && memoryQueueDetails.MessageProcessingMaxCount > 0)
            {
                LifLogHandler.LogDebug("MemoryQueue Adapter ProcessReceivedMessage- Resending the message, QueueName {0},Message Id {1}",
                    LifLogHandler.Layer.IntegrationLayer, queueName, queueMessage.Id);
              
                HandleMessageReappearance(queueMessage, memoryQueueDetails, queue);
            }

        }

       
        private void ReceiveMessageForAsync(MemoryQueueMessage message, MemoryQueueDetails memoryQueueDetails,
            ConcurrentQueue<MemoryQueueMessage> queue)
        {
           
            Task.Factory.StartNew(() => {
                ProcessReceivedMessage(message, memoryQueueDetails, queue);
            });

        }

         
        private void HandleMessageReappearance(MemoryQueueMessage queueMessage, MemoryQueueDetails memoryQueueDetails, ConcurrentQueue<MemoryQueueMessage> queue)
        {
            LifLogHandler.LogDebug("MemoryQueue Adapter- HandleMessageReappearance is called", LifLogHandler.Layer.IntegrationLayer);
            try
            {
                
                string[] labelParts = queueMessage.Label.Split('$');
                if (labelParts.Length >= 2)
                {
                    int dequeueCount = int.Parse(labelParts[1]);
                    LifLogHandler.LogDebug("MemoryQueue Adapter- HandleMessageReappearance and dequeueCount {0}",
                          LifLogHandler.Layer.IntegrationLayer, dequeueCount);
                    dequeueCount++;
                        
                    if (dequeueCount >= memoryQueueDetails.MessageProcessingMaxCount)
                    {
                        LifLogHandler.LogDebug("MemoryQueue Adapter- HandleMessageReappearance - the maximum dequeue count has reached, " +
                            "so the message will be moved to the poison/dead-letter queue", LifLogHandler.Layer.IntegrationLayer);
                   
                    } else
                    {
                        string messageLabel = labelParts[0] + "$" + (dequeueCount).ToString();
                        MemoryQueueMessage newMesaage = ConstructMessage(queueMessage.Body, messageLabel);
                     
                        queue.Enqueue(newMesaage);
                    }


                }
            }
            catch (Exception ex)
            {
                string error = ex.Message;
                if (ex.InnerException != null)
                    error = error + ".Inner Exception- " + ex.InnerException.Message;
                LifLogHandler.LogError("MemoryQueue Adapter- HandleMessageReappearance FAILED. Reason- {0}", LifLogHandler.Layer.IntegrationLayer, error);
                throw ex;
            }

        }

        
        private void BroadCastMessageToSecondaryQueues(MemoryQueueDetails memoryQueueDetails, MemoryQueueMessage message)
        {
            LifLogHandler.LogDebug("MemoryQueue Adapter -  Broadcast the message to secondary queues ", LifLogHandler.Layer.IntegrationLayer);
            try
            {
                if (!string.IsNullOrEmpty(memoryQueueDetails.SecondaryQueues))
                {
                    string[] alternateQueues = memoryQueueDetails.SecondaryQueues.Split(';');
                    for (int i = 0; i < alternateQueues.Length; i++)
                    {
                        string secondaryQueueName = alternateQueues[i];
                        MemoryQueueMessage newMessage = ConstructMessage(message.Body, message.Label);
                        ConcurrentQueue<MemoryQueueMessage> secondaryQueue = getQueueDetails(secondaryQueueName);
                        secondaryQueue.Enqueue(newMessage);
                    }
                }
            }
            catch (Exception ex)
            {
                string error = ex.Message;
                if (ex.InnerException != null)
                    error = error + ".Inner Exception- " + ex.InnerException.Message;
                LifLogHandler.LogError("MemoryQueue Adapter- BroadCastMessageToSecondaryQueues FAILED. Reason- {0}", LifLogHandler.Layer.IntegrationLayer, error);
                throw ex;
            }
        }



    }
}
