/*=============================================================================================================== *
 * Copyright 2025 Infosys Ltd.                                                                                    *
 * Use of this source code is governed by Apache License Version 2.0 that can be found in the LICENSE file or at  *
 * http://www.apache.org/licenses/                                                                                *
 * ===============================================================================================================*/
﻿using OpenCvSharp;
using Infosys.Solutions.Ainauto.VideoAnalytics.BusinessEntity;
using Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.Common;
using Infosys.Solutions.Ainauto.VideoAnalytics.Resource.Entity.Queue;
using SE = Infosys.Solutions.Ainauto.VideoAnalytics.Services.MaskDetector.Contracts;
using Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.PythonLoader;
using Infosys.Solutions.Ainauto.VideoAnalytics.BusinessComponent;
using System.Runtime.Caching;

namespace Infosys.Solutions.Ainauto.VideoAnalytics.Renderer
{
    public class HeatMap : IRender
    {
        ObjectCache cache = MemoryCache.Default;
        List<Point2f> filteredPoints = new List<Point2f>();
        List<Point2f> filteredPointsapped = new List<Point2f>();
        Dictionary<(int, int), int> gridCounts = new Dictionary<(int, int), int>();
        static List<Rect> persistentHotzones = new List<Rect>();
        static BackgroundSubtractorMOG2 subtractor = BackgroundSubtractorMOG2.Create();
        // Background subtractor, initialize as needed
        //  private BackgroundSubtractor subtractor;

        // Dictionary to hold previous motion sums per ROI (persist across frames)
        static Dictionary<Rect, float> previousMotionLevels = new Dictionary<Rect, float>();

        // List of ROIs - set as needed or pass dynamically
        static List<Rect> rois;
        static Mat heatmap = new Mat();
  
        static int nextId = 0;
        const int STILLNESS_THRESHOLD = 1; // Number of frames standing still
        const int MOVEMENT_TOLERANCE = 5;   // Pixels of tolerated movement

        public Point CurrentPosition { get; set; }
        public Point LastPosition { get; set; }
        public int StillFrames { get; set; } = 0;
        public int Radius { get; set; } = 20;
        static int personIdCounter = 0;

        static int nextPersonId = 0;
 

   
        public const int INITIAL_RADIUS = 20;
        public const int MAX_RADIUS = 100;
        public const int RADIUS_GROWTH_RATE = 2;
        public const double MOVE_THRESHOLD = 10.0;
        public Mat RenderFrame(List<Predictions> objectList, Mat image, int frameWidth, int frameHeight, string modelName, string info, DeviceDetails deviceDetails, string Ad, FrameDetails frameDetails)
        {
            Scalar color = new Scalar();
            if (deviceDetails.CrowdCounting.Equals("yes", StringComparison.InvariantCultureIgnoreCase))
            {
                try
                {
                    Predictions face = objectList[0];
#if DEBUG
                    LogHandler.LogInfo(String.Format("v0:{0} v1:{1} v2:{2}", color.Val0,
                        color.Val1, color.Val2), LogHandler.Layer.Business, null);
#endif
                    var x = new int[face.Tpc.Count];
                    var y = new int[face.Tpc.Count];
                    for (var z = 0; z < face.Tpc.Count; z++)
                    {
                        var point1 = Convert.ToInt32(face.Tpc[z][0] * image.Width);
                        var point2 = Convert.ToInt32(face.Tpc[z][1] * image.Height);
                        x[z] = point1;
                        y[z] = point2;
                    }
                    string base64_image = "";

                    using (MemoryStream MyMemoryStream = new MemoryStream())
                    {
                        Cv2.ImEncode(".jpg", image, out byte[] jpgBytes);
                        MyMemoryStream.Write(jpgBytes, 0, jpgBytes.Length);

                        base64_image = Convert.ToBase64String(MyMemoryStream.ToArray());
                        MyMemoryStream.Dispose();
                    }
                    SE.Message.CrowdCounting reqMsg = new SE.Message.CrowdCounting()
                    {
                        x = x,
                        y = y,
                        Base_64 = base64_image
                    };
                    PythonNet pNet = new PythonNet();
                    pNet = PythonNet.GetInstance;
                    var val = "";

                    val = "";

                    string base64_return_image = "";
                    base64_return_image = val.ToString();
                    byte[] bytes = Convert.FromBase64String(base64_return_image);

                    System.IO.Stream imageStream = new MemoryStream();
                    using (MemoryStream ms = new MemoryStream())
                    {
                        imageStream.CopyTo(ms);
                        byte[] imageBytes = bytes.ToArray();




                        FrameGrabberHelper.CompressImage(imageBytes, ms, 100).CopyTo(ms);
                        FrameGrabberHelper.UploadToBlob(ms, frameDetails.FrameId + ApplicationConstants.FileExtensions.jpg);
                        ms.Dispose();
                    }

                }
                catch (Exception ex)
                {
                    LogHandler.LogError("Error in HeatMap.RenderFrame, exception: {0}, inner exception: {1}, stack trace: {2}",
                        LogHandler.Layer.FrameRenderer, ex.Message, ex.InnerException, ex.StackTrace);
                    throw ex;
                }
            }
            else if (deviceDetails.HeatMap.Equals("yes", StringComparison.InvariantCultureIgnoreCase)) //Heatmap level2
            {
                Mat heatMapnew = CacheHelper.GetMat("heatMap");
                if (heatMapnew == null)
                {
                    heatmap = new Mat(image.Height, image.Width, MatType.CV_32FC1);
                    heatmap.SetTo(new Scalar(0)); // Start with zero heat
                }
                else
                {
                    heatmap = heatMapnew;
                }



                Mat prevGray = CacheHelper.GetMat("prevGray");
                Mat frame = new Mat();

                Mat grayImage = image.Clone();

                // Image<Bgr, byte> image = image.ToImage<Bgr, byte>();
                Mat gray = new Mat();
                Cv2.CvtColor(image, gray, ColorConversionCodes.BGR2GRAY);

                // Detect moving people (or replace with object detection)
                List<Point> detections = prevGray != null ? DetectMotion(gray, prevGray) : new List<Point>();

                List<Point2f> coordinates1 = new List<Point2f>();
                List<Point> coordinates2 = new List<Point>();

                // Sample coordinates (replace with your actual data)
                foreach (Predictions predictions in objectList)
                {
                    //Console.WriteLine(predictions.Dm.X);
                    // Console.WriteLine(predictions.Dm.Y);
                    coordinates2.Add(new Point((int)Math.Round(double.Parse(predictions.Dm.X) * image.Width), (int)Math.Round(double.Parse(predictions.Dm.Y) * image.Height)));
                    // coordinates1.Add(new PointF((float.Parse(predictions.Dm.X) * image.Width), (float.Parse(predictions.Dm.X) * image.Height)));
                }
                var cacheItemPolicy = new CacheItemPolicy
                {
                    AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(60)
                };

                //cache.Add(new CacheItem("Coordinates", coordinates1), cacheItemPolicy);
                CacheHelper.StorePoints("Coordinates", coordinates2);

                List<Point> externalPoints = CacheHelper.GetPoints("Coordinates");

                // Combine both sources
                // List<Point> allPoints = new();
                List<Point> allPoints = new List<Point>();
                foreach (var normPt in detections)
                {
                    int x = (int)(normPt.X);
                    int y = (int)(normPt.Y);
                    allPoints.Add(new Point(x, y));
                }
                //foreach (var normPt in externalPoints)
                //{
                //    int x = (int)(normPt.X);
                //    int y = (int)(normPt.Y);
                //    allPoints.Add(new Point(x, y));
                //}

                //if (detections != null) allPoints.AddRange(detections);
                //if (coordinates2 != null) allPoints.AddRange(externalPoints);

                // If we have enough data, compute mean location
                if (allPoints.Count > 0)
                {
                    // Compute centroid (mean of all x and y)
                    int sumX = 0, sumY = 0;
                    foreach (var pt in allPoints)
                    {
                        sumX += pt.X;
                        sumY += pt.Y;
                    }

                    Point centroid = new Point(sumX / allPoints.Count, sumY / allPoints.Count);

                    float scaleX = heatmap.Width / (float)image.Width;
                    float scaleY = heatmap.Height / (float)image.Height;

                    int scaledX = (int)(centroid.X * scaleX);
                    int scaledY = (int)(centroid.Y * scaleY);

                    // Apply heat at mean position
                    using Mat temp = new Mat(heatmap.Size(), MatType.CV_32FC1);
                    temp.SetTo(new Scalar(0));

                    Cv2.Circle(temp, centroid, 30, new Scalar(5.0), -1); // Heat at mean
                    Cv2.GaussianBlur(temp, temp, new Size(61, 61), 0);
                    if (heatMapnew != null)
                    {
                        Cv2.Add(heatmap, temp, heatMapnew); // Accumulate
                    }
                    else
                    {
                        Cv2.Add(heatmap, temp, heatmap); // Accumulate
                    }
                }
                CacheHelper.StoreMat("heatMap", heatmap, TimeSpan.FromMinutes(30));
                Mat output = OverlayHeatmap(grayImage, heatmap);
                image = output;
                prevGray?.Dispose();
                prevGray = gray.Clone();
                CacheHelper.StoreMat("prevGray", prevGray, TimeSpan.FromMinutes(30));
            }  
            return image;
        }
        static Mat OverlayHeatmap(Mat frame, Mat heatmap)
        {
            using Mat norm = new Mat();
            using Mat colorMap = new Mat();
            using Mat overlay = new Mat();

            Cv2.Normalize(heatmap, norm, 0, 255, NormTypes.MinMax);
            norm.ConvertTo(norm, MatType.CV_8UC1);
            Cv2.ApplyColorMap(norm, colorMap, ColormapTypes.Jet);
            Cv2.AddWeighted(frame, 0.5, colorMap, 0.5, 0, overlay);

            return overlay.Clone();
        }
        static List<Point> DetectMotion(Mat current, Mat previous)
        {
            using Mat diff = new Mat();
            Cv2.Absdiff(current, previous, diff);
            Cv2.Threshold(diff, diff, 30, 255, ThresholdTypes.Binary);
            Cv2.GaussianBlur(diff, diff, new Size(5, 5), 0);

            Cv2.FindContours(diff, out OpenCvSharp.Point[][] contours, out _, RetrievalModes.External, ContourApproximationModes.ApproxSimple);

            List<Point> detections = new();
            for (int i = 0; i < contours.Length; i++)
            {
                if (Cv2.ContourArea(contours[i]) < 800) continue;
                Rect rect = Cv2.BoundingRect(contours[i]);
                Point center = new Point(rect.X + rect.Width / 2, rect.Y + rect.Height / 2);
                detections.Add(center);
            }

            return detections;
        }
        public static class CacheHelper
        {
            private static readonly MemoryCache _cache = MemoryCache.Default;

            public static void StorePoints(string key, List<Point> newpoint)
            {
                //var policy = new CacheItemPolicy
                //{
                //    AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(10)
                //};

                //_cache.Set(key, points, policy);
                if (newpoint == null || newpoint.Count == 0)
                    return;

                // Retrieve existing list from cache or create new
                var existingPoints = _cache.Get(key) as List<Point> ?? new List<Point>();

                // Append new points
                existingPoints.AddRange(newpoint);


                // Store it back with expiration policy
                var policy = new CacheItemPolicy
                {
                    AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(10)
                };

                _cache.Set(key, existingPoints, policy);
            }

            public static List<Point> GetPoints(string key)
            {
                return _cache.Get(key) as List<Point> ?? new List<Point>();
            }
            public static Mat GetPointsMat(string key)
            {
                return _cache.Get(key) as Mat ?? new Mat();
            }
            public static void StoreMat(string key, Mat mat, TimeSpan? duration = null)
            {
                var policy = new CacheItemPolicy
                {
                    AbsoluteExpiration = DateTimeOffset.Now.Add(duration ?? TimeSpan.FromMinutes(10))
                };
                _cache.Set(key, mat, policy);
                //   Console.WriteLine($"[CACHE] Stored '{key}'");
            }

            public static Mat GetMat(string key)
            {
                if (_cache.Contains(key))
                {
                    //   Console.WriteLine($"[CACHE] Retrieved '{key}'");
                    return _cache.Get(key) as Mat;
                }

                //  Console.WriteLine($"[CACHE] '{key}' not found");
                return null;
            }
        }
    }
}
