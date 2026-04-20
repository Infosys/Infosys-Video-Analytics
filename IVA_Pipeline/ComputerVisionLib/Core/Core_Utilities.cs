/*=============================================================================================================== *
 * Copyright 2025 Infosys Ltd.                                                                                    *
 * Use of this source code is governed by Apache License Version 2.0 that can be found in the LICENSE file or at  *
 * http://www.apache.org/licenses/                                                                                *
 * ===============================================================================================================*/
﻿using OpenCvSharp;
using Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.ComputerVisionLib.Core.Utilities;


namespace Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.ComputerVisionLib.Core
{
    public class Core_Utilities
    {
        public double ScaleStep { get; set; }
        public int MaxScaleSteps { get; set; }
        public ImageBgr TemplateMatchMapBorderColor { get; set; }
        public int TemplateMatchMapBorderThickness { get; set; }

        public byte[] TemplateMatchMapScreen { get { return templateMatchMapScreen; } }

        private byte[] templateMatchMapScreen;

        const int DEFAULT_TIMEOUT_PERINSTANCE = 2;
        const int THREAD_SLEEP_DURATION = 100; 

        public  List<TemplateMatching> FindAllInstances(string filename, int timeout = DEFAULT_TIMEOUT_PERINSTANCE, double confidence = 80, bool multipleScaleMatching = true, Stream sourceImageToMatch = null, bool enableTemplateMatchMap = false)
        {
           
            List<TemplateMatching> rects = new List<TemplateMatching>();
            List<Point[]> boxes = new List<Point[]>();
            int currentCount = 0;
            bool forever = false;

            TemplateMatching[] elementRectTemp;
            DateTime startTime = DateTime.Now;
            Mat sourceMatches = null;
            try
            {
                if (timeout == 0)
                    timeout = DEFAULT_TIMEOUT_PERINSTANCE; 
                if (timeout < 0)
                    forever = true;
                if (System.IO.File.Exists(filename))
                {
                    Mat template = Cv2.ImRead(filename, ImreadModes.Grayscale);                    
                    bool backgroundProcessing = false;
                    
                    while ((forever && currentCount == 0) || (System.DateTime.Now - startTime).TotalMilliseconds <= timeout * 1000 ||
                        (rects.Count - currentCount > 0))
                    {
                        currentCount = rects.Count;
                        if (currentCount > 0)
                            forever = false;
                        Mat source = null;
                        if (sourceImageToMatch != null)
                        {                           
                            sourceImageToMatch.Position = 0;
                            using (var ms = new MemoryStream()) { sourceImageToMatch.CopyTo(ms); source = Cv2.ImDecode(ms.ToArray(), ImreadModes.Grayscale); }

                            backgroundProcessing = true;
                            if (enableTemplateMatchMap)
                            {
                                if (templateMatchMapScreen != null)
                                {
                                    sourceMatches = Cv2.ImDecode(templateMatchMapScreen, ImreadModes.Color);
                                }
                                else
                                {
                                    sourceImageToMatch.Position = 0;
                                    using (var ms2 = new MemoryStream()) { sourceImageToMatch.CopyTo(ms2); sourceMatches = Cv2.ImDecode(ms2.ToArray(), ImreadModes.Color); }

                                }
                            }
                        }
                        
                        if (boxes != null && boxes.Count > 0)
                        {
                            boxes.ForEach(b =>
                            {
                                Cv2.FillConvexPoly(source, b, new Scalar(0));
                            });
                            boxes.ForEach(b =>
                            {

                                Cv2.Polylines(sourceMatches, new[]{b}, true,
                                    new Scalar(TemplateMatchMapBorderColor.Blue, TemplateMatchMapBorderColor.Green, TemplateMatchMapBorderColor.Red),
                                    TemplateMatchMapBorderThickness);

                            });

                        }
                        if (multipleScaleMatching)
                        {
                            
                            int direction = 1;                            
                            int countBoxes = 0;
                            for (int i = 0; i <= MaxScaleSteps; i++)
                            {
                                
                                double scale = 1 + direction * i * ScaleStep;
                                Mat templateTemp = ResizeTemplate(template, scale);

                                if (templateTemp != null)
                                {
                                    elementRectTemp = FindRectangles(source, templateTemp, confidence);
                                    if (elementRectTemp != null && elementRectTemp.Count() > 0)
                                    {
                                        
                                        for (int icount = 0; icount < elementRectTemp.Count(); icount++)
                                        {
                                            if (elementRectTemp[icount]?.BoundingBox.Height > 0 && elementRectTemp[icount]?.BoundingBox.Width > 0)
                                            {
                                                rects.Add(elementRectTemp[icount]);
                                                var boundingBox = RectToBox(elementRectTemp[icount].BoundingBox);
                                                boxes.Add(boundingBox);
                                                countBoxes++;
                                            }
                                        }

                                        if (countBoxes > 0)
                                            break;
                                    }
                                }

                                
                                scale = 1 + (-direction) * i * ScaleStep;
                                templateTemp = ResizeTemplate(template, scale);

                                if (templateTemp != null)
                                {
                                    elementRectTemp = FindRectangles(source, templateTemp, confidence);
                                    if (elementRectTemp != null && elementRectTemp.Count() > 0)
                                    {
                                        
                                        for (int icount = 0; icount < elementRectTemp.Count(); icount++)
                                        {
                                            if (elementRectTemp[icount]?.BoundingBox.Height > 0 && elementRectTemp[icount]?.BoundingBox.Width > 0)
                                            {
                                                rects.Add(elementRectTemp[icount]);
                                                var boundingBox = RectToBox(elementRectTemp[icount].BoundingBox);
                                                boxes.Add(boundingBox);
                                                countBoxes++;
                                            }
                                        }
                                       
                                        if (countBoxes > 0)
                                            break; 
                                    }
                                }
                            }

                            if (countBoxes == 0)  
                            {
                                break;
                            }
                        }
                        else
                        {
                            bool noMatchAboveConfidenceScore = false;
                            elementRectTemp = FindRectangles(source, template, confidence);
                            if (elementRectTemp.Count() > 0)
                            {
                                int countBoxes = 0;
                                for (int icount = 0; icount < elementRectTemp.Count(); icount++)
                                {
                                    if (elementRectTemp[icount]?.BoundingBox.Height > 0 && elementRectTemp[icount]?.BoundingBox.Width > 0)
                                    {
                                        rects.Add(elementRectTemp[icount]);
                                        var boundingBox = RectToBox(elementRectTemp[icount].BoundingBox);
                                        boxes.Add(boundingBox);
                                        countBoxes++;
                                    }
                                }

                                if (countBoxes == 0)
                                {
                                    noMatchAboveConfidenceScore = true;
                                }

                                if (countBoxes > 0 || noMatchAboveConfidenceScore)
                                    break; 
                            }
                        }
                       
                        System.Threading.Thread.Sleep(THREAD_SLEEP_DURATION);
                    }
                }
                if (rects.Count == 0)
                    throw new Exception("Could not match the template provided in this trial. Probably in the subsequent trial, the template would be matched.");
            }
            catch (Exception ex)
            {
               
                string exMessage = ex.Message;
                string innerExMessage = ex.InnerException != null ? ex.InnerException.Message : "";
                LogHandler.LogError(string.Format(Logging.ErrorMessages.EXCEPTION, "FindElement", exMessage, innerExMessage), LogHandler.Layer.Business);
            }
            if (enableTemplateMatchMap)
            {
                if (sourceMatches != null)
                {
                    Cv2.ImEncode(".jpg", sourceMatches, out byte[] encoded);
                    templateMatchMapScreen = encoded;
                }
            }
            return rects;
        }

        private Mat ResizeTemplate(Mat template, double scale)
        {
            Mat resizedTemplate = null;
            try
            {
                if (scale == 0)
                    
                    return null;
                else if (scale < 0)
                {
                    scale = 1 / (scale);  
                                         
                }
                if (scale < 0.5) 
                    return null;

                if (template.Width * template.Height < 250)
                    return template;

                resizedTemplate = new Mat();
                Cv2.Resize(template, resizedTemplate, new OpenCvSharp.Size(0, 0), scale, scale, InterpolationFlags.Lanczos4);

                if (resizedTemplate.Width * resizedTemplate.Height < 250)
                    return null;
            }
            catch
            {
                
            }
            return resizedTemplate;
        }

        private TemplateMatching[] FindRectangles(Mat source, Mat template, double confidence)
        {
            TemplateMatching[] objTemplates = null;
            try
            {
                confidence = confidence / 100; 
                
                using (Mat result = new Mat())
                {
                    Cv2.MatchTemplate(source, template, result, TemplateMatchModes.CCoeffNormed);
                    Cv2.MinMaxLoc(result, out double minValue, out double maxValue, out Point minLocation, out Point maxLocation);
                    double[] minValues = new[] { minValue };
                    double[] maxValues = new[] { maxValue };
                    Point[] minLocations = new[] { minLocation };
                    Point[] maxLocations = new[] { maxLocation };
                    objTemplates = new TemplateMatching[maxValues.Count()];
                    for (int iCount = 0; iCount < maxValues.Count(); iCount++)
                    {

                        if ((maxValues[iCount] <= 1.0) && (maxValues[iCount] >= confidence))
                        {                              
                            Rect rect = new Rect(maxLocations[0].X, maxLocations[0].Y, template.Width, template.Height);
                            TemplateMatching objTemplate = new TemplateMatching();
                            objTemplate.BoundingBox = rect;
                            objTemplate.ConfidenceScore = maxValues[iCount];
                            objTemplates.SetValue(objTemplate, iCount);
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                
                Exception ex1 = ex;
            }
            return objTemplates;
        }

        public List<TemplateMatching> FindAllInstancesInTrueColor(string filename, int timeout = DEFAULT_TIMEOUT_PERINSTANCE, double confidence = 80, bool multipleScaleMatching = true, Stream sourceImageToMatch = null, bool enableTemplateMatchMap = false)
        {
           
            List<TemplateMatching> rects = new List<TemplateMatching>();
            List<Point[]> boxes = new List<Point[]>();
            int currentCount = 0;
            bool forever = false;

            
            TemplateMatching[] elementRectTemp;

            Mat sourceMatches = null;
            DateTime startTime = DateTime.Now;
            try
            {

                if (timeout <= 0)
                    timeout = DEFAULT_TIMEOUT_PERINSTANCE; 
                if (timeout < 0)
                    forever = true;
                if (System.IO.File.Exists(filename))
                {
                    Mat template = Cv2.ImRead(filename, ImreadModes.Color);
                    bool backgroundProcessing = false;

                    
                    while ((forever && currentCount == 0)
                        || (System.DateTime.Now - startTime).TotalMilliseconds <= timeout * 1000
                        || (rects.Count - currentCount > 0))
                    {
                        currentCount = rects.Count;
                        if (currentCount > 0)
                            forever = false;

                        Mat source = null;
                        if (sourceImageToMatch != null)
                        {
                            sourceImageToMatch.Position = 0;
                            using (var ms = new MemoryStream()) { sourceImageToMatch.CopyTo(ms); source = Cv2.ImDecode(ms.ToArray(), ImreadModes.Color); }
                            backgroundProcessing = true;
                        }
                        
                        if (enableTemplateMatchMap)
                        {
                            if (templateMatchMapScreen != null)
                            {                               
                                sourceMatches = Cv2.ImDecode(templateMatchMapScreen, ImreadModes.Color);
                            }
                            else
                            {                               
                                sourceMatches = source.Clone();
                            }

                        }
                        
                        if (boxes != null && boxes.Count > 0)
                        {
                            boxes.ForEach(b =>
                            {
                                Cv2.FillConvexPoly(source, b, new Scalar(0, 0, 0));
                            });
                            boxes.ForEach(b =>
                            {
                                Cv2.Polylines(sourceMatches, new[]{b}, true,
                                    new Scalar(TemplateMatchMapBorderColor.Blue, TemplateMatchMapBorderColor.Green, TemplateMatchMapBorderColor.Red),
                                    TemplateMatchMapBorderThickness);

                            });
                        }
                        if (multipleScaleMatching)
                        {
                            
                            int direction = 1;
                           
                            int countBoxes = 0;
                            for (int i = 0; i <= MaxScaleSteps; i++)
                            {
                                
                                double scale = 1 + direction * i * ScaleStep;
                                Mat templateTemp = ResizeTemplateInTrueColor(template, scale);
                                if (templateTemp != null)
                                {
                                   
                                    elementRectTemp = FindRectanglesInTrueColor(source, templateTemp, confidence);

                                    if (elementRectTemp?.Count() > 0)
                                    {
                                       
                                        for (int icount = 0; icount < elementRectTemp.Count(); icount++)
                                        {
                                            if (elementRectTemp[icount]?.BoundingBox.Height > 0 && elementRectTemp[icount]?.BoundingBox.Width > 0)
                                            {
                                                rects.Add(elementRectTemp[icount]);
                                                var boundingBox = RectToBox(elementRectTemp[icount].BoundingBox);
                                                boxes.Add(boundingBox);
                                                countBoxes++;
                                            }
                                        }                                        

                                        if (countBoxes > 0)
                                            break; 
                                    }
                                }

                                
                                scale = 1 + (-direction) * i * ScaleStep;
                                templateTemp = ResizeTemplateInTrueColor(template, scale);
                                if (templateTemp != null)
                                {
                                    elementRectTemp = FindRectanglesInTrueColor(source, templateTemp, confidence);
                                    if (elementRectTemp?.Count() > 0)
                                    {
                                        
                                        for (int icount = 0; icount < elementRectTemp.Count(); icount++)
                                        {
                                            if (elementRectTemp[icount]?.BoundingBox.Height > 0 && elementRectTemp[icount]?.BoundingBox.Width > 0)
                                            {
                                                rects.Add(elementRectTemp[icount]);                                                
                                                var boundingBox = RectToBox(elementRectTemp[icount].BoundingBox);
                                                boxes.Add(boundingBox);
                                                countBoxes++;
                                            }
                                        }
                                        if (countBoxes > 0)
                                            break; 
                                    }
                                }
                            }

                            if (countBoxes == 0)  
                            {

                                break;
                            }

                        }
                        else
                        {
                            bool noMatchAboveConfidenceScore = false;
                            elementRectTemp = FindRectanglesInTrueColor(source, template, confidence);

                            if (elementRectTemp?.Count() > 0)
                            {
                                int countBoxes = 0;
                                for (int icount = 0; icount < elementRectTemp.Count(); icount++)
                                {
                                    if (elementRectTemp[icount]?.BoundingBox.Height > 0 && elementRectTemp[icount]?.BoundingBox.Width > 0)
                                    {
                                        rects.Add(elementRectTemp[icount]);                                        
                                        var boundingBox = RectToBox(elementRectTemp[icount].BoundingBox);
                                        boxes.Add(boundingBox);
                                        countBoxes++;
                                    }


                                }
                                
                                if (countBoxes == 0)
                                {
                                    noMatchAboveConfidenceScore = true;
                                }

                                if (countBoxes > 0 || noMatchAboveConfidenceScore)
                                    break;
                            }
                        }
                        
                        System.Threading.Thread.Sleep(THREAD_SLEEP_DURATION);
                    }
                }
                if (rects.Count == 0)
                    throw new Exception("Could not match the template provided in this trial. Probably in the subsequent trial, the template would be matched.");
            }
            catch (Exception ex)
            {
                
                string exMessage = ex.Message;
                string innerExMessage = ex.InnerException != null ? ex.InnerException.Message : "";
                LogHandler.LogError(string.Format(Logging.ErrorMessages.EXCEPTION, "FindElement", exMessage, innerExMessage), LogHandler.Layer.Business);
            }
            if (enableTemplateMatchMap)
            {
                if (sourceMatches != null)
                {
                    Cv2.ImEncode(".jpg", sourceMatches, out byte[] encoded);
                    templateMatchMapScreen = encoded;
                }
            }
            return rects;
        }      

        private Point[] RectToBox(Rect rectangle)
        {
            Point[] box = null;
            if (rectangle.Width > 0 || rectangle.Height > 0)
            {
                Point topLeft = new Point(rectangle.X, rectangle.Y);
                Point bottomRight = new Point(topLeft.X + rectangle.Width, topLeft.Y + rectangle.Height);
                Point topRight = new Point(bottomRight.X, topLeft.Y);
                Point bottomLeft = new Point(topLeft.X, bottomRight.Y);
                box = new Point[] { topRight, topLeft, bottomLeft, bottomRight };
            }
            return box;
        }

        private Mat ResizeTemplateInTrueColor(Mat template, double scale)
        {
            Mat resizedTemplate = null;
            try
            {
                if (scale == 0)
                    return null;
                else if (scale < 0)
                {
                    scale = 1 / (scale); 
                                         
                }
                if (scale < 0.5) 
                    return null;
                resizedTemplate = new Mat();
                Cv2.Resize(template, resizedTemplate, new OpenCvSharp.Size(0, 0), scale, scale, InterpolationFlags.Lanczos4);
            }
            catch
            {
                
            }
            return resizedTemplate;
        }

        private TemplateMatching[] FindRectanglesInTrueColor(Mat source, Mat template, double confidence)
        {

            

            TemplateMatching[] objTemplates = null;

            try
            {
                confidence = confidence / 100;
                using (Mat result = new Mat())
                {
                    Cv2.MatchTemplate(source, template, result, TemplateMatchModes.CCoeffNormed);
                    Cv2.MinMaxLoc(result, out double minValue, out double maxValue, out Point minLocation, out Point maxLocation);
                    double[] minValues = new[] { minValue };
                    double[] maxValues = new[] { maxValue };
                    Point[] minLocations = new[] { minLocation };
                    Point[] maxLocations = new[] { maxLocation };
                    
                    objTemplates = new TemplateMatching[maxValues.Count()];
                    
                    for (int iCount = 0; iCount < maxValues.Count(); iCount++)
                    {

                        if ((maxValues[iCount] <= 1.0) && (maxValues[iCount] >= confidence))
                        {   
                            

                            Rect rect = new Rect(maxLocations[iCount].X, maxLocations[iCount].Y, template.Width, template.Height);
                            TemplateMatching objTemplate = new TemplateMatching();
                            objTemplate.BoundingBox = rect;
                            objTemplate.ConfidenceScore = maxValues[iCount];

                            objTemplates.SetValue(objTemplate, iCount);
                        }

                    }

                }
            }
            catch (Exception ex)
            {
                
                Exception ex1 = ex;
            }
            
            return objTemplates;
        }
    }
}
