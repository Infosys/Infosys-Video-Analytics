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
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.Common;
using Infosys.Solutions.Ainauto.VideoAnalytics.Services.MaskDetector.Contracts.Message;
using Newtonsoft.Json;

namespace Infosys.Solutions.Ainauto.VideoAnalytics.AIModels
{
    public class Helper
    {

        public static List<BoundingBox> RemoveDuplicateRegions(List<BoundingBox> boundingBoxes, float overlapThreshold)
        {




            List<BoundingBox> boundingBoxesA = new List<BoundingBox>();

            for (int i = 0; i < boundingBoxes.Count; i++)
            {
                boundingBoxesA.Add(boundingBoxes[i]);
            }

        START:

            for (int i = 0; i < boundingBoxesA.Count; i++)
            {
                for (int j = 0; j < boundingBoxesA.Count; j++)
                {
                    if (boundingBoxesA[i].Cs != boundingBoxesA[j].Cs)
                    {

                        RectangleF rectangleA = new RectangleF((boundingBoxesA[i]).Dm.X, (boundingBoxesA[i]).Dm.Y, (boundingBoxesA[i]).Dm.H, (boundingBoxesA[i]).Dm.W);
                        RectangleF rectangleB = new RectangleF((boundingBoxesA[j]).Dm.X, (boundingBoxesA[j]).Dm.Y, (boundingBoxesA[j]).Dm.H, (boundingBoxesA[j]).Dm.W);

                        if (rectangleA.IntersectsWith(rectangleB))
                        {
                            var overlapp = IntersectionOverUnion(rectangleA, rectangleB);

                            if (overlapp > overlapThreshold)
                            {
                                if (boundingBoxesA[i].Cs > boundingBoxesA[j].Cs)
                                {
                                    boundingBoxesA.RemoveAt(j);
                                }
                                else
                                {
                                    boundingBoxesA.RemoveAt(i);
                                }
                                goto START;
                            }
                        }
                    }
                }
            }



            return boundingBoxesA;



        }


        public static ObjectDetectorAPIResMsg RemoveDuplicateRegionsAPI(ObjectDetectorAPIResMsg ObjectDetectResMsg, float overlapThreshold)
        {

#if DEBUG
            LogHandler.LogUsage(String.Format("API RemoveDuplicateRegions is getting executed at : {0}", DateTime.UtcNow.ToLongTimeString()), null);

            using (LogHandler.TraceOperations("Helper:RemoveDuplicateRegionsAPI", LogHandler.Layer.MaskPrediction, Guid.NewGuid(), null))
            {
#endif

                if (ObjectDetectResMsg.Fs.Count != 0 || ObjectDetectResMsg.Fs != null)
                {
                    foreach (var fs in ObjectDetectResMsg.Fs)
                    {
                        if (string.IsNullOrEmpty(fs.Lb))
                        {
                            fs.Lb = "";
                        }
                    }
                    List<PersonDetails> boundingBoxes = ObjectDetectResMsg.Fs;
                    List<PersonDetails> boundingBoxesA = new List<PersonDetails>();


                    for (int i = 0; i < boundingBoxes.Count; i++)
                    {
                        boundingBoxesA.Add(boundingBoxes[i]);
                    }

                START:


                    for (int i = 0; i < boundingBoxesA.Count; i++)
                    {
                        for (int j = 0; j < boundingBoxesA.Count; j++)
                        {
                            if (boundingBoxesA[i].Cs != boundingBoxesA[j].Cs)
                            {

                                RectangleF rectangleA = new RectangleF(float.Parse(boundingBoxesA[i].Dm.X), float.Parse(boundingBoxesA[i].Dm.Y), float.Parse(boundingBoxesA[i].Dm.H), float.Parse(boundingBoxesA[i].Dm.W));
                                RectangleF rectangleB = new RectangleF(float.Parse(boundingBoxesA[j].Dm.X), float.Parse(boundingBoxesA[j].Dm.Y), float.Parse(boundingBoxesA[j].Dm.H), float.Parse(boundingBoxesA[j].Dm.W));

                                if (rectangleA.IntersectsWith(rectangleB))
                                {
                                    var overlapp = IntersectionOverUnion(rectangleA, rectangleB);

                                    if (overlapp > overlapThreshold)
                                    {
                                        if (float.Parse(boundingBoxesA[i].Cs) > float.Parse(boundingBoxesA[j].Cs))
                                        {
                                            boundingBoxesA.RemoveAt(j);
                                        }
                                        else
                                        {
                                            boundingBoxesA.RemoveAt(i);
                                        }
                                        goto START;
                                    }
                                }
                            }
                        }

                    }
                    ObjectDetectResMsg.Fs = boundingBoxesA;
                }

#if DEBUG
                LogHandler.LogUsage(String.Format("API RemoveDuplicateRegions finished execution at : {0}", DateTime.UtcNow.ToLongTimeString()), null);
#endif
                return ObjectDetectResMsg;
#if DEBUG
            }
#endif
        }


        public static float IntersectionOverUnion(RectangleF rectangleA, RectangleF rectangleB)
        {



            var areaA = rectangleA.Width * rectangleA.Height;

            if (areaA <= 0)
                return 0;

            var areaB = rectangleB.Width * rectangleB.Height;

            if (areaB <= 0)
                return 0;

            var minX = Math.Max(rectangleA.Left, rectangleB.Left);
            var minY = Math.Max(rectangleA.Top, rectangleB.Top);
            var maxX = Math.Min(rectangleA.Left + rectangleA.Width, rectangleB.Left + rectangleB.Width);
            var maxY = Math.Min(rectangleA.Top + rectangleA.Height, rectangleB.Top + rectangleB.Height);

            var intersectionArea = Math.Max(maxY - minY, 0) * Math.Max(maxX - minX, 0);

            var iou = intersectionArea / (areaA + areaB - intersectionArea);



            return iou;

        }

        public static dynamic FaceMaskApi(dynamic reqMsg, string URL, string methodType)
        {
            using (var client = new WebClient())
            {
                client.Headers.Add("Content-Type:application/json");
                client.Headers.Add("Accept:application/json");
                string result;
                if (methodType.ToUpper() != "GET")
                    result = client.UploadString(URL, methodType, JsonConvert.SerializeObject(reqMsg));
                else
                    result = client.DownloadString(URL);

                var metadata = System.Text.Json.JsonSerializer.Deserialize<dynamic>(result);


                return metadata;
            }
        }

        public static dynamic GetMaskPrediction_API(dynamic reqMsg, string URL)
        {
            string predictionstring = "";


            var jsonString = Newtonsoft.Json.JsonConvert.SerializeObject(reqMsg);




            WebRequest request = WebRequest.Create(URL);

            request.Method = "POST";

        
            string postData = jsonString;
            byte[] byteArray = Encoding.UTF8.GetBytes(postData);


            request.ContentType = "application/json";

            request.ContentLength = byteArray.Length; 

            
            Stream dataStream = request.GetRequestStream();

            dataStream.Write(byteArray, 0, byteArray.Length);

            dataStream.Close();


            WebResponse response = request.GetResponse();


            using (dataStream = response.GetResponseStream())
            {

                StreamReader reader = new StreamReader(dataStream);

                predictionstring = reader.ReadToEnd();

            }


            response.Close();
            var metadata = System.Text.Json.JsonSerializer.Deserialize<dynamic>(predictionstring);

            return metadata;
        }

        public async void MaskPrediction_API(dynamic reqMsg, string URL)
        {
            Uri requestUri = new Uri(URL);
            string json = "";
            json = Newtonsoft.Json.JsonConvert.SerializeObject(reqMsg);
            var objClient = new HttpClient();
            System.Net.Http.HttpResponseMessage respon = await objClient.PostAsync(requestUri, new StringContent(json, System.Text.Encoding.UTF8, "application/json"));
            string responJsonText = await respon.Content.ReadAsStringAsync();
        }
    


    }
}

