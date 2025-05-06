/*=============================================================================================================== *
 * Copyright 2025 Infosys Ltd.                                                                                    *
 * Use of this source code is governed by Apache License Version 2.0 that can be found in the LICENSE file or at  *
 * http://www.apache.org/licenses/                                                                                *
 * ===============================================================================================================*/
/* 
 * © 2012-2013 Infosys Limited, Bangalore, India. All Rights Reserved.
 * Version: 1.0 b
 * Except for any open source software components embedded in this Infosys proprietary software program ("Program"),
 * this Program is protected by copyright laws, international treaties and other pending or existing intellectual
 * property rights in India, the United States and other countries. Except as expressly permitted, any unauthorized
 * reproduction, storage, transmission in any form or by any means (including without limitation electronic, mechanical,
 * printing, photocopying, recording or otherwise), or any distribution of this Program, or any portion of it, may
 * results in severe civil and criminal penalties, and will be prosecuted to the maximum extent possible under the law.
 */

using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Xml;
using System.Xml.Serialization;

using Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.Common;
using Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.TaskRoute;
using Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.ProcessScheduler.Framework;
using Infosys.Solutions.Ainauto.VideoAnalytics.BusinessEntity;

namespace Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.ProcessScheduler
{
    public class Tasks
    {
        private static AppSettings appSettings=Config.AppSettings;
        DeviceDetails deviceDetails=ConfigHelper.SetDeviceDetails(appSettings.TenantID.ToString(),appSettings.DeviceID,CacheConstants.FrameRendererCode);
        public void InitialiseComponent(int robotId, int runInstanceId, int robotTaskMapId)
        {
            try
            {
                const string PROCESS_STARTMETHOD = "Start";

                string directory = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
               
                File.AppendAllText(deviceDetails.ProcessLoaderTraceFile,$"{DateTime.Now} || Information || CodeGenWorker - starting the worker role\n");
                

                string configFilePath = null;
                if (!String.IsNullOrEmpty(appSettings.ProcessConfigFilePath))
                {
                    configFilePath = appSettings.ProcessConfigFilePath;
                }
                else
                {
                    configFilePath = @"/Configurations/Processes.config";
                }
                string xmlstring = File.ReadAllText(directory + configFilePath);
                File.AppendAllText(deviceDetails.ProcessLoaderTraceFile,$"{DateTime.Now} || Information || CodeGenWorker - read the Processes.Config\n");
                
                File.AppendAllText(deviceDetails.ProcessLoaderTraceFile,$"{DateTime.Now} || Information || Contents of file Processes.Config...\n {xmlstring}\n");
                

                

                string MaxThreadOnPool=deviceDetails.MaxThreadOnPool.ToString();
                string MinThreadOnPool=deviceDetails.MinThreadOnPool.ToString();
                int minThread = 0;
                int maxThread = 0;
                if (!String.IsNullOrEmpty(MinThreadOnPool))
                {
                    minThread = Convert.ToInt32(MinThreadOnPool);
                }
                if (!String.IsNullOrEmpty(MaxThreadOnPool))
                {
                    maxThread = Convert.ToInt32(MaxThreadOnPool);
                }


                XmlSerializer xs = new XmlSerializer(typeof(Processes));

                MemoryStream memoryStream = new MemoryStream(SerializationOfProcess.StringToUTF8ByteArray(xmlstring));
                XmlTextWriter xmlTextWriter = new XmlTextWriter(memoryStream, Encoding.UTF8);
                
                Processes processes = (Processes)xs.Deserialize(memoryStream);

                
                File.AppendAllText(deviceDetails.ProcessLoaderTraceFile,$"{DateTime.Now} || Information || Parsed the processes from xml. Process count: {processes.Processes2Execute.Count()}\n");

                string driveLetter = "";

                int minWorker, minIOC, maxWorker, maxIOC;

                ThreadPool.GetMinThreads(out minWorker, out minIOC);
                ThreadPool.GetMaxThreads(out maxWorker, out maxIOC);

                File.AppendAllText(deviceDetails.ProcessLoaderTraceFile,$"{DateTime.Now} || Information || Getting min and max thread - minWorker: {minWorker}, minIOC: {minIOC}, maxWorker: {maxWorker}, maxIOC: {maxIOC}\n");
                if (minThread != 0)
                {
                    bool isMinThreadCreated = ThreadPool.SetMinThreads(minThread, minIOC);
                    
                    File.AppendAllText(deviceDetails.ProcessLoaderTraceFile,$"{DateTime.Now} || Information || Setting min thread - isMinThreadCreated: {isMinThreadCreated}, minThread: {minThread}\n");
                }

                if (maxThread != 0)
                {
                    bool isMaxThreadCreated = ThreadPool.SetMaxThreads(maxThread, maxIOC);
                    File.AppendAllText(deviceDetails.ProcessLoaderTraceFile,$"{DateTime.Now} || Information || Setting max thread - isMaxThreadCreated: {isMaxThreadCreated}, maxThread: {maxThread}\n");
                }


                ThreadPool.GetMinThreads(out minWorker, out minIOC);
                ThreadPool.GetMaxThreads(out maxWorker, out maxIOC);

                File.AppendAllText(deviceDetails.ProcessLoaderTraceFile,$"{DateTime.Now} || Information || After setting min and max thread - minWorker: {minWorker}, minIOC: {minIOC}, maxWorker: {maxWorker}, maxIOC: {maxIOC}\n");

              
                for (int procIndex = 0; procIndex < processes.Processes2Execute.Length; procIndex++)
                {
                    var process = processes.Processes2Execute[procIndex];
                    
                    File.AppendAllText(deviceDetails.ProcessLoaderTraceFile,$"{DateTime.Now} || Information || Try to search for path: {Path.Combine(directory,process.Dll)}\n");
                    Assembly assembly = Assembly.LoadFile(Path.Combine(directory, process.Dll));
                    Type type = assembly.GetType(process.FullClassName);
                    
                    File.AppendAllText(deviceDetails.ProcessLoaderTraceFile,$"{DateTime.Now} || Information || Trying to create object of class: {process.FullClassName} from assembly: {process.Dll}...\n");
                    object obj = Activator.CreateInstance(type, process.Id);

                    

                    if (obj == null)
                    {
                        File.AppendAllText(deviceDetails.ProcessLoaderTraceFile,$"{DateTime.Now} || Information || Type name: {process.FullClassName} not found in assemble: {process.Dll}.\n");
                        return;
                    }

                    File.AppendAllText(deviceDetails.ProcessLoaderTraceFile,$"{DateTime.Now} || Information || Created object of class: {process.FullClassName}\n");
                    

                    MethodInfo method = obj.GetType().GetMethod(PROCESS_STARTMETHOD,
                        new Type[] {
                            typeof(Drive[]), typeof(ModeType), typeof(string), typeof(string),
                             typeof(int) , typeof(int),typeof(int) });
                    if (method != null)
                    {


                        ThreadStart startMethod = delegate {
                            method.Invoke(obj,
new object[] {
                                process.Drives, process.Mode, process.EntityToBeWatched, process.Id, robotId , runInstanceId , robotTaskMapId});
                        };
                        new Thread(startMethod).Start();
                    }
                }
            }
            catch (Exception ex)
            {
               
                File.AppendAllText(deviceDetails.ProcessLoaderTraceFile,$"{DateTime.Now} || Error || CodeGenWorker - exception occured:\n {ex}\n");
                File.AppendAllText(deviceDetails.ProcessLoaderTraceFile,$"{DateTime.Now} || Information || No drives to mount - {ex.Message}\n");
            }
        }
    }
}
