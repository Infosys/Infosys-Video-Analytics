/*=============================================================================================================== *
 * Copyright 2025 Infosys Ltd.                                                                                    *
 * Use of this source code is governed by Apache License Version 2.0 that can be found in the LICENSE file or at  *
 * http://www.apache.org/licenses/                                                                                *
 * ===============================================================================================================*/
﻿#region Namespaces
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
using RabbitMQ.Client;
using Microsoft.Extensions.Caching.Memory;
using RabbitMQ.Client.Exceptions;
using RabbitMQ.Client.Events;
using System.Runtime.CompilerServices;
using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Amqp.Framing;
using System.Reflection;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
#endregion

namespace Infosys.Lif
{
    public class RabbitMQAdapter : IAdapter
    {
        private const string REGION = "Region";
        private const string TRANSPORT_SECTION = "TransportSection";
        private const string SUCCESSFUL_SENT_MESSAGE = "Message successfully sent to RabbitMQ Queue";
        private const string SUCCESSFUL_RECEIVE_MESSAGE = "Message successfully received from RabbitMQ Queue";
        private const string PROCESSING_INCOMPLETE = "Processing is incomplete in RabbitMQ Queue";
        private const string AZUREIoTHUB_NOTFOUND = "server not found.";
        private const int SUCCESSFUL_STATUS_CODE = 0;
        private const int UNSUCCESSFUL_STATUS_CODE = 1000;
        private const string dateFormat = "dd-MMM-yyyy-HH:mm:ss.fffff";
        private const string MESSAGE_TO_BE_DELETED = "MessageToBeDeleted";
        private const string LI_FILENAME = "LiSettings.json";
        private const string LI_CONFIGURATION = "LISettings";
        private const string ZERO = "$0";

        private string response = PROCESSING_INCOMPLETE;
    
        private static CacheItemPolicy policy;

        private static ServiceClient serviceClient;
        private    static DeviceClient deviceClient;
      
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
            
        }

        public void Receive(ListDictionary adapterDetails)
        {
            LifLogHandler.LogDebug("RabbitMQ Message Adapter- Receive called", LifLogHandler.Layer.IntegrationLayer);

            Infosys.Lif.LegacyIntegratorService.RabbitMQAdapter transportSection = null;
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
                        transportSection = items.Value as Infosys.Lif.LegacyIntegratorService.RabbitMQAdapter;
                    }
                }

               
                MessageDetails messageDetails = ValidateTransportName(transportSection, regionToBeUsed.TransportName);
                string queueName = messageDetails.QueueName;
            
                if (queueName != null)
                {
                    
                    Thread receiveOphandler = new Thread((ThreadStart)delegate { HandleMessage(OperationType.Receive, messageDetails, null); });
                    receiveOphandler.Start();
                  
                }
                else
                {
                    LifLogHandler.LogError("RabbitMQ Message Adapter- Receive FAILED", LifLogHandler.Layer.IntegrationLayer, queueName);
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
            LifLogHandler.LogDebug("RabbitMQ Message Adapter- Send called", LifLogHandler.Layer.IntegrationLayer);

            Infosys.Lif.LegacyIntegratorService.RabbitMQAdapter transportSection = null;
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
                        transportSection = items.Value as Infosys.Lif.LegacyIntegratorService.RabbitMQAdapter;
                    
                    }
                }

               
                MessageDetails messageDetails = ValidateTransportName(transportSection, regionToBeUsed.TransportName);
                string queueName = messageDetails.QueueName;
 
                if (queueName != null && message != null)
                {
                    response = HandleMessage(OperationType.Send, messageDetails, message);
               
                }
                else
                {
                    LifLogHandler.LogError("RabbitMQ Message Adapter- Send FAILED, because server does not exit for {0}",
                   LifLogHandler.Layer.IntegrationLayer, queueName);
                }

            }
            catch (LegacyException exception)
            {
                LifLogHandler.LogError("RabbitMQ Message Adapter- Send FAILED, because server does not exit for {0}",
                  LifLogHandler.Layer.IntegrationLayer);
                throw exception;
            }
            catch (Exception exception)
            {
                LifLogHandler.LogError("RabbitMQ Message Adapter- Send FAILED, because server does not exit for {0}",
                  LifLogHandler.Layer.IntegrationLayer);
                throw exception;
            }
            return response;
        }

        #endregion

      
        private MessageDetails ValidateTransportName(Infosys.Lif.LegacyIntegratorService.RabbitMQAdapter transportSection, string transportName)
        {
            MessageDetails messageDetails = null;
            bool isTransportNameExists = false;
            
            for (int count = 0; count < transportSection.MessageDetails.Count; count++)
            {
                messageDetails = transportSection.MessageDetails[count] as MessageDetails;
                if (messageDetails.TransportName == transportName)
                {
                    isTransportNameExists = true;
                    break;
                }
            }
     
            if (!isTransportNameExists)
            {
                throw new LegacyException(transportName + " is not defined in MessageDetails section");
            }
            return messageDetails;
        }


        private string HandleMessage(OperationType operation, MessageDetails messageDetails, string message)
        {
            Stopwatch stopwatch2 = Stopwatch.StartNew();

            LifLogHandler.LogDebug("RabbitMQ Message Adapter- Handle Message called for operation of type- " + operation.ToString(), LifLogHandler.Layer.IntegrationLayer);
            try
            {
                switch (operation)
                {
                    case OperationType.Send:
                        StartSendingMessages(messageDetails, message);
                      
                        response = SUCCESSFUL_SENT_MESSAGE;
                        break;
                    case OperationType.Receive:

                        StartReceivingMessages(messageDetails);
                        break;

                }
            }

            catch (Exception e)
            {
                LifLogHandler.LogError("RabbitMQ Message Adapter HandleMessage method FAILED for " +
                    " TopicName {0}. Exception Message: {1} and Exception StackTrace: {2}",
                LifLogHandler.Layer.IntegrationLayer, messageDetails.DeviceName, e.Message, e.StackTrace);
                

            }
            return response;
    
            
        }

        enum OperationType
        {
            Send, Receive, Delete
        }
     

        public void StartMessages(MessageDetails messageDetails, string message)
        {
            try
            {
                if (messageDetails.TransportType == "mqtt")
                {
                    MqttClient client = new MqttClient("localhost");
                    string clientId = Guid.NewGuid().ToString();
                  
                    client.Connect(clientId, messageDetails.UserName, messageDetails.Password);
                    client.MqttMsgPublished += new MqttClient.MqttMsgPublishedEventHandler(client_MqttMsgPublished);
                    ushort t = client.Publish(messageDetails.QueueName, Encoding.UTF8.GetBytes(message), MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, false);
                    client.Disconnect();
                   
                }
                else
                {
                    var factory = new ConnectionFactory()
                    {
                        HostName = messageDetails.HostName,
                        Port = Protocols.DefaultProtocol.DefaultPort,
                        UserName = messageDetails.UserName,
                        Password = messageDetails.Password,
                        VirtualHost = messageDetails.VirtualHost,

                        RequestedHeartbeat = new TimeSpan(10, 0, 0, 0),
                        AutomaticRecoveryEnabled = true,
                        NetworkRecoveryInterval = TimeSpan.FromSeconds(10)           
                    };
                    
                    var connection = factory.CreateConnection();
                    using var channel = connection.CreateModel();

                    channel.ExchangeDeclare(exchange: messageDetails.TransportName, type: ExchangeType.Topic, durable: true);
                    channel.QueueDeclare(messageDetails.QueueName, durable: true,
                                         exclusive: false,
                                      
                                         arguments: null);
                    channel.QueueBind(messageDetails.QueueName,
                      exchange: messageDetails.TransportName,
                      routingKey: messageDetails.TransportName);
                    var json = JsonConvert.SerializeObject(message);
                    var body = Encoding.UTF8.GetBytes(json);

                   
                    channel.BasicPublish(exchange: messageDetails.TransportName, routingKey: messageDetails.TransportName, basicProperties: null, body: body);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
        private static void client_MqttMsgPublished(object sender, MqttMsgPublishedEventArgs e)
        {
            Console.WriteLine("Message Published");
            Console.WriteLine(e.IsPublished);
            Console.WriteLine(e.MessageId);
        }
      
        public void StartSendingMessages(MessageDetails messageDetails, string message)
        {            
             StartMessages(messageDetails, message);             
        }
        private void StartReceivingMessages1(MessageDetails messageDetails)
        {
            Thread.Sleep(5000);
            Console.WriteLine("Start Receiving Message");
        }
        
        private  void StartReceivingMessages(MessageDetails messageDetails) 
        {
     
            int initialoffset = 0, currentoffset = 0, count = 0;
            DateTime starttime = DateTime.Now;
            while (true)
            {
                if (messageDetails.TransportType == "mqtt")
                {
                    MqttClient client = new MqttClient("localhost");
                    string clientId = Guid.NewGuid().ToString();
                    Console.WriteLine("Receive Client Id {0}",clientId);
                    client.Connect(clientId, messageDetails.UserName, messageDetails.Password);
             
                    string[] topic = { messageDetails.QueueName };
                    byte[] qoslevels = { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE };
                    client.Subscribe(topic, qoslevels);
                   
                    client.MqttMsgPublishReceived += client_MqttMsgPublishReceived;
 
                }

                else
                {
                    var factory = new ConnectionFactory
                    {                    
                        HostName = messageDetails.HostName,
                        Port = Protocols.DefaultProtocol.DefaultPort,
                        UserName = messageDetails.UserName,
                        Password = messageDetails.Password,
                        VirtualHost = messageDetails.VirtualHost,

                        RequestedHeartbeat = new TimeSpan(10, 0, 0, 0),
                        AutomaticRecoveryEnabled = true,
                        NetworkRecoveryInterval = TimeSpan.FromSeconds(10)                      
                    };
                    using var connection = factory.CreateConnection();
                    using var channel = connection.CreateModel();

                    var message = string.Empty;
                    channel.QueueDeclare(queue: "" +
                        messageDetails.QueueName, durable: true,
                                    exclusive: false,
                                  
                                    arguments: null);
                    channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);
                    int messageCount = Convert.ToInt16(channel.MessageCount(messageDetails.QueueName));
                    try
                    {

                        var consumer = new EventingBasicConsumer(channel);
                        consumer.Received += async (model, ea) =>
                    {
                        var body = ea.Body.ToArray();
                        message = Encoding.UTF8.GetString(body);
                        channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);

                        Thread.Sleep(1000);
                        ProcessReceivedMessage(message, messageDetails, null);
                    };

                        channel.BasicConsume(queue: messageDetails.QueueName, autoAck: false, consumer: consumer);

                        Thread.Sleep(1000 * messageCount);
                        Console.ReadLine();
                        channel.Close();
                        if (String.IsNullOrEmpty(message))
                        {
                            Console.WriteLine("Messages are not found in RabbitMQ Queues");
                            break;
                        }


                    }
                    catch (RabbitMQClientException ex)
                    {
                        if (ex.Message.ToLower().Contains("Queue not available"))
                        {
                          
                        }
                       
                        Console.WriteLine($"Consume error: {ex.Message}");
                        Console.WriteLine("Exiting consumer...");
                        LifLogHandler.LogError("RabbitMQ Adapter StartReceivingMessages method FAILED for " +
                            " TopicName {0}. Exception Message: {1} and Exception StackTrace: {2}",
                        LifLogHandler.Layer.IntegrationLayer, messageDetails.QueueName, ex.Message, ex.StackTrace);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                    finally
                    {
                        
                    }
                }
            }
        
            
        }
        static void client_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
        {
            
            Console.WriteLine("Received = " + Encoding.UTF8.GetString(e.Message) + " on topic " + e.Topic);
        }
        private static void client_PublishArrived(object sender, MqttMsgPublishEventArgs e)
        {
            Console.WriteLine("Message Received");
            Console.WriteLine(e.Topic);
            Console.WriteLine(Encoding.UTF8.GetString(e.Message));
            
            
        }
        private static async Task Consumer_Received(object sender, BasicDeliverEventArgs eevent)
        {
            var message = Encoding.UTF8.GetString(eevent.Body.ToArray());

            Console.WriteLine($"Begin processing {message}");

            await Task.Delay(250);

            Console.WriteLine($"End processing {message}");
        }
       
            private void ProcessReceivedMessage(string msg, MessageDetails message, ConcurrentQueue<string> queue)
        {
            
            string queueName = message.QueueName;
            try
            {
               
                Guid msgId = Guid.NewGuid();
                ReceiveEventArgs args = ConstructResponse(msg, msgId.ToString(),message.TransportType);

                if (Received != null)
                {
                    Received(args);
                }
            }
            catch (Exception ex)
            {
                         
                LifLogHandler.LogError("RabbitMQ Adapter ProcessReceivedMessage method FAILED for " +
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
            args.ResponseDetails.Add("Status", response +" "+ transportype);
            
            if (response == SUCCESSFUL_SENT_MESSAGE || response == SUCCESSFUL_RECEIVE_MESSAGE)
                args.ResponseDetails.Add("StatusCode", SUCCESSFUL_STATUS_CODE);
            else
                args.ResponseDetails.Add("StatusCode", UNSUCCESSFUL_STATUS_CODE);
            return args;
        }
        private static  void ReceiveC2dAsync(MessageDetails messageDetails)
        {
            

        }
    }
}
