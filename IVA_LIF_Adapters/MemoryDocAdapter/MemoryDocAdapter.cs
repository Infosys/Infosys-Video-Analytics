/*=============================================================================================================== *
 * Copyright 2025 Infosys Ltd.                                                                                    *
 * Use of this source code is governed by Apache License Version 2.0 that can be found in the LICENSE file or at  *
 * http://www.apache.org/licenses/                                                                                *
 * ===============================================================================================================*/
﻿using Infosys.Lif.LegacyIntegratorService;
using Infosys.Lif.LegacyCommon;
using System;
using System.Collections.Specialized;
using System.Runtime.Caching;
using System.Linq;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using MemoryDocAdapter;

namespace Infosys.Lif
{
    public class MemoryDocAdapter : IAdapter
    {
        private const string REGION = "Region";
        private const string TRANSPORT_SECTION = "TransportSection";
        private const string DATA = "Data";
        private const string TARGETURLDETAILS = "TargetURLDetails";
        private const string SUCCESSFUL_DATA_SENT = "Document successfully uploaded.";
        private const string SUCCESSFUL_DATA_RECEIVED = "Document successfully downloaded.";
        private const string UNSUCCESSFUL_DATA_SENT = "Document couldn't be uploaded successfully.";
        private const string UNSUCCESSFUL_DATA_RECEIVED = "Document couldn't be downloaded successfully.";
        private const string SUCCESSFUL_DATA_DELETED = "Document successfully Deleted.";
        private const string UNSUCCESSFUL_DATA_DELETED = "Document couldn't be deleted successfully. ";
        private const int SUCCESSFUL_STATUS_CODE = 0;
        private const int UNSUCCESSFUL_STATUS_CODE = 1000;
        static private Dictionary<string, MemoryCacheItem> memoryCacheCollection = new Dictionary<string, MemoryCacheItem>();

        private NameValueCollection targetURLDetails;

        #region IAdapter Members

        public event ReceiveHandler Received;

        

        private const string LI_CONFIGURATION = "LISettings";
        static private int file_expired_count = 0;
        public MemoryDocAdapter()
        {

            CacheItemPolicy policy = new CacheItemPolicy();
            LifLogHandler.LogDebug("MemoryDocAdapter Adapter- MemoryDocAdapter static constructor executed", LifLogHandler.Layer.IntegrationLayer);


            LISettings liSettings = new LISettings();
            var appconfig = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json").Build();
            var liSettingPath = appconfig.GetSection("LISettings").GetSection("Path").Value;

            var config = new ConfigurationBuilder().AddJsonFile(liSettingPath).Build();
            config.Bind(LI_CONFIGURATION, liSettings);

            
            MemoryDoc memoryDoc = liSettings.MemoryDoc;
            IList<MemoryDocDetails> docDetailsCollection = memoryDoc.MemoryDocDetails;
            double expirationValue = 0;
            int gcTriggerExpireCount = 0;
            int cacheMemoryLimitBytes = 0;
            int physicalMemoryLimitPrctg = 0;
            string cachePollingInterval = string.Empty;
            string memoryCacheRegion = string.Empty;
            int memoryControlMode = 0;
            foreach (MemoryDocDetails memoryDocDetails in docDetailsCollection)
            {
                expirationValue = memoryDocDetails.MemoryCacheSlidingExpirationInMins;
                cacheMemoryLimitBytes = memoryDocDetails.CacheMemoryLimitBytes;
                memoryControlMode = memoryDocDetails.MemoryControlMode;
                physicalMemoryLimitPrctg = memoryDocDetails.PhysicalMemoryLimitPrctg;
                NameValueCollection cacheSettings = new NameValueCollection();
                memoryCacheRegion = memoryDocDetails.TransportName;

                if (!memoryCacheCollection.ContainsKey(memoryCacheRegion))
                {
                    switch (memoryControlMode)
                    {
                        case 1: 
                            gcTriggerExpireCount = memoryDocDetails.ExpirationTriggerCount;
                            break;
                        case 2:
                            SetCacheSettingsForMemoryControl(cacheMemoryLimitBytes, physicalMemoryLimitPrctg, cacheSettings);
                            break;
                        case 3: 
                            gcTriggerExpireCount = memoryDocDetails.ExpirationTriggerCount;
                            SetCacheSettingsForMemoryControl(cacheMemoryLimitBytes, physicalMemoryLimitPrctg, cacheSettings);
                            break;
                        default:
                            break;

                    }
                    
                    if (memoryDocDetails.CachePollingIntervalInSec > 0)
                    {
                        TimeSpan time = TimeSpan.FromSeconds(memoryDocDetails.CachePollingIntervalInSec);
                        cachePollingInterval = time.ToString(@"hh\:mm\:ss");
                        cacheSettings.Add("pollingInterval", Convert.ToString(cachePollingInterval));
                    }

                   
                    if (expirationValue > 0)
                    {
                        policy.SlidingExpiration = TimeSpan.FromMinutes(expirationValue);
                    }


                    MemoryCache cache = null;
                    if (cacheSettings.Count > 0)
                    {
                        cache = new MemoryCache(memoryCacheRegion, cacheSettings);

                    }
                    else
                    {
                        cache = new MemoryCache(memoryCacheRegion);
                    }
                    
                    CacheEntryRemovedCallback removeCallback = new CacheEntryRemovedCallback(OnExpire);
                    policy.RemovedCallback = removeCallback;


                    lock (memoryCacheCollection)
                    {
                        MemoryCacheItem memoryCacheItem = new MemoryCacheItem()
                        {
                            Cache = cache,
                            CachePolicy = policy,
                            GCTriggerExpireCount = gcTriggerExpireCount

                        };

                        memoryCacheCollection[memoryCacheRegion] = memoryCacheItem;
                    }
                        

                }
            }

            static void SetCacheSettingsForMemoryControl(int cacheMemoryLimitBytes, int physicalMemoryLimitPrctg, NameValueCollection CacheSettings)
            {
                if (cacheMemoryLimitBytes > 0)
                {
                    CacheSettings.Add("cacheMemoryLimitMegabytes", Convert.ToString(cacheMemoryLimitBytes));
                }
                
                if (physicalMemoryLimitPrctg > 0)
                {
                    CacheSettings.Add("physicalMemoryLimitPercentage", Convert.ToString(physicalMemoryLimitPrctg));  //set % here
                }
            }
        }


        public bool Delete(ListDictionary adapterDetails)
        {
            
            Infosys.Lif.LegacyIntegratorService.MemoryDoc transport = null;
            Infosys.Lif.LegacyIntegratorService.Region region = null;
            foreach (DictionaryEntry items in adapterDetails)
            {
                if (items.Key.ToString() == REGION)
                {
                    region = items.Value as Region;
                }
                else if (items.Key.ToString() == TRANSPORT_SECTION)
                {
                    transport = items.Value as Infosys.Lif.LegacyIntegratorService.MemoryDoc;
                }
                else if (items.Key.ToString() == TARGETURLDETAILS)
                {
                    targetURLDetails = items.Value as NameValueCollection;
                }
            }
            if (region == null
                || transport == null
                || targetURLDetails == null)
            {
                LifLogHandler.LogError(
                    "MemoryDoc Adapter- Delete- One of Region, Transport section, Target details is not passed!",
                    LifLogHandler.Layer.IntegrationLayer);

                throw new ArgumentException(string.Format("{0}, {1}, {2} are required for delete operation!",
                    REGION, TRANSPORT_SECTION, TARGETURLDETAILS));
            }

           
            bool check = targetURLDetails.AllKeys.Contains("container_name")
                            && !string.IsNullOrWhiteSpace(targetURLDetails["container_name"]);
            if (!check)
                throw new LegacyException("MemoryDoc Adapter- 'container_name' missing in metadata.");

            check = targetURLDetails.AllKeys.Contains("file_name")
                               && !string.IsNullOrWhiteSpace(targetURLDetails["file_name"]);
            if (!check)
                throw new LegacyException("MemoryDoc Adapter- 'file_name' missing in metadata.");

            
            MemoryDocDetails docDetails = ValidateTransportName(transport, region.TransportName);

            if (docDetails == null)
                throw new LegacyException(
                    string.Format("MemoryDoc Adapter- Could not find transport by name: {0}", region.TransportName));

          
            var response = HandleStream(docDetails, DocAccessType.Delete, null);

            LifLogHandler.LogDebug("Response for delete: {0}", LifLogHandler.Layer.IntegrationLayer, response);

            bool isDeleted = true;

            if (UNSUCCESSFUL_DATA_DELETED.Equals(response))
            {
                isDeleted = false;
            }
            return isDeleted;
        }

        public void Receive(ListDictionary adapterDetails)
        {
            Infosys.Lif.LegacyIntegratorService.MemoryDoc transportSection = null;
            Infosys.Lif.LegacyIntegratorService.Region regionToBeUsed = null;
            try
            {
                LifLogHandler.LogDebug("MemoryDoc Adapter- Receive called", LifLogHandler.Layer.IntegrationLayer);
                foreach (DictionaryEntry items in adapterDetails)
                {
                    if (items.Key.ToString() == REGION)
                    {
                        regionToBeUsed = items.Value as Region;
                    }
                    else if (items.Key.ToString() == TRANSPORT_SECTION)
                    {
                        transportSection = items.Value as Infosys.Lif.LegacyIntegratorService.MemoryDoc;
                    }
                    else if (items.Key.ToString() == TARGETURLDETAILS)
                    {
                        targetURLDetails = items.Value as NameValueCollection;
                    }
                }

               
                if (targetURLDetails == null)
                    throw new LegacyException("MemoryDoc Adapter- Metadata details are not provided.");

               

                bool check = targetURLDetails.AllKeys.Contains("container_name")
                                && !string.IsNullOrWhiteSpace(targetURLDetails["container_name"]);
                if (!check)
                    throw new LegacyException("MemoryDoc Adapter- 'container_name' missing in metadata.");

                check = targetURLDetails.AllKeys.Contains("file_name")
                                && !string.IsNullOrWhiteSpace(targetURLDetails["file_name"]);
                if (!check)
                    throw new LegacyException("MemoryDoc Adapter- 'file_name' missing in metadata.");



                
                MemoryDocDetails docDetails = ValidateTransportName(transportSection, regionToBeUsed.TransportName);

                HandleStream(docDetails, DocAccessType.Receive, null);

            }
            catch (LegacyException exception)
            {
                LifLogHandler.LogError("MemoryDoc Adapter- Receive- exception raised, reason- " + exception.Message,
                    LifLogHandler.Layer.IntegrationLayer);
                throw exception;
            }
            catch (Exception exception)
            {
                LifLogHandler.LogError("MemoryDoc Adapter- Receive- exception raised, reason- " + exception.Message,
                    LifLogHandler.Layer.IntegrationLayer);
                throw exception;
            }

        }

        public string Send(System.Collections.Specialized.ListDictionary adapterDetails, string message)
        {
            string response = SUCCESSFUL_DATA_SENT;
            Infosys.Lif.LegacyIntegratorService.MemoryDoc transportSection = null;
            Infosys.Lif.LegacyIntegratorService.Region regionToBeUsed = null;
            Stream dataStream = null;
            try
            {
                LifLogHandler.LogDebug("MemoryDoc Adapter- Send called", LifLogHandler.Layer.IntegrationLayer);
                foreach (DictionaryEntry items in adapterDetails)
                {
                    if (items.Key.ToString() == REGION)
                    {
                        regionToBeUsed = items.Value as Region;
                    }
                    else if (items.Key.ToString() == TRANSPORT_SECTION)
                    {
                        transportSection = items.Value as Infosys.Lif.LegacyIntegratorService.MemoryDoc;
                    }
                    else if (items.Key.ToString() == DATA)
                    {
                        dataStream = items.Value as Stream;
                    }
                    else if (items.Key.ToString() == TARGETURLDETAILS)
                    {
                        targetURLDetails = items.Value as NameValueCollection;
                    }
                }

                
                if (dataStream == null && targetURLDetails == null)
                    throw new LegacyException("MemoryDoc Adapter- File Stream and metadata details both cannot be empty.");

             
                bool check = targetURLDetails.AllKeys.Contains("container_name")
                                && !string.IsNullOrWhiteSpace(targetURLDetails["container_name"]);
                if (!check)
                    throw new LegacyException("MemoryDoc Adapter- 'container_name' missing in metadata.");

                check = targetURLDetails.AllKeys.Contains("file_name")
                                && !string.IsNullOrWhiteSpace(targetURLDetails["file_name"]);
                if (!check)
                    throw new LegacyException("MemoryDoc Adapter- 'file_name' missing in metadata.");

           
                MemoryDocDetails docDetails = ValidateTransportName(transportSection, regionToBeUsed.TransportName);

                response = HandleStream(docDetails, DocAccessType.Send, dataStream);
            }
            catch (LegacyException exception)
            {
                LifLogHandler.LogError("MemoryDoc Adapter- Send- exception raised, reason- " + exception.Message,
                    LifLogHandler.Layer.IntegrationLayer);
                throw exception;

            }
            catch (Exception exception)
            {
                LifLogHandler.LogError("MemoryDoc Adapter- Send- exception raised, reason- " + exception.Message,
                    LifLogHandler.Layer.IntegrationLayer);
                throw exception;
            }
            finally
            {
                if (dataStream != null)
                {
                    dataStream.Dispose();
                    dataStream = null;
                }
            }
            return response;
        }
        #endregion

       
        private MemoryDocDetails ValidateTransportName(Infosys.Lif.LegacyIntegratorService.MemoryDoc transportSection,
            string transportName)
        {
            LifLogHandler.LogDebug("MemoryDoc Adapter- ValidateTransportName called...",
                LifLogHandler.Layer.IntegrationLayer);
            MemoryDocDetails blobDetails = null;
            bool isTransportNameExists = false;
         
            for (int count = 0; count < transportSection.MemoryDocDetails.Count; count++)
            {
                blobDetails = transportSection.MemoryDocDetails[count] as MemoryDocDetails;
                if (blobDetails.TransportName == transportName)
                {
                    isTransportNameExists = true;
                    break;
                }
            }
            
            if (!isTransportNameExists)
            {
                throw new LegacyException("MemoryDoc Adapter- " + transportName + " is not defined in MSMQDetails section");
            }
            return blobDetails;
        }

        private string HandleStream(MemoryDocDetails docDetails, DocAccessType accessType, Stream dataStream)
        {
            LifLogHandler.LogDebug("MemoryDoc Adapter- HandleStream called for access type- " + accessType.ToString(),
                LifLogHandler.Layer.IntegrationLayer);
            string response = "";
            try
            {
                if (docDetails != null)
                {
                    
                    string documentsVDFromRoot = docDetails.DocumentsVirtualDirectoryFromRoot;
                    string containerName = targetURLDetails["container_name"];
                    string fileName = targetURLDetails["file_name"];
                    string cacheKey = @"/" + documentsVDFromRoot + @"/" + containerName + @"/" + fileName;
                    MemoryCacheItem memoryCacheItem = memoryCacheCollection[docDetails.TransportName];
                    MemoryCache cache = memoryCacheItem.Cache;
                    CacheItemPolicy policy = memoryCacheItem.CachePolicy;
                    switch (accessType)
                    {
                        case DocAccessType.Send:
                            if (dataStream != null)
                            {
                                byte[] _buffer = new byte[dataStream.Length];

                                int _bytesRead = 0;

                                while ((_bytesRead = dataStream.Read(_buffer, 0, _buffer.Length)) != 0)
                                {
                                                                    

                                }


                                cache.Set(cacheKey, _buffer, policy);
                                LifLogHandler.LogDebug("MemoryDoc Adapter- Data uploaded into memory :" + cacheKey,
               LifLogHandler.Layer.IntegrationLayer);
                                response = SUCCESSFUL_DATA_SENT;
                                _buffer = null;
                            }

                            break;
                        case DocAccessType.Receive:
                          


                            byte[] DataStream = cache[cacheKey] as byte[];

                            if (DataStream != null)
                            {
                                LifLogHandler.LogDebug("MemoryDoc Adapter- Data downloaded from memory :" + cacheKey,
               LifLogHandler.Layer.IntegrationLayer);
                                Stream outDataStream = new MemoryStream(DataStream);
                                ReceiveEventArgs args = new ReceiveEventArgs();
                                args.ResponseDetails = new ListDictionary();
                                args.ResponseDetails.Add("DataStream", outDataStream);
                                args.ResponseDetails.Add("FileName", targetURLDetails["file_name"]);
                                args.ResponseDetails.Add("Response", SUCCESSFUL_DATA_RECEIVED);
                                args.ResponseDetails.Add("StatusCode", SUCCESSFUL_STATUS_CODE);
                                if (Received != null)
                                {
                                    Received(args);
                                }
                            }
                            else
                            {
                                response = UNSUCCESSFUL_DATA_RECEIVED;
                                string errormsg = response + "Because data is not avavilable in memory cache:" + cacheKey;
                              
                                Exception exResponse = new Exception(errormsg);
                                throw exResponse;
                            }

                            break;
                        case DocAccessType.Delete:
                            if (cache.Contains(cacheKey))
                            {
                                cache.Remove(cacheKey);
                                response = SUCCESSFUL_DATA_DELETED;
                            }
                            else
                            {
                                response = UNSUCCESSFUL_DATA_DELETED;
                            }
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                if (accessType == DocAccessType.Send)
                    response = UNSUCCESSFUL_DATA_SENT + ex.Message;
                else if (accessType == DocAccessType.Receive)
                    response = UNSUCCESSFUL_DATA_RECEIVED + ex.Message;
                else
                    response = ex.Message;

                if (ex.InnerException != null)
                {
                    response = response + ". Inner Error Message- " + ex.InnerException.Message;
                }

                //and then raise the event for receive operation
                LifLogHandler.LogError("MemoryDoc Adapter- HandleStream- exception raised, reason- {0}",
                    LifLogHandler.Layer.IntegrationLayer, ex);
                ReceiveEventArgs args = new ReceiveEventArgs();
                args.ResponseDetails = new ListDictionary();
                args.ResponseDetails.Add("Response", response);
                args.ResponseDetails.Add("StatusCode", UNSUCCESSFUL_STATUS_CODE);
                if (Received != null)
                {
                    Received(args);
                }
            }

            return response;
        }

        private static void OnExpire(CacheEntryRemovedArguments cacheEntryRemovedArguments)
        {
            string transportName = ((MemoryCache)cacheEntryRemovedArguments.Source).Name;
            MemoryCacheItem memoryCacheItem = memoryCacheCollection[transportName];
            if (memoryCacheItem.GCTriggerExpireCount != 0)
            {
                file_expired_count += 1;
                if (file_expired_count % memoryCacheItem.GCTriggerExpireCount == 0)
                {
                    GC.Collect();
                }
            }
#if DEBUG
            LifLogHandler.LogInfo("MemoryDoc Adapter- CacheEntry OnExpire, Key - {0} Expired. Reason: {1} File expired count : {2}, ",
              LifLogHandler.Layer.IntegrationLayer,
              cacheEntryRemovedArguments.CacheItem.Key, cacheEntryRemovedArguments.RemovedReason.ToString(), file_expired_count);
#endif
        }


    }



    enum DocAccessType
    {
        Send,
        Receive,
        Delete
    }
}
