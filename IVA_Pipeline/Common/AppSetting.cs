/*=============================================================================================================== *
 * Copyright 2024 Infosys Ltd.                                                                                    *
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
            var config = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json").Build();
            config.Bind("AppSettings", _appSettings);
            config.Bind("DeviceConfiguration", _deviceConfig);


        }

        public static string GetConnectionString(string connectingStringName)
        {
            var config = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json").Build();

            return config.GetConnectionString(connectingStringName);
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
        public bool EnableAllLogs { get; set; }
        public bool EnablePerformanceLog { get; set; }
        public int TenantID { get; set; }
        public string DeviceID { get; set; }
        public int MaxFailCount { get; set; }
        public int MaxThreadOnPool { get; set; }
        public int MinThreadOnPool { get; set; }
        public double ReduceFrameQualityTo { get; set; }
        public List<string> VideoFormatsToUse { get; set; }
        public int OfflineProcessInterval { get; set; }
        public string ServiceBaseUrl { get; set; }
        public int FTPCycle { get; set; }
        public double FrameTimeDifferenceIgnoreThreshold { get; set; }
        public string ConfigWebApi { get; set; }
        public string FrameDetailsWebApi { get; set; }
        public int EmptyFrameProcessInterval { get; set; }
        public int MaxEmptyFrameCount { get; set; }
        public string CalculateFrameGrabberFPR { get; set; }
        public int FARCheckWaitTime { get; set; }
        public string CounterInstanceToBeReset { get; set; }
        public string ProcessLoaderTraceFile { get; set; }
        public int ClientConnectionWaitingTime { get; set; }
        public string DataStreamTimeOut { get; set; }
        public int FrameRenderer_WaitTimeForTransportms { get; set; }
        public bool FrameRender_IsSharedResource { get; set; }
        public string DebugImageFilePath { get; set; }
        public string ImageDebugEnabled { get; set; }
        public bool EnablePing { get; set; }
        public int ClientConnectionRetryCount { get; set; }
        public string PredictionType { get; set; }

        public string AnalyticsPredictionType { get; set; }
        public string MetricIngestorJobName { get; set; }
        public string FfmpegExeFile { get; set; }
        public string ConfigSource { get; set; }
        public bool DBEnabled { get; set; }
        public string ConfigFilePath { get; set; }
        public string FgDebugImageFilePath { get; set; }
        public string ProcessConfigFilePath { get; set; }
        public string ModelXmlFilePath { get; set; }
        public string FpDebugImageFilePath { get; set; }
        public string PredictionModel { get; set; }
        public string PreviewVideoFolder { get; set; }
        public string ServiceFolder { get; set; }
        public string Resources { get; set; }

        public int FrameGrabRateThrottlingSleepDurationMsec { get; set; }
        public int FrameGrabRateThrottlingSleepFrameCount { get; set; }

        public int frameRenderer_WaitTimeForSequencingMsec { get; set; }
        public string FrameRenderer_EOF_File_Path { get; set; }
        public int FrameRenderer_EOF_Count { get; set; }

        public string ElasticsearchUrl { get; set; }

        public List<string> ImageFormatsToUse { get; set; }  

        public string RenderImageEnabled { get; set; }

        public string RenderImageFilePath { get; set; }

        public string ElasticStoreIndexName { get; set; }

    }
}
