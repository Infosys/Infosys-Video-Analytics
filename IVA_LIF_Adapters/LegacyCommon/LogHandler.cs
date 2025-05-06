/*=============================================================================================================== *
 * Copyright 2025 Infosys Ltd.                                                                                    *
 * Use of this source code is governed by Apache License Version 2.0 that can be found in the LICENSE file or at  *
 * http://www.apache.org/licenses/                                                                                *
 * ===============================================================================================================*/
﻿using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using NLog;
using NLog.Extensions.Logging;

namespace Infosys.Lif.LegacyCommon
{

    public class LifLogHandler
    {
      
        static Logger logger;

       
        static LifLogHandler()
        {
            var config = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json", optional: true, reloadOnChange: true).Build();

            LogManager.Configuration = new NLogLoggingConfiguration(config.GetSection("NLog"));
            logger = LogManager.GetCurrentClassLogger();
        }
        
        public enum Layer
        {
            IntegrationLayer = 510
        }

        
        

        public static void LogDebug(string message, Layer applicationLayer, params object[] messageArguments)
        {
            try
            {
                logger.Debug(message, messageArguments);
               
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
        public static void LogInfo(string message, Layer applicationLayer, params object[] messageArguments)
        {
            try
            {
                logger.Info(message, messageArguments);
                

            }
            catch (Exception) { }
        }
    }
}
