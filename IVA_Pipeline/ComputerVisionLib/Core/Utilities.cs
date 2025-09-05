/*=============================================================================================================== *
 * Copyright 2025 Infosys Ltd.                                                                                    *
 * Use of this source code is governed by Apache License Version 2.0 that can be found in the LICENSE file or at  *
 * http://www.apache.org/licenses/                                                                                *
 * ===============================================================================================================*/
﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

using System.Drawing;

using Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.Common;
using System.Diagnostics;

using System.ComponentModel;
using System.Reflection;
using System.IO;
using Microsoft.Practices.EnterpriseLibrary.Common.Utility;
using Unity.Interception.Utilities;
using OpenCvSharp;
using Point = OpenCvSharp.Point;
using OpenCvSharp.Extensions;

namespace Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.ComputerVisionLib.Core
{
    public class Utilities
    {
       

        private const string stopFile = "stop.iap";

        
        private const string className = "Utilities";

        private static System.Collections.Hashtable cacheControlPathElements = new Hashtable();
        private static System.Collections.Hashtable cacheFocusedElements = new Hashtable();

        public static double ScaleStep {get;set;}
        public static double RotationStep {get;set;}
        public static int MaxScaleSteps {get;set;}
        public static byte[] TemplateMatchMapScreen {get {return templateMatchMapScreen;}}

        private static byte[] templateMatchMapScreen;
        public static int TemplateMatchMapBorderThickness {get;set;}
        public static ImageBgr TemplateMatchMapBorderColor {get;set;}

        const bool MULTIPLE_SCALE = true;
        const int DEFAULT_TIMEOUT = 10;
        const int DEFAULT_TIMEOUT_PERINSTANCE = 2;
        const int DEFAULT_CONFIDENCE = 80;
        const int THREAD_SLEEP_DURATION = 100; 

        const string ieWebBrowser = "internet explorer";
        const string firefoxWebBrowser = "firefox";
        const string chromeWebBrowser = "chrome";
       
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool SetDllDirectory(string path);

        public static Stream IapwPackage = null;


       
        public static void SetDLLsPath()
        {
            if (Assembly.GetEntryAssembly() != null) 
            {
                string path = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
                path = System.IO.Path.Combine(path, IntPtr.Size == 8 ? "x64" : "x86");
                bool pathFound = SetDllDirectory(path);
                if (!pathFound)
                    throw new System.ComponentModel.Win32Exception();
            }
        }

       
        static Utilities()
        {
            SetDLLsPath();
        }

        

        private static string FormControlQualifierControlElement(string automationId, string automationName, string controlType, string fullControlQualifier)
        {
            string qualifierRep = FormControlQualifierPath("", fullControlQualifier);
            qualifierRep = qualifierRep + automationId + "." + automationName + "." + controlType;
            return qualifierRep;
        }
        private static string FormControlQualifierPath(string ctlTreePath, string fullControlQualifier)
        {
            string[] controlNameSegments = fullControlQualifier.Split('.');
            string fullControlPath = "";
            if (controlNameSegments.Length > 2) 
            {
                fullControlPath = String.Join(".", controlNameSegments, 0, 2);
            }
            else if (controlNameSegments.Length == 2)
            {
                fullControlPath = controlNameSegments[0];
            }

            fullControlPath = fullControlPath + "." + ctlTreePath;
            return fullControlPath;
        }

        
        public static void ClearCache()
        {
            if (cacheControlPathElements != null && cacheControlPathElements.Count > 0)
                cacheControlPathElements.Clear();
            if (cacheFocusedElements != null && cacheFocusedElements.Count > 0)
                cacheFocusedElements.Clear();
        }

       
        public static void ClearCache(string controlTreePath)
        {
            if (cacheControlPathElements != null && cacheControlPathElements.Count > 0 && cacheControlPathElements.ContainsKey(controlTreePath))
                cacheControlPathElements.Remove(controlTreePath);
            if (cacheFocusedElements != null && cacheFocusedElements.Count > 0 && cacheFocusedElements.ContainsKey(controlTreePath))
                cacheFocusedElements.Remove(controlTreePath);
        }

        
        private static Rectangle firstControl = Rectangle.Empty;
        private static Control controlToTrack = null;
        private static bool firstChanceDone = false;
        private static int maxLoopCount = 50;
        private static int currentCount = 0;

        
        private static System.Threading.AutoResetEvent arEvent = new System.Threading.AutoResetEvent(false);

        

        private static string UpdateStartingLevel(string sourcePath, string targetPath)
        {
            
            if (!string.IsNullOrEmpty(sourcePath) && !string.IsNullOrEmpty(targetPath))
            {
                string[] sourcePathParts = sourcePath.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                if (sourcePathParts.Length > 0)
                {
                    string sourcePathPart = sourcePathParts[0]; 
                    int level = int.Parse(sourcePathPart.Split('[')[0]); 

                    
                    string[] targetPathParts = targetPath.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                    List<string> peerIndexes = new List<string>();
                    foreach (string st in targetPathParts)
                    {
                        string[] sts = st.Substring(0, st.Length - 1).Split('[');
                        peerIndexes.Add(sts[1]);
                    }
                    peerIndexes.Reverse();
                    targetPath = "";
                    for (int i = 0; i < peerIndexes.Count; i++)
                    {
                        targetPath += "/" + level + "[" + peerIndexes[i] + "]";
                        level++;
                    }
                }
            }
            return targetPath;
        }

       

        private static int GetCurrentLevelInAppTree(string appTreePath)
        {
            
            if (!string.IsNullOrEmpty(appTreePath))
            {
                string[] appTreePathParts = appTreePath.Split('/');
                string controlAppTreePathPart = appTreePathParts[appTreePathParts.Length - 1]; 
                int controlLevel = int.Parse(controlAppTreePathPart.Split('[')[0]); 
                return controlLevel;
            }
            else
                return -2; 
        }

        

       

        #region old key press wioth modifier

        

        #endregion

        #region old key press with modifier
        

        #endregion
        
      

        

        public static ControlImageReference GetBoundingRectangle(ControlImageReference imageRef, bool ifImageThenOriginalScale = false, bool ifImageThenWaitIdenfForTemp = false, bool useTrueColorTemplateMatching = false, object searchRegion = null, string atrFolder = "", Stream sourceImageToMatch = null)
        {
            Rectangle rect = Rectangle.Empty;
            imageRef.CurrentBoundingRectangle = rect;
            bool requestedWaitforeverFlag = ifImageThenWaitIdenfForTemp;
           
            if (imageRef != null && imageRef.SupportedStates != null && imageRef.SupportedStates.Count > 0)
            {
                if (imageRef.SupportedStates.Count > 1)
                    ifImageThenWaitIdenfForTemp = requestedWaitforeverFlag = false;

                do
                {
                    foreach (var item in imageRef.SupportedStates)
                    {
                        
                        if (!string.IsNullOrEmpty(item.ImagePath) && item.ImagePath.StartsWith("$"))
                            item.ImagePath = item.ImagePath.Replace("$", atrFolder);

                        if (ifImageThenWaitIdenfForTemp)
                            rect = WaitForElement(item.ImagePath, ifImageThenOriginalScale, useTrueColorTemplateMatching, sourceImageToMatch);
                        else
                        {

                            if (useTrueColorTemplateMatching)
                                rect = FindElementInTrueColor(item.ImagePath, DEFAULT_TIMEOUT, DEFAULT_CONFIDENCE, !ifImageThenOriginalScale, searchRegion, sourceImageToMatch);
                            else
                                rect = FindElement(item.ImagePath, DEFAULT_TIMEOUT, DEFAULT_CONFIDENCE, !ifImageThenOriginalScale, searchRegion, sourceImageToMatch);
                        }
                        if (rect != Rectangle.Empty)
                        {
                            imageRef.CurrentState = item.State;
                            imageRef.CurrentBoundingRectangle = rect;
                            break;
                        }
                    }
                }
                while (requestedWaitforeverFlag && imageRef.CurrentBoundingRectangle == Rectangle.Empty);
            }

            return imageRef;
        }

       
        public static Rectangle FindElement(string filename, int timeout = DEFAULT_TIMEOUT, double confidence = 80, bool multipleScaleMatching = true, object searchRegion = null, Stream sourceImageToMatch = null)
        {
            if (Core.Utilities.IsStopRequested())
                throw new Core.CVExceptions.StopRequested();


            Rectangle elementRectTemp = Rectangle.Empty;
            Rectangle searchRect = Rectangle.Empty;
            DateTime startTime = DateTime.Now;
            try
            {
                if (timeout <= 0)
                    timeout = DEFAULT_TIMEOUT; 
                if (System.IO.File.Exists(filename) || (IapwPackage != null && IapwPackage.Length > 0))
                {
                    Mat template = new Mat();

                    if (IapwPackage == null)
                        template = new Mat(filename, ImreadModes.Color);
                    else
                    {

                        Stream templateStream = Packaging.ExtractFile(IapwPackage, filename);
                        var bmp = new Bitmap(templateStream);

                        template = BitmapConverter.ToMat(bmp);


                        Packaging.ClosePackage();
                    }
                    bool backgroundProcessing = false;
                    while ((System.DateTime.Now - startTime).TotalMilliseconds <= timeout * 1000 && elementRectTemp == Rectangle.Empty)
                    {
                        if (searchRegion != null && (Rectangle)searchRegion != Rectangle.Empty)
                        {
                            searchRect = (Rectangle)searchRegion;
                        }
                        Mat source = new Mat();
                        if (sourceImageToMatch != null)
                        {
                            

                            backgroundProcessing = true;
                        }
                       
                        if (multipleScaleMatching)
                        {
                           
                            int direction = 1;
                            for (int i = 0; i <= MaxScaleSteps; i++)
                            {
                                
                                double scale = 1 + direction * i * ScaleStep;
                                Mat templateTemp = ResizeTemplate(template, scale);
                                if (templateTemp != null)
                                {
                                    elementRectTemp = FindRectangle(source, templateTemp, confidence);
                                    if (elementRectTemp != Rectangle.Empty)
                                    {
                                        break;
                                    }
                                }

                                
                                scale = 1 + (-direction) * i * ScaleStep;
                                templateTemp = ResizeTemplate(template, scale);
                                if (templateTemp != null)
                                {
                                    elementRectTemp = FindRectangle(source, templateTemp, confidence);
                                    if (elementRectTemp != Rectangle.Empty)
                                    {
                                        break;
                                    }
                                }
                            }
                        }
                        else
                        {
                            elementRectTemp = FindRectangle(source, template, confidence);
                            if (elementRectTemp != Rectangle.Empty)
                            {
                                break;
                            }
                        }
                        System.Threading.Thread.Sleep(THREAD_SLEEP_DURATION);
                    }
                }
                else
                    throw new System.IO.FileNotFoundException("Image Template " + filename + " not found");
                if (elementRectTemp == Rectangle.Empty)
                    throw new Exception("Could not match the template provided in this trial. Probably in the subsequent trial, the template would be matched.");
            }
            catch (System.IO.FileNotFoundException ex)
            {
                
                string exMessage = ex.Message;
                string innerExMessage = ex.InnerException != null ? ex.InnerException.Message : "";
                LogHandler.LogError(string.Format(Logging.ErrorMessages.EXCEPTION, "FindElement", exMessage, innerExMessage), LogHandler.Layer.Business);
                throw ex;
            }
            catch (Exception ex)
            {
                
                string exMessage = ex.Message;
                string innerExMessage = ex.InnerException != null ? ex.InnerException.Message : "";
                LogHandler.LogError(string.Format(Logging.ErrorMessages.EXCEPTION, "FindElement", exMessage, innerExMessage), LogHandler.Layer.Business);
                
            }

            
            if (searchRegion != null && searchRect != Rectangle.Empty)
            {
                elementRectTemp.X += searchRect.X;
                elementRectTemp.Y += searchRect.Y;
            }
            return elementRectTemp;
        }

        
        public static Rectangle FindElementInTrueColor(string filename, int timeout = DEFAULT_TIMEOUT, double confidence = 80, bool multipleScaleMatching = true, object searchRegion = null, Stream sourceImageToMatch = null)
        {
            
            if (Core.Utilities.IsStopRequested())
                throw new CVExceptions.StopRequested();

            Rectangle elementRectTemp = Rectangle.Empty;
            Rectangle searchRect = Rectangle.Empty;
            DateTime startTime = DateTime.Now;
            try
            {
                if (timeout <= 0)
                    timeout = DEFAULT_TIMEOUT; 
                if (System.IO.File.Exists(filename) || (IapwPackage != null && IapwPackage.Length > 0))
                {
                    Mat template = new Mat();

                    if (IapwPackage == null)
                        template = new Mat(filename, ImreadModes.Color);
                    else
                    {

                        Stream templateStream = Packaging.ExtractFile(IapwPackage, filename);
                        var bmp = new Bitmap(templateStream);
                        template = BitmapConverter.ToMat(bmp);

                        Packaging.ClosePackage();
                    }
                    bool backgroundProcessing = false;
                    while ((System.DateTime.Now - startTime).TotalMilliseconds <= timeout * 1000 && elementRectTemp == Rectangle.Empty)
                    {
                        if (searchRegion != null && (Rectangle)searchRegion != Rectangle.Empty)
                        {
                            searchRect = (Rectangle)searchRegion;
                        }
                        Mat source = new Mat();
                        if (sourceImageToMatch != null)
                        {
                            
                            var bmp = new Bitmap(sourceImageToMatch);
                            source = BitmapConverter.ToMat(bmp);
                            backgroundProcessing = true;
                        }
                        
                        if (multipleScaleMatching)
                        {
                            
                            int direction = 1;
                            for (int i = 0; i <= MaxScaleSteps; i++)
                            {
                                
                                double scale = 1 + direction * i * ScaleStep;
                                Mat templateTemp = ResizeTemplateInTrueColor(template, scale);
                                if (templateTemp != null)
                                {
                                    elementRectTemp = FindRectangleInTrueColor(source, templateTemp, confidence);
                                    if (elementRectTemp != Rectangle.Empty)
                                    {
                                        break;
                                    }
                                }

                                
                                scale = 1 + (-direction) * i * ScaleStep;
                                templateTemp = ResizeTemplateInTrueColor(template, scale);
                                if (templateTemp != null)
                                {
                                    elementRectTemp = FindRectangleInTrueColor(source, templateTemp, confidence);
                                    if (elementRectTemp != Rectangle.Empty)
                                    {
                                        break;
                                    }
                                }
                            }
                        }
                        else
                        {
                            elementRectTemp = FindRectangleInTrueColor(source, template, confidence);
                            if (elementRectTemp != Rectangle.Empty)
                            {
                                break;
                            }
                        }
                        System.Threading.Thread.Sleep(THREAD_SLEEP_DURATION);
                    }
                }
                else
                    throw new System.IO.FileNotFoundException("Image Template " + filename + " not found");

                if (elementRectTemp == Rectangle.Empty)
                    throw new Exception("Could not match the template provided in this trial. Probably in the subsequent trial, the template would be matched.");
            }
            catch (System.IO.FileNotFoundException ex)
            {
                
                string exMessage = ex.Message;
                string innerExMessage = ex.InnerException != null ? ex.InnerException.Message : "";
                LogHandler.LogError(string.Format(Logging.ErrorMessages.EXCEPTION, "FindElementInTrueColor", exMessage, innerExMessage), LogHandler.Layer.Business);
                throw ex;
            }
            catch (Exception ex)
            {
                
                string exMessage = ex.Message;
                string innerExMessage = ex.InnerException != null ? ex.InnerException.Message : "";
                LogHandler.LogError(string.Format(Logging.ErrorMessages.EXCEPTION, "FindElementInTrueColor", exMessage, innerExMessage), LogHandler.Layer.Business);
                
            }

           
            if (searchRegion != null && searchRect != Rectangle.Empty)
            {
                elementRectTemp.X += searchRect.X;
                elementRectTemp.Y += searchRect.Y;
            }
            return elementRectTemp;
        }

        public static Rectangle WaitForElement(string filename, bool templateMachingInOriginalScale = false, bool useTrueColorTemplateMatching = false, object searchRegion = null, Stream sourceImageToMatch = null)
        {
            Rectangle elementRectTemp = Rectangle.Empty;
            while (elementRectTemp == Rectangle.Empty)
            {
                try
                {
                    if (useTrueColorTemplateMatching)
                        elementRectTemp = FindElementInTrueColor(filename, DEFAULT_TIMEOUT, DEFAULT_CONFIDENCE, !templateMachingInOriginalScale, searchRegion, sourceImageToMatch);
                    else
                        elementRectTemp = FindElement(filename, DEFAULT_TIMEOUT, DEFAULT_CONFIDENCE, !templateMachingInOriginalScale, searchRegion, sourceImageToMatch);
                }
                catch (System.IO.FileNotFoundException ex)
                {
                    throw ex;
                }
                catch (CVExceptions.StopRequested ex)
                {
                    throw ex;
                }
                catch (Exception ex)
                {
                    
                }
                System.Threading.Thread.Sleep(THREAD_SLEEP_DURATION);
            }
            return elementRectTemp;
        }

        
        public static List<TemplateMatching> FindAllInstances(string filename,out double angle,int timeout=DEFAULT_TIMEOUT_PERINSTANCE,double confidence=80,bool multipleScaleMatching=true,bool multiRotationMatching=true,Stream sourceImageToMatch=null,bool enableTemplateMatchMap=false) {
           
            List<TemplateMatching> rects=new List<TemplateMatching>();
            List<Point[]> boxes=new List<Point[]>();
            int currentCount=0;
            bool forever=false;
            TemplateMatching[] elementRectTemp;
            DateTime startTime=DateTime.Now;
            Mat sourceMatches = new Mat();
            angle=0;
            try
            {
                if (timeout == 0)
                    timeout = DEFAULT_TIMEOUT_PERINSTANCE; 
                if (timeout < 0)
                    forever = true;
                if (System.IO.File.Exists(filename))
                {
                    Mat template = new Mat(filename, ImreadModes.Color);
                   
                    bool backgroundProcessing = false;
                   
                    while ((forever && currentCount == 0) || (System.DateTime.Now - startTime).TotalMilliseconds <= timeout * 1000 || (rects.Count - currentCount > 0))
                    {
                        currentCount = rects.Count;
                        if (currentCount > 0)
                            forever = false;
                        Mat source = new Mat();
                        if (sourceImageToMatch != null)
                        {
                            
                            var bmp = new Bitmap(sourceImageToMatch);
                            source = BitmapConverter.ToMat(bmp);

                            backgroundProcessing = true;
                            if (enableTemplateMatchMap)
                            {
                                if (templateMatchMapScreen != null)
                                {

                                   
                                    var btmp = new Bitmap(new MemoryStream(templateMatchMapScreen));
                                    sourceMatches = BitmapConverter.ToMat(btmp);
                                }
                                else
                                {
                                   
                                    var btmp = new Bitmap(sourceImageToMatch);
                                    sourceMatches = BitmapConverter.ToMat(btmp);

                                }
                            }
                        }
                       
                        if (boxes != null && boxes.Count > 0)
                        {
                            boxes.ForEach(b =>
                            {
                                //source.FillConvexPoly(b, new Gray(0));
                                source.FillConvexPoly(b, new Scalar(0, 0, 0));
                            });
                            Cv2.Polylines(source, boxes, true, new Scalar(TemplateMatchMapBorderColor.Blue, TemplateMatchMapBorderColor.Green, TemplateMatchMapBorderColor.Red), TemplateMatchMapBorderThickness);
                            //boxes.ForEach(b =>
                            //{

                            //    sourceMatches.DrawPolyline(b, true,
                            //        new Bgr(TemplateMatchMapBorderColor.Blue, TemplateMatchMapBorderColor.Green, TemplateMatchMapBorderColor.Red),
                            //        TemplateMatchMapBorderThickness);

                            //});

                        }
                        if(multipleScaleMatching && multiRotationMatching) {
                           
                            int direction=1;
                            bool noMatchAboveConfidenceScore=false; 
                            int countBoxes=0;
                            
                            for(int i=0;i<=MaxScaleSteps;i++) {
                                
                                try {
                                    
                                    double scale=1+direction*i*ScaleStep;
                                    Mat templateTemp=ResizeTemplate(template,scale);
                                    if(templateTemp!=null) {
                                        
                                        for(double j=0;j<=180;j+=RotationStep) {
                                            
                                            Mat imageTemp=RotateImage(source,j);
                                            if(imageTemp!=null) {
                                                
                                                elementRectTemp=FindRectangles(imageTemp,templateTemp,confidence);
                                                if(elementRectTemp?.Count()>0) {
                                                    
                                                    for(int icount=0;icount<elementRectTemp.Count();icount++) {
                                                        if(elementRectTemp[icount]?.BoundingBox.Height>0 && elementRectTemp[icount]?.BoundingBox.Width>0) {
                                                            rects.Add(elementRectTemp[icount]);
                                                            var boundingBox=RectToBox(elementRectTemp[icount].BoundingBox);
                                                            boxes.Add(boundingBox);
                                                            countBoxes++;
                                                        }
                                                    }
                                                    if(countBoxes>0) {
                                                        angle=j;
                                                        break;
                                                    }
                                                    
                                                }
                                            }
                                            
                                            imageTemp=RotateImage(source,-i);
                                            if(imageTemp!=null) {
                                                
                                                elementRectTemp=FindRectangles(imageTemp,templateTemp,confidence);
                                                if(elementRectTemp?.Count()>0) {
                                                    
                                                    for(int icount=0;icount<elementRectTemp.Count();icount++) {
                                                        if(elementRectTemp[icount]?.BoundingBox.Height>0 && elementRectTemp[icount]?.BoundingBox.Width>0) {
                                                            rects.Add(elementRectTemp[icount]);
                                                            
                                                            var boundingBox=RectToBox(elementRectTemp[icount].BoundingBox);
                                                            boxes.Add(boundingBox);
                                                            countBoxes++;
                                                        }
                                                    }
                                                    if(countBoxes>0) {
                                                        angle=-j;
                                                        break;
                                                    }
                                                    
                                                }
                                            }
                                        }
                                    }
                                    if(countBoxes>0) {
                                        break;
                                    }
                                    
                                    scale=1+(-direction)*i*ScaleStep;
                                    templateTemp=ResizeTemplate(template,scale);
                                    if(templateTemp!=null) {
                                        
                                        for(double j=0;j<=180;j+=RotationStep) {
                                            
                                            Mat imageTemp=RotateImage(source,j);
                                            if(imageTemp!=null) {
                                                
                                                elementRectTemp=FindRectangles(imageTemp,templateTemp,confidence);
                                                if(elementRectTemp?.Count()>0) {
                                                    
                                                    for(int icount=0;icount<elementRectTemp.Count();icount++) {
                                                        if(elementRectTemp[icount]?.BoundingBox.Height>0 && elementRectTemp[icount]?.BoundingBox.Width>0) {
                                                            rects.Add(elementRectTemp[icount]);
                                                            var boundingBox=RectToBox(elementRectTemp[icount].BoundingBox);
                                                            boxes.Add(boundingBox);
                                                            countBoxes++;
                                                        }
                                                    }
                                                    if(countBoxes>0) {
                                                        angle=j;
                                                        break;
                                                    }
                                                    
                                                }
                                            }
                                            
                                            imageTemp=RotateImage(source,-i);
                                            if(imageTemp!=null) {
                                                
                                                elementRectTemp=FindRectangles(imageTemp,templateTemp,confidence);
                                                if(elementRectTemp?.Count()>0) {
                                                   
                                                    for(int icount=0;icount<elementRectTemp.Count();icount++) {
                                                        if(elementRectTemp[icount]?.BoundingBox.Height>0 && elementRectTemp[icount]?.BoundingBox.Width>0) {
                                                            rects.Add(elementRectTemp[icount]);
                                                            
                                                            var boundingBox=RectToBox(elementRectTemp[icount].BoundingBox);
                                                            boxes.Add(boundingBox);
                                                            countBoxes++;
                                                        }
                                                    }
                                                    if(countBoxes>0) {
                                                        angle=-j;
                                                        break;
                                                    }
                                                    
                                                }
                                            }
                                        }
                                    }
                                    if(countBoxes>0) {
                                        break;
                                    }
                                }
                                catch(Exception ex) {
                                    LogHandler.LogError(ex.ToString(),LogHandler.Layer.ComputerVision);
                                }
                                
                            }
                            
                            if(countBoxes==0) {
                                noMatchAboveConfidenceScore=true;
                            }
                            if(countBoxes>0 || noMatchAboveConfidenceScore)
                                break; 
                        }
                        else if (multipleScaleMatching)
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
                        else if(multiRotationMatching) {
                            
                            bool noMatchAboveConfidenceScore=false;
                            int direction=1;
                            int countBoxes=0;
                            for(double i=0;i<=180;i+=RotationStep) {
                                try {
                                    
                                    Mat imageTemp=RotateImage(source,i);
                                    if(imageTemp!=null) {
                                        
                                        elementRectTemp=FindRectangles(imageTemp,template,confidence);
                                        if(elementRectTemp?.Count()>0) {
                                            
                                            for(int icount=0;icount<elementRectTemp.Count();icount++) {
                                                if(elementRectTemp[icount]?.BoundingBox.Height>0 && elementRectTemp[icount]?.BoundingBox.Width>0) {
                                                    rects.Add(elementRectTemp[icount]);
                                                    var boundingBox=RectToBox(elementRectTemp[icount].BoundingBox);
                                                    boxes.Add(boundingBox);
                                                    countBoxes++;
                                                }
                                            }
                                            if(countBoxes>0) {
                                                angle=i;
                                                break;
                                            }
                                            
                                        }
                                    }
                                    
                                    imageTemp=RotateImage(source,-i);
                                    if(imageTemp!=null) {
                                        
                                        elementRectTemp=FindRectangles(imageTemp,template,confidence);
                                        if(elementRectTemp?.Count()>0) {
                                            
                                            for(int icount=0;icount<elementRectTemp.Count();icount++) {
                                                if(elementRectTemp[icount]?.BoundingBox.Height>0 && elementRectTemp[icount]?.BoundingBox.Width>0) {
                                                    rects.Add(elementRectTemp[icount]);
                                                    
                                                    var boundingBox=RectToBox(elementRectTemp[icount].BoundingBox);
                                                    boxes.Add(boundingBox);
                                                    countBoxes++;
                                                }
                                            }
                                            if(countBoxes>0) {
                                                angle=-i;
                                                break;
                                            }
                                           
                                        }
                                    }
                                }
                                catch(Exception ex) {
                                    LogHandler.LogError(ex.ToString(),LogHandler.Layer.ComputerVision);
                                }
                            }
                            
                            if(countBoxes==0) {
                                noMatchAboveConfidenceScore=true;
                            }
                            if(countBoxes>0 || noMatchAboveConfidenceScore)
                                break; 
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
                    templateMatchMapScreen = sourceMatches.ImEncode(".jpg");
            }
            return rects;
        }

       
        public static List<TemplateMatching> FindAllInstancesInTrueColor(string filename,out double angle,int timeout=DEFAULT_TIMEOUT_PERINSTANCE,double confidence=80,bool multipleScaleMatching=true,bool multiRotationMatching=true,Stream sourceImageToMatch=null,bool enableTemplateMatchMap=false) {
            
            List<TemplateMatching> rects=new List<TemplateMatching>();
            List<Point[]> boxes=new List<Point[]>();
            int currentCount=0;
            bool forever=false;
            
            TemplateMatching[] elementRectTemp;
            Mat sourceMatches = new Mat();
            DateTime startTime=DateTime.Now;
            angle=0;
            try
            {

                if (timeout <= 0)
                    timeout = DEFAULT_TIMEOUT_PERINSTANCE; 
                if (timeout < 0)
                    forever = true;
                if (System.IO.File.Exists(filename))
                {
                    Mat template = new Mat(filename, ImreadModes.Color);
                    bool backgroundProcessing = false;

                    
                    while ((forever && currentCount == 0)
                        || (System.DateTime.Now - startTime).TotalMilliseconds <= timeout * 1000
                        || (rects.Count - currentCount > 0))
                    {
                        currentCount = rects.Count;
                        if (currentCount > 0)
                            forever = false;

                        Mat source = new Mat();
                        if (sourceImageToMatch != null)
                        {
                            
                            var bmp = new Bitmap(sourceImageToMatch);
                            source = BitmapConverter.ToMat(bmp);
                            backgroundProcessing = true;
                        }
                        
                        if (enableTemplateMatchMap)
                        {
                            if (templateMatchMapScreen != null)
                            {

                                
                                var btmp = new Bitmap(new MemoryStream(templateMatchMapScreen));
                                sourceMatches = BitmapConverter.ToMat(btmp);
                            }
                            else
                            {

                              
                                var btmp = source.ToBitmap();
                                sourceMatches = BitmapConverter.ToMat(btmp);
                            }

                        }
                       
                        if (boxes != null && boxes.Count > 0)
                        {
                            boxes.ForEach(b =>
                            {
                                //source.FillConvexPoly(b, new Bgr(Color.Black));
                                Cv2.FillConvexPoly(source, b, new Scalar(0, 0, 0));
                            });
                            Cv2.Polylines(source, boxes, true, new Scalar(TemplateMatchMapBorderColor.Blue, TemplateMatchMapBorderColor.Green, TemplateMatchMapBorderColor.Red),
                                    TemplateMatchMapBorderThickness);
                            
                            //boxes.ForEach(b =>
                            //{
                                
                            //    sourceMatches.DrawPolyline(b, true,
                            //        new Bgr(TemplateMatchMapBorderColor.Blue, TemplateMatchMapBorderColor.Green, TemplateMatchMapBorderColor.Red),
                            //        TemplateMatchMapBorderThickness);

                            //});
                        }
                        if(multipleScaleMatching && multiRotationMatching) {
                            
                            int direction=1;
                            bool noMatchAboveConfidenceScore=false; 
                            int countBoxes=0;
                            
                            for(int i=0;i<=MaxScaleSteps;i++) {
                                
                                try {
                                    
                                    double scale=1+direction*i*ScaleStep;
                                    Mat templateTemp=ResizeTemplateInTrueColor(template,scale);
                                    if(templateTemp!=null) {
                                        
                                        for(double j=0;j<=180;j+=RotationStep) {
                                           
                                            Mat imageTemp=RotateImageInTrueColor(source,j);
                                            if(imageTemp!=null) {
                                                
                                                elementRectTemp=FindRectanglesInTrueColor(imageTemp,templateTemp,confidence);
                                                if(elementRectTemp?.Count()>0) {
                                                    
                                                    for(int icount=0;icount<elementRectTemp.Count();icount++) {
                                                        if(elementRectTemp[icount]?.BoundingBox.Height>0 && elementRectTemp[icount]?.BoundingBox.Width>0) {
                                                            rects.Add(elementRectTemp[icount]);
                                                            var boundingBox=RectToBox(elementRectTemp[icount].BoundingBox);
                                                            boxes.Add(boundingBox);
                                                            countBoxes++;
                                                        }
                                                    }
                                                    if(countBoxes>0) {
                                                        angle=j;
                                                        break;
                                                    }
                                                   
                                                }
                                            }
                                            
                                            imageTemp=RotateImageInTrueColor(source,-i);
                                            if(imageTemp!=null) {
                                               
                                                elementRectTemp=FindRectanglesInTrueColor(imageTemp,templateTemp,confidence);
                                                if(elementRectTemp?.Count()>0) {
                                                    
                                                    for(int icount=0;icount<elementRectTemp.Count();icount++) {
                                                        if(elementRectTemp[icount]?.BoundingBox.Height>0 && elementRectTemp[icount]?.BoundingBox.Width>0) {
                                                            rects.Add(elementRectTemp[icount]);
                                                            
                                                            var boundingBox=RectToBox(elementRectTemp[icount].BoundingBox);
                                                            boxes.Add(boundingBox);
                                                            countBoxes++;
                                                        }
                                                    }
                                                    if(countBoxes>0) {
                                                        angle=-j;
                                                        break;
                                                    }
                                                    
                                                }
                                            }
                                        }
                                    }
                                    if(countBoxes>0) {
                                        break;
                                    }
                                   
                                    scale=1+(-direction)*i*ScaleStep;
                                    templateTemp=ResizeTemplateInTrueColor(template,scale);
                                    if(templateTemp!=null) {
                                        
                                        for(double j=0;j<=180;j+=RotationStep) {
                                           
                                            Mat imageTemp=RotateImageInTrueColor(source,j);
                                            if(imageTemp!=null) {
                                                
                                                elementRectTemp=FindRectanglesInTrueColor(imageTemp,templateTemp,confidence);
                                                if(elementRectTemp?.Count()>0) {
                                                   
                                                    for(int icount=0;icount<elementRectTemp.Count();icount++) {
                                                        if(elementRectTemp[icount]?.BoundingBox.Height>0 && elementRectTemp[icount]?.BoundingBox.Width>0) {
                                                            rects.Add(elementRectTemp[icount]);
                                                            var boundingBox=RectToBox(elementRectTemp[icount].BoundingBox);
                                                            boxes.Add(boundingBox);
                                                            countBoxes++;
                                                        }
                                                    }
                                                    if(countBoxes>0) {
                                                        angle=j;
                                                        break;
                                                    }
                                                    
                                                }
                                            }
                                            
                                            imageTemp=RotateImageInTrueColor(source,-i);
                                            if(imageTemp!=null) {
                                                
                                                elementRectTemp=FindRectanglesInTrueColor(imageTemp,templateTemp,confidence);
                                                if(elementRectTemp?.Count()>0) {
                                                    
                                                    for(int icount=0;icount<elementRectTemp.Count();icount++) {
                                                        if(elementRectTemp[icount]?.BoundingBox.Height>0 && elementRectTemp[icount]?.BoundingBox.Width>0) {
                                                            rects.Add(elementRectTemp[icount]);
                                                           
                                                            var boundingBox=RectToBox(elementRectTemp[icount].BoundingBox);
                                                            boxes.Add(boundingBox);
                                                            countBoxes++;
                                                        }
                                                    }
                                                    if(countBoxes>0) {
                                                        angle=-j;
                                                        break;
                                                    }
                                                    
                                                }
                                            }
                                        }
                                    }
                                    if(countBoxes>0) {
                                        break;
                                    }
                                }
                                catch(Exception ex) {
                                    LogHandler.LogError(ex.ToString(),LogHandler.Layer.ComputerVision);
                                }
                               
                            }
                            
                            if(countBoxes==0) {
                                noMatchAboveConfidenceScore=true;
                            }
                            if(countBoxes>0 || noMatchAboveConfidenceScore)
                                break; 
                        }
                        else if (multipleScaleMatching)
                        {
                            
                            int direction = 1;
                           
                            int countBoxes = 0;

                           

                            for (int i = 0; i <= MaxScaleSteps; i++)
                            {
                                
                                try
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
                                catch (Exception ex)
                                {
                                    LogHandler.LogError(ex.ToString(), LogHandler.Layer.ComputerVision);
                                }

                                
                            }

                            

                            if (countBoxes == 0)  
                            {
                                break;
                            }

                        }
                        else if(multiRotationMatching) {
                            
                            bool noMatchAboveConfidenceScore=false;
                            int direction=1;
                            int countBoxes=0;
                            for(double i=0;i<=180;i+=RotationStep) {
                                try {
                                    
                                    Mat imageTemp=RotateImageInTrueColor(source,i);
                                    if(imageTemp!=null) {
                                        
                                        elementRectTemp=FindRectanglesInTrueColor(imageTemp,template,confidence);
                                        if(elementRectTemp?.Count()>0) {
                                           
                                            for(int icount=0;icount<elementRectTemp.Count();icount++) {
                                                if(elementRectTemp[icount]?.BoundingBox.Height>0 && elementRectTemp[icount]?.BoundingBox.Width>0) {
                                                    rects.Add(elementRectTemp[icount]);
                                                    var boundingBox=RectToBox(elementRectTemp[icount].BoundingBox);
                                                    boxes.Add(boundingBox);
                                                    countBoxes++;
                                                }
                                            }
                                            if(countBoxes>0) {
                                                angle=i;
                                                break;
                                            }
                                           
                                        }
                                    }
                                    
                                    imageTemp=RotateImageInTrueColor(source,-i);
                                    if(imageTemp!=null) {
                                        
                                        elementRectTemp=FindRectanglesInTrueColor(imageTemp,template,confidence);
                                        if(elementRectTemp?.Count()>0) {
                                            
                                            for(int icount=0;icount<elementRectTemp.Count();icount++) {
                                                if(elementRectTemp[icount]?.BoundingBox.Height>0 && elementRectTemp[icount]?.BoundingBox.Width>0) {
                                                    rects.Add(elementRectTemp[icount]);
                                                    
                                                    var boundingBox=RectToBox(elementRectTemp[icount].BoundingBox);
                                                    boxes.Add(boundingBox);
                                                    countBoxes++;
                                                }
                                            }
                                            if(countBoxes>0) {
                                                angle=-i;
                                                break;
                                            }
                                            
                                        }
                                    }
                                }
                                catch(Exception ex) {
                                    LogHandler.LogError(ex.ToString(),LogHandler.Layer.ComputerVision);
                                }
                            }
                            
                            if(countBoxes==0) {
                                noMatchAboveConfidenceScore=true;
                            }
                            if(countBoxes>0 || noMatchAboveConfidenceScore)
                                break; 
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
                    templateMatchMapScreen = sourceMatches.ImEncode(".jpg");
            }
            return rects;
        }


       


        public static void SaveImageGray(Mat image, string filePathToSave)
        {
            image.ImWrite(filePathToSave);
        }
        public static void SaveImageTrueColor(Mat image, string filePathToSave)
        {
            image.ImWrite(filePathToSave);
        }

       

       

        private static Mat ResizeTemplate(Mat template, double scale)
        {
            Mat resizedTemplate = new Mat();
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

                resizedTemplate = template.Resize(new OpenCvSharp.Size(), scale, scale, InterpolationFlags.Lanczos4);

                if (resizedTemplate.Width * resizedTemplate.Height < 250)
                    return null;
            }
            catch
            {
                

                
            }
            return resizedTemplate;
        }

        private static Mat ResizeTemplateInTrueColor(Mat template, double scale)
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
                resizedTemplate = template.Resize(new OpenCvSharp.Size(), scale, scale, InterpolationFlags.Lanczos4);
            }
            catch
            {
                
            }
            return resizedTemplate;
        }

        private static Mat RotateImageInTrueColor(Mat image,double angle) {
            Mat rotatedImage = new Mat();
            try {
                Point2f center = new Point2f(image.Width / 2.0f, image.Height / 2.0f);
                InputArray rotationMatrix = Cv2.GetRotationMatrix2D(center, angle, 1.0);
                Cv2.WarpAffine(image, rotatedImage, rotationMatrix, image.Size(), InterpolationFlags.Linear, BorderTypes.Constant, new Scalar(255, 255, 255));
            }
            catch {
                
            }
            return rotatedImage;
        }

        private static Mat RotateImage(Mat image,double angle) {
            Mat rotatedImage = new Mat();
            try {
                Point2f center = new Point2f(image.Width / 2.0f, image.Height / 2.0f);
                InputArray rotationMatrix = Cv2.GetRotationMatrix2D(center, angle, 1.0);
                Cv2.WarpAffine(image, rotatedImage, rotationMatrix, image.Size(), InterpolationFlags.Linear, BorderTypes.Constant, new Scalar(255, 255, 255));
            }
            catch {
               
            }
            return rotatedImage;
        }

        private static Rectangle FindRectangle(Mat source, Mat template, double confidence)
        {
            Rectangle rect = Rectangle.Empty;
            try
            {
                confidence = confidence / 100;
                using (Mat result = source.MatchTemplate(template, TemplateMatchModes.CCoeffNormed))
                {
                    double minValues, maxValues;
                    Point minLocations, maxLocations;
                    result.MinMaxLoc(out minValues, out maxValues, out minLocations, out maxLocations);

                    if ((maxValues <= 1.0) && (maxValues >= confidence))
                    {

                        rect = new Rectangle(maxLocations.X, maxLocations.Y, template.Size().Width, template.Size().Height);

                    }
                }
            }
            catch (Exception ex)
            {
                
                Exception ex1 = ex;
            }
            return rect;
        }

       
        private static TemplateMatching[] FindRectangles(Mat source, Mat template, double confidence)
        {
            TemplateMatching[] objTemplates = null;
            try
            {
                confidence = confidence / 100;

                using (Mat result = source.MatchTemplate(template, TemplateMatchModes.CCorrNormed))
                {
                    double minValues, maxValues;
                    Point minLocations, maxLocations;
                    result.MinMaxLoc(out minValues, out maxValues, out minLocations, out maxLocations);


                    objTemplates = new TemplateMatching[1];
                    for (int iCount = 0; iCount < maxValues; iCount++)
                    {

                        if ((maxValues <= 1.0) && (maxValues >= confidence))
                        {
                            Rectangle rect = new Rectangle(maxLocations.X, maxLocations.Y, template.Size().Width, template.Size().Height);
                            TemplateMatching objTemplate = new TemplateMatching();
                            objTemplate.BoundingBox = rect;
                            objTemplate.ConfidenceScore = maxValues;
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
        private static Rectangle FindRectangleInTrueColor(Mat source, Mat template, double confidence)
        {
            Rectangle rect = Rectangle.Empty;
            try
            {
                confidence = confidence / 100;
                using (Mat result = source.MatchTemplate(template, TemplateMatchModes.CCoeffNormed))
                {
                    double minValues, maxValues;
                    Point minLocations, maxLocations;
                    result.MinMaxLoc(out minValues, out maxValues, out minLocations, out maxLocations);
                    if ((maxValues <= 1.0) && (maxValues >= confidence))
                    {
                        rect = new Rectangle(maxLocations.X, maxLocations.Y, template.Size().Width, template.Size().Height);
                    }
                }
            }
            catch
            {
                
            }
            return rect;
        }
        

        private static TemplateMatching[] FindRectanglesInTrueColor(Mat source, Mat template, double confidence)
        {

            

            TemplateMatching[] objTemplates = null;

            try
            {
                confidence = confidence / 100;
                using (Mat result = source.MatchTemplate(template, TemplateMatchModes.CCoeffNormed))
                {
                    double minValues, maxValues, imgMatchConfidenceScores;
                    Point minLocations, maxLocations;
                    result.MinMaxLoc(out minValues, out maxValues, out minLocations, out maxLocations);

                    objTemplates = new TemplateMatching[1];



                    if ((maxValues <= 1.0) && (maxValues >= confidence))
                    {

                        Rectangle rect = new Rectangle(maxLocations.X, maxLocations.Y, template.Size().Width, template.Size().Height);
                        TemplateMatching objTemplate = new TemplateMatching();
                        objTemplate.BoundingBox = rect;
                        objTemplate.ConfidenceScore = maxValues;


                        objTemplates.SetValue(objTemplate, 0);
                    }
                }
            }
            catch (Exception ex)
            {
                
                Exception ex1 = ex;
            }
            
            return objTemplates;
        }

        private static Point[] RectToBox(Rectangle rectangle)
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


        
        public static int LaunchApplication(string appPath, string appType, string webBrowser = "", bool showWaitBox = true, string appArgument = "")
        {
            Core.Utilities.WriteLog("launching application- " + appPath);
            int processId = 0;
            List<string> allowedBrowsers = new List<string>() { ieWebBrowser, firefoxWebBrowser, chromeWebBrowser };

             
            if (!string.IsNullOrEmpty(appPath))
            {
                ProcessStartInfo processStart = new ProcessStartInfo();
                processStart.CreateNoWindow = false;
                
                processStart.WindowStyle = ProcessWindowStyle.Maximized;

                
                if (appType.ToLower() == "web")
                {
                    if ((!String.IsNullOrEmpty(webBrowser)) && allowedBrowsers.Contains(webBrowser.ToLower()))
                    {
                        webBrowser = webBrowser.ToLower();
                    }
                    switch (webBrowser)
                    {
                        case ieWebBrowser:
                            processStart.FileName = "iexplore.exe";
                            
                            processStart.Arguments = "-new " + appPath;
                            break;
                        case firefoxWebBrowser:
                            processStart.FileName = "firefox.exe";
                            
                            processStart.Arguments = "-new " + appPath;
                            break;
                        case chromeWebBrowser:
                            processStart.FileName = "chrome.exe";
                            
                            processStart.Arguments = "-new " + appPath;
                            break;
                        default:

                            string argumentParams = "";
                            if (processStart.FileName.ToLower().Contains("iexplore"))
                            {
                                
                                argumentParams = "-new ";
                            }
                            else if (processStart.FileName.ToLower().Contains("firefox"))
                            {
                                

                                argumentParams = "-new ";

                            }
                            else if (processStart.FileName.ToLower().Contains("chrome"))
                            {
                                
                                argumentParams = "-new ";
                            }

                            processStart.Arguments = argumentParams + appPath;

                            break;
                    }
                    processStart.Arguments = processStart.Arguments + " " + appArgument.Trim();
                }
                

                
                else
                {
                   
                    string[] appPathParts = GetExecutableAndArguement(appPath);

                    if (ValidationUtility.InvalidCharValidatorForFile(System.IO.Path.GetFileNameWithoutExtension(appPathParts[0])))
                    {
                        throw new Exception("Please provide the file name without Special Characters");
                    }
                    processStart.FileName = appPathParts[0];
                    if (appPathParts.Length == 2)
                        processStart.Arguments = appPathParts[1];
                    if (!string.IsNullOrEmpty(appArgument))
                        processStart.Arguments = appArgument;
                }
                using (Process process = Process.Start(processStart))
                {
                    
                    processId = process.Id;
                    
                    System.Threading.Thread.Sleep(2000);
                }
            }
            else
            {
               
            }
            Core.Utilities.WriteLog("application process id- " + processId.ToString());
            return processId;
        }


        private static string[] GetExecutableAndArguement(string completeAppPath)
        {
            string delimiter = ".exe";
            string[] parts = completeAppPath.ToLower().Split(new string[] { delimiter }, StringSplitOptions.RemoveEmptyEntries);
            parts[0] = parts[0] + delimiter;
            if (parts.Length == 2)
            {
                parts[1] = parts[1].Trim();
                if (!parts[1].StartsWith("\""))
                {
                    parts[1] = "\"" + parts[1];
                }
                if (!parts[1].EndsWith("\""))
                {
                    parts[1] = parts[1] + "\"";
                }
            }
            return parts;
        }

        


        
        private static string CleanifyBrowserPath(string browserPath)
        {
            string result = string.Empty;

            if (!String.IsNullOrEmpty(browserPath))
            {
                int position = browserPath.IndexOf(".exe");
                if (position > 0)
                    result = browserPath.Substring(0, position);
            }
            return result;
        }
       

       

        

        private static int GetBrowserVersion(string ver)
        {
            int version = 0;
            int position = ver.IndexOf(".");
            if (position > 0)
            {
                ver = ver.Substring(0, position);
                Int32.TryParse(ver, out version);
            }

            return version;
        }

        

        public static byte[] StreamToByteArray(Stream input)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                input.CopyTo(memoryStream);
                return memoryStream.ToArray();
            }
        }

        public static bool IsStopRequested()
        {
            if (File.Exists(stopFile))
            {
                File.Delete(stopFile);
                return true;
            }
            else
                return false;
        }

        

        public static void WriteLog(string message)
        {
            
            LogHandler.LogDebug(message, LogHandler.Layer.Infrastructure); 
        }
        public struct ImageBgr
        {
            public ImageBgr(double blue, double green, double red)
            {
                Blue = blue;
                Green = green;
                Red = red;
            }

            public double Blue, Green, Red;


        }
    }

    




    public enum DragDestinationType
    {
        AbsolutePosition,
        RelativePosition
    }
}

