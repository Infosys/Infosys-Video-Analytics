/*=============================================================================================================== *
 * Copyright 2024 Infosys Ltd.                                                                                    *
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
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
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
                    Image<Gray, byte> template = null;
                  
                    if (IapwPackage == null)
                        template = new Image<Gray, byte>(filename);
                    else
                    {
                       
                        Stream templateStream = Packaging.ExtractFile(IapwPackage, filename);
                        var bmp = new Bitmap(templateStream);
                                             
                        template = bmp.ToImage<Gray, byte>();

                       
                        Packaging.ClosePackage();
                    }
                    bool backgroundProcessing = false;
                    while ((System.DateTime.Now - startTime).TotalMilliseconds <= timeout * 1000 && elementRectTemp == Rectangle.Empty)
                    {
                        if (searchRegion != null && (Rectangle)searchRegion != Rectangle.Empty)
                        {
                            searchRect = (Rectangle)searchRegion;
                        }
                        Image<Gray, byte> source = null;
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
                                Image<Gray, byte> templateTemp = ResizeTemplate(template, scale);
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
                    Image<Bgr, byte> template = null;
                   
                    if (IapwPackage == null)
                        template = new Image<Bgr, byte>(filename);
                    else
                    {
                       
                        Stream templateStream = Packaging.ExtractFile(IapwPackage, filename);
                        var bmp = new Bitmap(templateStream);
                        template = bmp.ToImage<Bgr, byte>();
                                               
                        Packaging.ClosePackage();
                    }
                    bool backgroundProcessing = false;
                    while ((System.DateTime.Now - startTime).TotalMilliseconds <= timeout * 1000 && elementRectTemp == Rectangle.Empty)
                    {
                        if (searchRegion != null && (Rectangle)searchRegion != Rectangle.Empty)
                        {
                            searchRect = (Rectangle)searchRegion;
                        }
                        Image<Bgr, byte> source = null;
                        if (sourceImageToMatch != null)
                        {
                            var bmp = new Bitmap(sourceImageToMatch);
                            source = bmp.ToImage<Bgr, byte>();
                            backgroundProcessing = true;
                        }
                       
                        if (multipleScaleMatching)
                        {
                            int direction = 1;
                            for (int i = 0; i <= MaxScaleSteps; i++)
                            {
                               
                                double scale = 1 + direction * i * ScaleStep;
                                Image<Bgr, byte> templateTemp = ResizeTemplateInTrueColor(template, scale);
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
            Image<Bgr,byte> sourceMatches=null;
            angle=0;
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
                   
                    while ((forever && currentCount == 0) || (System.DateTime.Now - startTime).TotalMilliseconds <= timeout * 1000 || (rects.Count - currentCount > 0))
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
                        if(multipleScaleMatching && multiRotationMatching) {
                           
                            int direction=1;
                            bool noMatchAboveConfidenceScore=false; 
                            int countBoxes=0;
                          
                            for(int i=0;i<=MaxScaleSteps;i++) {
                             
                                try {
                                   
                                    double scale=1+direction*i*ScaleStep;
                                    Image<Gray,byte> templateTemp=ResizeTemplate(template,scale);
                                    if(templateTemp!=null) {
                                        
                                        for(double j=0;j<=180;j+=RotationStep) {
                                            
                                            Image<Gray,byte> imageTemp=RotateImage(source,j);
                                            if(imageTemp!=null) {
                                               
                                                elementRectTemp=FindRectangles(imageTemp,templateTemp,confidence);
                                                if(elementRectTemp?.Count()>0) {
                                                    /* int countBoxes=0; */
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
                                           
                                            Image<Gray,byte> imageTemp=RotateImage(source,j);
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
                                /* });
                                tasks.Add(t); */
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
                        else if(multiRotationMatching) {
                           
                            bool noMatchAboveConfidenceScore=false;
                            int direction=1;
                            int countBoxes=0;
                            for(double i=0;i<=180;i+=RotationStep) {
                                try {
                                   
                                    Image<Gray,byte> imageTemp=RotateImage(source,i);
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
                    templateMatchMapScreen = sourceMatches.ToJpegData();
            }
            return rects;
        }

       
        public static List<TemplateMatching> FindAllInstancesInTrueColor(string filename,out double angle,int timeout=DEFAULT_TIMEOUT_PERINSTANCE,double confidence=80,bool multipleScaleMatching=true,bool multiRotationMatching=true,Stream sourceImageToMatch=null,bool enableTemplateMatchMap=false) {
            /* Before
            List<Rectangle> rects=new List<Rectangle>();
            After */
            List<TemplateMatching> rects=new List<TemplateMatching>();
            List<Point[]> boxes=new List<Point[]>();
            int currentCount=0;
            bool forever=false;
            /* Before
            Rectangle[] elementRectTemp;
            After */
            TemplateMatching[] elementRectTemp;
            Image<Bgr,byte> sourceMatches=null;
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
                        if(multipleScaleMatching && multiRotationMatching) {
                            
                            int direction=1;
                            bool noMatchAboveConfidenceScore=false; 
                            int countBoxes=0;
                         
                            for(int i=0;i<=MaxScaleSteps;i++) {
                                
                                try {
                                   
                                    double scale=1+direction*i*ScaleStep;
                                    Image<Bgr,byte> templateTemp=ResizeTemplateInTrueColor(template,scale);
                                    if(templateTemp!=null) {
                                       
                                        for(double j=0;j<=180;j+=RotationStep) {
                                           
                                            Image<Bgr,byte> imageTemp=RotateImageInTrueColor(source,j);
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
                                    /* Then scale down and verify for confidence */
                                    scale=1+(-direction)*i*ScaleStep;
                                    templateTemp=ResizeTemplateInTrueColor(template,scale);
                                    if(templateTemp!=null) {
                                        /* Test purpose only start
                                        templateTemp.Save(@"d:\images\templateTempDown_"+System.DateTime.Now.Ticks+".jpg");
                                        Test purpose only end */
                                        for(double j=0;j<=180;j+=RotationStep) {
                                            /* Rotate clockwise and verify for confidence */
                                            Image<Bgr,byte> imageTemp=RotateImageInTrueColor(source,j);
                                            if(imageTemp!=null) {
                                                /* Test purpose only start
                                                imageTemp.Save(@"C:\Users\riya.sharma03\Downloads\outputvideos\"+System.DateTime.Now.Ticks+".jpg");
                                                Test purpose only end */
                                                elementRectTemp=FindRectanglesInTrueColor(imageTemp,templateTemp,confidence);
                                                if(elementRectTemp?.Count()>0) {
                                                    /* int countBoxes=0; */
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
                                                    /* tcs.SetResult(true); */
                                                }
                                            }
                                            /* Then rotate anticlockwise and verify for confidence */
                                            imageTemp=RotateImageInTrueColor(source,-i);
                                            if(imageTemp!=null) {
                                                /* Test purpose only start
                                                imageTemp.Save(@"C:\Users\riya.sharma03\Downloads\outputvideos\"+System.DateTime.Now.Ticks+".jpg");
                                                Test purpose only end */
                                                elementRectTemp=FindRectanglesInTrueColor(imageTemp,templateTemp,confidence);
                                                if(elementRectTemp?.Count()>0) {
                                                    /* int countBoxes=0; */
                                                    for(int icount=0;icount<elementRectTemp.Count();icount++) {
                                                        if(elementRectTemp[icount]?.BoundingBox.Height>0 && elementRectTemp[icount]?.BoundingBox.Width>0) {
                                                            rects.Add(elementRectTemp[icount]);
                                                            /* boxes.Add(RectToBox(elementRectTemp[icount])); */
                                                            var boundingBox=RectToBox(elementRectTemp[icount].BoundingBox);
                                                            boxes.Add(boundingBox);
                                                            countBoxes++;
                                                        }
                                                    }
                                                    if(countBoxes>0) {
                                                        angle=-j;
                                                        break;
                                                    }
                                                    /* tcs.SetResult(true); */
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
                                /* });
                                tasks.Add(t); */
                            }
                            /* Task.WhenAny(tcs.Task,Task.WhenAll(tasks)).Wait(); */
                            if(countBoxes==0) {
                                noMatchAboveConfidenceScore=true;
                            }
                            if(countBoxes>0 || noMatchAboveConfidenceScore)
                                break; /* Break if any template matches have been found at the given resolution */
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
                            /* Scale up or down the template */
                            bool noMatchAboveConfidenceScore=false;
                            int direction=1;
                            int countBoxes=0;
                            for(double i=0;i<=180;i+=RotationStep) {
                                try {
                                    /* Rotate clockwise and verify for confidence */
                                    Image<Bgr,byte> imageTemp=RotateImageInTrueColor(source,i);
                                    if(imageTemp!=null) {
                                        /* Test purpose only start
                                        imageTemp.Save(@"C:\Users\riya.sharma03\Downloads\outputvideos\"+System.DateTime.Now.Ticks+".jpg");
                                        Test purpose only end */
                                        elementRectTemp=FindRectanglesInTrueColor(imageTemp,template,confidence);
                                        if(elementRectTemp?.Count()>0) {
                                            /* int countBoxes=0; */
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
                                            /* tcs.SetResult(true); */
                                        }
                                    }
                                    /* Then rotate anticlockwise and verify for confidence */
                                    imageTemp=RotateImageInTrueColor(source,-i);
                                    if(imageTemp!=null) {
                                        /* Test purpose only start
                                        imageTemp.Save(@"C:\Users\riya.sharma03\Downloads\outputvideos\"+System.DateTime.Now.Ticks+".jpg");
                                        Test purpose only end */
                                        elementRectTemp=FindRectanglesInTrueColor(imageTemp,template,confidence);
                                        if(elementRectTemp?.Count()>0) {
                                            /* int countBoxes=0; */
                                            for(int icount=0;icount<elementRectTemp.Count();icount++) {
                                                if(elementRectTemp[icount]?.BoundingBox.Height>0 && elementRectTemp[icount]?.BoundingBox.Width>0) {
                                                    rects.Add(elementRectTemp[icount]);
                                                    /* boxes.Add(RectToBox(elementRectTemp[icount])); */
                                                    var boundingBox=RectToBox(elementRectTemp[icount].BoundingBox);
                                                    boxes.Add(boundingBox);
                                                    countBoxes++;
                                                }
                                            }
                                            if(countBoxes>0) {
                                                angle=-i;
                                                break;
                                            }
                                            /* tcs.SetResult(true); */
                                        }
                                    }
                                }
                                catch(Exception ex) {
                                    LogHandler.LogError(ex.ToString(),LogHandler.Layer.ComputerVision);
                                }
                            }
                            /* Task.WhenAny(tcs.Task,Task.WhenAll(tasks)).Wait(); */
                            if(countBoxes==0) {
                                noMatchAboveConfidenceScore=true;
                            }
                            if(countBoxes>0 || noMatchAboveConfidenceScore)
                                break; /* Break if any template matches have been found at the given resolution */
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


       


        public static void SaveImageGray(Image<Gray, byte> image, string filePathToSave)
        {

            image.Save(filePathToSave);

        }
        public static void SaveImageTrueColor(Image<Bgr, byte> image, string filePathToSave)
        {
            image.Save(filePathToSave);
        }

       

       
        private static Image<Gray, byte> ResizeTemplate(Image<Gray, byte> template, double scale)
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

        private static Image<Bgr, byte> ResizeTemplateInTrueColor(Image<Bgr, byte> template, double scale)
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

        private static Image<Bgr,byte> RotateImageInTrueColor(Image<Bgr,byte> image,double angle) {
            Image<Bgr,byte> rotatedImage=null;
            try {
               
                rotatedImage=image.Rotate(angle,new Bgr(255,255,255),false);
            }
            catch {
               
            }
            return rotatedImage;
        }

        private static Image<Gray,byte> RotateImage(Image<Gray,byte> image,double angle) {
            Image<Gray,byte> rotatedImage=null;
            try {
               
                rotatedImage=image.Rotate(angle,new Gray(1),false);
            }
            catch {
             
            }
            return rotatedImage;
        }

        private static Rectangle FindRectangle(Image<Gray, byte> source, Image<Gray, byte> template, double confidence)
        {
            Rectangle rect = Rectangle.Empty;
            try
            {
                confidence = confidence / 100; 
                using (Image<Gray, float> result = source.MatchTemplate(template, TemplateMatchingType.CcoeffNormed))
                {
                    double[] minValues, maxValues;
                    Point[] minLocations, maxLocations;
                    result.MinMax(out minValues, out maxValues, out minLocations, out maxLocations);

                    if ((maxValues[0] <= 1.0) && (maxValues[0] >= confidence))
                    {    
                        rect = new Rectangle(maxLocations[0].X, maxLocations[0].Y, template.Size.Width, template.Size.Height);

                    }
                }
            }
            catch (Exception ex)
            {
              
                Exception ex1 = ex;
            }
            return rect;
        }

       
        private static TemplateMatching[] FindRectangles(Image<Gray, byte> source, Image<Gray, byte> template, double confidence)
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
        private static Rectangle FindRectangleInTrueColor(Image<Bgr, byte> source, Image<Bgr, byte> template, double confidence)
        {
            Rectangle rect = Rectangle.Empty;
            try
            {
                confidence = confidence / 100; 
                using (Image<Gray, float> result = source.MatchTemplate(template, TemplateMatchingType.CcoeffNormed))
                {
                    double[] minValues, maxValues;
                    Point[] minLocations, maxLocations;
                    result.MinMax(out minValues, out maxValues, out minLocations, out maxLocations);
                    if ((maxValues[0] <= 1.0) && (maxValues[0] >= confidence))
                    {   
                        rect = new Rectangle(maxLocations[0].X, maxLocations[0].Y, template.Size.Width, template.Size.Height);
                    }
                }
            }
            catch
            {
               
            }
            return rect;
        }
      
        private static TemplateMatching[] FindRectanglesInTrueColor(Image<Bgr, byte> source, Image<Bgr, byte> template, double confidence)
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

