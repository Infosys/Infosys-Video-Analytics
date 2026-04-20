using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using NLog;
using NLog.Extensions.Logging;

namespace Infosys.Lif.LegacyCommon
{

    public class LifLogHandler
    {
        //Logger writer = LogManager.GetCurrentClassLogger();
        static Logger logger;

        // Reads nlog config from appsettings.json
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

        
        /// <summary>
        /// Log debug statements in the application
        /// </summary>
        /// <param name="message">Message to be logged</param>
        /// <param name="applicationLayer">Is the architecture layer of the application in which the debug statements have to be placed</param>
        /// <param name="messageArguments">Optional. Arguments to assign dynamic values for the placeholders in the message</param>

        public static void LogDebug(string message, Layer applicationLayer, params object[] messageArguments)
        {
            try
            {
                logger.Debug(message, messageArguments);
                //LogEntry logEntry = new LogEntry();
                //logEntry.EventId = (int) applicationLayer;
                //logEntry.Priority = 10;
                //logEntry.Severity = System.Diagnostics.TraceEventType.Verbose;
                //if (null != messageArguments)
                //{
                //    logEntry.Message = string.Format(message, messageArguments);
                //}
                //else
                //{
                //    logEntry.Message = message;
                //}
                //logEntry.Categories.Add("General");

                //if (Logger.ShouldLog(logEntry))
                //{
                //    Logger.Write(logEntry);
                //}
            }
            catch (Exception) { }
        }

        
        /// <summary>
        /// Log errors in the application
        /// </summary>
        /// <param name="message">Message to be logged</param>
        /// <param name="applicationLayer">Is the architecture layer of the application in which the debug statements have to be placed</param>
        /// <param name="messageArguments">Optional. Arguments to assign dynamic values for the placeholders in the message</param>

        public static void LogError(string message, Layer applicationLayer, params object[] messageArguments)
        {
            try
            {
                logger.Error(message, messageArguments);
                //LogEntry logEntry = new LogEntry();
                //logEntry.EventId = (int) applicationLayer;
                //logEntry.Priority = 2;
                //logEntry.Severity = System.Diagnostics.TraceEventType.Error;
                //if (null != messageArguments)
                //{
                //    logEntry.Message = string.Format(message, messageArguments);
                //}
                //else
                //{
                //    logEntry.Message = message;
                //}
                //logEntry.Categories.Add("General");

                //if (Logger.ShouldLog(logEntry))
                //{
                //    Logger.Write(logEntry);
                //}
            }
            catch (Exception) { }
        }
        public static void LogInfo(string message, Layer applicationLayer, params object[] messageArguments)
        {
            try
            {
                logger.Info(message, messageArguments);
                //LogEntry logEntry = new LogEntry();
                //logEntry.EventId = (int) applicationLayer;
                //logEntry.Priority = 2;
                //logEntry.Severity = System.Diagnostics.TraceEventType.Error;
                //if (null != messageArguments)
                //{
                //    logEntry.Message = string.Format(message, messageArguments);
                //}
                //else
                //{
                //    logEntry.Message = message;
                //}
                //logEntry.Categories.Add("General");

                //if (Logger.ShouldLog(logEntry))
                //{
                //    Logger.Write(logEntry);
                //}
            }
            catch (Exception) { }
        }
    }
}
