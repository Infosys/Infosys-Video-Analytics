/*=============================================================================================================== *
 * Copyright 2024 Infosys Ltd.                                                                                    *
 * Use of this source code is governed by Apache License Version 2.0 that can be found in the LICENSE file or at  *
 * http://www.apache.org/licenses/                                                                                *
 * ===============================================================================================================*/

﻿using Infosys.Ainauto.Framework.Facade;
using Infosys.Ainauto.Framework.Facade.Entity;
using Infosys.Solutions.Ainauto.VideoAnalytics.BusinessEntity;
using Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.Common;
using static Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.Common.ApplicationConstants;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Infosys.Solutions.Ainauto.VideoAnalytics.BusinessEntity.Queue;
using BE = Infosys.Solutions.Ainauto.VideoAnalytics.BusinessEntity;
using Infosys.Solutions.Ainauto.VideoAnalytics.Resource.Entity.Queue;
using System.Linq;
using System.Runtime.Caching;
using SC = Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.ServiceClientLibrary;
using System.Text;
using Infosys.Solutions.Ainauto.VideoAnalytics.Services.MaskDetector.Contracts.Message;
using DA = Infosys.Solutions.Ainauto.VideoAnalytics.Resource.DataAccess;
using Infosys.Solutions.Ainauto.VideoAnalytics.Resource.Entity.VideoAnalytics;
using Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.TaskRoute;

namespace Infosys.Solutions.Ainauto.VideoAnalytics.BusinessComponent
{
    public class FacadeHelper
    {
        Facade facade = new Facade();
        public static string applicationName = "";
        public static string eventType = "";
        public static string metricIngestor_JobName = System.Configuration.ConfigurationManager.AppSettings["metricIngestorJobName"];
        public static string blobService = "";
        string metricIngestorUrl = string.Empty;
        string metricIngestorPort = string.Empty;
        string metricIngestor_Endpoint = string.Empty;
        ObjectCache cache = MemoryCache.Default;
        CacheItemPolicy policy = new CacheItemPolicy();
        SC.MaskDetector maskDetector = new SC.MaskDetector();
        string tid = "";
        public string getAccessToken(JobInfo jobDetails)
        {
            string clientId = jobDetails.clientId;
            string clientSecret = jobDetails.clientSecret;
            FacadeResult facadeResult = null;
            Dictionary<string, string> dictConfigOverride = new Dictionary<string, string>();
            string jsonBody = "{\"clientId\": \""+ clientId + "\",\r\n\t\"clientSecret\": \""+ clientSecret + "\"}";
            dictConfigOverride.Add(FacadeClient.baseURI, jobDetails.baseURI);
            dictConfigOverride.Add(FacadeClient.operation, FacadeClient.post);
            dictConfigOverride.Add(FacadeClient.port, jobDetails.authentication_port);
            dictConfigOverride.Add(FacadeClient.endPoint, jobDetails.authentication_EndPoint);

            dictConfigOverride.Add(FacadeClient.body, jsonBody);
            Dictionary<string, string> dictReqParams = new Dictionary<string, string>();
            string authToken = null;
            using (LogHandler.TraceOperations("FacadeHelper:getAccessToken", LogHandler.Layer.WebServiceHost, Guid.NewGuid()))
            {
                facadeResult = facade.Execute(FacadeClient.R2W, FacadeClient.apiAdapter, FacadeClient.postMethod, "Authentication", dictReqParams, dictConfigOverride);
            }
                
            if (facadeResult.Status == FacadeResultStatus.Success)
            {

                JObject jsonObj = (JObject)JsonConvert.DeserializeObject(facadeResult.Data);
                var data = jsonObj.SelectToken("data");
                authToken = (string)data.SelectToken("access_token");
#if DEBUG
                LogHandler.LogDebug(String.Format("R2W getToken call is successful:  Argument passed to getToken api :" +
                            " BaseURI={0}; Operation={1};Port={2}; EndPoint={3}, Request Body={4},Response={5} ", jobDetails.baseURI, "Port", 
                            jobDetails.authentication_port, jobDetails.authentication_EndPoint, jsonBody, jsonObj),
                             LogHandler.Layer.Business, null);
#endif
            }
            else
            {
#if DEBUG
                LogHandler.LogDebug(String.Format("R2W getToken call is Failed:  Argument passed to getToken api :" +
                            " BaseURI={0}; Operation={1};Port={2}; EndPoint={3}, Request Body={4},ResponseStatus={5} ", jobDetails.baseURI, "Port",
                            jobDetails.authentication_port, jobDetails.authentication_EndPoint, jsonBody, facadeResult.Status),
                             LogHandler.Layer.Business, null);
#endif
            }
            return authToken;

        }

        public Boolean sendDataToR2W(string frameEntity, JobInfo jobDetails)
        {
            string baseUri = jobDetails.baseURI;
            string port = jobDetails.r2w_port;
            string r2w_EndPoint = jobDetails.r2w_EndPoint;

            try
            {
                List<String> msgList = new List<String>();
                FacadeResult facadeResult = null;
                Dictionary<string, string> dictReqParams = new Dictionary<string, string>();
                Dictionary<string, string> dictConfigOverride = new Dictionary<string, string>();
                string accessToken = getAccessToken(jobDetails);
                if (accessToken == null)
                {
#if DEBUG
                    LogHandler.LogDebug("No access token found while calling R2W call:",
                              LogHandler.Layer.Business, null);
#endif
                    return false;
                }
                string authorizationString = "Bearer " + accessToken;

                if (frameEntity != null && port != null && baseUri != null && r2w_EndPoint != null)
                {


                    dictConfigOverride.Add(FacadeClient.baseURI, baseUri);
                    dictConfigOverride.Add(FacadeClient.operation, FacadeClient.post);
                    dictConfigOverride.Add(FacadeClient.port, port);
                    dictConfigOverride.Add("HttpHeaders.Authorization", authorizationString);
                    dictConfigOverride.Add(FacadeClient.endPoint, r2w_EndPoint);
                    dictConfigOverride.Add(FacadeClient.body, frameEntity);


                    using (LogHandler.TraceOperations("FacadeHelper:sendDataToR2W", LogHandler.Layer.WebServiceHost, Guid.NewGuid()))
                    {
                        facadeResult = facade.Execute(FacadeClient.R2W, FacadeClient.apiAdapter, FacadeClient.postMethod, "MaskData", dictReqParams, dictConfigOverride);
                    }                        
                                        
                    if (facadeResult.Status == FacadeResultStatus.Success)
                    {

                        JObject jsonObj = (JObject)JsonConvert.DeserializeObject(facadeResult.Data);
                        bool result = (Boolean)jsonObj.SelectToken("result");
#if DEBUG
                        LogHandler.LogDebug(String.Format("R2W call is successful:  Argument passed to R2W api :" +
                            " BaseURI={0}; Operation={1};Port={2}; EndPoint={3}, Request Body={4},Response={5} ", baseUri, "Post", port, r2w_EndPoint, frameEntity,jsonObj),
                             LogHandler.Layer.Business, null);
#endif
                        return result;
                    }
                    else
                    {
#if DEBUG
                        LogHandler.LogDebug(String.Format("R2W call is Failed:  Argument passed to R2W api :" +
                            " BaseURI={0}; Operation={1};Port={2}; EndPoint={3}, Request Body={4},ResponseStatus={5}",
                            baseUri, "Post", port, r2w_EndPoint, frameEntity, facadeResult.Status),LogHandler.Layer.Business, null);
#endif
                    }

                }
                else
                {
                    if(frameEntity == null)
                    {
                        throw new Exception("Body is null");
                    }
                    else
                    {
                        throw new ArgumentNullException();
                    }
                    
                }   
            }
           catch (Exception ex)
           {
#if DEBUG
                LogHandler.LogDebug(String.Format("Exception occured while calling R2W call:  Argument passed to R2W api : BaseURI={0}; Operation={1};Port={2}; EndPoint={3}", baseUri, "Post", port, r2w_EndPoint),
                              LogHandler.Layer.Business, null);
#endif
                LogHandler.LogError("Exception in sendDataToR2W", LogHandler.Layer.Business,ex.Message);
                throw ex;
           }
            
            return false;

        }




        public void SendMetricIngestorData(BE.Queue.MetricIngestorMetadata metricIngestorMetadata)
        {
            using (LogHandler.TraceOperations("SendMetricIngestorData:FacadeHelper", LogHandler.Layer.Business, Guid.NewGuid(), null))
            {
                MetricData metricData = new MetricData();
                Metrics metrics = new Metrics();
                metricData.MetricMessages = new List<Metrics>();
                metrics.Application = applicationName;
                metrics.Source = applicationName;
                metrics.EventType = eventType;
                metrics.ResourceId = metricIngestorMetadata.Did;
                string container = metricIngestorMetadata.Tid + "_" + metricIngestorMetadata.Did;
                
                long frameGrabTimeTick = long.Parse(metricIngestorMetadata.Fid);

                DateTime frameGrabTime = new DateTime(frameGrabTimeTick);
                metrics.MetricTime = frameGrabTime.ToString();
                tid = metricIngestorMetadata.Tid;
                DeviceDetails deviceDetails = ConfigHelper.SetDeviceDetails(metricIngestorMetadata.Tid, metricIngestorMetadata.Did, CacheConstants.MetricIngestorCode);
                string metricIngestorLink = string.Format(MetricIngestorFacadeClient.fileUrlFormat, deviceDetails.StorageBaseUrl , blobService, container , metricIngestorMetadata.Fid + FileExtensions.jpg);

                string descriptionFormat = deviceDetails.EmailNoticationDescription;
                metricIngestorLink =  String.Format("<a href=\"{0}\">Click here</a>", metricIngestorLink);

                var objectList = metricIngestorMetadata.Fs;
                List<string> metricList = new List<string>();
                metricList = deviceDetails.MetricType.Split(',').ToList();
                for (var i = 0; i < metricList.Count; i++)
                {
                    string metricType = metricList[i];
                    switch (metricType)
                    {
                        case "ClassDetected":
                            for (int j = 0; j < objectList.Count(); j++)
                            {
                                metrics.MetricName = deviceDetails.MetricType;
                                metrics.MetricValue = objectList[i].Lb;
                                metrics.Description = constructDescription(descriptionFormat, metrics.MetricName, metrics.MetricValue,metrics.MetricTime, metricIngestorLink);                            
                                metricData.MetricMessages.Add(metrics);
                            }
                            break;
                        case "ClassAggregation":
                            var labelList = objectList.Select(x => { return x.Lb; }).Distinct().ToList();
                            for (int j = 0; j < labelList.Count(); j++)
                            {
                                metrics.MetricName = labelList[i];
                                metrics.MetricValue = objectList.Where(x => x.Lb == labelList[i]).Count().ToString();
                                metrics.Description = constructDescription(descriptionFormat, metrics.MetricName, metrics.MetricValue, metrics.MetricTime, metricIngestorLink);
                                metricData.MetricMessages.Add(metrics);
                            }
                            break;
                    }
                }
                if (metricData.MetricMessages.Count > 0)
                {
                    string metricIngestorString = JsonConvert.SerializeObject(metricData);
                    sendDataToMetricIngestor(metricIngestorString);


                }



            }

        }

        public  string constructDescription( string s,string metricName,string metricValue, string MetricTime,string frameLink)
        {
            StringBuilder sb = new StringBuilder(s);
            
            sb.Replace("[METRICNAME]", metricName);
            sb.Replace("[METRICVALUE]", metricValue);
            sb.Replace("[METRICTIME]", MetricTime);
            sb.Replace("[FRAMELINK]", frameLink);
          
            return sb.ToString();
        }



        public Boolean sendDataToMetricIngestor(string metricIngestorMetadata)
        {
   
            try
            {
                List<String> msgList = new List<String>();
                FacadeResult facadeResult = null;
                Dictionary<string, string> dictReqParams = new Dictionary<string, string>();
                Dictionary<string, string> dictConfigOverride = new Dictionary<string, string>();
                ConfigData configData = null;
               

                if (cache[MetricIngestorFacadeClient.metricIngestorConfigKey] == null)
                {
                    string jobName = Config.AppSettings.MetricIngestorJobName;

                    var uri = String.Format($"{Config.AppSettings.ConfigWebApi}Configuration/GetConfiguration?tid={tid}&jobname={jobName}");
                    var apiResponse = ServiceCaller.ApiCaller(null, uri, "GET");
                    configData = JsonConvert.DeserializeObject<ConfigData>(apiResponse);


                    cache.Set(MetricIngestorFacadeClient.metricIngestorConfigKey, configData, policy);
                }
                else
                {
                    configData = (ConfigData)cache[MetricIngestorFacadeClient.metricIngestorConfigKey];
                }
                metricIngestorUrl = configData.BaseURI;
                metricIngestorPort = configData.Port;
                metricIngestor_Endpoint = configData.MetricIngestor_EndPoint;

                if (metricIngestorMetadata != null && metricIngestorPort != null && metricIngestorUrl != null && metricIngestor_Endpoint != null)
                {
                    

                    using (LogHandler.TraceOperations("FacadeHelper:sendDataToMetricIngestor", LogHandler.Layer.WebServiceHost, Guid.NewGuid()))
                    {
                        var uri = String.Format($"http://{metricIngestorUrl}:{metricIngestorPort}/{metricIngestor_Endpoint}");
                        var apiResponse = ServiceCaller.ApiCaller(JsonConvert.DeserializeObject<MetricData>(metricIngestorMetadata), uri, "POST");
                        if (!apiResponse.Contains("Error"))
                        {

#if DEBUG
                            LogHandler.LogDebug(String.Format("sendDataToMetricIngestor call is successful:  Argument passed to MetricIngestor api :" +
                                " BaseURI={0}; Operation={1};Port={2}; EndPoint={3}, Request Body={4},Response={5} ", metricIngestorUrl, "Post", metricIngestorPort, metricIngestor_Endpoint, metricIngestorMetadata, apiResponse),
                                 LogHandler.Layer.Business, null);
#endif
                            return true;
                        }
                        else
                        {
#if DEBUG
                            LogHandler.LogDebug(String.Format("sendDataToMetricIngestor call is Failed:  Argument passed to MetricIngestor api :" +
                                " BaseURI={0}; Operation={1};Port={2}; EndPoint={3}, Request Body={4},ResponseStatus={5}",
                                metricIngestorUrl, "Post", metricIngestorPort, metricIngestor_Endpoint, metricIngestorMetadata, apiResponse), LogHandler.Layer.Business, null);
#endif

                        }
                    }
                }
                else
                {
                    if (metricIngestorMetadata == null)
                    {
                        throw new Exception("Body is null");
                    }
                    else
                    {
                        throw new ArgumentNullException();
                    }

                }
                return false;
            }
            catch (Exception ex)
            {
#if DEBUG
                LogHandler.LogDebug(String.Format("Exception occured while calling metricIngestorIngestor call:  Argument passed to metricIngestorIngestor api : BaseURI={0}; Operation={1};Port={2}; EndPoint={3}", metricIngestorUrl, "Post", metricIngestorUrl, metricIngestor_Endpoint),
                              LogHandler.Layer.Business, null);
#endif
                LogHandler.LogError("Exception in sendDataTometricIngestorIngestor", LogHandler.Layer.Business, ex.Message);
                throw ex;
            }


        }

       
        public ConfigData getConfig(List<Configuration> configurations)
        {
            ConfigData configData = new ConfigData();

            Dictionary<string, string> dict = new Dictionary<string, string>();
            try
            {
                configurations.ForEach(x => dict.Add(x.ReferenceKey, x.ReferenceValue));                

                if (dict.ContainsKey("BaseURI"))
                {
                    configData.BaseURI = dict["BaseURI"];
                }
                if (dict.ContainsKey("SuperBot_EndPoint"))
                {
                    configData.MetricIngestor_EndPoint = dict["SuperBot_EndPoint"];
                }
                if (dict.ContainsKey("Port"))
                {
                    configData.Port = dict["Port"];
                }

              
            }
            catch (Exception e)
            {
                LogHandler.LogError("Exeception in getConfig : {0}", LogHandler.Layer.Business, e.Message);
                throw e;
            }
            return configData;
        }

    }
}
