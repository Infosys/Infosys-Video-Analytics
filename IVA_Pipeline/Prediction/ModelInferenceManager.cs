/*=============================================================================================================== *
 * Copyright 2024 Infosys Ltd.                                                                                    *
 * Use of this source code is governed by Apache License Version 2.0 that can be found in the LICENSE file or at  *
 * http://www.apache.org/licenses/                                                                                *
 * ===============================================================================================================*/

﻿
using Emgu.CV;

using Infosys.Solutions.Ainauto.VideoAnalytics.AIModels;
using Infosys.Solutions.Ainauto.VideoAnalytics.BusinessEntity;
using Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.Common;

using Microsoft.AspNetCore.Mvc.ModelBinding;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;



namespace Infosys.Solutions.Ainauto.VideoAnalytics.AIModels
{

    public class ModelInferenceManager
    {

        private static AppSettings appSettings = Config.AppSettings;

        public static string ModelInference(ModelParameters modeltoInfer,Stream st) {
            #if DEBUG
            using(LogHandler.TraceOperations("ModelInferenceManager:ModelInference",LogHandler.Layer.MaskPrediction,Guid.NewGuid(),null)) {
                #endif
                
                string metadata=string.Empty;
                bool status=true;
                ExecuteBase client=null;
               
                modeltoInfer.ModelType=GetModelType(modeltoInfer.ModelName);
                modeltoInfer.PredictionKey=GetPredictionKey(modeltoInfer.ModelName);
                modeltoInfer.AuthenticationUrl=GetAuthenticationUrl(modeltoInfer.ModelName);
                modeltoInfer.Host=GetHost(modeltoInfer.ModelName);
                modeltoInfer.BaseUrl=GetModelUrl(modeltoInfer.ModelName);
                modeltoInfer.ModelPath=GetModelPath(modeltoInfer.ModelName);
                modeltoInfer.ModelLabelPath=GetModelLabelPath(modeltoInfer.ModelName);
                modeltoInfer.CanonicalPath=GetCanonicalPath(modeltoInfer.ModelName);
                modeltoInfer.ImagePixelExtractionOrder=GetImagePixelExtractionOrder(modeltoInfer.ModelName);
                modeltoInfer.GPUDeviceId=GetGPUDevceId(modeltoInfer.ModelName);
                modeltoInfer.CPUFallbackValue=GetCPUFallbackValue(modeltoInfer.ModelName);
                /* Getting AWS details */
                var awsDetails=GetAWSDetails(modeltoInfer.ModelName);
                if(awsDetails.Count>0) {
                    modeltoInfer.AWSEndpointName=awsDetails[0];
                    modeltoInfer.AWSAccessKey=awsDetails[1];
                    modeltoInfer.AWSSecretKey=awsDetails[2];
                    modeltoInfer.AWSSessionToken=awsDetails[3];
                    modeltoInfer.AWSRegionName=awsDetails[4];
                }
                if(!String.IsNullOrEmpty(modeltoInfer.ModelType)) {
                    modeltoInfer.ModelType="Infosys.Solutions.Ainauto.VideoAnalytics.AIModels"+"."+modeltoInfer.ModelType;
                    client=(ExecuteBase)Activator.CreateInstance(Type.GetType(modeltoInfer.ModelType));
                }
                else
                    throw new NullReferenceException();
                if(client != null) {
                 
                    if(!(string.IsNullOrEmpty(modeltoInfer.ModelLabelPath) && string.IsNullOrEmpty(modeltoInfer.ModelPath))) {
                          status=client.InitializeModel(modeltoInfer);
                        
                    }
                    else {
                     
                        status=client.InitializeModel();
                    }
                   
                    if(status) {
                        metadata=client.MakePrediction(st,modeltoInfer);
                        #region Commenting old cod for IVA new request/response structure
                        
                        #endregion
                    }
                    else
                        throw new NullReferenceException();
                }
                else
                    throw new NullReferenceException();
                return metadata;
                #if DEBUG
            }
            #endif
        }

        private static string GetModelType(string modelType)
        {
#if DEBUG
            using (LogHandler.TraceOperations("ModelInferenceManager:GetModelType", LogHandler.Layer.MaskPrediction, Guid.NewGuid(), null))
            {
#endif
                string type = "";
                string modelUrl = "";

                XElement root = XElement.Load(AppDomain.CurrentDomain.BaseDirectory + GetModelXmlPath());
                var objType = from modeltype in root.Elements("Type")
                              where modeltype.Attribute("key").Value.ToLower().Contains(modelType?.ToLower())
                              select modeltype.Value;

                if (objType.FirstOrDefault() != null)
                    type = objType.FirstOrDefault().ToString();

                return type;
#if DEBUG
            }
#endif
        }

        private static string GetModelPath(string modelType)
        {
#if DEBUG
            using (LogHandler.TraceOperations("ModelInferenceManager:GetModelType", LogHandler.Layer.MaskPrediction, Guid.NewGuid(), null))
            {
#endif
                string modelPath = string.Empty;

                XElement root = XElement.Load(AppDomain.CurrentDomain.BaseDirectory + GetModelXmlPath());


                var objModelUrl = from modeltype in root.Elements("Type")
                                  where modeltype.Attribute("key").Value.ToLower().Contains(modelType.ToLower())
                                  select modeltype?.Attribute("modelPath")?.Value;

                if (objModelUrl?.FirstOrDefault() != null)
                    modelPath = objModelUrl.FirstOrDefault().ToString();

                return modelPath;
#if DEBUG
            }
#endif
        }

        private static string GetImagePixelExtractionOrder(string modelType)
        {
#if DEBUG
            using (LogHandler.TraceOperations("ModelInferenceManager:GetImagePixelExtractionOrder", LogHandler.Layer.MaskPrediction, Guid.NewGuid(), null))
            {
#endif
                string imagePixelExtractionOrder = string.Empty;

                XElement root = XElement.Load(AppDomain.CurrentDomain.BaseDirectory + GetModelXmlPath());


                var objModelUrl = from modeltype in root.Elements("Type")
                                  where modeltype.Attribute("key").Value.ToLower().Contains(modelType.ToLower())
                                  select modeltype?.Attribute("imagePixelExtractionOrder")?.Value;

                if (objModelUrl?.FirstOrDefault() != null)
                    imagePixelExtractionOrder = objModelUrl.FirstOrDefault().ToString();

                return imagePixelExtractionOrder;
#if DEBUG
            }
#endif
        }

        private static string GetGPUDevceId(string modelType)
        {
#if DEBUG
            using (LogHandler.TraceOperations("ModelInferenceManager:GetGPUDevceId", LogHandler.Layer.MaskPrediction, Guid.NewGuid(), null))
            {
#endif
                string gpuDeviceId = string.Empty;

                XElement root = XElement.Load(AppDomain.CurrentDomain.BaseDirectory + GetModelXmlPath());


                var objModelUrl = from modeltype in root.Elements("Type")
                                  where modeltype.Attribute("key").Value.ToLower().Contains(modelType.ToLower())
                                  select modeltype?.Attribute("gpuDeviceId")?.Value;

                if (objModelUrl?.FirstOrDefault() != null)
                    gpuDeviceId = objModelUrl.FirstOrDefault().ToString();

                return gpuDeviceId;
#if DEBUG
            }
#endif
        }

        private static string GetCPUFallbackValue(string modelType)
        {
#if DEBUG
            using (LogHandler.TraceOperations("ModelInferenceManager:GetCpuFallbackValue", LogHandler.Layer.MaskPrediction, Guid.NewGuid(), null))
            {
#endif
                string cpuFallbackValue = string.Empty;

                XElement root = XElement.Load(AppDomain.CurrentDomain.BaseDirectory + GetModelXmlPath());


                var objModelUrl = from modeltype in root.Elements("Type")
                                  where modeltype.Attribute("key").Value.ToLower().Contains(modelType.ToLower())
                                  select modeltype?.Attribute("cpuFallbackValue")?.Value;

                if (objModelUrl?.FirstOrDefault() != null)
                    cpuFallbackValue = objModelUrl.FirstOrDefault().ToString();

                return cpuFallbackValue;
#if DEBUG
            }
#endif
        }

        private static string GetModelUrl(string modelType)
        {
#if DEBUG
            using (LogHandler.TraceOperations("ModelInferenceManager:GetModelType", LogHandler.Layer.MaskPrediction, Guid.NewGuid(), null))
            {
#endif
                string modelUrl = "";

                XElement root = XElement.Load(AppDomain.CurrentDomain.BaseDirectory + GetModelXmlPath());


                var objModelUrl = from modeltype in root.Elements("Type")
                                  where modeltype.Attribute("key").Value.ToLower().Contains(modelType.ToLower())
                                  select modeltype.Attribute("modelUrl")?.Value;

                if (objModelUrl.FirstOrDefault() != null)
                    modelUrl = objModelUrl.FirstOrDefault().ToString();

                return modelUrl;
#if DEBUG
            }
#endif
        }

        private static string GetCanonicalPath(string modelType)
        {
#if DEBUG
            using (LogHandler.TraceOperations("ModelInferenceManager:GetModelType", LogHandler.Layer.MaskPrediction, Guid.NewGuid(), null))
            {
#endif
                string canonicalPath = string.Empty;

                XElement root = XElement.Load(AppDomain.CurrentDomain.BaseDirectory + GetModelXmlPath());


                var objModelUrl = from modeltype in root.Elements("Type")
                                  where modeltype.Attribute("key").Value.ToLower().Contains(modelType.ToLower())
                                  select modeltype?.Attribute("canonicalPath")?.Value;

                if (objModelUrl?.FirstOrDefault() != null)
                    canonicalPath = objModelUrl.FirstOrDefault().ToString();

                return canonicalPath;
#if DEBUG
            }
#endif
        }

        private static string GetCanonicalPathVersion(string modelType)
        {
#if DEBUG
            using (LogHandler.TraceOperations("ModelInferenceManager:GetModelType", LogHandler.Layer.MaskPrediction, Guid.NewGuid(), null))
            {
#endif
                string canonicalPathVersion = string.Empty;

                XElement root = XElement.Load(AppDomain.CurrentDomain.BaseDirectory + GetModelXmlPath());


                var objModelUrl = from modeltype in root.Elements("Type")
                                  where modeltype.Attribute("key").Value.ToLower().Contains(modelType.ToLower())
                                  select modeltype?.Attribute("ver")?.Value;

                if (objModelUrl?.FirstOrDefault() != null)
                    canonicalPathVersion = objModelUrl.FirstOrDefault().ToString();

                return canonicalPathVersion;
#if DEBUG
            }
#endif
        }

        private static string GetModelLabelPath(string modelType)
        {
#if DEBUG
            using (LogHandler.TraceOperations("ModelInferenceManager:GetModelType", LogHandler.Layer.MaskPrediction, Guid.NewGuid(), null))
            {
#endif
                string modelLabelPath = string.Empty;

                XElement root = XElement.Load(AppDomain.CurrentDomain.BaseDirectory + GetModelXmlPath());


                var objModelUrl = from modeltype in root.Elements("Type")
                                  where modeltype.Attribute("key").Value.ToLower().Contains(modelType.ToLower())
                                  select modeltype?.Attribute("modelLabelPath")?.Value;

                if (objModelUrl.FirstOrDefault() != null)
                    modelLabelPath = objModelUrl.FirstOrDefault().ToString();

                return modelLabelPath;
#if DEBUG
            }
#endif
        }

        private static string GetAuthenticationUrl(string modelType)
        {
#if DEBUG
            using (LogHandler.TraceOperations("ModelInferenceManager:GetauthenticationUrl", LogHandler.Layer.MaskPrediction, Guid.NewGuid(), null))
            {
#endif
                string authenticationUrl = string.Empty;

                XElement root = XElement.Load(AppDomain.CurrentDomain.BaseDirectory + GetModelXmlPath());


                var objModelAuthenticationUrl = from modeltype in root.Elements("Type")
                                                where modeltype.Attribute("key").Value.ToLower().Contains(modelType.ToLower())
                                                select modeltype.Attribute("authenticationUrl")?.Value;

                if (objModelAuthenticationUrl != null && objModelAuthenticationUrl.FirstOrDefault() != null)
                    authenticationUrl = objModelAuthenticationUrl.FirstOrDefault().ToString();

                return authenticationUrl;
#if DEBUG
            }
#endif
        }

        private static string GetHost(string modelType)
        {
#if DEBUG
            using (LogHandler.TraceOperations("ModelInferenceManager:Gethost", LogHandler.Layer.MaskPrediction, Guid.NewGuid(), null))
            {
#endif
                string host = string.Empty;

                XElement root = XElement.Load(AppDomain.CurrentDomain.BaseDirectory + GetModelXmlPath());

                var objModelHost = from modeltype in root.Elements("Type")
                                   where modeltype.Attribute("key").Value.ToLower().Contains(modelType.ToLower())
                                   select modeltype.Attribute("host")?.Value;


                if (objModelHost != null && objModelHost.FirstOrDefault() != null)
                    host = objModelHost.FirstOrDefault().ToString();

                return host;
#if DEBUG
            }
#endif
        }


        private static string GetPredictionKey(string modelType)
        {
#if DEBUG
            using (LogHandler.TraceOperations("ModelInferenceManager:GetPredictionKey", LogHandler.Layer.MaskPrediction, Guid.NewGuid(), null))
            {
#endif
                string predictionKey = string.Empty;

                XElement root = XElement.Load(AppDomain.CurrentDomain.BaseDirectory + GetModelXmlPath());

                var r = root.Elements("Type").ToList();


                var objModelPredictionKey = from modeltype in root.Elements("Type")
                                            where modeltype.Attribute("key").Value.ToLower().Contains(modelType.ToLower())
                                            select modeltype.Attribute("predictionKey")?.Value;



                if (objModelPredictionKey != null && objModelPredictionKey.FirstOrDefault() != null)
                    predictionKey = objModelPredictionKey.FirstOrDefault().ToString();

                return predictionKey;
#if DEBUG
            }
#endif
        }

     
        public static string ModelInference(ModelParameters modeltoInfer, string st)
        {
#if DEBUG
            using (LogHandler.TraceOperations("ModelInferenceManager:ModelInference", LogHandler.Layer.MaskPrediction, Guid.NewGuid(), null))
            {
#endif
                string baseUrl = "";
                string modelType = "";
                string modelName = "";
                string metadata = "";
                bool status = true;

                ExecuteBase client = null;

                modelName = modeltoInfer.ModelName;
                modelType = GetModelType(modelName);

                baseUrl = GetModelUrl(modelName);
         
                if (!String.IsNullOrEmpty(modelType))
                {
                    modelType = "Infosys.Solutions.Ainauto.VideoAnalytics.AIModels" + "." + modelType;
                    client = (ExecuteBase)Activator.CreateInstance(Type.GetType(modelType));
                }
                else
                    throw new NullReferenceException();

                if (client != null)
                {
                    status = client.InitializeModel();

                    if (status)
                        metadata = client.MakePrediction(st, baseUrl, modelName);
                    else
                        throw new NullReferenceException();
                }
                else
                    throw new NullReferenceException();

                return metadata;
#if DEBUG
            }
#endif
        }

       
        public static string ModelInference(ModelParameters modeltoInfer, Stream stream, string st)
        {
#if DEBUG
            using (LogHandler.TraceOperations("ModelInferenceManager:ModelInference", LogHandler.Layer.MaskPrediction, Guid.NewGuid(), null))
            {
#endif
                string baseUrl = "";
                string modelType = "";
                string modelName = "";
                string metadata = "";
                bool status = true;
                string authenticationUrl = "";
                ExecuteBase client = null;

                modelName = modeltoInfer.ModelName = modeltoInfer.ModelName;
                modelType = GetModelType(modelName);
                modeltoInfer.AuthenticationUrl = GetAuthenticationUrl(modelName);
                modeltoInfer.BaseUrl = GetModelUrl(modelName);

                var host = GetHost(modelName);
                string confidenceThreshold = modeltoInfer.ConfidenceThreshold.ToString();
                float overlapThreshold = modeltoInfer.OverlapThreshold;

                if (!String.IsNullOrEmpty(modelType))
                {
                    modelType = "Infosys.Solutions.Ainauto.VideoAnalytics.AIModels" + "." + modelType;
                    client = (ExecuteBase)Activator.CreateInstance(Type.GetType(modelType));
                }
                else
                    throw new NullReferenceException();

                if (client != null)
                {
                    status = client.InitializeModel();
                    
                    if (status)
                    {
                        #region Commented Old Code for IVA New Request/Response
                        /*
                        if (!string.IsNullOrEmpty(authenticationUrl) && !string.IsNullOrEmpty(host))
                        {
                            metadata = client.MakePrediction(stream, st, confidenceThreshold, overlapThreshold,
                                baseUrl, modelName, authenticationUrl, host, modeltoInfer.TokenCacheExpirationTime);
                        }
                        else if (!string.IsNullOrEmpty(authenticationUrl))
                        {
                            metadata = client.MakePrediction(stream, st, confidenceThreshold, overlapThreshold,
                                baseUrl, modelName, authenticationUrl, modeltoInfer.TokenCacheExpirationTime);
                        }
                        else
                            metadata = client.MakePrediction(stream, st, baseUrl, modelName);
                        */
                        #endregion
                        metadata = client.MakePrediction(stream, modeltoInfer);
                    }
                    else
                        throw new NullReferenceException();

                }
                else
                    throw new NullReferenceException();

                return metadata;
#if DEBUG
            }
#endif
        }


        private static string GetModelXmlPath()
        {
            string xmlFilePath = "";
            if (!String.IsNullOrEmpty(appSettings.ModelXmlFilePath))
            {
                xmlFilePath = appSettings.ModelXmlFilePath;
            }
            else
            {
                xmlFilePath = @"XML/ModelType.xml";
            }
            return xmlFilePath;
        }

        private static List<string> GetAWSDetails(string modelType)
        {
#if DEBUG
            using (LogHandler.TraceOperations("ModelInferenceManager:GetauthenticationUrl", LogHandler.Layer.MaskPrediction, Guid.NewGuid(), null))
            {
#endif
                List<string> attribute_values = new List<string>();
                

                XElement root = XElement.Load(AppDomain.CurrentDomain.BaseDirectory + GetModelXmlPath());

                List<string> attributes = new List<string>()
                {
                    "AWSEndpointName",
                    "AWSAccessKey",
                    "AWSSecretKey",
                    "AWSSessionToken",
                    "AWSRegionName"
                };

                foreach (var attribute in attributes)
                {
                    var objModelAuthenticationUrl = from modeltype in root.Elements("Type")
                                                    where modeltype.Attribute("key").Value.ToLower().Contains(modelType.ToLower())
                                                    select modeltype.Attribute(attribute)?.Value;
                    if (objModelAuthenticationUrl != null && objModelAuthenticationUrl.FirstOrDefault() != null)
                        attribute_values.Add(objModelAuthenticationUrl.FirstOrDefault().ToString());
                }
                return attribute_values;
#if DEBUG
            }
#endif
        }
    }
}