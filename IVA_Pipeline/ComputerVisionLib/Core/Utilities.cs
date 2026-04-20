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

using OpenCvSharp;

using Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.Common;
using System.Diagnostics;

using System.ComponentModel;
using System.Reflection;
using System.IO;
using Microsoft.Practices.EnterpriseLibrary.Common.Utility;
using Unity.Interception.Utilities;

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

        
        private static Rect firstControl = new Rect();
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
            Rect rect = new Rect();
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
                        if (rect.Width > 0 || rect.Height > 0)
                        {
                            imageRef.CurrentState = item.State;
                            imageRef.CurrentBoundingRectangle = rect;
                            break;
                        }
                    }
                }
                while (requestedWaitforeverFlag && imageRef.CurrentBoundingRectangle.Width == 0 && imageRef.CurrentBoundingRectangle.Height == 0);
            }

            return imageRef;
        }

       
        public static Rect FindElement(string filename, int timeout = DEFAULT_TIMEOUT, double confidence = 80, bool multipleScaleMatching = true, object searchRegion = null, Stream sourceImageToMatch = null)
        {
            if (Core.Utilities.IsStopRequested())
                throw new Core.CVExceptions.StopRequested();


            Rect elementRectTemp = new Rect();
            Rect searchRect = new Rect();
            DateTime startTime = DateTime.Now;
            try
            {
                if (timeout <= 0)
                    timeout = DEFAULT_TIMEOUT; 
                if (System.IO.File.Exists(filename) || (IapwPackage != null && IapwPackage.Length > 0))
                {
                    Mat template = null;
                   
                    if (IapwPackage == null)
                        template = Cv2.ImRead(filename, ImreadModes.Grayscale);
                    else
                    {
                       
                        Stream templateStream = Packaging.ExtractFile(IapwPackage, filename);
                        using (var ms = new MemoryStream()) { templateStream.CopyTo(ms); template = Cv2.ImDecode(ms.ToArray(), ImreadModes.Grayscale); }

                       
                        Packaging.ClosePackage();
                    }
                    bool backgroundProcessing = false;
                    while ((System.DateTime.Now - startTime).TotalMilliseconds <= timeout * 1000 && elementRectTemp.Width == 0 && elementRectTemp.Height == 0)
                    {
                        if (searchRegion != null && ((Rect)searchRegion).Width > 0)
                        {
                            searchRect = (Rect)searchRegion;
                        }
                        Mat source = null;
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
                                    if (elementRectTemp.Width > 0 || elementRectTemp.Height > 0)
                                    {
                                        break;
                                    }
                                }

                                
                                scale = 1 + (-direction) * i * ScaleStep;
                                templateTemp = ResizeTemplate(template, scale);
                                if (templateTemp != null)
                                {
                                    elementRectTemp = FindRectangle(source, templateTemp, confidence);
                                    if (elementRectTemp.Width > 0 || elementRectTemp.Height > 0)
                                    {
                                        break;
                                    }
                                }
                            }
                        }
                        else
                        {
                            elementRectTemp = FindRectangle(source, template, confidence);
                            if (elementRectTemp.Width > 0 || elementRectTemp.Height > 0)
                            {
                                break;
                            }
                        }
                        System.Threading.Thread.Sleep(THREAD_SLEEP_DURATION);
                    }
                }
                else
                    throw new System.IO.FileNotFoundException("Image Template " + filename + " not found");
                if (elementRectTemp.Width == 0 && elementRectTemp.Height == 0)
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

            
            if (searchRegion != null && (searchRect.Width > 0 || searchRect.Height > 0))
            {
                elementRectTemp.X += searchRect.X;
                elementRectTemp.Y += searchRect.Y;
            }
            return elementRectTemp;
        }

        
        public static Rect FindElementInTrueColor(string filename, int timeout = DEFAULT_TIMEOUT, double confidence = 80, bool multipleScaleMatching = true, object searchRegion = null, Stream sourceImageToMatch = null)
        {
            
            if (Core.Utilities.IsStopRequested())
                throw new CVExceptions.StopRequested();

            Rect elementRectTemp = new Rect();
            Rect searchRect = new Rect();
            DateTime startTime = DateTime.Now;
            try
            {
                if (timeout <= 0)
                    timeout = DEFAULT_TIMEOUT; 
                if (System.IO.File.Exists(filename) || (IapwPackage != null && IapwPackage.Length > 0))
                {
                    Mat template = null;
                    
                    if (IapwPackage == null)
                        template = Cv2.ImRead(filename, ImreadModes.Color);
                    else
                    {
                       
                        Stream templateStream = Packaging.ExtractFile(IapwPackage, filename);
                        using (var ms = new MemoryStream()) { templateStream.CopyTo(ms); template = Cv2.ImDecode(ms.ToArray(), ImreadModes.Color); }
                                               
                        Packaging.ClosePackage();
                    }
                    bool backgroundProcessing = false;
                    while ((System.DateTime.Now - startTime).TotalMilliseconds <= timeout * 1000 && elementRectTemp.Width == 0 && elementRectTemp.Height == 0)
                    {
                        if (searchRegion != null && ((Rect)searchRegion).Width > 0)
                        {
                            searchRect = (Rect)searchRegion;
                        }
                        Mat source = null;
                        if (sourceImageToMatch != null)
                        {
                            sourceImageToMatch.Position = 0;
                            using (var ms = new MemoryStream()) { sourceImageToMatch.CopyTo(ms); source = Cv2.ImDecode(ms.ToArray(), ImreadModes.Color); }
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
                                    if (elementRectTemp.Width > 0 || elementRectTemp.Height > 0)
                                    {
                                        break;
                                    }
                                }

                                
                                scale = 1 + (-direction) * i * ScaleStep;
                                templateTemp = ResizeTemplateInTrueColor(template, scale);
                                if (templateTemp != null)
                                {
                                    elementRectTemp = FindRectangleInTrueColor(source, templateTemp, confidence);
                                    if (elementRectTemp.Width > 0 || elementRectTemp.Height > 0)
                                    {
                                        break;
                                    }
                                }
                            }
                        }
                        else
                        {
                            elementRectTemp = FindRectangleInTrueColor(source, template, confidence);
                            if (elementRectTemp.Width > 0 || elementRectTemp.Height > 0)
                            {
                                break;
                            }
                        }
                        System.Threading.Thread.Sleep(THREAD_SLEEP_DURATION);
                    }
                }
                else
                    throw new System.IO.FileNotFoundException("Image Template " + filename + " not found");

                if (elementRectTemp.Width == 0 && elementRectTemp.Height == 0)
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

           
            if (searchRegion != null && (searchRect.Width > 0 || searchRect.Height > 0))
            {
                elementRectTemp.X += searchRect.X;
                elementRectTemp.Y += searchRect.Y;
            }
            return elementRectTemp;
        }

        public static Rect WaitForElement(string filename, bool templateMachingInOriginalScale = false, bool useTrueColorTemplateMatching = false, object searchRegion = null, Stream sourceImageToMatch = null)
        {
            Rect elementRectTemp = new Rect();
            while (elementRectTemp.Width == 0 && elementRectTemp.Height == 0)
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
            Mat sourceMatches=null;
            angle=0;
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
                   
                    while ((forever && currentCount == 0) || (System.DateTime.Now - startTime).TotalMilliseconds <= timeout * 1000 || (rects.Count - currentCount > 0))
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
                {
                    Cv2.ImEncode(".jpg", sourceMatches, out byte[] encoded);
                    templateMatchMapScreen = encoded;
                }
            }
            return rects;
        }

       
        public static List<TemplateMatching> FindAllInstancesInTrueColor(string filename,out double angle,int timeout=DEFAULT_TIMEOUT_PERINSTANCE,double confidence=80,bool multipleScaleMatching=true,bool multiRotationMatching=true,Stream sourceImageToMatch=null,bool enableTemplateMatchMap=false) {
            
            List<TemplateMatching> rects=new List<TemplateMatching>();
            List<Point[]> boxes=new List<Point[]>();
            int currentCount=0;
            bool forever=false;
            
            TemplateMatching[] elementRectTemp;
            Mat sourceMatches=null;
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
                {
                    Cv2.ImEncode(".jpg", sourceMatches, out byte[] encoded);
                    templateMatchMapScreen = encoded;
                }
            }
            return rects;
        }


       


        public static void SaveImageGray(Mat image, string filePathToSave)
        {
            Cv2.ImWrite(filePathToSave, image);
        }
        public static void SaveImageTrueColor(Mat image, string filePathToSave)
        {
            Cv2.ImWrite(filePathToSave, image);
        }

       

       

        private static Mat ResizeTemplate(Mat template, double scale)
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
                resizedTemplate = new Mat();
                Cv2.Resize(template, resizedTemplate, new OpenCvSharp.Size(0, 0), scale, scale, InterpolationFlags.Lanczos4);
            }
            catch
            {
                
            }
            return resizedTemplate;
        }

        private static Mat RotateImageInTrueColor(Mat image,double angle) {
            Mat rotatedImage=null;
            try {
                OpenCvSharp.Point2f center = new OpenCvSharp.Point2f(image.Width / 2f, image.Height / 2f);
                Mat rotMat = Cv2.GetRotationMatrix2D(center, angle, 1.0);
                rotatedImage = new Mat();
                Cv2.WarpAffine(image, rotatedImage, rotMat, image.Size(), InterpolationFlags.Linear, BorderTypes.Constant, new Scalar(255,255,255));
            }
            catch {
                
            }
            return rotatedImage;
        }

        private static Mat RotateImage(Mat image,double angle) {
            Mat rotatedImage=null;
            try {
                OpenCvSharp.Point2f center = new OpenCvSharp.Point2f(image.Width / 2f, image.Height / 2f);
                Mat rotMat = Cv2.GetRotationMatrix2D(center, angle, 1.0);
                rotatedImage = new Mat();
                Cv2.WarpAffine(image, rotatedImage, rotMat, image.Size(), InterpolationFlags.Linear, BorderTypes.Constant, new Scalar(1));
            }
            catch {
               
            }
            return rotatedImage;
        }

        private static Rect FindRectangle(Mat source, Mat template, double confidence)
        {
            Rect rect = new Rect();
            try
            {
                confidence = confidence / 100; 
                using (Mat result = new Mat())
                {
                    Cv2.MatchTemplate(source, template, result, TemplateMatchModes.CCoeffNormed);
                    Cv2.MinMaxLoc(result, out double minValue, out double maxValue, out Point minLocation, out Point maxLocation);

                    if ((maxValue <= 1.0) && (maxValue >= confidence))
                    {    
                        
                        rect = new Rect(maxLocation.X, maxLocation.Y, template.Width, template.Height);

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
                
                using (Mat result = new Mat())
                {
                    Cv2.MatchTemplate(source, template, result, TemplateMatchModes.CCoeffNormed);
                    Cv2.MinMaxLoc(result, out double minValue, out double maxValue, out Point minLocation, out Point maxLocation);
                    double[] maxValues = new[] { maxValue };
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
        private static Rect FindRectangleInTrueColor(Mat source, Mat template, double confidence)
        {
            Rect rect = new Rect();
            try
            {
                confidence = confidence / 100; 
                using (Mat result = new Mat())
                {
                    Cv2.MatchTemplate(source, template, result, TemplateMatchModes.CCoeffNormed);
                    Cv2.MinMaxLoc(result, out double minValue, out double maxValue, out Point minLocation, out Point maxLocation);
                    if ((maxValue <= 1.0) && (maxValue >= confidence))
                    {   
                        rect = new Rect(maxLocation.X, maxLocation.Y, template.Width, template.Height);
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
                using (Mat result = new Mat())
                {
                    Cv2.MatchTemplate(source, template, result, TemplateMatchModes.CCoeffNormed);
                    Cv2.MinMaxLoc(result, out double minValue, out double maxValue, out Point minLocation, out Point maxLocation);
                    double[] maxValues = new[] { maxValue };
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

        private static Point[] RectToBox(Rect rectangle)
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

