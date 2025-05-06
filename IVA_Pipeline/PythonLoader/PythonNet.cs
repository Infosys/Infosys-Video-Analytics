/*=============================================================================================================== *
 * Copyright 2025 Infosys Ltd.                                                                                    *
 * Use of this source code is governed by Apache License Version 2.0 that can be found in the LICENSE file or at  *
 * http://www.apache.org/licenses/                                                                                *
 * ===============================================================================================================*/
﻿using System.Diagnostics;
 
using Infosys.Solutions.Ainauto.VideoAnalytics.BusinessEntity;
using Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.TaskRoute;
using Python.Runtime;


namespace Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.PythonLoader
{
    public class PythonNet
    {

        
        dynamic? scope;
        dynamic? inferenceModule;
        private static PythonNet? instance = null;
        private static readonly object Instancelock = new object();
       
        public static string MILLibraryName = "";
        
        private static DeviceDetails deviceDetails = null;

        public PythonNet()
        {

            if (ConfigHelper.Cache != null)
                deviceDetails = (DeviceDetails)ConfigHelper.Cache[ConfigHelper.DeviceDetailsCacheKey]; 
            MILLibraryName= deviceDetails.MILLibraryName;
            var pathToVirtualEnv = deviceDetails.PythonVirtualPath;
            if(deviceDetails.PythonVersion.Contains("python39.dll"))
            {
                #region Python Virtual Environment   3.9
                if (Runtime.PythonDLL == null)
                {
                    Runtime.PythonDLL = deviceDetails.PythonVersion;
                    Environment.SetEnvironmentVariable("path", pathToVirtualEnv, EnvironmentVariableTarget.Process);
                    Environment.SetEnvironmentVariable("pythonhome", pathToVirtualEnv, EnvironmentVariableTarget.Process);
                    Environment.SetEnvironmentVariable("pythonpath", $"{pathToVirtualEnv}\\Lib\\site-packages;{pathToVirtualEnv}\\Lib", EnvironmentVariableTarget.Process);
                    PythonEngine.PythonPath = PythonEngine.PythonPath + ";" + Environment.GetEnvironmentVariable("pythonpath", EnvironmentVariableTarget.Process);
                    PythonEngine.PythonHome = pathToVirtualEnv;
                    PythonEngine.Initialize();
                    PythonEngine.BeginAllowThreads();
                }
                #endregion
            }
            else 
            {
                #region python version >3.9 
                if (Runtime.PythonDLL == null)
                {
                    Runtime.PythonDLL = deviceDetails.PythonVersion;
                    PythonEngine.Initialize();
                    PythonEngine.BeginAllowThreads();
                }
                #endregion
            }
             
            try
            {
                using (Py.GIL())
                {
                   scope = Py.CreateScope();
                }
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        


        private PythonNet(string name)
        {
            if (inferenceModule == null)
            {
                using (Py.GIL())
                {
                    inferenceModule = Py.Import(name);
                }
            }
        }

        public static PythonNet GetInstance
        {
            get
            {
                if (instance == null)
                {
                    lock (Instancelock)
                    {
                        if (instance == null)
                        {
                            instance = new PythonNet(MILLibraryName);
                        }
                    }
                }
                return instance;
            }
        }

        public dynamic Inference(dynamic req, string modelName)
        {
            
            dynamic detection;
            using (Py.GIL())
            {
               
                detection = inferenceModule.executeModel(req);
              
            }
            return detection;

        }

        
        public dynamic HeatMapInference(dynamic req)
        {
            
            dynamic detection;
            using (Py.GIL())
            {
                int x = 1;
                int y = 1;
                
                detection = inferenceModule.getHeatMap(req);
               
            }
            return detection;

        }
    }

}