/*=============================================================================================================== *
 * Copyright 2024 Infosys Ltd.                                                                                    *
 * Use of this source code is governed by Apache License Version 2.0 that can be found in the LICENSE file or at  *
 * http://www.apache.org/licenses/                                                                                *
 * ===============================================================================================================*/

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Collections;
using System.Linq;
using System.Text;

//using System.Messaging;
using Experimental.System.Messaging;

using System.Threading;
using System.Runtime.Caching;
using Infosys.Lif.LegacyCommon;
using Infosys.Lif.LegacyIntegratorService;



namespace Infosys.Lif
{
    /// <summary>
    /// The new MSMQ Adapter provides a greater level of reliability and availability by:
    /// 1. Delaying the deletion of message as much as possible
    /// 2. Relying on the MSMQ for the message availability instead of keeping copy within the adapter 
    /// </summary>
    public partial class MSMQAdapter : IAdapter
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

        #region public variables
        public MSMQAdapter()
        {
        }
        #endregion

        #region IAdapter Members

        public event ReceiveHandler Received;

        public string Send(System.Collections.Specialized.ListDictionary adapterDetails, string message)
        {
            LifLogHandler.LogDebug("MSMQ Adapter- Send called", LifLogHandler.Layer.IntegrationLayer);

            Infosys.Lif.LegacyIntegratorService.MSMQ transportSection = null;
            Infosys.Lif.LegacyIntegratorService.Region regionToBeUsed = null;
            string response = string.Empty;
            try
            {

                if (string.IsNullOrEmpty(message))
                    throw new ArgumentException ("Message parameter cannot be Empty","message");
                foreach (DictionaryEntry items in adapterDetails)
                {
                    if (items.Key.ToString() == REGION)
                    {
                        regionToBeUsed = items.Value as Region;
                    }
                    else if (items.Key.ToString() == TRANSPORT_SECTION)
                    {
                        transportSection = items.Value as Infosys.Lif.LegacyIntegratorService.MSMQ;
                    }
                }

                // Validates whether TransportName specified in the region, exists in MSMQDetails section.
                MSMQDetails msMQDetails = ValidateTransportName(transportSection, regionToBeUsed.TransportName);
                if(msMQDetails.IsQueueTransactional)
                    response = HandleTransactionalMessage(MSMQOperationType.Send, msMQDetails, message);
                else
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

        public void Receive(ListDictionary adapterDetails)
        {
            LifLogHandler.LogDebug("MSMQ Adapter- Receive called", LifLogHandler.Layer.IntegrationLayer);

            Infosys.Lif.LegacyIntegratorService.MSMQ transportSection = null;
            Infosys.Lif.LegacyIntegratorService.Region regionToBeUsed = null;
            responseMessage = null;
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
                        transportSection = items.Value as Infosys.Lif.LegacyIntegratorService.MSMQ;
                    }
                }

                // Validates whether TransportName specified in the region, exists in MSMQDetails section.
                MSMQDetails msMQDetails = ValidateTransportName(transportSection, regionToBeUsed.TransportName);

                if (msMQDetails.QueueReadingType == MSMQReadType.Peek)
                {
                    //HandleMessage(MSMQOperationType.Peek, msMQDetails, null);
                    if (msMQDetails.IsQueueTransactional)
                        response = HandleTransactionalMessage(MSMQOperationType.Peek, msMQDetails, null);
                    else
                        HandleMessage(MSMQOperationType.Peek, msMQDetails, null);
                }
                if (msMQDetails.QueueReadingType == MSMQReadType.Receive)
                {
                    if (msMQDetails.IsQueueTransactional)
                    {
                        //The Handle Transaction Message does not have the fix for removing messages from queue in bulk and moving
                        //inavlid messages (Empty or Root Element is missing) to poison queue. Also issue of messagequeue read prperty filters getting reset
                        //in async is not updated. Also the check to error our empty messages has not been included during send
                        Thread receiveOphandler = new Thread((ThreadStart)delegate { HandleTransactionalMessage(MSMQOperationType.Receive, msMQDetails, null); });
                        receiveOphandler.Start();
                    }
                    else
                    {
                        maxErrorCount = msMQDetails.MessageProcessingMaxCount;
                        LifLogHandler.LogDebug("MSMQ Adapter- Receive method MaxErrorCount {0}, QueueNotRespondTime {1},Queue Name {2}",
                           LifLogHandler.Layer.IntegrationLayer, msMQDetails.MessageProcessingMaxCount, msMQDetails.TransactionWaitTime, msMQDetails.QueueName);

                        Thread receiveOphandler = new Thread((ThreadStart)delegate {
                            while (errorCount <= maxErrorCount)
                            {
                                LifLogHandler.LogDebug("MSMQ Adapter- Receive called and errorCount value {0}, maxErrorCount {1},Queue Name {2}",
                            LifLogHandler.Layer.IntegrationLayer, errorCount, maxErrorCount, msMQDetails.QueueName);
                                try
                                {
                                   lastProcessedTime = DateTime.Now;
                                   String responseFromHandleMessage = HandleMessage(MSMQOperationType.Receive, msMQDetails, null);
                                   LifLogHandler.LogDebug("MSMQ Adapter- Receive called and responseFromHandleMessage value {0},Queue Name {1}",
                           LifLogHandler.Layer.IntegrationLayer, responseFromHandleMessage, msMQDetails.QueueName);
                                   if (!PROCESSING_INCOMPLETE.Equals(responseFromHandleMessage))
                                    {
                                        break;
                                    }
                                }catch (QueueNotRespondedException ex)
                                {
                                    LifLogHandler.LogError("MSMQ Adapter- Receive method Failed due to QueueNotRespondedException in Queue Name {0} and Exception Message- {1}",
                                        LifLogHandler.Layer.IntegrationLayer, msMQDetails.QueueName, ex.Message);                                   
                                    
                                }
                                catch (Exception ex)
                                {
                                    errorCount++;
                                    Thread thr = Thread.CurrentThread;
                                    LifLogHandler.LogError("MSMQ Adapter- Receive method Failed and while calling HandleMessage Queue Name {0} and Current thread state:{1}," +
                                        "errorCount value {2}, maxErrorCount {3}. Exception Message- {4}. Exception StackTrace- {5} ",
                                        LifLogHandler.Layer.IntegrationLayer, msMQDetails.QueueName,thr.ThreadState, errorCount, maxErrorCount, ex.Message, ex.StackTrace);                                   
                                    //throw ex;
                                }

                            }
                           

                        });
                        receiveOphandler.Start();
                        
                        LifLogHandler.LogDebug("MSMQ Adapter- Receive called and receiveOphandler Thread state:{0}",
                            LifLogHandler.Layer.IntegrationLayer, receiveOphandler.ThreadState);
                    }
                }


            }
            catch (LegacyException exception)
            {
                LifLogHandler.LogError("MSMQ Adapter- Receive method Failed due to LegacyException and  Message- {0}. Exception StackTrace- {1}",
                                    LifLogHandler.Layer.IntegrationLayer, exception.Message, exception.StackTrace);
                throw exception;
            }
            catch (Exception exception)
            {
                LifLogHandler.LogError("MSMQ Adapter- Receive method Failed due to Exception Message- {0}. Exception StackTrace- {1}",
                                    LifLogHandler.Layer.IntegrationLayer, exception.Message, exception.StackTrace);
                throw exception;
            }
            //return responseMessage;
        }

        public bool Delete(ListDictionary messageDetails)
        {
            LifLogHandler.LogDebug("MSMQ Adapter- Delete called for message with Id- " + messageDetails["MessageIdentifier"].ToString(), LifLogHandler.Layer.IntegrationLayer);

            //in case of MSMQ, the delete operation is like fire and forget
            bool response = true;
            //messageToDelete = messageDetails["MessageIdentifier"].ToString();
            // incase of Async mode, System put the message id messagesToDeleteForAsync and it will process in queue_ReceiveCompletedForAsync method
            if (tempMsMQDetails != null && tempMsMQDetails.QueueReadingMode == MSMQReadMode.Async)
            {
                if (tempMsMQDetails.MessageProcessingMaxCount > 0)
                {
                    string messageId = messageDetails["MessageIdentifier"].ToString();
                    LifLogHandler.LogDebug("MSMQ Adapter- Delete called for Queue - {0} , Message Id {1} ", LifLogHandler.Layer.IntegrationLayer, tempMsMQDetails.QueueName, messageId);
                    DateTime deleteStartTime = DateTime.Now;
                    lock (messagesToDeleteForAsyncLock)
                    {
                        messagesToDeleteForAsync.Add(messageId, MESSAGE_TO_BE_DELETED);
                    }

                    LifLogHandler.LogDebug("MSMQ Adapter- Delete - Message Added to messagesToDeleteForAsync" +
                                " QueueName {0},Message Id {1}",
                                LifLogHandler.Layer.IntegrationLayer, tempMsMQDetails.QueueName, messageId);

                    //try 
                    //{
                    //    if (queueForDelete != null)
                    //    {
                    //        queueForDelete.ReceiveById(messageId, new TimeSpan(tempMsMQDetails.QueueReadTimeout));
                    //        LifLogHandler.LogDebug("MSMQ Adapter After Deleting- QueueName {0}, messageId {1} , QueueReadTimeout {2}, ToalTimeTakenInMilliseconds {3},",
                    //LifLogHandler.Layer.IntegrationLayer, tempMsMQDetails.QueueName, messageId, tempMsMQDetails.QueueReadTimeout,DateTime.Now.Subtract(deleteStartTime).TotalMilliseconds);
                    //    }
                    //    else
                    //    {
                    //        response = false;
                    //        LifLogHandler.LogError("MSMQ Adapter- In Delete Queue Obeject not created for Queue - {0}, Message Id {1}", 
                    //            LifLogHandler.Layer.IntegrationLayer, tempMsMQDetails.QueueName, messageId);
                    //    }                        

                    //} catch (Exception ex)
                    //{
                    //    LifLogHandler.LogDebug("MSMQ Adapter Exception in Async Delete- QueueName {0}, messageId {1} , QueueReadTimeout {2}, ToalTimeTakenInMilliseconds {3},",
                    //LifLogHandler.Layer.IntegrationLayer, tempMsMQDetails.QueueName, messageId, tempMsMQDetails.QueueReadTimeout, DateTime.Now.Subtract(deleteStartTime).TotalMilliseconds);

                    //    LifLogHandler.LogError("MSMQ Adapter- Async Delete FAILED for Queue - {0} , Message Id {1}, Reason {2}", LifLogHandler.Layer.IntegrationLayer, 
                    //        tempMsMQDetails.QueueName, messageId,ex.Message);
                    //    response = false;
                    //}                   

                }

            } else
            {
                messagesToDelete.Add(messageDetails["MessageIdentifier"].ToString());
            }   
            
            
            //
            return response;
        }
        
        #endregion

        #region private methods

        private void constructQueueForDelete()
        {
            if (queueForDelete == null)
            {
                queueForDelete = new MessageQueue();
                // Set the queue's MessageReadPropertyFilter property to enable the
                // message's ArrivedTime property.
                queueForDelete.MessageReadPropertyFilter.ArrivedTime = true;

                //set the queue to keep the message even after m/c start-up, this will make the msmq more available and reliable
                queueForDelete.DefaultPropertiesToSend.Recoverable = true;

                if (tempMsMQDetails.QueueType == MSMQType.Private)
                {
                    if (tempMsMQDetails.ServerName.Contains('.') && tempMsMQDetails.ServerName != ".") //i.e. IP address is given for the server
                        queueForDelete.Path = "FormatName:Direct=TCP:" + tempMsMQDetails.ServerName + @"\Private$\" + tempMsMQDetails.QueueName;
                    else
                        queueForDelete.Path = "FormatName:Direct=OS:" + tempMsMQDetails.ServerName + @"\Private$\" + tempMsMQDetails.QueueName;                    
                }
                else if (tempMsMQDetails.QueueType == MSMQType.Public)
                    queueForDelete.Path = tempMsMQDetails.ServerName + @"\" + tempMsMQDetails.QueueName;
            }
        }

        /// <summary>
        /// Method to handle Send, Receive and Peek operations on the MSMQ (non transactional)
        /// </summary>
        /// <param name="operation">the type of operation to be done on the MSMQ</param>
        /// <param name="msMQDetails">the details related to MSMQ to be used by the MSMQAdapter</param>
        /// <param name="message">the message to be dropped to the MSMQ in case of Send operation type,
        /// for other operation type its value is sent as null</param>
        /// <returns></returns>
        private string HandleMessage(MSMQOperationType operation, MSMQDetails msMQDetails, string message)
        {
            LifLogHandler.LogDebug("MSMQ Adapter- Handle Message called for operation of type- " + operation.ToString(), LifLogHandler.Layer.IntegrationLayer);
            //string response = string.Empty;
            MessageQueue queue = new MessageQueue();
            // Set the queue's MessageReadPropertyFilter property to enable the
            // message's ArrivedTime property.
            queue.MessageReadPropertyFilter.ArrivedTime = true;

            //set the queue to keep the message even after m/c start-up, this will make the msmq more available and reliable
            queue.DefaultPropertiesToSend.Recoverable = true;

            if (msMQDetails.QueueType == MSMQType.Private)
            {
                //queue.Path = "FormatName:Direct=OS:" + msMQDetails.ServerName + @"\Private$\" + msMQDetails.QueueName;
                if (msMQDetails.ServerName.Contains('.') && msMQDetails.ServerName != ".") //i.e. IP address is given for the server
                    queue.Path = "FormatName:Direct=TCP:" + msMQDetails.ServerName + @"\Private$\" + msMQDetails.QueueName;
                else
                    queue.Path = "FormatName:Direct=OS:" + msMQDetails.ServerName + @"\Private$\" + msMQDetails.QueueName;
                //queue.Path = msMQDetails.ServerName + @"\Private$\" + msMQDetails.QueueName;
            }
            else if (msMQDetails.QueueType == MSMQType.Public)
                queue.Path = msMQDetails.ServerName + @"\" + msMQDetails.QueueName;

            //assign the formatter with the target type of the message body
            ((XmlMessageFormatter)queue.Formatter).TargetTypes = new Type[] { typeof(string) };

            try
            {
                //check if the msmq exists
                //if (MessageQueue.Exists(queue.Path)) //commented as the with new format for the queue path, the check for existence doesnt work
                {
                    switch (operation)
                    {
                        case MSMQOperationType.Send:
                            //check the MSMQ send pattern configured
                            switch (msMQDetails.SendPattern)
                            {
                                case MSMQSendPattern.None:
                                    LifLogHandler.LogDebug("MSMQ Adapter (transport- {0})- Send pattern configured- None", LifLogHandler.Layer.IntegrationLayer, msMQDetails.TransportName);    
                                    //set the message dequeue count also
                                    queue.Send(message, msMQDetails.MessageLabel + "$0");
                                    response = SUCCESSFUL_SENT_MESSAGE;
                                    queue.Close();
                                    errorCount = 0;
                                    break;
                                case MSMQSendPattern.RoundRobin:
                                    LifLogHandler.LogDebug("MSMQ Adapter (RoundRobin, transport- {0})- Send pattern configured- RoundRobin", LifLogHandler.Layer.IntegrationLayer, msMQDetails.TransportName);
                                    //get the last queue populated from memory cache (if any), and then add the subsequent queue name 
                                    ObjectCache memoryCache = MemoryCache.Default;
                                    CacheItemPolicy cachePolicy = new CacheItemPolicy();
                                    
                                    //to make sure that the last populated queue will never get removed from cache unless explicitly updated
                                    cachePolicy.Priority = CacheItemPriority.NotRemovable; 

                                    //check if there is any queue name in the cache
                                    string lastQueuePopulated = "", nextQueueTobePopulated="";
                                    if(memoryCache.Contains(KEY_FOR_LAST_QUEUE_POPULATED + msMQDetails.TransportName))
                                    {
                                        lastQueuePopulated = memoryCache[KEY_FOR_LAST_QUEUE_POPULATED + msMQDetails.TransportName] as string;
                                        LifLogHandler.LogDebug("MSMQ Adapter (RoundRobin, transport- {0})- last queue populated-" + lastQueuePopulated, LifLogHandler.Layer.IntegrationLayer, msMQDetails.TransportName);
                                    }

                                    //get the next queue to be populated
                                    nextQueueTobePopulated = GetNextQueueTobePopulatedForRoundRobin(msMQDetails.QueueName, lastQueuePopulated, msMQDetails.SecondaryQueues);
                                    //update the memory cache with the new queue name
                                    memoryCache.Set(KEY_FOR_LAST_QUEUE_POPULATED + msMQDetails.TransportName, nextQueueTobePopulated, cachePolicy);
                                    LifLogHandler.LogDebug("MSMQ Adapter (RoundRobin, transport- {0})- updating memory cache with queue name as-" + nextQueueTobePopulated, LifLogHandler.Layer.IntegrationLayer, msMQDetails.TransportName);

                                    //update the queue path
                                    queue.Path = queue.Path.Substring(0, queue.Path.LastIndexOf('\\') + 1) + nextQueueTobePopulated;
                                    //set the message dequeue count also
                                    queue.Send(message, msMQDetails.MessageLabel + "$0");
                                    response = SUCCESSFUL_SENT_MESSAGE;
                                    queue.Close();
                                    errorCount = 0;
                                    break;
                                case MSMQSendPattern.QueueLoad:
                                    LifLogHandler.LogDebug("MSMQ Adapter (QueueLoad, transport- {0})- Send pattern configured- QueueLoad", LifLogHandler.Layer.IntegrationLayer, msMQDetails.TransportName);
                                    //get the last queue populated from memory cache (if any), and then add the subsequent queue name 
                                    memoryCache = MemoryCache.Default;
                                    cachePolicy = new CacheItemPolicy();

                                    //to make sure that the last populated queue will never get removed from cache unless explicitly updated
                                    cachePolicy.Priority = CacheItemPriority.NotRemovable;

                                    //check if there is any queue name in the cache
                                    lastQueuePopulated = ""; 
                                    nextQueueTobePopulated = "";
                                    if (memoryCache.Contains(KEY_FOR_LAST_QUEUE_POPULATED + msMQDetails.TransportName))
                                    {
                                        lastQueuePopulated = memoryCache[KEY_FOR_LAST_QUEUE_POPULATED + msMQDetails.TransportName] as string;
                                        LifLogHandler.LogDebug("MSMQ Adapter (QueueLoad, transport- {0})- last queue populated-" + lastQueuePopulated, LifLogHandler.Layer.IntegrationLayer, msMQDetails.TransportName);
                                    }
                                    else
                                    {
                                        lastQueuePopulated = msMQDetails.QueueName;
                                        memoryCache.Set(KEY_FOR_LAST_QUEUE_POPULATED + msMQDetails.TransportName, lastQueuePopulated, cachePolicy);
                                        LifLogHandler.LogDebug("MSMQ Adapter (QueueLoad, transport- {0})- updating memory cache with queue name as-" + lastQueuePopulated, LifLogHandler.Layer.IntegrationLayer, msMQDetails.TransportName);
                                    }
                                    
                                    //check the message count for the last populated queue
                                    if(!string.IsNullOrEmpty(lastQueuePopulated))
                                        queue.Path = queue.Path.Substring(0, queue.Path.LastIndexOf('\\') + 1) + lastQueuePopulated;

                                    if (queue.GetAllMessages().Count() >= msMQDetails.QueueLoadLimit)
                                    {
                                        LifLogHandler.LogDebug("MSMQ Adapter (QueueLoad, transport- {0})- queue is full hence getting the next queue", LifLogHandler.Layer.IntegrationLayer, msMQDetails.TransportName);
                                        //get the next probable queue to be populated
                                        nextQueueTobePopulated = GetNextQueueTobePopulatedForQueueLoad(msMQDetails.QueueName, lastQueuePopulated, msMQDetails.SecondaryQueues, msMQDetails.QueueLoadLimit, queue);
                                        queue.Path = queue.Path.Substring(0, queue.Path.LastIndexOf('\\') + 1) + nextQueueTobePopulated;

                                        //update the memory cache with the new queue name
                                        memoryCache.Set(KEY_FOR_LAST_QUEUE_POPULATED + msMQDetails.TransportName, nextQueueTobePopulated, cachePolicy);
                                        LifLogHandler.LogDebug("MSMQ Adapter (QueueLoad, transport- {0})- updating memory cache with queue name as-" + nextQueueTobePopulated, LifLogHandler.Layer.IntegrationLayer, msMQDetails.TransportName);
                                    }                          

                                    //set the message dequeue count also
                                    queue.Send(message, msMQDetails.MessageLabel + "$0");
                                    response = SUCCESSFUL_SENT_MESSAGE;
                                    queue.Close();
                                    errorCount = 0;
                                    break;
                                default:
                                    LifLogHandler.LogDebug("MSMQ Adapter- Right Send pattern configuration is missing, hence considering - None", LifLogHandler.Layer.IntegrationLayer);
                                    //set the message dequeue count also
                                    queue.Send(message, msMQDetails.MessageLabel + "$0");
                                    response = SUCCESSFUL_SENT_MESSAGE;
                                    queue.Close();
                                    errorCount = 0;
                                    break;
                            }
                            
                            break;
                        case MSMQOperationType.Receive:
                            //threadName = Guid.NewGuid().ToString();
                            tempMsMQDetails = msMQDetails;
                            Thread thr = Thread.CurrentThread;
                            if (tempMsMQDetails.QueueReadingMode == MSMQReadMode.Async)
                            {
                                try { 
                                    LifLogHandler.LogDebug("MSMQ Adapter- Receive in ASYNC mode is requested", LifLogHandler.Layer.IntegrationLayer);

                                    //constructQueueForDelete();
                                    queueForDelete = queue;
                                    //queue.ReceiveCompleted += new ReceiveCompletedEventHandler(queue_ReceiveCompleted);
                                    //Receive is internally calling Peek but the deletion of message is handled by the framework itself.
                                    //this is to avoid missing the message ina any Unfortunate circumstances.
                                    //Commented for queue_PeekCompletedForReceive 
                                    // queue.PeekCompleted += new PeekCompletedEventHandler(queue_PeekCompletedForReceive);
                                    queue.ReceiveCompleted += new ReceiveCompletedEventHandler(queue_ReceiveCompletedForAsync);

                                    queue.MessageReadPropertyFilter.SetDefaults();

                                    while (true)
                                    {
                                        // the following properties had to be explicitly included in async as they were being reset
                                        // when the message was traffic was high above 1000 msgs being received in matter of 2-3 seconds.
                                        queue.MessageReadPropertyFilter.Label = true;
                                        queue.MessageReadPropertyFilter.Body = true;
                                        queue.MessageReadPropertyFilter.ArrivedTime = true;
                                        DateTime currentDateTime = DateTime.Now;
                                        string currentTime = currentDateTime.ToString(dateFormat);
                                      //  int currentThreadId = thr.ManagedThreadId;
                                        //string currentIdentity = currentThreadId + "_" + currentTime;

                                        LifLogHandler.LogDebug("MSMQ Adapter- Receive in ASYNC mode, Queue Name {0}, Can Read {1}, QueueReadTimeout {2}, CurrentIdentity {3}",
                                        LifLogHandler.Layer.IntegrationLayer, tempMsMQDetails.QueueName, queue.CanRead, tempMsMQDetails.QueueReadTimeout, currentTime);


                                        //   queue.BeginPeek(new TimeSpan(tempMsMQDetails.QueueReadTimeout), currentDateTime);
                                        // Incase of Async message processing, System receives the message from queue and process it
                                        queue.BeginReceive(new TimeSpan(tempMsMQDetails.QueueReadTimeout), currentDateTime);

                                        TimeSpan processedFor = DateTime.Now.Subtract(lastProcessedTime);
                                        double timeInSecond = processedFor.TotalSeconds;
                                        if (timeInSecond >= tempMsMQDetails.TransactionWaitTime)
                                        {
                                            LifLogHandler.LogDebug("MSMQ Adapter- Queue Name {0} is not processed more than {1) second, lastProcessedTime {2}",
                                       LifLogHandler.Layer.IntegrationLayer, tempMsMQDetails.QueueName, tempMsMQDetails.TransactionWaitTime, lastProcessedTime.ToString(dateFormat));
                                            String errorMessage = "Queue Name: " + tempMsMQDetails.QueueName + " is not processed more than "+
                                                tempMsMQDetails.TransactionWaitTime + " second";
                                            throw new QueueNotRespondedException(errorMessage);
                                        }


                                        if (!tempMsMQDetails.ContinueToReceive) {
                                            LifLogHandler.LogDebug("MSMQ Adapter- Inside not ContinueToReceive, Queue Name {0},ContinueToReceive value {1} ",
                                       LifLogHandler.Layer.IntegrationLayer, tempMsMQDetails.QueueName, tempMsMQDetails.ContinueToReceive);
                                            break;
                                        }
                                       // LifLogHandler.LogDebug("MSMQ Adapter- Receive in ASYNC mode, Queue Name {0},Queue PollingRestDuration value {1} ",
                                       //LifLogHandler.Layer.IntegrationLayer, tempMsMQDetails.QueueName, tempMsMQDetails.PollingRestDuration);
                                        Thread.Sleep(tempMsMQDetails.PollingRestDuration);
                                        errorCount = 0;
                                    }                                    
                                    LifLogHandler.LogDebug("MSMQ Adapter- Receive in ASYNC mode after while loop, Current thread state:{0}, Queue Name {1} ",
                                        LifLogHandler.Layer.IntegrationLayer, thr.ThreadState, tempMsMQDetails.QueueName);
                                }
                             catch (Exception ex)
                                {                                    
                                    LifLogHandler.LogError("MSMQ Adapter- Receive in ASYNC mode, unexpected Exception occured. Current thread state:{0}, Queue Name {1} ," +
                                        " Exception Message: {2} and Exception StackTrace: {3}",
                                        LifLogHandler.Layer.IntegrationLayer, thr.ThreadState,tempMsMQDetails.QueueName,ex.Message, ex.StackTrace);
                                    throw ex;
                                }
                                
                                //response = SUCCESSFUL_RECEIVE_MESSAGE;
                            }
                            else if (tempMsMQDetails.QueueReadingMode == MSMQReadMode.Sync)
                            {
                                LifLogHandler.LogDebug("MSMQ Adapter- Receive in SYNC mode is requested", LifLogHandler.Layer.IntegrationLayer);
                                
                                while (true)
                                {
                                    //the below boolean to be used in case the received message is to be put back as it is
                                    bool isReceived = false;
                                    try
                                    {
                                        LifLogHandler.LogDebug("Queue.CanRead for queue {0} = {1}",
                                            LifLogHandler.Layer.IntegrationLayer, tempMsMQDetails.QueueName, queue.CanRead);

                                        //responseMessage = queue.Receive(new TimeSpan(msMQDetails.QueueReadTimeout));
                                        DateTime initiatedTime = DateTime.Now;
                                        responseMessage = queue.Peek(new TimeSpan(tempMsMQDetails.QueueReadTimeout));
                                        LifLogHandler.LogDebug("MSMQ Adapter HandleMessage Receive Sync - QueueName {0}, PeekTimeDiffInMilliseconds {1}",
                   LifLogHandler.Layer.IntegrationLayer, tempMsMQDetails.QueueName, DateTime.Now.Subtract(initiatedTime).TotalMilliseconds);

                                        //queue.Close();
                                        response = SUCCESSFUL_RECEIVE_MESSAGE;
                                        bool messageToBeDeleted = false;
                                        if (responseMessage != null)
                                        {
                                            LifLogHandler.LogDebug("MSMQ Adapter HandleMessage Receive Sync- responsemessage is peeked and is not null", LifLogHandler.Layer.IntegrationLayer);
                                            //Check if message is a valid msg - it is not have an invalid xml format or empty message. Need to do as there is no property
                                            //which can read length of message to confirm an empty message
                                            string messageLabel = "";
                                            string messageId = "";
                                            bool isMessagValid = false;
                                            try
                                            {
                                                messageLabel = responseMessage.Label;
                                                messageId = responseMessage.Id;
                                                string msgTest = (string)responseMessage.Body; // just to test if message body is valid.
                                                if (string.IsNullOrEmpty(msgTest))
                                                {
                                                    //Remove from main queue and Move message to poison queue as message is empty
                                                    LifLogHandler.LogError("MSMQ Adapter HandleMessage Receive Sync- message with label {0}, messageid {1} is Invalid (Message is Empty)", LifLogHandler.Layer.IntegrationLayer, messageLabel, messageId);
                                                    queue.ReceiveById(messageId, new TimeSpan(tempMsMQDetails.QueueReadTimeout));

                                                    LifLogHandler.LogDebug("MSMQ Adapter HandleMessage Receive Sync - QueueName {0},messageId {1}, ReceiveByIdTimeDiffInMilliseconds {2}",
                  LifLogHandler.Layer.IntegrationLayer, tempMsMQDetails.QueueName, messageId,DateTime.Now.Subtract(initiatedTime).TotalMilliseconds);

                                                    SendToPoisonQueue(queue, string.Empty, messageLabel, messageId);
                                                }
                                                else
                                                {
                                                    isMessagValid = true;
                                                    LifLogHandler.LogDebug("MSMQ Adapter HandleMessage Receive Sync- message with label {0}, messageid {1} is Valid", LifLogHandler.Layer.IntegrationLayer, messageLabel, messageId);
                                                    
                                                }

                                            }
                                            catch (Exception ex)
                                            {
                                                LifLogHandler.LogError("MSMQ Adapter HandleMessage Receive Sync- exception when checking for message validity message with label {0}, messageid {1}, Exception Message {2}", LifLogHandler.Layer.IntegrationLayer, messageLabel, messageId, ex.Message);
                                                if (ex.Message.ToLower().Contains("root element is missing"))
                                                {
                                                    LifLogHandler.LogError("MSMQ Adapter HandleMessage Receive Sync- message with label {0}, messageid {1} is Invalid (Root Element is Missing)", LifLogHandler.Layer.IntegrationLayer, messageLabel, messageId);
                                                    //Remove from main queue and Move message to Poison Queue as it is an invalid message or could be an empty message
                                                    queue.ReceiveById(messageId, new TimeSpan(tempMsMQDetails.QueueReadTimeout));

                                                    LifLogHandler.LogDebug("MSMQ Adapter HandleMessage Receive Sync - QueueName {0},messageId {1}, ReceiveByIdTimeDiffInMilliseconds {2}",
                  LifLogHandler.Layer.IntegrationLayer, tempMsMQDetails.QueueName, messageId, DateTime.Now.Subtract(initiatedTime).TotalMilliseconds);
                                                    
                                                    SendToPoisonQueue(queue, string.Empty, messageLabel, messageId);
                                                    
                                                }
                                                else
                                                {
                                                    throw ex;
                                                }                                                
                                            }
                                            //Process message if it is found to be valid
                                            if (isMessagValid)
                                            {
                                                //check if the message is already requested to be deleted
                                                LifLogHandler.LogDebug("MSMQ Adapter- checking if the delete for the received message is requested. Peeked Message id-" + responseMessage.Id + "and Label-" + responseMessage.Label, LifLogHandler.Layer.IntegrationLayer);


                                                /**
                                                *  The below delete logic does not work for Sync mode, While handling a message for processing,
                                                *   we are constructing new label and add as a new message into the queue,
                                                *   so the processed message (newly constructed message) does not pick immediately and
                                                *   need to wait to end of all the message being processed for delete so we are commenting this section.
                                                *   Added new logic for deleting message from queue
                                                * /
                                                
                                              /*
                                              foreach (string msg in messagesToDelete)
                                              {
                                                  if (msg == responseMessage.Id)
                                                  {
                                                      queue.ReceiveById(responseMessage.Id, new TimeSpan(msMQDetails.QueueReadTimeout));
                                                      messagesToDelete.Remove(msg);
                                                      LifLogHandler.LogDebug("MSMQ Adapter- YES, delete for this message is requested and hence deleted. Deleted Message id-" + responseMessage.Id, LifLogHandler.Layer.IntegrationLayer);
                                                      messageToBeDeleted = true;
                                                      break;
                                                  }
                                              } 
                                              */
                                                // New logic to handle deletion in SYNC mode
                                                for (int i = messagesToDelete.Count - 1; i >= 0; i--)
                                                {

                                                    string msg = messagesToDelete.ElementAtOrDefault(i);
                                                    LifLogHandler.LogDebug(String.Format("MSMQ Adapter- Handle Message Receive Sync, messagesToDelete count={0}, message={1},index {2}", messagesToDelete.Count, msg, i), LifLogHandler.Layer.IntegrationLayer);
                                                    try
                                                    {
                                                        if (!string.IsNullOrEmpty(msg))
                                                        {
                                                            queue.ReceiveById(msg, new TimeSpan(tempMsMQDetails.QueueReadTimeout));

                                                            LifLogHandler.LogDebug("MSMQ Adapter HandleMessage Receive Sync - QueueName {0},messageId {1}, ReceiveByIdTimeDiffInMilliseconds {2}",
                  LifLogHandler.Layer.IntegrationLayer, tempMsMQDetails.QueueName, msg, DateTime.Now.Subtract(initiatedTime).TotalMilliseconds);

                                                            LifLogHandler.LogDebug(String.Format("MSMQ Adapter- Handle Message Receive Sync, Deleted from Queue {0}", msg), LifLogHandler.Layer.IntegrationLayer);
                                                            if (!string.IsNullOrEmpty(messagesToDelete.ElementAtOrDefault(i)))
                                                                messagesToDelete.RemoveAt(i);
                                                        }
                                                        else
                                                        {
                                                            messagesToDelete.RemoveAt(i);
                                                        }

                                                    }
                                                    catch (Experimental.System.Messaging.MessageQueueException ex)
                                                    {
                                                        LifLogHandler.LogDebug(String.Format("MSMQ Adapter- Handle Message Receive Sync, message is already deleted. " +
                                                            "Message id ={0}, exception message {1}, Error Code {2}", msg, ex.Message, ex.MessageQueueErrorCode), LifLogHandler.Layer.IntegrationLayer);
                                                        if (ex.MessageQueueErrorCode == MessageQueueErrorCode.IOTimeout)
                                                        {
                                                            LifLogHandler.LogDebug(String.Format("MSMQ Adapter- Handle Message Receive Sync, Deleted from Queue {0}", msg), LifLogHandler.Layer.IntegrationLayer);
                                                            try
                                                            {
                                                                if (!string.IsNullOrEmpty(messagesToDelete.ElementAtOrDefault(i)))
                                                                    messagesToDelete.RemoveAt(i);
                                                            }
                                                            catch (ArgumentOutOfRangeException exception)
                                                            {
                                                                LifLogHandler.LogDebug("MSMQ Adapter- Handle Message Receive Sync - inside exception, ArgumentOutOfRangeException Occured in Exception Block and message {0} ", LifLogHandler.Layer.IntegrationLayer, exception.Message);
                                                                //Do nothing
                                                            }
                                                        }
                                                        else
                                                        {
                                                            throw ex;
                                                        }
                                                    }
                                                    catch (ArgumentOutOfRangeException exception)
                                                    {
                                                        LifLogHandler.LogDebug("MSMQ Adapter- Handle Message Receive Sync, ArgumentOutOfRangeException Occured in Exception Block and message {0} ", LifLogHandler.Layer.IntegrationLayer, exception.Message);
                                                        //Do nothing
                                                    }

                                                    LifLogHandler.LogDebug("MSMQ Adapter- Handle Message Receive Sync. YES, delete for this message is requested and hence deleted. Deleted Message id-" + msg, LifLogHandler.Layer.IntegrationLayer);
                                                    if (msg == responseMessage.Id)
                                                    {
                                                        messageToBeDeleted = true;
                                                    }

                                                }
                                                if (!messageToBeDeleted)
                                                {
                                                    LifLogHandler.LogDebug("MSMQ Adapter- checking if the message is being processed", LifLogHandler.Layer.IntegrationLayer);
                                                    //check if the message is being processed
                                                    if (IsBeingProcessed(responseMessage))
                                                    {
                                                        LifLogHandler.LogDebug("MSMQ Adapter- YES, message is being processed", LifLogHandler.Layer.IntegrationLayer);
                                                        //check if the processing timeout is reached
                                                        LifLogHandler.LogDebug("MSMQ Adapter- checking if the message processing timeout is reached", LifLogHandler.Layer.IntegrationLayer);
                                                        TimeSpan processedFor = DateTime.Now.Subtract(responseMessage.ArrivedTime);
                                                        double timeInMilliSecond = processedFor.TotalMilliseconds;
                                                        if (timeInMilliSecond >= tempMsMQDetails.MessaseInvisibilityTimeout)
                                                        {
                                                            responseMessage = queue.Receive(new TimeSpan(tempMsMQDetails.QueueReadTimeout));

                                                            LifLogHandler.LogDebug("MSMQ Adapter HandleMessage Receive Sync - QueueName {0}, ReceiveTimeDiffInMilliseconds {1}",
                  LifLogHandler.Layer.IntegrationLayer, tempMsMQDetails.QueueName, DateTime.Now.Subtract(initiatedTime).TotalMilliseconds);

                                                            isReceived = true;
                                                            LifLogHandler.LogDebug("MSMQ Adapter- YES, message processing timeout is reached", LifLogHandler.Layer.IntegrationLayer);
                                                            HandleMessageReappearance(responseMessage, tempMsMQDetails);
                                                        }
                                                        else
                                                        {
                                                            //do nothing
                                                            //this message is in-process and the processing timeout is not yet reached
                                                            LifLogHandler.LogDebug("MSMQ Adapter- NO, message processing is within timeout limit.",
                                                                LifLogHandler.Layer.IntegrationLayer, responseMessage.Id);
                                                        }
                                                    }
                                                    else
                                                    {
                                                        //i.e. the message is yet to be processed
                                                        responseMessage = queue.Receive(new TimeSpan(tempMsMQDetails.QueueReadTimeout));

                                                        LifLogHandler.LogDebug("MSMQ Adapter HandleMessage Receive Sync - QueueName {0}, ReceiveTimeDiffInMilliseconds {1}",
                 LifLogHandler.Layer.IntegrationLayer, tempMsMQDetails.QueueName, DateTime.Now.Subtract(initiatedTime).TotalMilliseconds);

                                                        isReceived = true;
                                                        LifLogHandler.LogDebug("MSMQ Adapter- NO, message is yet to be processed. Received Message id-" + responseMessage.Id + "and Label-" + responseMessage.Label, LifLogHandler.Layer.IntegrationLayer);
                                                        string newMessageId = HandleMessageProcessing(responseMessage, tempMsMQDetails);
                                                        if (!string.IsNullOrEmpty(newMessageId))
                                                        {
                                                            ReceiveEventArgs args = ConstructResponse(responseMessage, newMessageId);
                                                            if (Received != null)
                                                            {
                                                                Received(args);
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            LifLogHandler.LogDebug("Did not receive any message from queue {0}",
                                                LifLogHandler.Layer.IntegrationLayer, tempMsMQDetails.QueueName);
                                        }
                                    }
                                    catch (Experimental.System.Messaging.MessageQueueException ex)
                                    {
                                        if (ex.Message.Contains("Timeout for the requested operation has expired."))
                                        {
                                            //do nothing...
                                        }
                                        else
                                        {
                                            //throw ex;
                                            string error = ex.Message;
                                            if (ex.InnerException != null)
                                                error = error + ".Inner Exception- " + ex.InnerException.Message;
                                            //log error
                                            LifLogHandler.LogError("MSMQ Adapter- HandleMessage(inner) FAILED  for- {0}. Reason- {1}", LifLogHandler.Layer.IntegrationLayer, operation.ToString(), error);

                                            //then put the message back to the queue
                                            if (isReceived)
                                            {
                                                LifLogHandler.LogDebug("MSMQ Adapter- as exception is raised at HandleMessage(inner), putting the message to the queue, message label- " + responseMessage.Label, LifLogHandler.Layer.IntegrationLayer);
                                                if (msMQDetails.IsQueueTransactional)
                                                    queue.Send(responseMessage, responseMessage.Label, MessageQueueTransactionType.Single);
                                                else
                                                    queue.Send(responseMessage, responseMessage.Label);
                                            }
                                        }
                                    }
                                    catch (Exception ex2)
                                    {
                                        LifLogHandler.LogError("Unexpected error in MSMQAdapter: {0}\n{1}",
                                            LifLogHandler.Layer.IntegrationLayer, ex2, ex2.StackTrace);
                                    }
                                    finally
                                    {
                                        queue.Close();
                                    }
                                    if (!msMQDetails.ContinueToReceive)
                                        break;
                                    Thread.Sleep(msMQDetails.PollingRestDuration);
                                    errorCount = 0;
                                }
                               
                            }
                            break;
                        case MSMQOperationType.Peek:
                            if (msMQDetails.QueueReadingMode == MSMQReadMode.Async)
                            {
                                queue.PeekCompleted += new PeekCompletedEventHandler(queue_PeekCompleted);
                                queue.BeginPeek(new TimeSpan(msMQDetails.QueueReadTimeout));
                                response = SUCCESSFUL_PEEK_MESSAGE;
                                errorCount = 0;
                            }
                            else if (msMQDetails.QueueReadingMode == MSMQReadMode.Sync)
                            {
                                responseMessage = queue.Peek(new TimeSpan(msMQDetails.QueueReadTimeout));
                                queue.Close();
                                response = SUCCESSFUL_PEEK_MESSAGE;

                                ReceiveEventArgs args = ConstructResponse(responseMessage, responseMessage.Id);
                                if (Received != null)
                                {
                                    Received(args);
                                }
                                errorCount = 0;
                            }

                            break;
                    }
                }
                //else
                //{
                //    response = QUEUE_NOTFOUND;
                //    ReceiveEventArgs args = ConstructResponse(null);
                //    if (Received != null)
                //    {
                //        Received(args);
                //    }
                //}
            }
            catch (Exception ex)
            {
                queue.Close();
                try { 
                    queueForDelete.Close();
                } catch (Exception e)
                {
                    LifLogHandler.LogError("MSMQ Adapter- HandleMessage FAILED in queueForDelete close for- {0} and Queue Name {1}. Reason- {2}", 
                        LifLogHandler.Layer.IntegrationLayer, operation.ToString(),tempMsMQDetails.QueueName, e.Message);
                }            
                queueForDelete = null;
                response = ex.Message;
                if (ex.InnerException != null)
                    response = response + ".Inner Exception- " + ex.InnerException.Message;
                //log error
                LifLogHandler.LogError("MSMQ Adapter- HandleMessage(outer) FAILED for- {0}. Reason- {1}", LifLogHandler.Layer.IntegrationLayer, operation.ToString(), response);
                ReceiveEventArgs args = ConstructResponse(null);
                if (Received != null)
                {
                    Received(args);
                }
                throw ex; //sid 26-SEP-2020 Uncommented as this was causing some critical exceptions to be consumed and the trhead is going in sleep mode
            }// finally queue.Close(); ignore exception
            errorCount = 0;
            return response;
        }        

        void queue_PeekCompleted(object sender, PeekCompletedEventArgs e)
        {
            MessageQueue queue = ((MessageQueue)sender);
            responseMessage = queue.EndPeek(e.AsyncResult);
            queue.Close();
            //check if asyncresult has any property which tells the status of the peek operation.

            ReceiveEventArgs args = ConstructResponse(responseMessage, responseMessage.Id);
            if (Received != null)
            {
                Received(args);
            }
        }

        void queue_PeekCompletedForReceive(object sender, PeekCompletedEventArgs e)
        {
            MessageQueue queue = ((MessageQueue)sender);
            //the below boolean to be used in case the received message is to be put back as it is
            bool isReceived = false;
            Message latestResponseMessage = null;
            try
            {
                lastProcessedTime = DateTime.Now;

                LifLogHandler.LogDebug("MSMQ Adapter queue_PeekCompletedForReceive- Inside Method and QueueName {0},lastProcessedTime {1}", 
                    LifLogHandler.Layer.IntegrationLayer, tempMsMQDetails.QueueName, lastProcessedTime.ToString(dateFormat));

                latestResponseMessage = queue.EndPeek(e.AsyncResult);

                DateTime initiatedTime = (DateTime)e.AsyncResult.AsyncState;
                double timeDiffInMilliseconds = lastProcessedTime.Subtract(initiatedTime).TotalMilliseconds;
                LifLogHandler.LogDebug("MSMQ Adapter queue_PeekCompletedForReceive- QueueName {0},lastProcessedTime {1},Queue_BeginPeekTime {2},timeDiffInMilliseconds {3}",
                    LifLogHandler.Layer.IntegrationLayer, tempMsMQDetails.QueueName, lastProcessedTime.ToString(dateFormat), initiatedTime.ToString(dateFormat), timeDiffInMilliseconds);


                //queue.Close();

                if (latestResponseMessage != null)
                {
                    LifLogHandler.LogDebug("MSMQ Adapter queue_PeekCompletedForReceive- responsemessage is peeked and is not null", LifLogHandler.Layer.IntegrationLayer);
                    response = SUCCESSFUL_RECEIVE_MESSAGE;
                    //Check if message is a valid msg - it is not have an invalid xml format or empty message. Need to do as there is no property
                    //which can read length of message to confirm an empty message
                    string messageLabel="";
                    string messageId = "";

                    try
                    {
                       messageLabel = latestResponseMessage.Label;
                       messageId = latestResponseMessage.Id;

                        LifLogHandler.LogDebug("MSMQ Adapter queue_PeekCompletedForReceive- QueueName {0} , message with label {1}, messageid {2} check to see if message is valid", 
                           LifLogHandler.Layer.IntegrationLayer, tempMsMQDetails.QueueName,messageLabel, messageId);

                        string msgTest = (string)latestResponseMessage.Body; // just to test if message body is valid.

                        if (string.IsNullOrEmpty(msgTest))
                        {
                            //Remove from main queue and Move message to poison queue as message is empty
                            LifLogHandler.LogError("MSMQ Adapter queue_PeekCompletedForReceive- message with label {0}, messageid {1} is Invalid (Message is Empty)", LifLogHandler.Layer.IntegrationLayer, messageLabel, messageId);

                            queue.ReceiveById(messageId, new TimeSpan(tempMsMQDetails.QueueReadTimeout));

                            LifLogHandler.LogDebug("MSMQ Adapter ReceiveById- QueueName {0},messageId {1}, ReceiveByIdTimeDiffInMilliseconds {2}",
                   LifLogHandler.Layer.IntegrationLayer, tempMsMQDetails.QueueName, messageId, DateTime.Now.Subtract(initiatedTime).TotalMilliseconds);

                            SendToPoisonQueue(queue, string.Empty, messageLabel, messageId);
                            return;

                        }
                        LifLogHandler.LogDebug("MSMQ Adapter queue_PeekCompletedForReceive- message with label {0}, messageid {1} is Valid", LifLogHandler.Layer.IntegrationLayer, messageLabel, messageId);

                    }
                    catch(Exception ex)
                    {
                        LifLogHandler.LogError("MSMQ Adapter queue_PeekCompletedForReceive- exception when checking for message validity message with label {0}, messageid {1}, Exception Message {2}, Exception Stack Trace {3}", LifLogHandler.Layer.IntegrationLayer, messageLabel, messageId,ex.Message,ex.StackTrace);
                        if (ex.Message.ToLower().Contains("root element is missing"))
                        {
                            LifLogHandler.LogError("MSMQ Adapter queue_PeekCompletedForReceive- message with label {0}, messageid {1} is Invalid (Root Element is Missing)", LifLogHandler.Layer.IntegrationLayer, messageLabel, messageId);
                            //Remove from main queue and Move to Poison Queue as it is an invalid message or could be an empty message
                            queue.ReceiveById(messageId, new TimeSpan(tempMsMQDetails.QueueReadTimeout));

                            LifLogHandler.LogDebug("MSMQ Adapter ReceiveById- QueueName {0},messageId {1}, ReceiveByIdTimeDiffInMilliseconds {2}",
                   LifLogHandler.Layer.IntegrationLayer, tempMsMQDetails.QueueName, messageId, DateTime.Now.Subtract(initiatedTime).TotalMilliseconds);

                            SendToPoisonQueue(queue,string.Empty, messageLabel, messageId);
                            return;
                        }

                        throw ex;
                    }

                    bool messageToBeDeleted = false;
                    //check if the message is already requested to be deleted
                    LifLogHandler.LogDebug("MSMQ Adapter- checking if the delete for the received message is requested", LifLogHandler.Layer.IntegrationLayer);
                    //foreach (string msg in messagesToDelete)
                    //{
                    //    if (msg == responseMessage.Id)
                    //    {
                    //        queue.ReceiveById(responseMessage.Id, new TimeSpan(tempMsMQDetails.QueueReadTimeout));
                    //        messagesToDelete.Remove(msg);
                    //        LifLogHandler.LogDebug("MSMQ Adapter- YES, delete for this message is requested and hence deleted", LifLogHandler.Layer.IntegrationLayer);
                    //        messageToBeDeleted = true;
                    //        break;
                    //    }
                    //}
                    // New logic to handle deletion in ASYNC mode
                    
                    for (int i = messagesToDelete.Count - 1; i >= 0; i--)
                    {

                        LifLogHandler.LogDebug(String.Format("MSMQ Adapter- queue_PeekCompletedForReceive, messagesToDelete count={0}, index {1}",
                            messagesToDelete.Count,i), LifLogHandler.Layer.IntegrationLayer);

                        string msg = messagesToDelete.ElementAtOrDefault(i);

                        LifLogHandler.LogDebug(String.Format("MSMQ Adapter- queue_PeekCompletedForReceive, messagesToDelete count={0}, message={1},index {2}", messagesToDelete.Count, msg,i), LifLogHandler.Layer.IntegrationLayer);
                        try
                        {
                            if(!string.IsNullOrEmpty(msg))
                            {
                                queue.ReceiveById(msg, new TimeSpan(tempMsMQDetails.QueueReadTimeout));

                                LifLogHandler.LogDebug("MSMQ Adapter ReceiveById- QueueName {0},messageId {1}, ReceiveByIdTimeDiffInMilliseconds {2}",
                   LifLogHandler.Layer.IntegrationLayer, tempMsMQDetails.QueueName, msg, DateTime.Now.Subtract(initiatedTime).TotalMilliseconds);

                            }
                            else
                            {
                                messagesToDelete.RemoveAt(i);
                            }                            
                            LifLogHandler.LogDebug(String.Format("MSMQ Adapter- queue_PeekCompletedForReceive, Deleted from Queue {0}", msg), LifLogHandler.Layer.IntegrationLayer);
                            if(!string.IsNullOrEmpty(messagesToDelete.ElementAtOrDefault(i)))
                                messagesToDelete.RemoveAt(i);
                        } catch (Experimental.System.Messaging.MessageQueueException ex)
                        {
                            LifLogHandler.LogDebug(String.Format("MSMQ Adapter- queue_PeekCompletedForReceive, message is already deleted. " +
                                "Message id ={0}, exception message {1}, Error Code {2}", msg, ex.Message,ex.MessageQueueErrorCode), LifLogHandler.Layer.IntegrationLayer);
                            if (ex.MessageQueueErrorCode == MessageQueueErrorCode.IOTimeout)
                            {
                                LifLogHandler.LogDebug(String.Format("MSMQ Adapter- queue_PeekCompletedForReceive, Deleted from Queue {0}", msg), LifLogHandler.Layer.IntegrationLayer);
                                try
                                {
                                    if (!string.IsNullOrEmpty(messagesToDelete.ElementAtOrDefault(i)))
                                        messagesToDelete.RemoveAt(i);
                                }
                                catch (ArgumentOutOfRangeException exception)
                                {
                                    LifLogHandler.LogDebug("MSMQ Adapter- queue_PeekCompletedForReceive, ArgumentOutOfRangeException Occured in Exception Block and message {0} ", LifLogHandler.Layer.IntegrationLayer, exception.Message);
                                    //Do nothing
                                }
                            } else
                            {
                                throw ex;
                            }                            
                        }
                        catch (ArgumentOutOfRangeException ex)
                        {
                            LifLogHandler.LogDebug("MSMQ Adapter- queue_PeekCompletedForReceive, ArgumentOutOfRangeException Occured and message {0} ", LifLogHandler.Layer.IntegrationLayer,ex.Message);
                            //Do nothing
                        }

                        LifLogHandler.LogDebug("MSMQ Adapter- queue_PeekCompletedForReceive. YES, delete for this message is requested and hence deleted. Deleted Message id-" + msg, LifLogHandler.Layer.IntegrationLayer);
                        if (msg == latestResponseMessage.Id)
                        {
                            messageToBeDeleted = true;
                        }

                    }
                    if (!messageToBeDeleted)
                    {
                        //check if the message is being processed
                        LifLogHandler.LogDebug("MSMQ Adapter- checking if the message is being processed, Message Id {0}", LifLogHandler.Layer.IntegrationLayer, latestResponseMessage.Id);
                       
                        if (IsBeingProcessed(latestResponseMessage))
                        {
                            LifLogHandler.LogDebug("MSMQ Adapter- YES, message is being processed", LifLogHandler.Layer.IntegrationLayer);
                            //check if the processing timeout is reached
                            LifLogHandler.LogDebug("MSMQ Adapter- checking if the message processing timeout is reached", LifLogHandler.Layer.IntegrationLayer);
                            
                            TimeSpan processedFor = DateTime.Now.Subtract(latestResponseMessage.ArrivedTime);
                            double timeInMilliSecond = processedFor.TotalMilliseconds;
                            LifLogHandler.LogDebug("MSMQ Adapter- Queue Name {0}, Message Id {1}, Queue Arrived Time {2},timeInMilliSecond {3},MessaseInvisibilityTimeout {4}", 
                                LifLogHandler.Layer.IntegrationLayer, tempMsMQDetails.QueueName, latestResponseMessage.Id, latestResponseMessage.ArrivedTime, 
                                timeInMilliSecond, tempMsMQDetails.MessaseInvisibilityTimeout);
                            if (timeInMilliSecond >= tempMsMQDetails.MessaseInvisibilityTimeout)
                            {
                                latestResponseMessage = queue.Receive(new TimeSpan(tempMsMQDetails.QueueReadTimeout));

                                LifLogHandler.LogDebug("MSMQ Adapter- Receive- QueueName {0}, ReceiveTimeDiffInMilliseconds {1}",
                  LifLogHandler.Layer.IntegrationLayer, tempMsMQDetails.QueueName,  DateTime.Now.Subtract(initiatedTime).TotalMilliseconds);

                                isReceived = true;
                                LifLogHandler.LogDebug("MSMQ Adapter- YES, message processing timeout is reached", LifLogHandler.Layer.IntegrationLayer);
                                HandleMessageReappearance(latestResponseMessage, tempMsMQDetails);
                            }
                            else
                            {
                                //do nothing
                                //this message is in-process and the processing timeout is not yet reached
                            }
                        }
                        else
                        {
                            //i.e. the message is yet to be processed
                            latestResponseMessage = queue.Receive(new TimeSpan(tempMsMQDetails.QueueReadTimeout));

                            LifLogHandler.LogDebug("MSMQ Adapter Receive- QueueName {0}, ReceiveTimeDiffInMilliseconds {1}",
                    LifLogHandler.Layer.IntegrationLayer, tempMsMQDetails.QueueName, DateTime.Now.Subtract(initiatedTime).TotalMilliseconds);

                            isReceived = true;
                            LifLogHandler.LogDebug("MSMQ Adapter- NO, message is yet to be processed", LifLogHandler.Layer.IntegrationLayer);
                            string newMessageId = HandleMessageProcessing(latestResponseMessage, tempMsMQDetails);
                            if (!string.IsNullOrEmpty(newMessageId))
                            {
                                ReceiveEventArgs args = ConstructResponse(latestResponseMessage, newMessageId);
                                if (Received != null)
                                {
                                    Received(args);
                                }
                                // debug to find
                            }
                        }
                    }
                }
            }
            catch (Experimental.System.Messaging.MessageQueueException ex)
            {
                if (ex.Message.Contains("Timeout for the requested operation has expired."))
                {
                    //do nothing...
                }
                else
                {
                    response = ex.Message;
                    if (ex.InnerException != null)
                        response = response + ".Inner Exception- " + ex.InnerException.Message;
                    //log error
                   // LifLogHandler.LogError("MSMQ Adapter- Peek For Receive FAILED. MessageQueueException Reason- " + response, LifLogHandler.Layer.IntegrationLayer);
                    LifLogHandler.LogError("MSMQ Adapter- Peek For Receive FAILED, MessageQueueException occured. Exception Message: {0} and Exception StackTrace: {1}",
                      LifLogHandler.Layer.IntegrationLayer, response, ex.StackTrace);

                    //then put the message back to the queue
                    if (isReceived)
                    {
                        LifLogHandler.LogDebug("MSMQ Adapter- Peek For Receive, as exception is raised at queue_PeekCompletedForReceive, putting the message to the queue, message label- " + latestResponseMessage.Label, LifLogHandler.Layer.IntegrationLayer);
                        queue.Send(latestResponseMessage, latestResponseMessage.Label);
                    }
                }
            }
            catch (Exception ex)
            {
                response = ex.Message;
                if (ex.InnerException != null)
                    response = response + ".Inner Exception- " + ex.InnerException.Message;
                //log error
               // LifLogHandler.LogError("MSMQ Adapter- Peek For Receive FAILED.Exception Reason- " + response, LifLogHandler.Layer.IntegrationLayer);
                LifLogHandler.LogError("MSMQ Adapter- Peek For Receive FAILED, unexpected Exception occured. Exception Message: {0} and Exception StackTrace: {1}",
                       LifLogHandler.Layer.IntegrationLayer, response, ex.StackTrace);

                //then put the message back to the queue
                if (isReceived)
                {
                    LifLogHandler.LogDebug("MSMQ Adapter- Peek For Receive, as exception is raised at queue_PeekCompletedForReceive, putting the message to the queue, message label- " + latestResponseMessage.Label, LifLogHandler.Layer.IntegrationLayer);
                    queue.Send(latestResponseMessage, latestResponseMessage.Label);
                }


            }
            finally
            {
                queue.Close();
            }
        }


        /// <summary>
        /// Incase of Async message processing, System receives the message from queue and process it. To avoid multiple theard process the same message again and again. 
        /// Once system reveices the message, message will be taken from the queue, so next thread will process the next message.
        /// System keeps the message body in the memory, incase of any exception. it will construct the new message and send into queue for processing
        /// </summary>
        /// <param name="sender">Sender Object</param>
        /// <param name="e">Receive Completed Event Args</param>    
        void queue_ReceiveCompletedForAsync(object sender, ReceiveCompletedEventArgs e)
        {
            MessageQueue queue = ((MessageQueue)sender);
            //the below boolean to be used in case the received message is to be put back as it is
            bool isReceived = false;
            Message latestResponseMessage = null;
            try
            {
                lastProcessedTime = DateTime.Now;
                DateTime processStartedTime = DateTime.Now;
                LifLogHandler.LogDebug("MSMQ Adapter queue_ReceiveCompletedForAsync- Inside Method and QueueName {0},lastProcessedTime {1}",
                    LifLogHandler.Layer.IntegrationLayer, tempMsMQDetails.QueueName, lastProcessedTime.ToString(dateFormat));

                latestResponseMessage = queue.EndReceive(e.AsyncResult);
                

                DateTime initiatedTime = (DateTime)e.AsyncResult.AsyncState;
                double timeDiffInMilliseconds = lastProcessedTime.Subtract(initiatedTime).TotalMilliseconds;
                LifLogHandler.LogDebug("MSMQ Adapter queue_ReceiveCompletedForAsync- QueueName {0},lastProcessedTime {1},EndReceive {2},timeDiffInMilliseconds {3}",
                    LifLogHandler.Layer.IntegrationLayer, tempMsMQDetails.QueueName, lastProcessedTime.ToString(dateFormat), initiatedTime.ToString(dateFormat), timeDiffInMilliseconds);
                string messageLabel = "";
                string messageId = "";
                if (latestResponseMessage != null)
                {
                    response = SUCCESSFUL_RECEIVE_MESSAGE;
                    //Check if message is a valid msg - it is not have an invalid xml format or empty message. Need to do as there is no property
                    //which can read length of message to confirm an empty message
                    

                    try
                    {
                        messageLabel = latestResponseMessage.Label;
                        messageId = latestResponseMessage.Id;

                        LifLogHandler.LogDebug("MSMQ Adapter queue_ReceiveCompletedForAsync Received Message- QueueName {0} , messageId {1}, messageLabel {2} ",
                           LifLogHandler.Layer.IntegrationLayer, tempMsMQDetails.QueueName, messageId, messageLabel);

                        string msgTest = (string)latestResponseMessage.Body; // just to test if message body is valid.

                        if (string.IsNullOrEmpty(msgTest))
                        {
                            // Move message to poison queue as message is empty
                                                   
                            SendToPoisonQueue(queue, string.Empty, messageLabel, messageId);
                            return;

                        }
                        LifLogHandler.LogDebug("MSMQ Adapter queue_ReceiveCompletedForAsync- message with label {0}, messageid {1} is Valid", 
                            LifLogHandler.Layer.IntegrationLayer, messageLabel, messageId);

                    }
                    catch (Exception ex)
                    {
                        LifLogHandler.LogError("MSMQ Adapter queue_ReceiveCompletedForAsync- exception when checking for message validity message with label {0}, messageid {1}, " +
                            " QueueName {2}, Exception Message {3}, Exception Stack Trace {4}", LifLogHandler.Layer.IntegrationLayer, messageLabel, messageId, tempMsMQDetails.QueueName, 
                            ex.Message, ex.StackTrace);

                        if (ex.Message.ToLower().Contains("root element is missing"))
                        {
                            LifLogHandler.LogError("MSMQ Adapter queue_ReceiveCompletedForAsync- message with label {0}, messageid {1} , QueueName {2}" +
                                " is Invalid (Root Element is Missing)", LifLogHandler.Layer.IntegrationLayer, messageLabel, messageId, tempMsMQDetails.QueueName);
                            //Move to Poison Queue as it is an invalid message or could be an empty message
                            
                            SendToPoisonQueue(queue, string.Empty, messageLabel, messageId);
                            return;
                        }

                        throw ex;
                    }
                   if (tempMsMQDetails.MessageProcessingMaxCount <= 0)
                    {
                        LifLogHandler.LogDebug("MSMQ Adapter queue_ReceiveCompletedForAsync- " +
                            " MessageProcessingMaxCount less than 0, Message Id {0} , QueueName {1}, MessageProcessingMaxCount {2} ", 
                            LifLogHandler.Layer.IntegrationLayer, latestResponseMessage.Id, tempMsMQDetails.QueueName, tempMsMQDetails.MessageProcessingMaxCount);

                        ReceiveEventArgs args = ConstructResponse(latestResponseMessage, messageId);
                        if (Received != null)
                        {
                            Received(args);
                        }
                    }
                    else 
                    {
                        LifLogHandler.LogDebug("MSMQ Adapter queue_ReceiveCompletedForAsync- " +
                            " MessageProcessingMaxCount greater than 0, Message Id {0} , QueueName {1}, MessageProcessingMaxCount {2} ",
                            LifLogHandler.Layer.IntegrationLayer, latestResponseMessage.Id, tempMsMQDetails.QueueName, tempMsMQDetails.MessageProcessingMaxCount);

                        bool isMessageToBeResend = false;
                        try {
                            ReceiveEventArgs args = ConstructResponse(latestResponseMessage, latestResponseMessage.Id);
                            if (Received != null)
                            {
                                Received(args);
                            }
                        } catch (Exception ex)
                        {
                            //do nothing and resend the message again
                            isMessageToBeResend = true;
                            LifLogHandler.LogError("MSMQ Adapter queue_ReceiveCompletedForAsync- Received FAILED for " +
                                " QueueName {0} messageid {1}. Exception Message: {0} and Exception StackTrace: {1}",
                            LifLogHandler.Layer.IntegrationLayer, tempMsMQDetails.QueueName, latestResponseMessage.Id,ex.Message, ex.StackTrace);
                            
                        }
                        // Check if the message id exists in the messagesToDeleteForAsync. If Yes remove the message id from the messagesToDeleteForAsync
                        // Becasue the message is already processed successfully, so no need to process it again
                        // If the messgae id does not exit in the messagesToDeleteForAsync, delete operation did not call, so we need to process the message again.
                        // Need to consutruct the message and send it to Queue
                        if (messagesToDeleteForAsync.ContainsKey(latestResponseMessage.Id))
                        {
                            LifLogHandler.LogDebug("MSMQ Adapter queue_ReceiveCompletedForAsync- Message remvoed from messagesToDeleteForAsync" +
                                " QueueName {0},Message Id {1}",
                                LifLogHandler.Layer.IntegrationLayer, tempMsMQDetails.QueueName, latestResponseMessage.Id);
                            messagesToDeleteForAsync.Remove(latestResponseMessage.Id);
                        }else
                        {
                            LifLogHandler.LogDebug("MSMQ Adapter queue_ReceiveCompletedForAsync- Message not exist in messagesToDeleteForAsync" +
                                " QueueName {0},Message Id {1}",
                                LifLogHandler.Layer.IntegrationLayer, tempMsMQDetails.QueueName, latestResponseMessage.Id);

                            isMessageToBeResend = true;
                        }

                        if (isMessageToBeResend)
                        {
                            isReceived = true;
                            LifLogHandler.LogDebug("MSMQ Adapter queue_ReceiveCompletedForAsync- Resending the message, QueueName {0},Message Id {1}",
                                LifLogHandler.Layer.IntegrationLayer, tempMsMQDetails.QueueName, latestResponseMessage.Id);
                            // We need to increament the DeQueue count to next value 
                            HandleMessageReappearance(latestResponseMessage, tempMsMQDetails,true);                            
                        }                 
                        
                    }
                }

                LifLogHandler.LogDebug("MSMQ Adapter queue_ReceiveCompletedForAsync ProcessedTime- QueueName {0},messageId {1},messageLabel {2}, ToalTimeTakenInMilliseconds {3}",
                    LifLogHandler.Layer.IntegrationLayer, tempMsMQDetails.QueueName, messageId, messageLabel, DateTime.Now.Subtract(processStartedTime).TotalMilliseconds);

            }
            catch (Experimental.System.Messaging.MessageQueueException ex)
            {
                if (ex.Message.Contains("Timeout for the requested operation has expired."))
                {
                    //do nothing...
                }
                else
                {
                    response = ex.Message;
                    if (ex.InnerException != null)
                        response = response + ".Inner Exception- " + ex.InnerException.Message;
                    //log error
                    // LifLogHandler.LogError("MSMQ Adapter- Peek For Receive FAILED. MessageQueueException Reason- " + response, LifLogHandler.Layer.IntegrationLayer);
                    LifLogHandler.LogError("MSMQ Adapter queue_ReceiveCompletedForAsync- Receive FAILED, MessageQueueException occured. Exception Message: {0} and Exception StackTrace: {1}",
                      LifLogHandler.Layer.IntegrationLayer, response, ex.StackTrace);

                    //then put the message back to the queue
                    if (isReceived)
                    {
                        LifLogHandler.LogDebug("MSMQ Adapter queue_ReceiveCompletedForAsync- as exception is raised, putting the message to the queue, message label- " + latestResponseMessage.Label, LifLogHandler.Layer.IntegrationLayer);
                        queue.Send(latestResponseMessage, latestResponseMessage.Label);
                    }
                }
            }
            catch (Exception ex)
            {
                response = ex.Message;
                if (ex.InnerException != null)
                    response = response + ".Inner Exception- " + ex.InnerException.Message;
                //log error
                // LifLogHandler.LogError("MSMQ Adapter- Peek For Receive FAILED.Exception Reason- " + response, LifLogHandler.Layer.IntegrationLayer);
                LifLogHandler.LogError("MSMQ Adapter queue_ReceiveCompletedForAsync- Receive FAILED, unexpected Exception occured. Exception Message: {0} and Exception StackTrace: {1}",
                       LifLogHandler.Layer.IntegrationLayer, response, ex.StackTrace);

                //then put the message back to the queue
                if (isReceived)
                {
                    LifLogHandler.LogDebug("MSMQ Adapter queue_ReceiveCompletedForAsync- as exception is raised, putting the message to the queue, message label- " + latestResponseMessage.Label, LifLogHandler.Layer.IntegrationLayer);
                    queue.Send(latestResponseMessage, latestResponseMessage.Label);
                }


            }
            finally
            {
                queue.Close();
            }
        }

        private void SendToPoisonQueue(MessageQueue queue,string message, string messageLabel, string messageId)
        {
            LifLogHandler.LogDebug("MSMQ Adapter SendToPoisonQueue- message being dispatched to poison queue with label {0}, messageid {1} ", LifLogHandler.Layer.IntegrationLayer, messageLabel, messageId);
            string poisonQueuePath = "";
            //poisonQueuePath
            if (tempMsMQDetails.QueueType == MSMQType.Private)
            {
                poisonQueuePath = "FormatName:Direct=OS:" + tempMsMQDetails.ServerName + @"\Private$\" + tempMsMQDetails.PoisonQueueName;
                if (tempMsMQDetails.ServerName.Contains('.') && tempMsMQDetails.ServerName != ".") //i.e. IP address is given for the server
                    poisonQueuePath = "FormatName:Direct=TCP:" + tempMsMQDetails.ServerName + @"\Private$\" + tempMsMQDetails.PoisonQueueName;
                else
                    poisonQueuePath = "FormatName:Direct=OS:" + tempMsMQDetails.ServerName + @"\Private$\" + tempMsMQDetails.PoisonQueueName;
            }
            else if (tempMsMQDetails.QueueType == MSMQType.Public)
                poisonQueuePath = tempMsMQDetails.ServerName + @"\" + tempMsMQDetails.PoisonQueueName;

            //first close the normal transaction queue
            string mainQPath = queue.Path;
            queue.Close();
            queue.Path = poisonQueuePath;

            //set the queue to keep the message even after m/c start-up, this will make the msmq more available and reliable
            queue.DefaultPropertiesToSend.Recoverable = true;
            if (tempMsMQDetails.IsQueueTransactional)
                queue.Send(message, messageLabel, MessageQueueTransactionType.Single);
            else
                queue.Send(message, messageLabel);
            LifLogHandler.LogDebug("MSMQ Adapter SendToPoisonQueue- message is sent to the poison queue with label {0}, messageid {1}, poison queue name {2} ", LifLogHandler.Layer.IntegrationLayer, messageLabel, messageId, poisonQueuePath);
            queue.Close();
            queue.Path = mainQPath;
        }

        /// <summary>
        /// Constructs the response to be sent back to the client calling the Receive operation
        /// </summary>
        /// <param name="msg">the old message read from the queue</param>
        /// <param name="messageId">the Id of the newly added message</param>
        /// <returns>response to be sent back</returns>
        private ReceiveEventArgs ConstructResponse(Message msg, string newMessageId = "")
        {
            ReceiveEventArgs args = new ReceiveEventArgs();
            args.ResponseDetails = new ListDictionary();
            if (msg != null)
            {
                if (msg.Body != null)
                {
                    args.ResponseDetails.Add("MessageBody", msg.Body.ToString());
                }                
                args.ResponseDetails.Add("MessageBodyType", msg.BodyType);
                args.ResponseDetails.Add("MessageBodyStream", msg.BodyStream);
                args.ResponseDetails.Add("MessageIdentifier", newMessageId);
            }
            args.ResponseDetails.Add("Status", response);
            //assign the Status Code based on the "response"
            if (response == SUCCESSFUL_PEEK_MESSAGE || response == SUCCESSFUL_RECEIVE_MESSAGE)
                args.ResponseDetails.Add("StatusCode", SUCCESSFUL_STATUS_CODE);
            else
                args.ResponseDetails.Add("StatusCode", UNSUCCESSFUL_STATUS_CODE);

            return args;
        }

        /// <summary>
        /// Validates whether TransportName specified in the region, exists in MSMQDetails
        /// section. If it found, it returns corresponding MSMQDetails object.
        /// </summary>
        /// <param name="transportSection">MSMQ section</param>
        /// <param name="transportName">name of the transport</param>
        private MSMQDetails ValidateTransportName(Infosys.Lif.LegacyIntegratorService.MSMQ transportSection, string transportName)
        {
            MSMQDetails msMQDetails = null;
            bool isTransportNameExists = false;
            // Find the IBMMQ region to which it should connect for sending message.
            for (int count = 0; count < transportSection.MSMQDetailsCollection.Count; count++)
            {
                msMQDetails = transportSection.MSMQDetailsCollection[count] as MSMQDetails;
                if (msMQDetails.TransportName == transportName)
                {
                    isTransportNameExists = true;
                    break;
                }
            }
            // If MSMQ region is not set in the config then throw the exception
            if (!isTransportNameExists)
            {
                throw new LegacyException(transportName + " is not defined in MSMQDetails section");
            }
            return msMQDetails;
        }

        /// <summary>
        /// Checks if the currently peeked message is being processed or not.
        /// </summary>
        /// <param name="peekedMessage">the message peeked</param>
        /// <returns>true if it is being processed otherwise false</returns>
        private bool IsBeingProcessed(Message peekedMessage)
        {
            bool isBeingProcessed = false;
            LifLogHandler.LogDebug("MSMQ Adapter- IsBeingProcessed is called, Message Id {0}", LifLogHandler.Layer.IntegrationLayer, peekedMessage.Id);
            try
            {
                if(peekedMessage.Label != null)
                {
                    string[] labelParts = peekedMessage.Label.Split('$');
                    if (labelParts.Length == 3 && labelParts[2] == MessageProcessingStatus.InProcess.ToString())
                    {
                        isBeingProcessed = true;
                    }
                }
               
            } catch (Exception ex)
            {
                LifLogHandler.LogError("MSMQ Adapter- Exception in IsBeingProcessed, Exception Message{0} , Inner Exception Message {1} , Error Type {2}",
                    LifLogHandler.Layer.IntegrationLayer, ex.Message,ex.InnerException.Message,ex.GetType());
                throw ex;
            }
            
            LifLogHandler.LogDebug("MSMQ Adapter- IsBeingProcessed is completed, Message Id {0}", LifLogHandler.Layer.IntegrationLayer, peekedMessage.Id);
            return isBeingProcessed;
        }

        /// <summary>
        /// Changes the status of the peeked message to "InProcess and increments the message read count.
        /// </summary>
        /// <param name="peekedMessage">the message peeked</param>
        /// <param name="msmqDetails">the MSMQ access details</param>
        /// <returns>the Id of the newly added message</returns>
        private string HandleMessageProcessing(Message peekedMessage, MSMQDetails msmqDetails)
        {
            LifLogHandler.LogDebug("MSMQ Adapter- HandleMessageProcessing is called", LifLogHandler.Layer.IntegrationLayer);
            string newMessageId = "";
            MessageQueue queue = new MessageQueue();
            //bool isSuccess = false;
            try
            {
                if (peekedMessage.Label != null)
                {
                    string[] labelParts = peekedMessage.Label.Split('$');
                    string messageLabel = "";
                    if (labelParts.Length >= 2)
                    {
                        int dequeueCount = int.Parse(labelParts[1]);
                        dequeueCount++;
                        string transacQueuePath = "";

                        //create a queue path to point to the normal queue
                        if (msmqDetails.QueueType == MSMQType.Private)
                        {
                            //transacQueuePath = "FormatName:Direct=OS:" + msmqDetails.ServerName + @"\Private$\" + msmqDetails.QueueName;
                            if (msmqDetails.ServerName.Contains('.') && msmqDetails.ServerName != ".") //i.e. IP address is given for the server
                                transacQueuePath = "FormatName:Direct=TCP:" + msmqDetails.ServerName + @"\Private$\" + msmqDetails.QueueName;
                            else
                                transacQueuePath = "FormatName:Direct=OS:" + msmqDetails.ServerName + @"\Private$\" + msmqDetails.QueueName;
                        }
                        else if (msmqDetails.QueueType == MSMQType.Public)
                            transacQueuePath = msmqDetails.ServerName + @"\" + msmqDetails.QueueName;
                        
                        messageLabel = labelParts[0] + "$" + (dequeueCount).ToString() + "$" + MessageProcessingStatus.InProcess.ToString();
                        
                        //MessageQueue queue = new MessageQueue();

                        queue.Path = transacQueuePath;

                        //check if the message is already requested to be deleted
                        LifLogHandler.LogDebug("MSMQ Adapter- re-checking if the delete for the received message is requested. Input Message id-" + peekedMessage.Id + "and Label-" + peekedMessage.Label, LifLogHandler.Layer.IntegrationLayer);
                        foreach (string msg in messagesToDelete)
                        {
                            if (msg == peekedMessage.Id)
                            {
                                messagesToDelete.Remove(msg);
                                LifLogHandler.LogDebug("MSMQ Adapter- YES, delete for this message is requested and hence deleted", LifLogHandler.Layer.IntegrationLayer);
                                return "";
                            }
                        }

                        //the below code is currently commented to avoid the duplicate processing of the messages
                        //Message tempMessage = queue.ReceiveById(peekedMessage.Id, new TimeSpan(msmqDetails.QueueReadTimeout));
                        //LifLogHandler.LogDebug("MSMQ Adapter- checking if the original message is present", LifLogHandler.Layer.IntegrationLayer);
                        //if (tempMessage != null)
                        {
                            LifLogHandler.LogDebug("MSMQ Adapter- Constructing the new message with changed label.", LifLogHandler.Layer.IntegrationLayer);
                            Message newMessage = new Message(peekedMessage.Body);

                            newMessage.CorrelationId = peekedMessage.Id;

                            //set the queue to keep the message even after m/c start-up, this will make the msmq more available and reliable
                            queue.DefaultPropertiesToSend.Recoverable = true;

                            if (msmqDetails.IsQueueTransactional)
                                queue.Send(newMessage, messageLabel, MessageQueueTransactionType.Single);
                            else
                                queue.Send(newMessage, messageLabel);
                            LifLogHandler.LogDebug("MSMQ Adapter- new message is sent to the queue, trying to fetch its Id . new Message id-" + newMessage.Id + "and Message Label-" + newMessage.Label + "and constructed  Message Label-" + messageLabel, LifLogHandler.Layer.IntegrationLayer);
                            //get the id of the newly dropped message
                            //newMessageId = queue.PeekByCorrelationId(newMessage.CorrelationId).Id;
                            newMessageId = queue.PeekByCorrelationId(peekedMessage.Id, new TimeSpan(msmqDetails.QueueReadTimeout)).Id;
                            //isSuccess = true;
                            LifLogHandler.LogDebug("MSMQ Adapter- a new message is added to the queue with Id- " + newMessageId + "and Label- " + messageLabel, LifLogHandler.Layer.IntegrationLayer);

                            //now remove the old message
                            //LogHandler.LogDebug("MSMQ Adapter- now removing the old message", LogHandler.Layer.IntegrationLayer);
                            //queue.ReceiveById(peekedMessage.Id, new TimeSpan(msmqDetails.QueueReadTimeout));
                        }
                        //queue.Close();
                    }
                }
                
            }
            catch (Experimental.System.Messaging.MessageQueueException ex)
            {
                if (ex.Message.Contains("Timeout for the requested operation has expired."))
                {
                    //do nothing...
                    //this may come in case of peek by correlation
                }
                else
                {
                    //log the exception
                    string error = ex.Message;
                    if (ex.InnerException != null)
                        error = error + ".Inner Exception- " + ex.InnerException.Message;
                    LifLogHandler.LogError("MSMQ Adapter- HandleMessageProcessing FAILED. Reason- {0}", LifLogHandler.Layer.IntegrationLayer, error);

                    //then put the message back to the queue
                    if (!string.IsNullOrEmpty(queue.Path))
                    {
                        LifLogHandler.LogDebug("MSMQ Adapter- as exception is raised at HandleMessageProcessing, putting the message to the queue, message label- " + peekedMessage.Label, LifLogHandler.Layer.IntegrationLayer);
                        if (msmqDetails.IsQueueTransactional)
                            queue.Send(peekedMessage, peekedMessage.Label, MessageQueueTransactionType.Single);
                        else
                            queue.Send(peekedMessage, peekedMessage.Label);
                    }
                }
            }
            finally
            {
                queue.Close();
            }

            //return isSuccess;
            return newMessageId;
        }

        /// <summary>
        /// Handles the re-appearance of the message (if not deleted) in the normal queue after the “message invisibility time out”, otherwise deletes it.
        /// Also takes care of moving the message to the poison queue if the maximum number of allowed read has happened.
        /// Incase of Async mode, we need to increment the Dequeue
        /// </summary>
        /// <param name="peekedMessage">the message peeked</param>
        /// <param name="msmqDetails">the MSMQ access details</param>    
        /// <param name="canIncrementDequeueCount">Dequeue count need to increment or not</param>    
        private void HandleMessageReappearance(Message peekedMessage, MSMQDetails msmqDetails, bool canIncrementDequeueCount = false)
        {
            LifLogHandler.LogDebug("MSMQ Adapter- HandleMessageReappearance is called", LifLogHandler.Layer.IntegrationLayer);
            MessageQueue queue = new MessageQueue();
            try
            {
                string[] labelParts = peekedMessage.Label.Split('$');
                if (labelParts.Length >= 2)
                {
                    int dequeueCount = int.Parse(labelParts[1]);
                    LifLogHandler.LogDebug("MSMQ Adapter- HandleMessageReappearance canIncrementDequeueCount {0}, dequeueCount {1}",
                          LifLogHandler.Layer.IntegrationLayer, canIncrementDequeueCount, dequeueCount);
                    if (canIncrementDequeueCount)
                    {
                        LifLogHandler.LogDebug("MSMQ Adapter- HandleMessageReappearance and inside canIncrementDequeueCount block", LifLogHandler.Layer.IntegrationLayer);
                        dequeueCount++;
                    }

                    bool isToBePoisoned = false;
                    string transacQueuePath = "", poisonQueuePath = "";

                    //create a queue path to point to the normal queue
                    if (msmqDetails.QueueType == MSMQType.Private)
                    {
                        //transacQueuePath = "FormatName:Direct=OS:" + msmqDetails.ServerName + @"\Private$\" + msmqDetails.QueueName;
                        if (msmqDetails.ServerName.Contains('.') && msmqDetails.ServerName != ".") //i.e. IP address is given for the server
                            transacQueuePath = "FormatName:Direct=TCP:" + msmqDetails.ServerName + @"\Private$\" + msmqDetails.QueueName;
                        else
                            transacQueuePath = "FormatName:Direct=OS:" + msmqDetails.ServerName + @"\Private$\" + msmqDetails.QueueName;
                    }
                    else if (msmqDetails.QueueType == MSMQType.Public)
                        transacQueuePath = msmqDetails.ServerName + @"\" + msmqDetails.QueueName;

                    //check if the delete for this message is requested
                    LifLogHandler.LogDebug("MSMQ Adapter- Re-checking if the delete for this message is requested", LifLogHandler.Layer.IntegrationLayer);
                    
                    queue.Path = transacQueuePath;

                    foreach (string msg in messagesToDelete)
                    {
                        if (msg == peekedMessage.Id)
                        {
                            //queue.ReceiveById(peekedMessage.Id, new TimeSpan(msmqDetails.QueueReadTimeout));
                            messagesToDelete.Remove(msg);
                            LifLogHandler.LogDebug("MSMQ Adapter- YES, delete for this message is requested and hence deleted", LifLogHandler.Layer.IntegrationLayer);
                            return;
                        }
                    }

                    //check if the maximum dequeue count has reached    
                    LifLogHandler.LogDebug("MSMQ Adapter- checking if the maximum dequeue count has reached", LifLogHandler.Layer.IntegrationLayer);
                    if (dequeueCount >= msmqDetails.MessageProcessingMaxCount)
                    {
                        LifLogHandler.LogDebug("MSMQ Adapter- YES, the maximum dequeue count has reached, so the message will be moved to the poison/dead-letter queue", LifLogHandler.Layer.IntegrationLayer);

                        //create a queue path to point to the poison queue
                        isToBePoisoned = true;
                        if (msmqDetails.QueueType == MSMQType.Private)
                        {
                            //poisonQueuePath = "FormatName:Direct=OS:" + msmqDetails.ServerName + @"\Private$\" + msmqDetails.PoisonQueueName;
                            if (msmqDetails.ServerName.Contains('.') && msmqDetails.ServerName != ".") //i.e. IP address is given for the server
                                poisonQueuePath = "FormatName:Direct=TCP:" + msmqDetails.ServerName + @"\Private$\" + msmqDetails.PoisonQueueName;
                            else
                                poisonQueuePath = "FormatName:Direct=OS:" + msmqDetails.ServerName + @"\Private$\" + msmqDetails.PoisonQueueName;
                        }
                        else if (msmqDetails.QueueType == MSMQType.Public)
                            poisonQueuePath = msmqDetails.ServerName + @"\" + msmqDetails.PoisonQueueName;
                    }

                    string messageLabel = labelParts[0] + "$" + (dequeueCount).ToString();

                    //the below is commented to avoid the scenario of duplicate processing of the message
                    //if the original message is found then only try to delete and add a newly labeled message 
                    //either to the normal transaction or poison queue            
                    //Message tempMessage = queue.ReceiveById(peekedMessage.Id, new TimeSpan(msmqDetails.QueueReadTimeout));
                    //LifLogHandler.LogDebug("MSMQ Adapter- checking if the original message is found", LifLogHandler.Layer.IntegrationLayer);
                    //if (tempMessage != null)
                    {
                        //LifLogHandler.LogDebug("MSMQ Adapter- YES, the original message is found", LifLogHandler.Layer.IntegrationLayer);
                        if (isToBePoisoned)
                        {
                            //first close the normal transaction queue
                            queue.Close();
                            queue.Path = poisonQueuePath;
                        }

                        //set the queue to keep the message even after m/c start-up, this will make the msmq more available and reliable
                        queue.DefaultPropertiesToSend.Recoverable = true;
                        if (msmqDetails.IsQueueTransactional)
                            queue.Send(peekedMessage.Body.ToString(), messageLabel, MessageQueueTransactionType.Single);
                        else
                            queue.Send(peekedMessage.Body.ToString(), messageLabel);
                        LifLogHandler.LogDebug("MSMQ Adapter- message is sent to the destination queue with label- " + messageLabel, LifLogHandler.Layer.IntegrationLayer);
                    }
                    
                }
            }
            catch (Exception ex)
            {
                string error = ex.Message;
                if (ex.InnerException != null)
                    error = error + ".Inner Exception- " + ex.InnerException.Message;
                LifLogHandler.LogError("MSMQ Adapter- HandleMessageReappearance FAILED. Reason- {0}", LifLogHandler.Layer.IntegrationLayer, error);
                throw ex;
            }
            finally
            {
                queue.Close();
            }
        }

        #endregion
    }

    enum MSMQOperationType
    {
        Send, Receive, Peek
    }

    class ManageMessageDetails
    {
        public Message ReceivedMessage { get; set; }
        public DateTime VisibleAt { get; set; }
        public MSMQDetails MSMQ { get; set; }
    }

    enum MessageProcessingStatus
    {
        InProcess, Processed
    }

}
