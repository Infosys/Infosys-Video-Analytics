/*=============================================================================================================== *
 * Copyright 2025 Infosys Ltd.                                                                                    *
 * Use of this source code is governed by Apache License Version 2.0 that can be found in the LICENSE file or at  *
 * http://www.apache.org/licenses/                                                                                *
 * ===============================================================================================================*/
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.IO;
using Newtonsoft.Json;
using Microsoft.Practices.EnterpriseLibrary.Logging.Filters;
using System.Configuration;
using System.Text.Json;
using System.Reflection;

namespace Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.Common
{
    public static class Config
    {
        private readonly static object lockObj = new object();
        private static AppSettings _appSettings;
        private static DeviceConfig _deviceConfig;

        public static AppSettings AppSettings
        {
            get
            {
                lock (lockObj)
                {
                    RefreshConfig(); 
                    return _appSettings;
                }
            }
            set { _appSettings = value; }
        } 
        public static DeviceConfig DeviceConfig
        {
            get
            {
                lock (lockObj)
                {
                    RefreshConfig();
                    return _deviceConfig;
                }
            }
            set { _deviceConfig = value; }
        }
        public static void RefreshConfig()
        {
            _appSettings = new AppSettings();
            try
            {
                var config = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json", optional: false, reloadOnChange: true).Build();
                config.Bind("AppSettings", _appSettings);
                config.Bind("ConnectionStrings", _appSettings);
                if (!string.IsNullOrEmpty(_appSettings.ConfigurationManagement))
                {
                    Dictionary<string, string> secrets = new Dictionary<string, string>();
                    List<string> secretsList = config.GetSection("Appsettings:Secrets").Value.Split(',', StringSplitOptions.TrimEntries).ToList();
                    for (int i = 0; i < secretsList.Count; i++)
                    {
                        secrets.Add(secretsList[i], "");
                    }
                    string secretsResponse = new LIFAdapter().GetConfigurations(_appSettings.ConfigurationManagement, secrets);
                    LogHandler.LogDebug("Fetched secrets from {0}: {1}", LogHandler.Layer.Resource, _appSettings.ConfigurationManagement, secretsResponse);
                    JsonConvert.PopulateObject(secretsResponse, _appSettings);
                }
                config.Bind("DeviceConfiguration", _deviceConfig);
            }
            catch(Exception ex)
            {
                LogHandler.LogError($"Error while reading the configuration files", LogHandler.Layer.Resource);
            }
        }

        public static string GetConnectionString(string connectingStringName)
        {
            PropertyInfo propertyInfo = typeof(AppSettings).GetProperty(connectingStringName);
            return propertyInfo.GetValue(connectingStringName)?.ToString();
        }


    }


    public class DeviceConfig
    {
        public string TenantId { get; set; }
        public string TaskRoute { get; set; }
        public string TransportRegion { get; set; }
    }
    public class AppSettings
    {
        public int TenantID { get; set; }
        public string DeviceID { get; set; }
        public string ConfigWebApi { get; set; }
        
        public string MetricIngestorJobName { get; set; }
        public string ConfigSource { get; set; }
        public string ConfigFilePath { get; set; }
        public string FgDebugImageFilePath { get; set; }
        public string ProcessConfigFilePath { get; set; }
        public string ModelXmlFilePath { get; set; }
        public string FpDebugImageFilePath { get; set; }
        public string PredictionModel { get; set; }
        public string PreviewVideoFolder { get; set; }
        public string ServiceFolder { get; set; }
        public string Resources { get; set; }


        
        public string ElasticsearchUrl { get; set; }

        
        
        
        public string DBProvider { get; set; }
        public string ConfigurationManagement { get; set; }
        public string FrameDetailStore { get; set; }
        public string FaceMaskDetectionEntities { get; set; }

    }
}
