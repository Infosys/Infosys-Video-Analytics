/*=============================================================================================================== *
 * Copyright 2024 Infosys Ltd.                                                                                    *
 * Use of this source code is governed by Apache License Version 2.0 that can be found in the LICENSE file or at  *
 * http://www.apache.org/licenses/                                                                                *
 * ===============================================================================================================*/

﻿using Emgu.CV.Structure;
using Emgu.CV;
using Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.Common;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.ComputerVisionLib.Core.Utilities;
using Emgu.CV.CvEnum;


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
            Image<Bgr, byte> sourceMatches = null;
            try
            {
                if (timeout == 0)
                    timeout = DEFAULT_TIMEOUT_PERINSTANCE; 
                if (timeout < 0)
                    forever = true;
                if (System.IO.File.Exists(filename))
                {
                    Image<Gray, byte> template = new Image<Gray, byte>(filename);                    
                    bool backgroundProcessing = false;
                   
                    while ((forever && currentCount == 0) || (System.DateTime.Now - startTime).TotalMilliseconds <= timeout * 1000 ||
                        (rects.Count - currentCount > 0))
                    {
                        currentCount = rects.Count;
                        if (currentCount > 0)
                            forever = false;
                        Image<Gray, byte> source = null;
                        if (sourceImageToMatch != null)
                        {                           
                            var bmp = new Bitmap(sourceImageToMatch);
                            source = bmp.ToImage<Gray, byte>();

                            backgroundProcessing = true;
                            if (enableTemplateMatchMap)
                            {
                                if (templateMatchMapScreen != null)
                                {
                                    var btmp = new Bitmap(new MemoryStream(templateMatchMapScreen));
                                    sourceMatches = btmp.ToImage<Bgr, byte>();
                                }
                                else
                                {
                                    var btmp = new Bitmap(sourceImageToMatch);
                                    sourceMatches = btmp.ToImage<Bgr, byte>();

                                }
                            }
                        }
                        
                        if (boxes != null && boxes.Count > 0)
                        {
                            boxes.ForEach(b =>
                            {
                                source.FillConvexPoly(b, new Gray(0));
                            });
                            boxes.ForEach(b =>
                            {

                                sourceMatches.DrawPolyline(b, true,
                                    new Bgr(TemplateMatchMapBorderColor.Blue, TemplateMatchMapBorderColor.Green, TemplateMatchMapBorderColor.Red),
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
                                Image<Gray, byte> templateTemp = ResizeTemplate(template, scale);

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
                    templateMatchMapScreen = sourceMatches.ToJpegData();
            }
            return rects;
        }

        private Image<Gray, byte> ResizeTemplate(Image<Gray, byte> template, double scale)
        {
            Image<Gray, byte> resizedTemplate = null;
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

                resizedTemplate = template.Resize(scale, Inter.Lanczos4);

                if (resizedTemplate.Width * resizedTemplate.Height < 250)
                    return null;
            }
            catch
            {
               
            }
            return resizedTemplate;
        }

        private TemplateMatching[] FindRectangles(Image<Gray, byte> source, Image<Gray, byte> template, double confidence)
        {
            TemplateMatching[] objTemplates = null;
            try
            {
                confidence = confidence / 100; 
                
                using (Image<Gray, float> result = source.MatchTemplate(template, TemplateMatchingType.CcoeffNormed))
                {
                    double[] minValues, maxValues;
                    Point[] minLocations, maxLocations;
                    result.MinMax(out minValues, out maxValues, out minLocations, out maxLocations);
                    objTemplates = new TemplateMatching[maxValues.Count()];
                    for (int iCount = 0; iCount < maxValues.Count(); iCount++)
                    {

                        if ((maxValues[iCount] <= 1.0) && (maxValues[iCount] >= confidence))
                        {                              
                            Rectangle rect = new Rectangle(maxLocations[0].X, maxLocations[0].Y, template.Size.Width, template.Size.Height);
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

            Image<Bgr, byte> sourceMatches = null;
            DateTime startTime = DateTime.Now;
            try
            {

                if (timeout <= 0)
                    timeout = DEFAULT_TIMEOUT_PERINSTANCE; 
                if (timeout < 0)
                    forever = true;
                if (System.IO.File.Exists(filename))
                {
                    Image<Bgr, byte> template = new Image<Bgr, byte>(filename);
                    bool backgroundProcessing = false;

                    
                    while ((forever && currentCount == 0)
                        || (System.DateTime.Now - startTime).TotalMilliseconds <= timeout * 1000
                        || (rects.Count - currentCount > 0))
                    {
                        currentCount = rects.Count;
                        if (currentCount > 0)
                            forever = false;

                        Image<Bgr, byte> source = null;
                        if (sourceImageToMatch != null)
                        {
                            var bmp = new Bitmap(sourceImageToMatch);
                            source = bmp.ToImage<Bgr, byte>();
                            backgroundProcessing = true;
                        }
                        
                        if (enableTemplateMatchMap)
                        {
                            if (templateMatchMapScreen != null)
                            {                               
                                var btmp = new Bitmap(new MemoryStream(templateMatchMapScreen));
                                sourceMatches = btmp.ToImage<Bgr, byte>();
                            }
                            else
                            {                               
                                var btmp = source.ToBitmap();
                                sourceMatches = btmp.ToImage<Bgr, byte>();
                            }

                        }
                        
                        if (boxes != null && boxes.Count > 0)
                        {
                            boxes.ForEach(b =>
                            {
                                source.FillConvexPoly(b, new Bgr(Color.Black));
                            });
                            boxes.ForEach(b =>
                            {
                                sourceMatches.DrawPolyline(b, true,
                                    new Bgr(TemplateMatchMapBorderColor.Blue, TemplateMatchMapBorderColor.Green, TemplateMatchMapBorderColor.Red),
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
                                Image<Bgr, byte> templateTemp = ResizeTemplateInTrueColor(template, scale);
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
                    templateMatchMapScreen = sourceMatches.ToJpegData();
            }
            return rects;
        }      

        private Point[] RectToBox(Rectangle rectangle)
        {
            Point[] box = null;
            if (rectangle != Rectangle.Empty)
            {
                Point topLeft = new Point((int)rectangle.X, (int)rectangle.Y);
                Point bottomRight = new Point(topLeft.X + (int)rectangle.Width, topLeft.Y + (int)rectangle.Height);
                Point topRight = new Point(bottomRight.X, topLeft.Y);
                Point bottomLeft = new Point(topLeft.X, bottomRight.Y);
                box = new Point[] { topRight, topLeft, bottomLeft, bottomRight };
            }
            return box;
        }

        private Image<Bgr, byte> ResizeTemplateInTrueColor(Image<Bgr, byte> template, double scale)
        {
            Image<Bgr, byte> resizedTemplate = null;
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
                resizedTemplate = template.Resize(scale, Inter.Lanczos4);
            }
            catch
            {
            }
            return resizedTemplate;
        }

        private TemplateMatching[] FindRectanglesInTrueColor(Image<Bgr, byte> source, Image<Bgr, byte> template, double confidence)
        {


            TemplateMatching[] objTemplates = null;

            try
            {
                confidence = confidence / 100; 
                using (Image<Gray, float> result = source.MatchTemplate(template, TemplateMatchingType.CcoeffNormed))
                {
                    double[] minValues, maxValues, imgMatchConfidenceScores;
                    Point[] minLocations, maxLocations;
                    result.MinMax(out minValues, out maxValues, out minLocations, out maxLocations);
                    
                    objTemplates = new TemplateMatching[maxValues.Count()];
                    
                    for (int iCount = 0; iCount < maxValues.Count(); iCount++)
                    {

                        if ((maxValues[iCount] <= 1.0) && (maxValues[iCount] >= confidence))
                        {   
                            Rectangle rect = new Rectangle(maxLocations[iCount].X, maxLocations[iCount].Y, template.Size.Width, template.Size.Height);
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
