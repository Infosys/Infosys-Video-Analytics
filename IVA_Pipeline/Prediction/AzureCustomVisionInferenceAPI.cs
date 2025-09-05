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
using Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.Common;
using System;
using System.IO;
//using SC = Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.ServiceClientLibrary;
using SE = Infosys.Solutions.Ainauto.VideoAnalytics.Services.MaskDetector.Contracts;
using Newtonsoft.Json;
using Infosys.Solutions.Ainauto.VideoAnalytics.AIModels;
using System.Net.Http;
using Infosys.Solutions.Ainauto.VideoAnalytics.Services.MaskDetector.Contracts.Message;
using System.Net.Http.Headers;
using Newtonsoft.Json.Linq;
using Infosys.Solutions.Ainauto.VideoAnalytics.Resource.Entity.Queue;
using System.Threading;
//using Infosys.Solutions.Ainauto.VideoAnalytics.Entity;
using System.Diagnostics;
using Infosys.Solutions.Ainauto.VideoAnalytics.Entity;
using Infosys.Solutions.Ainauto.VideoAnalytics.BusinessEntity.Queue;
using static Microsoft.ML.Transforms.Text.LatentDirichletAllocationTransformer;

namespace Infosys.Solutions.Ainauto.VideoAnalytics.AIModels
{
    public class AzureCustomVisionInferenceAPI : ExecuteBase
    {
        ObjectDetectorAPIResMsg objectDetectorResponse = null;
        
        public override bool InitializeModel()
        {
            return true;
        }


        

        public override string MakePrediction(Stream st, ModelParameters modelParameters)
        {
            string sstime = DateTime.UtcNow.ToString("yyy-MM-dd,HH:mm:ss.fff tt");

            List<SE.Message.Mtp> MtpData = new List<SE.Message.Mtp>()
                {
                    new SE.Message.Mtp(){ Etime = modelParameters.Etime, Src = modelParameters.Src, Stime=modelParameters.Stime},
                    new SE.Message.Mtp(){ Etime = "", Src = "Frame Processor", Stime=sstime},
                };
#if DEBUG
            using (LogHandler.TraceOperations("AzureCustomVisionInferenceAPI:MakePrediction", LogHandler.Layer.MaskPrediction, Guid.NewGuid(), null))
            {

                LogHandler.LogUsage(String.Format("AzureCustomVisionInferenceAPI MakePrediction is getting executed at : {0}", DateTime.UtcNow.ToLongTimeString()), null);
#endif
                string metadata = "";
                st.Position = 0;
                byte[] image_byte_array;
               
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    st.CopyTo(memoryStream);
                    image_byte_array = memoryStream.ToArray();
                    memoryStream.Dispose();
                }
                
                MakePredictionRequestAsync(image_byte_array, modelParameters).GetAwaiter().GetResult();


                metadata = JsonConvert.SerializeObject(Helper.RemoveDuplicateRegionsAPI(objectDetectorResponse, modelParameters.OverlapThreshold));
#if DEBUG
                LogHandler.LogUsage(String.Format("AzureCustomVisionInferenceAPI MakePrediction finished execution at : {0}", DateTime.UtcNow.ToLongTimeString()), null);
#endif
                return metadata;
#if DEBUG
            }
#endif
        }


        public async Task MakePredictionRequestAsync(byte[] byteData, ModelParameters modelParameters)
        {
            try
            {
                double confidenceThreshold = Convert.ToDouble(modelParameters.ConfidenceThreshold);
                string baseUrl = modelParameters.BaseUrl;
                string predictionKey = modelParameters.PredictionKey;
                var client = new HttpClient();
                string response = "";

                
                client.DefaultRequestHeaders.Add("Prediction-Key", predictionKey);

                

                HttpResponseMessage httpresponse;

                DateTime stime = DateTime.Now;
                Stopwatch stopWatch = new Stopwatch();
                stopWatch.Start();
                using (var content = new ByteArrayContent(byteData))
                {
                    content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                    httpresponse = await client.PostAsync(baseUrl, content);
                    response = await httpresponse.Content.ReadAsStringAsync();
                }
                stopWatch.Stop();
                DateTime etime = DateTime.Now;

                

                var azurePredictionModel = (AzurePredictionModel)JsonConvert.DeserializeObject<AzurePredictionModel>(response);
                ObjectDetectorAPIResMsg result = new ObjectDetectorAPIResMsg();

                #region  Commenting old to bring new response structure
                /*
                result.Objects = new List<MaskDetectorAPIResMsg>();
                foreach (Prediction pred in azurePredictionModel.predictions)
                {
                    MaskDetectorAPIResMsg resMsg = new MaskDetectorAPIResMsg();
                    if (pred.probability >= confidenceThreshold)
                    {
                        resMsg.Lb = pred.tagName;
                        resMsg.Cs = pred.probability.ToString();
                        resMsg.Dm = new SE.Message.BoundingBox();
                        resMsg.Dm.H = pred.boundingBox.height.ToString();
                        resMsg.Dm.W = pred.boundingBox.width.ToString();
                        resMsg.Dm.X = pred.boundingBox.left.ToString();
                        resMsg.Dm.Y = pred.boundingBox.top.ToString();
                        result.Objects.Add(resMsg);
                    }
                }
                */
                #endregion

                result.Fs = new List<SE.Message.PersonDetails>();
                foreach (Prediction pred in azurePredictionModel.predictions)
                {
                    SE.Message.PersonDetails resMsg = new SE.Message.PersonDetails();
                    if (pred.probability >= confidenceThreshold)
                    {
                        resMsg.Lb = pred.tagName;
                        resMsg.Cs = pred.probability.ToString();
                        resMsg.Dm = new SE.Message.BoundingBox();
                        resMsg.Dm.H = pred.boundingBox.height.ToString();
                        resMsg.Dm.W = pred.boundingBox.width.ToString();
                        resMsg.Dm.X = pred.boundingBox.left.ToString();
                        resMsg.Dm.Y = pred.boundingBox.top.ToString();
                        resMsg.Info = "{}";
                        result.Fs.Add(resMsg);
                    }
                }
                result.Mtp = new List<SE.Message.Mtp>();
                var mtp = new SE.Message.Mtp();
                mtp.Stime = stime.ToString("HH:mm:ss tt");
                mtp.Etime = etime.ToString("HH:mm:ss tt");
                mtp.Src = "Azure Vision Inference API";
                result.Mtp.Add(mtp);
                
                result.Did = modelParameters.deviceId;
                result.Tid = modelParameters.tId;                
                result.Fid = modelParameters.Fid;
                
                result.Ts = modelParameters.Ts;
                result.Ts_ntp = modelParameters.Ts_ntp;
                
                result.Msg_ver = modelParameters.Msg_ver;
                result.Inf_ver = modelParameters.Inf_ver;
                result.Ad = modelParameters.Ad;
                result.Rc = 200;
                result.Rm = "success";
                result.Hp = modelParameters.Hp;

                objectDetectorResponse = result;
            }
            catch (Exception ex)
            {
                LogHandler.LogError(ex.Message, LogHandler.Layer.Business);
                LogHandler.LogError(ex.InnerException.Message, LogHandler.Layer.Business);
                throw ex;
            }

        }

    }

}
