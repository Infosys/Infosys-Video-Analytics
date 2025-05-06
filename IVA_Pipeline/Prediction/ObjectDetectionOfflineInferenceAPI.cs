/*=============================================================================================================== *
 * Copyright 2025 Infosys Ltd.                                                                                    *
 * Use of this source code is governed by Apache License Version 2.0 that can be found in the LICENSE file or at  *
 * http://www.apache.org/licenses/                                                                                *
 * ===============================================================================================================*/
﻿/* 
 *© 2019 Infosys Limited, Bangalore, India. All Rights Reserved. Infosys believes the information in this document is accurate as of its publication date; such information is subject to change without notice. Infosys acknowledges the proprietary rights of other companies to the trademarks, product names and such other intellectual property rights mentioned in this document. Except as expressly permitted, neither this document nor any part of it may be reproduced, stored in a retrieval system, or transmitted in any form or by any means, electronic, mechanical, printing, photocopying, recording or otherwise, without the prior permission of Infosys Limited and/or any named intellectual property rights holders under this document.   
 * 
 * © 2019 INFOSYS LIMITED. CONFIDENTIAL AND PROPRIETARY 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.Common;
using Newtonsoft.Json;
using System.IO;
//using MathFloat;
using System.Drawing;
using System.Configuration;
using Microsoft.ML;
using Microsoft.ML.Transforms.Image;
using Infosys.Solutions.Ainauto.VideoAnalytics.BusinessComponent;
using PD = Infosys.Solutions.Ainauto.VideoAnalytics.Services.MaskDetector.Contracts.Message;
using Infosys.Solutions.Ainauto.VideoAnalytics.Services.MaskDetector.Contracts.Message;
using Nest;
using static System.Net.WebRequestMethods;
using static Microsoft.ML.Transforms.Text.LatentDirichletAllocationTransformer;
using SE = Infosys.Solutions.Ainauto.VideoAnalytics.Services.MaskDetector.Contracts;
using Python.Runtime;

namespace Infosys.Solutions.Ainauto.VideoAnalytics.AIModels
{
    public class SingletonONNX
    {

        public ONNXMaskPredictor maskPredictor = null;
        private static SingletonONNX instance = null;
        private static readonly object Instancelock = new object();
        
        public static ModelParameters modeltoInfer = new ModelParameters();
        public Microsoft.ML.PredictionEngine<ImageInput, ImagePredictions> predictionEngine = null;
        public string[] labels;

        
        public static SingletonONNX GetInstance
        {
            get
            {
                if (instance == null)
                {
                    lock (Instancelock)
                    {
                        if (instance == null)
                        {
                            instance = new SingletonONNX(modeltoInfer /*modelPath, modelLabelPath, modelColorScheme*/);
                        }
                    }
                }
                return instance;
            }
        }

        [Flags] enum Colors { None = 0, Red = 1, Green = 2, Blue = 4 };

        
        private SingletonONNX(ModelParameters modeltoInfer /*string onnxModelPath, string onnxModelLabelPath, string onnxModelColorScheme*/)
        {
#if DEBUG
            LogHandler.LogUsage(String.Format("SingletonONNX method of FrameProcessor is getting executed at : {0}", DateTime.UtcNow.ToLongTimeString()), null);

            using (LogHandler.TraceOperations("SingletonONNX:SingletonONNX", LogHandler.Layer.MaskPrediction, Guid.NewGuid(), null))
            {
#endif
                
                MLContext mlContext = new MLContext();

                
                List<ImageInput> emptyData = new List<ImageInput>();
                var data = mlContext.Data.LoadFromEnumerable(emptyData);
                
                int? gpuDeviceId = null;
                bool gpuBlankCheck = string.IsNullOrEmpty(modeltoInfer.GPUDeviceId);
                if (!gpuBlankCheck)
                    gpuDeviceId = int.Parse(modeltoInfer.GPUDeviceId);

                bool fallbackToCpu = bool.Parse(modeltoInfer.CPUFallbackValue);

                Dictionary<string, ImagePixelExtractingEstimator.ColorsOrder> pixelExtractionOrder = new Dictionary<string, ImagePixelExtractingEstimator.ColorsOrder>()
                {
                    {"ARGB", ImagePixelExtractingEstimator.ColorsOrder.ARGB},
                    {"ARBG", ImagePixelExtractingEstimator.ColorsOrder.ARBG},
                    {"ABRG", ImagePixelExtractingEstimator.ColorsOrder.ABRG},
                    {"ABGR", ImagePixelExtractingEstimator.ColorsOrder.ABGR},
                    {"AGRB", ImagePixelExtractingEstimator.ColorsOrder.AGRB},
                    {"AGBR", ImagePixelExtractingEstimator.ColorsOrder.AGBR}
                };

                if (!string.IsNullOrEmpty(modeltoInfer.ImagePixelExtractionOrder))
                {
                    var pipeline = mlContext.Transforms.ResizeImages(resizing: ImageResizingEstimator.ResizingKind.Fill, outputColumnName: "data", imageWidth: ImageSettings.imageWidth, imageHeight: ImageSettings.imageHeight, inputColumnName: nameof(ImageInput.Image))
                              .Append(mlContext.Transforms.ExtractPixels(outputColumnName: "data", orderOfExtraction: pixelExtractionOrder.GetValueOrDefault(modeltoInfer.ImagePixelExtractionOrder)))
                              .Append(mlContext.Transforms.ApplyOnnxModel(modelFile: modeltoInfer.ModelPath, outputColumnName: "model_outputs0", inputColumnName: "data", gpuDeviceId: gpuDeviceId, fallbackToCpu: fallbackToCpu));

                    var model = pipeline.Fit(data);
                    predictionEngine = mlContext.Model.CreatePredictionEngine<ImageInput, ImagePredictions>(model);
                }
                else
                {

                    var pipeline = mlContext.Transforms.ResizeImages(resizing: ImageResizingEstimator.ResizingKind.Fill, outputColumnName: "data", imageWidth: ImageSettings.imageWidth, imageHeight: ImageSettings.imageHeight, inputColumnName: nameof(ImageInput.Image))
                              .Append(mlContext.Transforms.ExtractPixels(outputColumnName: "data"))
                              .Append(mlContext.Transforms.ApplyOnnxModel(modelFile: modeltoInfer.ModelPath, outputColumnName: "model_outputs0", inputColumnName: "data", gpuDeviceId: gpuDeviceId, fallbackToCpu: fallbackToCpu));

                    var model = pipeline.Fit(data);
                    predictionEngine = mlContext.Model.CreatePredictionEngine<ImageInput, ImagePredictions>(model);
                }

                labels = System.IO.File.ReadAllLines(modeltoInfer.ModelLabelPath);

                maskPredictor = new ONNXMaskPredictor();
#if DEBUG
            }

            LogHandler.LogUsage(String.Format("SingletonONNX method of FrameProcessor finished execution at : {0}", DateTime.UtcNow.ToLongTimeString()), null);
#endif
        }
    }

    public class ObjectDetectionOfflineInferenceAPI : ExecuteBase
    {
        SingletonONNX modelObject = null;

        
        public override bool InitializeModel(ModelParameters modeltoInfer /*string modelPath, string modelLabelPath*/)
        {
#if DEBUG
            using (LogHandler.TraceOperations("ObjectDetectionOfflineInferenceAPI:InitializeModel", LogHandler.Layer.MaskPrediction, Guid.NewGuid(), null))
            {
#endif
                
                SingletonONNX.modeltoInfer = modeltoInfer;
                modelObject = SingletonONNX.GetInstance;

                if (modelObject == null)
                    return false;
                else
                    return true;
#if DEBUG
            }
#endif
        }

        
        public override string MakePrediction(Stream st, ModelParameters modeltoInfer)
        {
            string sstime = DateTime.UtcNow.ToString("yyy-MM-dd,HH:mm:ss.fff tt");
            List<SE.Message.Mtp> MtpData = new List<SE.Message.Mtp>()
            {
                new SE.Message.Mtp(){ Etime = modeltoInfer.Etime, Src = modeltoInfer.Src, Stime=modeltoInfer.Stime},
                new SE.Message.Mtp(){ Etime = "", Src = "Frame Processor", Stime=sstime},
                };
#if DEBUG
            using (LogHandler.TraceOperations("ObjectDetectionOfflineInferenceAPI:MakePrediction", LogHandler.Layer.MaskPrediction, Guid.NewGuid(), null))
            {

                LogHandler.LogUsage(String.Format("ObjectDetectionOfflineInferenceAPI MakePrediction is getting executed at : {0}", DateTime.UtcNow.ToLongTimeString()), null);
#endif
                List<BoundingBox> metadata = modelObject.maskPredictor.MakePrediction(modelObject.predictionEngine, st, modelObject.labels, FrameGrabberHelper.overlapThreshold /*modeltoInfer.OverlapThreshold*/);

                
                foreach (BoundingBox data in metadata)
                {
                    data.TaskType = modeltoInfer.TaskType;
                }

                string etime = DateTime.UtcNow.ToString("yyy-MM-dd,HH:mm:ss.fff tt");
                for (int i = 0; i < MtpData.Count; i++)
                {
                    if (MtpData[i].Etime == "")
                    {
                        MtpData[i].Etime = etime;
                    }
                }
                
                ObjectDetectorAIResMsg objectDetectorAIResMsg = new ObjectDetectorAIResMsg()
                {
                    Did = modeltoInfer.deviceId,
                    Fid = modeltoInfer.Fid,
                    Tid = modeltoInfer.tId,
                    Ts = DateTime.UtcNow.ToString("yyy-MM-dd,HH:mm:ss.fff tt"),
                    Ts_ntp = DateTime.UtcNow.ToString("yyyyMMddHHmmss"),
                    Msg_ver = modeltoInfer.Msg_ver,
                    Inf_ver = modeltoInfer.Inf_ver,
                    Ad = modeltoInfer.Ad,
                    Fs = metadata,
                    Mtp = MtpData,
                    Ffp = modeltoInfer.Ffp,
                    Ltsize = modeltoInfer.Ltsize,
                    Lfp = modeltoInfer.Lfp,
                    Hp = modeltoInfer.Hp,
                };

                if (!string.IsNullOrEmpty(modeltoInfer.Prompt))
                {
                    LogHandler.LogDebug($"Formatting prompt: {modeltoInfer.Prompt} to list of list", LogHandler.Layer.Business);
                    objectDetectorAIResMsg.Prompt = JsonConvert.DeserializeObject<List<List<string>>>(modeltoInfer.Prompt);
                }
                else
                {
                    objectDetectorAIResMsg.Prompt = new List<List<string>>();
                    List<string> list = new List<string>();
                    objectDetectorAIResMsg.Prompt.Add(list);
                }

                string strmetadata = JsonConvert.SerializeObject(objectDetectorAIResMsg);

#if DEBUG
                LogHandler.LogUsage(String.Format("ObjectDetectionOfflineInferenceAPI MakePrediction finished execution at : {0}", DateTime.UtcNow.ToLongTimeString()), null);
#endif          
                return strmetadata;
#if DEBUG
            }
#endif
        }
    }
    public class ONNXMaskPredictor
    {
        public const int rowCount = 13, columnCount = 13;
        public const int featuresPerBox = 5;
        private static readonly (float x, float y)[] boxAnchors = { (0.573f, 0.677f), (1.87f, 2.06f), (3.34f, 5.47f), (7.88f, 3.53f), (9.77f, 9.17f) };

        
        public List<BoundingBox> MakePrediction(Microsoft.ML.PredictionEngine<ImageInput, ImagePredictions> predictionEngine, Stream st, string[] labels, float overlapThreshold)
        {
#if DEBUG
            using (LogHandler.TraceOperations("ONNXMaskPredictor:Predict", LogHandler.Layer.MaskPrediction, Guid.NewGuid(), null))
            {
#endif
                Bitmap predictionImage = (Bitmap)Image.FromStream(st);

                var predictionOutput = predictionEngine.Predict(new ImageInput { Image = predictionImage });

                var boundingBoxes = ParseOutputs(predictionOutput.PredictedLabels, labels);

                boundingBoxes = Helper.RemoveDuplicateRegions(boundingBoxes, overlapThreshold);
                
#if DEBUG
                
#endif
                predictionImage.Dispose();
                predictionImage = null;
                return boundingBoxes;
#if DEBUG
            }
#endif
        }

        
        public static List<BoundingBox> ParseOutputs(float[] modelOutput, string[] labels)
        {
#if DEBUG
            LogHandler.LogUsage(String.Format("ONNX ParseOutputs method is getting executed at : {0}", DateTime.UtcNow.ToLongTimeString()), null);
#endif
            float confidenceThreshold = FrameGrabberHelper.confidenceThreshold;
#if DEBUG
            using (LogHandler.TraceOperations("ONNXMaskPredictor:ParseOutputs", LogHandler.Layer.MaskPrediction, Guid.NewGuid(), null))
            {
#endif
                var boxes = new List<BoundingBox>();

                for (int row = 0; row < rowCount; row++)
                {
                    for (int column = 0; column < columnCount; column++)
                    {
                        for (int box = 0; box < boxAnchors.Length; box++)
                        {
                            var channel = box * (labels.Length + featuresPerBox);

                            var boundingBoxPrediction = ExtractBoundingBoxPrediction(modelOutput, row, column, channel);

                            var mappedBoundingBox = MapBoundingBoxToCell(row, column, box, boundingBoxPrediction);

                            if (boundingBoxPrediction.Confidence < confidenceThreshold)
                                continue;

                            float[] classProbabilities = ExtractClassProbabilities(modelOutput, row, column, channel, boundingBoxPrediction.Confidence, labels);

                            var (topProbability, topIndex) = classProbabilities.Select((probability, index) => (Score: probability, Index: index)).Max();

                            if (topProbability < confidenceThreshold)
                                continue;

                            boxes.Add(new BoundingBox
                            {
                                Dm = mappedBoundingBox,
                                Cs = topProbability,
                                Lb = labels[topIndex]
                            });
                        }
                    }
                }
#if DEBUG
                LogHandler.LogUsage(String.Format("ONNX ParseOutputs method finished execution at : {0}", DateTime.UtcNow.ToLongTimeString()), null);
#endif
                return boxes;
#if DEBUG
            }
#endif
        }

        
        private static BoundingBoxDimensions MapBoundingBoxToCell(int row, int column, int box, BoundingBoxPrediction boxDimensions)
        {
            
            const float cellWidth = ImageSettings.imageWidth / columnCount;
            const float cellHeight = ImageSettings.imageHeight / rowCount;

            var mappedBox = new BoundingBoxDimensions
            {
                X = (row + Sigmoid(boxDimensions.X)) * cellWidth,
                Y = (column + Sigmoid(boxDimensions.Y)) * cellHeight,
                W = MathF.Exp(boxDimensions.W) * cellWidth * boxAnchors[box].x,
                H = MathF.Exp(boxDimensions.H) * cellHeight * boxAnchors[box].y,

                
            };

            
            mappedBox.X -= mappedBox.W / 2;
            mappedBox.Y -= mappedBox.H / 2;

            mappedBox.X = mappedBox.X / ImageSettings.imageWidth;
            mappedBox.Y = mappedBox.Y / ImageSettings.imageHeight;
            mappedBox.W = mappedBox.W / ImageSettings.imageWidth;
            mappedBox.H = mappedBox.H / ImageSettings.imageHeight;

            return mappedBox;
            
        }

        private static BoundingBoxPrediction ExtractBoundingBoxPrediction(float[] modelOutput, int row, int column, int channel)
        {
            
            return new BoundingBoxPrediction
            {
                X = modelOutput[GetOffset(row, column, channel++)],
                Y = modelOutput[GetOffset(row, column, channel++)],
                W = modelOutput[GetOffset(row, column, channel++)],
                H = modelOutput[GetOffset(row, column, channel++)],
                Confidence = Sigmoid(modelOutput[GetOffset(row, column, channel++)])
            };
            
        }

        public static float[] ExtractClassProbabilities(float[] modelOutput, int row, int column, int channel, float confidence, string[] labels)
        {
            
            var classProbabilitiesOffset = channel + featuresPerBox;
            float[] classProbabilities = new float[labels.Length];
            for (int classProbability = 0; classProbability < labels.Length; classProbability++)
                classProbabilities[classProbability] = modelOutput[GetOffset(row, column, classProbability + classProbabilitiesOffset)];
            return Softmax(classProbabilities).Select(p => p * confidence).ToArray();
            
        }

        private static float Sigmoid(float value)
        {
            
            var k = MathF.Exp(value);
            return k / (1.0f + k);
            
        }

        private static float[] Softmax(float[] classProbabilities)
        {
            
            var max = classProbabilities.Max();
            var exp = classProbabilities.Select(v => MathF.Exp(v - max));
            var sum = exp.Sum();
            return exp.Select(v => v / sum).ToArray();
            
        }

        private static int GetOffset(int row, int column, int channel)
        {
            
            const int channelStride = rowCount * columnCount;
            return (channel * channelStride) + (column * columnCount) + row;
            
        }
    }

    public class BoundingBoxPrediction : BoundingBoxDimensions
    {
        public float Confidence { get; set; }
    }
}