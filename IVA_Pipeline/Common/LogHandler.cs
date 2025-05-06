/*=============================================================================================================== *
 * Copyright 2025 Infosys Ltd.                                                                                    *
 * Use of this source code is governed by Apache License Version 2.0 that can be found in the LICENSE file or at  *
 * http://www.apache.org/licenses/                                                                                *
 * ===============================================================================================================*/
﻿

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Configuration;
using NLog;
using NLog.Extensions.Logging;
using NLog.Layouts;


namespace Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.Common
{
    public class LogHandler
    {
        static Logger logger;

        static LogHandler()
        {
            var config = new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: true, reloadOnChange: true).Build();

            LogManager.Configuration = new NLogLoggingConfiguration(config.GetSection("NLog"));
            logger = LogManager.GetCurrentClassLogger();


            
            

        }

        public enum Layer
        {
            WebUI = 500,
            WebServiceHost = 501,
            Business = 502,
            Resource = 503,
            Infrastructure = 504,
            Job = 505,
            Facade = 506,
            FrameGrabber = 507,
            MaskPrediction = 508,
            TCPChannelCommunication = 509,
            BlobCleaner = 510,
            ComputerVision = 511,
            FrameProcessor = 512,
            FrameRenderer = 513,
            PromptHandler = 514,
            PromptInjector = 515,
            PcdHandler=516,
            DataAggregator=517
        }


       
        public static void LogDebug(string message, Layer applicationLayer, params object[] messageArguments)
        {
            try
            {
                string msg = applicationLayer + " | " + message;
                logger.Debug(msg, messageArguments);
            }
            catch (Exception) { }
        }



        public static void LogError(string message, Layer applicationLayer, params object[] messageArguments)
        {
            try
            {
                logger.Error(message, messageArguments);
            }
            catch (Exception) { }
        }

        public static void LogError1(string message, string layer)
        {
            try
            {
                logger.Error(message, layer);
            }
            catch (Exception) { }
        }
        public static void LogInfo(string message, Layer applicationLayer, params object[] messageArguments)
        {
            try
            {
              
                logger.Info(message, messageArguments);
            }
            catch (Exception) { }
        }

        public static void LogWarning(string message, Layer applicationLayer, params object[] messageArguments)
        {
            try
            {
                logger.Debug(message, messageArguments);
            }
            catch (Exception) { }
        }

        public static void LogUsage(string v, object p)
        {

        }

        public static IDisposable TraceOperations(string v, Layer maskPrediction, Guid guid, object p)
        {
            return null;
        }

        public static IDisposable TraceOperations(string message, Layer applicationLayer, Guid activityId, params object[] messageArguments)
        {
            return null;
           
        }

        #region Old Code
       
        
        private static bool enablePerfMonLogs;





        public static void ArchiveMessages(string serializedPresentationMsg, string transportRegion)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
