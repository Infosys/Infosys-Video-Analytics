#region Namespaces
using Infosys.Lif.LegacyIntegratorService;
using System.Collections.Specialized;
using System.Collections;
using Infosys.Lif.LegacyCommon;
using System.Collections.Concurrent;
using System.Runtime.Caching;
using System.Diagnostics;
using Newtonsoft.Json;
using System.Text;
using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Amqp;
 
using Microsoft.Extensions.Caching.Memory;
 
using System.Runtime.CompilerServices;
using Azure.Messaging.ServiceBus;
using Microsoft.AspNetCore.Connections;
using Microsoft.Azure.ServiceBus;
using Message = Microsoft.Azure.ServiceBus.Message;
using Microsoft.Azure.Devices.Client.Exceptions;
#endregion
namespace Infosys.Lif
{
    public class AzureIoTAdapter : IAdapter
    {
        private const string REGION = "Region";
        private const string TRANSPORT_SECTION = "TransportSection";
        private const string SUCCESSFUL_SENT_MESSAGE = "Message successfully sent to Azure IoT Adapter";
        private const string SUCCESSFUL_RECEIVE_MESSAGE = "Message successfully received from";
        private const string PROCESSING_INCOMPLETE = "Processing is incomplete.";
        private const string AZUREIoTHUB_NOTFOUND = "server not found.";
        private const int SUCCESSFUL_STATUS_CODE = 0;
        private const int UNSUCCESSFUL_STATUS_CODE = 1000;
        private const string dateFormat = "dd-MMM-yyyy-HH:mm:ss.fffff";
        private const string MESSAGE_TO_BE_DELETED = "MessageToBeDeleted";
        private const string LI_FILENAME = "LiSettings.json";
        private const string LI_CONFIGURATION = "LISettings";
        private const string ZERO = "$0";

        private string response = PROCESSING_INCOMPLETE;
        //  private static MemoryCache cache;
        private static CacheItemPolicy policy;
        static QueueClient queueClient;

        private static DeviceClient deviceClient;
        
        //private static string deviceName = "iva";
        static private Dictionary<string, ConcurrentQueue<string>> queueDetails = new Dictionary<string, ConcurrentQueue<string>>();



        #region IAdapter Members

        public event ReceiveHandler Received;

        public bool Delete(ListDictionary messageDetails)
        {
            try
            {
                return true;
            }
            catch (Exception ex)
            {

                throw;
            }
            //throw new NotImplementedException();
        }

        public void Receive(ListDictionary adapterDetails)
        {
            LifLogHandler.LogDebug("Azure IoT Adapter- Receive called", LifLogHandler.Layer.IntegrationLayer);

            Infosys.Lif.LegacyIntegratorService.AzureIoTAdapter transportSection = null;
            Infosys.Lif.LegacyIntegratorService.Region regionToBeUsed = null;
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
                        transportSection = items.Value as Infosys.Lif.LegacyIntegratorService.AzureIoTAdapter;
                    }
                }

                // Validates whether TransportName specified in the region, exists in MemoryQueueDetails section.
                AzureIoTDetails azureIoTDetails = ValidateTransportName(transportSection, regionToBeUsed.TransportName);
                string queueName = azureIoTDetails.DeviceName;
                // ConcurrentQueue<KafkaMessage> queue = getTopicDetails(topicName);
                if (queueName != null)
                {
                    //if (kafkaDetails.QueueReadingType == MemoryQueueReadType.Receive)
                    //{
                    Thread receiveOphandler = new Thread((ThreadStart)delegate { HandleMessage(OperationType.Receive, azureIoTDetails, null); });
                    receiveOphandler.Start();
                    //}
                    //HandleMessage(KafkaOperationType.Receive, kafkaDetails, null, queue);
                }
                else
                {
                    LifLogHandler.LogError("Azure IoT Adapter- Receive FAILED", LifLogHandler.Layer.IntegrationLayer, queueName);
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
            LifLogHandler.LogDebug("Azure IoT Adapter- Send called", LifLogHandler.Layer.IntegrationLayer);

            Infosys.Lif.LegacyIntegratorService.AzureIoTAdapter transportSection = null;
            Infosys.Lif.LegacyIntegratorService.Region regionToBeUsed = null;
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
                        transportSection = items.Value as Infosys.Lif.LegacyIntegratorService.AzureIoTAdapter;
                    }
                }

                // Validates whether TransportName specified in the region, exists in MemoryQueueDetails section.
                AzureIoTDetails azureIotDetails = ValidateTransportName(transportSection, regionToBeUsed.TransportName);
                string queueName = azureIotDetails.DeviceName;
                // string label = kafkaDetails.MessageLabel + ZERO;
                // ConcurrentQueue<KafkaMessage> queue = getTopicDetails(queueName);
                if (queueName != null && message != null)
                {
                    // KafkaMessage msg = JsonConvert.DeserializeObject<KafkaMessage>(message);
                    // MemoryQueueMessage queueMessage = ConstructMessage(message, label);
                    response = HandleMessage(OperationType.Send, azureIotDetails, message);
                    // Console.WriteLine("QueueName:"+ queueName+":" +queue.Count);
                }
                else
                {
                    LifLogHandler.LogError("Azure IoT Adapter- Send FAILED, because server does not exit for {0}",
                   LifLogHandler.Layer.IntegrationLayer, queueName);
                }

            }
            catch (LegacyException exception)
            {
                LifLogHandler.LogError("Azure IoT Adapter- Send FAILED, because server does not exit for {0}",
                  LifLogHandler.Layer.IntegrationLayer);
                throw exception;
            }
            catch (Exception exception)
            {
                LifLogHandler.LogError("Azure IoT Adapter- Send FAILED, because server does not exit for {0}",
                  LifLogHandler.Layer.IntegrationLayer);
                throw exception;
            }
            return response;
        }

        #endregion

        // <summary>
        /// Validates whether TransportName specified in the region, exists in IoTAdapter
        /// section. If it found, it returns corresponding IoTAdapter object.
        /// </summary>
        /// <param name="transportSection">IoT section</param>
        /// <param name="transportName">name of the transport</param>
        private AzureIoTDetails ValidateTransportName(Infosys.Lif.LegacyIntegratorService.AzureIoTAdapter transportSection, string transportName)
        {
            AzureIoTDetails messageDetails = null;
            bool isTransportNameExists = false;
            // Find the AzureIoT region to which it should connect for sending message.
            for (int count = 0; count < transportSection.AzureIoTDetails.Count; count++)
            {
                messageDetails = transportSection.AzureIoTDetails[count] as AzureIoTDetails;
                if (messageDetails.TransportName == transportName)
                {
                    isTransportNameExists = true;
                    break;
                }
            }
            // If MemoryQueue region is not set in the config then throw the exception
            if (!isTransportNameExists)
            {
                throw new LegacyException(transportName + " is not defined in MessageDetails section");
            }
            return messageDetails;
        }


        /// <summary>
        /// Handles the appearance of the message.
        /// </summary>
        /// <param name="operation">the message send/receive/delete</param>
        /// <param name="MessageDetails">Contains details of Message Adapter used to sent message e.g AzureIoTHub,RabbitMq</param>   
        /// <param name="message">Message details</param>  
        /// <param name="msg">Message details</param> 
        private string HandleMessage(OperationType operation, AzureIoTDetails messageDetails, string message)
        {
            Stopwatch stopwatch2 = Stopwatch.StartNew();

            LifLogHandler.LogDebug("Azure IoT Adapter- Handle Message called for operation of type- " + operation.ToString(), LifLogHandler.Layer.IntegrationLayer);
            try
            {
                switch (operation)
                {
                    case OperationType.Send:
                        StartSendingMessages(messageDetails, message);
                        //  StartMessages(messageDetails, message);
                        //    ReceiveFeedbackAsync();
                        response = SUCCESSFUL_SENT_MESSAGE;
                        break;
                    case OperationType.Receive:

                        StartReceivingMessages(messageDetails);
                        break;

                }
            }

            catch (Exception e)
            {
                LifLogHandler.LogError("Azure IoT Adapter HandleMessage method FAILED for " +
                    " TopicName {0}. Exception Message: {1} and Exception StackTrace: {2}",
                LifLogHandler.Layer.IntegrationLayer, messageDetails.DeviceName, e.Message, e.StackTrace);
                //throw new LegacyException($"Permanent error: {e.Message} for message (value: '{e.InnerException}')");

            }
            return response;
            stopwatch2.Stop();

        }

        enum OperationType
        {
            Send, Receive, Delete
        }

        /// <summary>
        /// Produce the messages to given kafka details
        /// </summary>
        /// <param name="MessageDetails">Details of MessageAdapter channels Azure Iot Hub, Rabbit MQ</param>
        /// <returns></returns>
        //  public async Task StartSendingMessages(AzureIoTDetails messageDetails, string message)
        public async Task StartSendingMessages(AzureIoTDetails messageDetails, string message)
        {
            try
            {
                if (messageDetails.TransportType == "Amqp")
                {
                    deviceClient = DeviceClient.CreateFromConnectionString(messageDetails.IoTConnectionString, Microsoft.Azure.Devices.Client.TransportType.Amqp);
                }
                else if (messageDetails.TransportType == "Mqtt")
                {
                    deviceClient = DeviceClient.CreateFromConnectionString(messageDetails.IoTConnectionString, Microsoft.Azure.Devices.Client.TransportType.Mqtt);
                }
         //       deviceClient = DeviceClient.CreateFromConnectionString(messageDetails.IoTConnectionString, messageDetails.TransportType);
           deviceClient.SetConnectionStatusChangesHandler(ConnectionStatusChangeHandler);
                // deviceClient = DeviceClient.CreateFromConnectionString(messageDetails.ConnectionString, messageDetails.TransportType);
                int i = 0;

                var msg = JsonConvert.SerializeObject(message);
                var commandMessage = new Microsoft.Azure.Devices.Client.Message(Encoding.ASCII.GetBytes(msg));
                //commandMessage. = Microsoft.Azure.Devices.DeliveryAcknowledgement.Full;
                await deviceClient.SendEventAsync(commandMessage).ConfigureAwait(false);
                      
           await  Task.Delay(1000 * 10);
                 
            }
            catch(IotHubException ex)
            {

                //do nothing          
                LifLogHandler.LogError("Azure Iot Adapter Send Message method FAILED for " +
                    " TopicName {0}. Exception Message: {1} and Exception StackTrace: {2}",
                LifLogHandler.Layer.IntegrationLayer, messageDetails.TransportType, ex.Message, ex.StackTrace);
            }
            finally
            {
                  deviceClient.CloseAsync();
            }
        }
        static void ConnectionStatusChangeHandler(ConnectionStatus status, ConnectionStatusChangeReason reason)
        {
            //do nothing          
            LifLogHandler.LogError("Azure Iot Adapter Failed to Connect IotHub for " +
                " TopicName {0}. Exception Message: {1} and Exception StackTrace: {2}",
            LifLogHandler.Layer.IntegrationLayer, status, reason);//, ex.StackTrace);
        }
        private async Task StartReceivingMessages(AzureIoTDetails messageDetails)
        {

            int initialoffset = 0, currentoffset = 0, count = 0;
            DateTime starttime = DateTime.Now;
            var message = string.Empty;
            while (true)
            {
                 var client = new ServiceBusClient(messageDetails.QueueConnectionString);
            try
            {                 
                    //Create processor instance from client object, pass queue name and ServiceBisProcessorOptions instance

                       var processor = client.CreateProcessor(messageDetails.ServiceBusQueueName, new ServiceBusProcessorOptions());
                    //Register ProcessMessageAsync event method
                    processor.ProcessMessageAsync += async (messageArgs) =>
                    {

                        await messageArgs.CompleteMessageAsync(messageArgs.Message);
                        var body = messageArgs.Message.Body.ToArray();
                        //ea.Body.ToArray();
                        message = Encoding.UTF8.GetString(body);

                        ProcessReceivedMessage(message, messageDetails, null);
                    };
                    //Register ErrorAsync event method
                    processor.ProcessErrorAsync += async (messageArgs) =>
                    {
                      Console.WriteLine(messageArgs.Exception.Message);
                    };
                    //Start the processor
               await     processor.StartProcessingAsync();
                    //  Console.ReadLine();
             await      Task.Delay(1000 * 2);
                  
                  //  client.DisposeAsync();
                   Console.ReadLine();      
            }
            catch (Exception ex)
            {
                if (ex.Message.ToLower().Contains("Queue not available"))
                {
                    //   CreateTopicAsync(kafkaDetails);
                }
                //consumerLoopFailed = true;
                Console.WriteLine($"Consume error: {ex.Message}");
                Console.WriteLine("Exiting consumer...");
                LifLogHandler.LogError("Azure IOT Adapter StartReceivingMessages method FAILED for " +
                    " TopicName {0}. Exception Message: {1} and Exception StackTrace: {2}",
                LifLogHandler.Layer.IntegrationLayer, messageDetails.ServiceBusQueueName, ex.Message, ex.StackTrace);
            }
            finally
            {
                //  Console.WriteLine("No.of Messages Processed : " + messageCount);
                Console.WriteLine("End Time : " + DateTime.Now);
                 client.DisposeAsync();
                //consumer.Close();
            }
            }

            //channel.BasicGet(queue: "framemetadata",
            //                     autoAck: true
            //                  );


            //ReceiveC2dAsync(messageDetails);
        }
        private void ProcessReceivedMessage(string msg, AzureIoTDetails message, ConcurrentQueue<string> queue)
        {
            string queueName = message.ServiceBusQueueName;
            try
            {
                // JObject jobject = new JObject(msg);
                //jobject.SelectToken("");
                Guid msgId = Guid.NewGuid();
                ReceiveEventArgs args = ConstructResponse(msg, msgId.ToString(),message.TransportType);

                if (Received != null)
                {
                    Received(args);
                }
            }
            catch (Exception ex)
            {
                //do nothing          
                LifLogHandler.LogError("Azure IoT Adapter ProcessReceivedMessage method FAILED for " +
                    " TopicName {0}. Exception Message: {1} and Exception StackTrace: {2}",
                LifLogHandler.Layer.IntegrationLayer, queueName, ex.Message, ex.StackTrace);

            }

        }

        private ReceiveEventArgs ConstructResponse(string msg, string newMessageId, string transportype)
        {
            ReceiveEventArgs args = new ReceiveEventArgs();
            args.ResponseDetails = new ListDictionary();
            if (msg != null)
            {
                if (msg.Contains(@"\\\"))
                {
                    msg = msg.TrimStart('\"').TrimEnd('\"').Replace("\\\"", "\"").Replace("\\\\", "\\");
                }
                else
                {
                    msg = msg.TrimStart('\"').TrimEnd('\"').Replace(@"\", "");
                }
                args.ResponseDetails.Add("MessageBody", msg);
            }
            args.ResponseDetails.Add("MessageIdentifier", newMessageId);
            args.ResponseDetails.Add("Status", response + " " + transportype);
            //assign the Status Code based on the "response"
            if (response == SUCCESSFUL_SENT_MESSAGE || response == SUCCESSFUL_RECEIVE_MESSAGE)
                args.ResponseDetails.Add("StatusCode", SUCCESSFUL_STATUS_CODE);
            else
                args.ResponseDetails.Add("StatusCode", UNSUCCESSFUL_STATUS_CODE);
            return args;
        }
    }
}