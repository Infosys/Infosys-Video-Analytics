/*=============================================================================================================== *
 * Copyright 2025 Infosys Ltd.                                                                                    *
 * Use of this source code is governed by Apache License Version 2.0 that can be found in the LICENSE file or at  *
 * http://www.apache.org/licenses/                                                                                *
 * ===============================================================================================================*/
﻿using Confluent.Kafka;
using Confluent.Kafka.Admin;
using Confluent.SchemaRegistry;
using Confluent.SchemaRegistry.Serdes;
using Infosys.Lif.LegacyCommon;
using Infosys.Lif.LegacyIntegratorService;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.Caching;
using Newtonsoft.Json.Linq;
using Avro;
using NLog.Fluent;

namespace Infosys.Lif
{
    public class KafkaAdapter : IAdapter
    {
        private const string REGION = "Region";
        private const string TRANSPORT_SECTION = "TransportSection";
        private const string SUCCESSFUL_SENT_MESSAGE = "Message successfully sent to the Kafka";
        private const string SUCCESSFUL_RECEIVE_MESSAGE = "Message successfully received from Kafka";
        private const string PROCESSING_INCOMPLETE = "Processing is incomplete.";
        private const string sKAFKA_NOTFOUND = "Kafka server not found.";
        private const int SUCCESSFUL_STATUS_CODE = 0;
        private const int UNSUCCESSFUL_STATUS_CODE = 1000;
        private const string dateFormat = "dd-MMM-yyyy-HH:mm:ss.fffff";
        private const string MESSAGE_TO_BE_DELETED = "MessageToBeDeleted";
        private const string LI_FILENAME = "LiSettings.json";
        private const string LI_CONFIGURATION = "LISettings";
        private const string ZERO = "$0";

        private string response = PROCESSING_INCOMPLETE;
        private static MemoryCache cache;
        private static CacheItemPolicy policy;


        static private Dictionary<string, ConcurrentQueue<string>> queueDetails = new Dictionary<string, ConcurrentQueue<string>>();
        static private Dictionary<string, IProducer<long, string>> producerBuilders = new Dictionary<string, IProducer<long, string>>();

        static KafkaAdapter()
        {


            cache = MemoryCache.Default;
            policy = new CacheItemPolicy();
            LifLogHandler.LogDebug("Kafka Adapter- KafkaAdapter static constructor executed", LifLogHandler.Layer.IntegrationLayer);

            
            LISettings liSettings = new LISettings();
            var appconfig = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json").Build();
            var liSettingPath = appconfig.GetSection("LISettings").GetSection("Path").Value;
            var config = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile(liSettingPath).Build();
            config.Bind(LI_CONFIGURATION, liSettings);

            Kafka kafkaConfiguration = liSettings.Kafka;
            if (kafkaConfiguration != null)
            {
                foreach (KafkaDetails details in kafkaConfiguration.KafkaDetails)
                {
                    string topicName = details.TopicName;
                    CreateTopicAsync(details).Wait();
                    CreateProducerBuilder(details);
                    if (!string.IsNullOrEmpty(topicName))
                    {
                        if (!queueDetails.ContainsKey(topicName))
                        {
                            queueDetails.Add(topicName, new ConcurrentQueue<string>());
                        }

                    }

                }
            }


        }

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
            LifLogHandler.LogDebug("Kafka Adapter- Receive called", LifLogHandler.Layer.IntegrationLayer);

            Infosys.Lif.LegacyIntegratorService.Kafka transportSection = null;
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
                        transportSection = items.Value as Infosys.Lif.LegacyIntegratorService.Kafka;
                    }
                }

                
                KafkaDetails kafkaDetails = ValidateTransportName(transportSection, regionToBeUsed.TransportName);
                string topicName = kafkaDetails.TopicName;
              
                if (topicName != null)
                {
                   
                    Thread receiveOphandler = new Thread((ThreadStart)delegate { HandleMessage(KafkaOperationType.Receive, kafkaDetails, null); });
                    receiveOphandler.Start();
                  
                }
                else
                {
                    LifLogHandler.LogError("Kafka Adapter- Receive FAILED",LifLogHandler.Layer.IntegrationLayer, topicName);
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
            LifLogHandler.LogDebug("Kafka Adapter- Send called", LifLogHandler.Layer.IntegrationLayer);

            Infosys.Lif.LegacyIntegratorService.Kafka transportSection = null;
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
                        transportSection = items.Value as Infosys.Lif.LegacyIntegratorService.Kafka;
                    }
                }

                
                KafkaDetails kafkaDetails = ValidateTransportName(transportSection, regionToBeUsed.TransportName);
                string queueName = kafkaDetails.TopicName;
             
                if (queueName != null && message != null)
                {

                    response = HandleMessage(KafkaOperationType.Send, kafkaDetails, message);

                }
                else
                {
                    LifLogHandler.LogError("Kafka Adapter- Send FAILED, because server does not exit for {0}",
                   LifLogHandler.Layer.IntegrationLayer, queueName);
                }

            }
            catch (LegacyException exception)
            {
                LifLogHandler.LogError("Kafka Adapter- Send FAILED, because server does not exit for {0}",
                  LifLogHandler.Layer.IntegrationLayer);
                throw exception;
            }
            catch (Exception exception)
            {
                LifLogHandler.LogError("Kafka Adapter- Send FAILED, because server does not exit for {0}",
                  LifLogHandler.Layer.IntegrationLayer);
                throw exception;
            }
            return response;
        }

        #endregion
        enum KafkaOperationType
        {
            Send, Receive, Delete
        }

       
        private KafkaDetails ValidateTransportName(Infosys.Lif.LegacyIntegratorService.Kafka transportSection, string transportName)
        {
            KafkaDetails kafkaDetails = null;
            bool isTransportNameExists = false;
          
            for (int count = 0; count < transportSection.KafkaDetails.Count; count++)
            {
                kafkaDetails = transportSection.KafkaDetails[count] as KafkaDetails;
                if (kafkaDetails.TransportName == transportName)
                {
                    isTransportNameExists = true;
                    break;
                }
            }
            
            if (!isTransportNameExists)
            {
                throw new LegacyException(transportName + " is not defined in KafkaDetails section");
            }
            return kafkaDetails;
        }

       
        
        private string HandleMessage(KafkaOperationType operation, KafkaDetails kafkaDetails, string message)
        {
            Stopwatch stopwatch2 = Stopwatch.StartNew();

            LifLogHandler.LogDebug("Kafka Adapter- Handle Message called for operation of type- " + operation.ToString(), LifLogHandler.Layer.IntegrationLayer);
            try
            {
                switch (operation)
                {
                    case KafkaOperationType.Send:
                        StartSendingMessages(kafkaDetails,message);
                        response = SUCCESSFUL_SENT_MESSAGE;
                        break;
                    case KafkaOperationType.Receive:
                       
                        StartReceivingMessages(kafkaDetails);                        
                        break;
                    
                }
            }
           
            catch (Exception e)
            {
                LifLogHandler.LogError("Kafka Adapter HandleMessage method FAILED for " +
                    " TopicName {0}. Exception Message: {1} and Exception StackTrace: {2}",
                LifLogHandler.Layer.IntegrationLayer, kafkaDetails.TopicName, e.Message, e.StackTrace);
               
                
            }
            return response;
            stopwatch2.Stop();
            Console.WriteLine("Handle Message took time: "+stopwatch2.ElapsedMilliseconds);
        }

       
        private void ReceiveMessageForAsync(string message, KafkaDetails kafkaDetails,
           ConcurrentQueue<string> queue)
        {
            
            Task.Factory.StartNew(() => {
                ProcessReceivedMessage(message, kafkaDetails, queue);
            });

        }

       
        public async Task StartSendingMessages(KafkaDetails kafkaDetails,string message)
        {
            _ = Task.Run(async () =>
              {
                  Stopwatch stopwatch1 = Stopwatch.StartNew();
                
                  string cacheKey = kafkaDetails.TransportName + "_ProducerBuilder";
                  IProducer<long, string> producer = (IProducer<long, string>)producerBuilders[cacheKey];
                  if (producer == null)
                  {
                    
                      LifLogHandler.LogInfo("producer builder not created", LifLogHandler.Layer.IntegrationLayer);

                  }


                  try
                  {
                     
                      LifLogHandler.LogInfo("\nProducer loop started...\n\n", LifLogHandler.Layer.IntegrationLayer);
                      int i = 0;

                      var msg = new Message<long, string>
                      {
                          Key = DateTime.UtcNow.Ticks,
                          Value = JsonConvert.SerializeObject(message)
                      };
                      Stopwatch stopwatch8 = Stopwatch.StartNew();
                      if(kafkaDetails.ProducerMode == "async")
                      {
                          var result = await producer.ProduceAsync(kafkaDetails.TopicName, msg);

                      }
                      else
                      {
                          producer.Produce(kafkaDetails.TopicName, msg);

                      }
                      stopwatch8.Stop();
                      LifLogHandler.LogInfo("Produce async took time: {0}", LifLogHandler.Layer.IntegrationLayer, stopwatch8.ElapsedMilliseconds);
                     

                  }
                  catch (ProduceException<long, string> e)
                  {
                      Console.WriteLine($"Permanent error: {e.Message} for message (value: '{e.DeliveryResult.Value}')");
                      Console.WriteLine("Exiting producer...");
                      LifLogHandler.LogError("Kafka Adapter StartSendingMessages method FAILED for " +
                          " TopicName {0}. Exception Message: {1} and Exception StackTrace: {2}",
                      LifLogHandler.Layer.IntegrationLayer, kafkaDetails.TopicName, e.Message, e.StackTrace);
                      throw new LegacyException($"Permanent error: {e.Message} for message (value: '{e.DeliveryResult.Value}')");
                  }
                  stopwatch1.Stop();
                  LifLogHandler.LogInfo("Start sending message took time: {0}", LifLogHandler.Layer.IntegrationLayer, stopwatch1.ElapsedMilliseconds);
              });
        }


        private static void CreateProducerBuilder(KafkaDetails kafkaDetails)
        {
            string cacheKey = kafkaDetails.TransportName + "_ProducerBuilder";
            
            IProducer<long, string> producer=null;
            if (producerBuilders.ContainsKey(cacheKey))
            {
                producer = (IProducer<long, string>)producerBuilders[cacheKey];

            }
            else
            {

            

            Stopwatch stopwatch6 = Stopwatch.StartNew();
                var _producerConfig = new ProducerConfig
                {
                    BootstrapServers = kafkaDetails.BootstrapServer,
                    ClientId = Dns.GetHostName(),
                   
                    Acks = Acks.All,
                    
                    MessageSendMaxRetries = kafkaDetails.MessageSendMaxRetries,
                    
                    RetryBackoffMs = kafkaDetails.RetryBackoffMs,
                    EnableIdempotence = kafkaDetails.EnableIdempotence,
                    SecurityProtocol = SecurityProtocol.Plaintext,

                };

                producer = new ProducerBuilder<long, string>(_producerConfig)
                .SetKeySerializer(Serializers.Int64)
                .SetValueSerializer(Serializers.Utf8)
                .SetLogHandler((_, message) =>
                    Console.WriteLine($"Facility: {message.Facility}-{message.Level} Message: {message.Message}"))
                .SetErrorHandler((_, e) => Console.WriteLine($"Error: {e.Reason}. Is Fatal: {e.IsFatal}"))
                .Build();
                stopwatch6.Stop();
                LifLogHandler.LogInfo("Creating Producer took time: {0}", LifLogHandler.Layer.IntegrationLayer, stopwatch6.ElapsedMilliseconds);
                Stopwatch stopwatch7 = Stopwatch.StartNew();
                producerBuilders[cacheKey] =  producer;
                stopwatch7.Stop();
                LifLogHandler.LogInfo("Setting Cache took time: {0}", LifLogHandler.Layer.IntegrationLayer, stopwatch7.ElapsedMilliseconds);
            }
        }

        
        private void StartReceivingMessages(KafkaDetails kafkaDetails)
        {
            
           
            int initialoffset=0,currentoffset=0, count = 0;
            var _consumerConfig = new ConsumerConfig
            {
                BootstrapServers = kafkaDetails.BootstrapServer,
                EnableAutoCommit = kafkaDetails.EnableAutoCommit,
                EnableAutoOffsetStore = kafkaDetails.EnableAutoOffsetStore,
               
                GroupId = kafkaDetails.GroupId,
                SecurityProtocol = SecurityProtocol.Plaintext, 
              
                AutoOffsetReset = AutoOffsetReset.Latest
               
            
            };
            bool consumerLoopFailed = false;
            int consumerConnectionRetryCount = 0;
            int connectionRetryCnt = kafkaDetails.ConsumerConnectionRetryCount;
            int connectionRetryWait = kafkaDetails.ConsumerConnectionRetryWait;
            while (true)
            {
                
                LifLogHandler.LogDebug("\nConsumer loop started...\n\n", LifLogHandler.Layer.IntegrationLayer);
                consumerLoopFailed = false;
                for (int i = 1; i <= kafkaDetails.NumPartitions; i++)
                {
                    using var consumer = new ConsumerBuilder<Ignore, string>(_consumerConfig)
                  .SetKeyDeserializer(Deserializers.Ignore)
                  .SetValueDeserializer(Deserializers.Utf8)
                 
                  .SetErrorHandler((_, e) => Console.WriteLine($"Unable to connect to server " + kafkaDetails.BootstrapServer))
                  .Build();

                    try
                    {
                        consumerConnectionRetryCount++;
                        
                        DateTime starttime = DateTime.Now;
                        
                        consumer.Subscribe(kafkaDetails.TopicName);
                        
                        while (true)
                        {
                            
                            Stopwatch stopwatch3 = Stopwatch.StartNew();
                            
                            var consumerResult = consumer.Consume();
                            var message = consumerResult?.Message?.Value;
                            DateTime MsgConsumedTime = DateTime.UtcNow;
                            if (String.IsNullOrEmpty(message))
                            {
                               

                                if (count > 0)
                                {
                                    Console.WriteLine("No.of Messages Processed : " + count);
                                    Console.WriteLine("Initial Offset : " + initialoffset + " && Current offset : " + currentoffset);
                                    Console.WriteLine("Duration in Minutes : " + DateTime.Now.Subtract(starttime).TotalMinutes);
                                }
                                Console.WriteLine("Messages are not found ");
                            }
                            else
                            {
                                LifLogHandler.LogInfo(

                                $"Received message: {consumerResult.Message.Key}  Offset: {consumerResult.Offset.Value}" +
                                $"  Topic: {consumerResult.Topic} & Partition: {consumerResult.Partition.Value} ",
                                LifLogHandler.Layer.IntegrationLayer);
                                response = SUCCESSFUL_RECEIVE_MESSAGE;
                                
                                LifLogHandler.LogInfo("MESSAGE : {0}", LifLogHandler.Layer.IntegrationLayer, message);
                               
                                count++;

                                if (count == 1)
                                    initialoffset = Convert.ToInt32(consumerResult.Offset.Value) - 1;
                                else
                                    currentoffset = Convert.ToInt32(consumerResult.Offset.Value);

                                consumer.Commit(consumerResult);
                                consumer.StoreOffset(consumerResult);
                            }
                           
                        }

                    }
                    catch (KafkaException e)
                    {
                        if(e.Message.ToLower().Contains("topic not available"))
                        {
                            CreateTopicAsync(kafkaDetails);
                        }
                        consumerLoopFailed = true;
                        Console.WriteLine($"Consume error: {e.Message}");
                        Console.WriteLine("Exiting consumer...");
                        LifLogHandler.LogError("Kafka Adapter StartReceivingMessages method FAILED for " +
                            " TopicName {0}. Exception Message: {1} and Exception StackTrace: {2}",
                        LifLogHandler.Layer.IntegrationLayer, kafkaDetails.TopicName, e.Message, e.StackTrace);
                       
                    }
                    finally
                    {
                        Console.WriteLine("No.of Messages Processed : " + count);
                        Console.WriteLine("End Time : " + DateTime.Now);
                        consumer.Close();
                    }
                }
               if(!consumerLoopFailed || (connectionRetryCnt > 0 &&
                    consumerConnectionRetryCount > connectionRetryCnt))
                {
                    
                    break;
                }
                else
                {
                    Console.WriteLine("Retrying Kafka connection , retry count : {0} ", 
                        consumerConnectionRetryCount);
                    Thread.Sleep(connectionRetryWait);
                }
                
            }
            
        }

        
        public static async Task CreateTopicAsync(KafkaDetails kafkaDetails)
        {
           

            using var adminClient = new AdminClientBuilder(
                                        new AdminClientConfig
                                        {
                                            BootstrapServers = kafkaDetails.BootstrapServer
                                        }).Build();
            try
            {
                var metadata = adminClient.GetMetadata(TimeSpan.FromSeconds(10));
                var topicsMetadata = metadata.Topics;
                var topicNames = metadata.Topics.Where(a => a.Topic==kafkaDetails.TopicName).FirstOrDefault();
                if (topicNames == null)
                {
                    await adminClient.CreateTopicsAsync(new[]
                    {
                    new TopicSpecification
                    {
                        Name = kafkaDetails.TopicName,
                        ReplicationFactor = kafkaDetails.ReplicationFactor,
                        NumPartitions = kafkaDetails.NumPartitions
                    }
                    });
                    Console.WriteLine("Topic {0} was created.", kafkaDetails.TopicName);
                }
               
            }
            catch (CreateTopicsException e) when (e.Results.Select(r => r.Error.Code)
                .Any(el => el == ErrorCode.TopicAlreadyExists))
            {
                Console.WriteLine($"Topic {e.Results[0].Topic} already exists");
                LifLogHandler.LogError("Kafka Adapter CreateTopic method FAILED for " +
                   " TopicName {0}. Exception Message: {1} and Exception StackTrace: {2}",
               LifLogHandler.Layer.IntegrationLayer, kafkaDetails.TopicName, e.Message, e.StackTrace);
            }
            catch (Exception ex)
            {
                         
                LifLogHandler.LogError("Kafka Adapter CreateTopic method FAILED for " +
                    " TopicName {0}. Exception Message: {1} and Exception StackTrace: {2}",
                LifLogHandler.Layer.IntegrationLayer, kafkaDetails.TopicName, ex.Message, ex.StackTrace);

            }
        }

       
        static void DeleteTopics(KafkaDetails kafkaDetails)
        {
            using var adminClient = new AdminClientBuilder(
                                       new AdminClientConfig
                                       {
                                           BootstrapServers = kafkaDetails.BootstrapServer
                                       }).Build();
            try
            {
                IEnumerable<string> topicList = new List<string>() { kafkaDetails.TopicName };
                adminClient.DeleteTopicsAsync(topicList, null);
            }
            catch (DeleteTopicsException e) when (e.Results.Select(r => r.Error.Code)
               .Any(el => el == ErrorCode.TopicDeletionDisabled))
            {
                Console.WriteLine($"Unable to delete Topic {e.Results[0].Topic} ");
                LifLogHandler.LogError("Kafka Adapter DeleteTopic method FAILED for " +
                   " TopicName {0}. Exception Message: {1} and Exception StackTrace: {2}",
               LifLogHandler.Layer.IntegrationLayer, kafkaDetails.TopicName, e.Message, e.StackTrace);
            }
            catch (Exception ex)
            {
                      
                LifLogHandler.LogError("Kafka Adapter DeleteTopic method FAILED for " +
                    " TopicName {0}. Exception Message: {1} and Exception StackTrace: {2}",
                LifLogHandler.Layer.IntegrationLayer, kafkaDetails.TopicName, ex.Message, ex.StackTrace);

            }


        }

        
        private ReceiveEventArgs ConstructResponse(string msg, string newMessageId)
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
            args.ResponseDetails.Add("Status", response);
            
            if (response == SUCCESSFUL_SENT_MESSAGE || response == SUCCESSFUL_RECEIVE_MESSAGE)
                args.ResponseDetails.Add("StatusCode", SUCCESSFUL_STATUS_CODE);
            else
                args.ResponseDetails.Add("StatusCode", UNSUCCESSFUL_STATUS_CODE);
            return args;
        }

        private void ProcessReceivedMessage(string msg, KafkaDetails kafkaDetails, ConcurrentQueue<string> queue)
        {            
            string queueName = kafkaDetails.TopicName;
            try
            {             
                
                Guid msgId = Guid.NewGuid();
                ReceiveEventArgs args = ConstructResponse(msg, msgId.ToString());

                if (Received != null)
                {
                    Received(args);
                }
            }
            catch (Exception ex)
            {               
                      
                LifLogHandler.LogError("Kafka Adapter ProcessReceivedMessage method FAILED for " +
                    " TopicName {0}. Exception Message: {1} and Exception StackTrace: {2}",
                LifLogHandler.Layer.IntegrationLayer, queueName, ex.Message, ex.StackTrace);

            }                    

        }
    }
       
}
