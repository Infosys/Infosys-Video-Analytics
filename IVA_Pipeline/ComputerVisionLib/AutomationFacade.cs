/*=============================================================================================================== *
 * Copyright 2025 Infosys Ltd.                                                                                    *
 * Use of this source code is governed by Apache License Version 2.0 that can be found in the LICENSE file or at  *
 * http://www.apache.org/licenses/                                                                                *
 * ===============================================================================================================*/
﻿using System;
using Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.Common;
using System.Net;
using System.IO;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using Infosys.IVA.ComputerVisionLib.OCREngine;
using ComputerVisionLib_Core = Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.ComputerVisionLib.Core;
using Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.ComputerVisionLib.Core;
using NLog.Fluent;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using System.Security.Cryptography;
using System.Security.Policy;

namespace Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.ComputerVisionLib
{
    public class AutomationFacade {
        int numOfTrials=3;
        int timeGapBetweenTrials=300; 
        bool highlightElement=false; 
        int timeoutInSec=10; 
        bool templateMachingInOriginalScale=false; 
        bool waitForeverForTemplate=false; 
        bool showWaitBox=true; 
        bool useTrueColorTemplateMatching=false; 
        string firstAppToStart="";
        bool getAllMatchingControls=false;
        private string className="AutomationFacade";
        Control parentControl=null; 
        string iapPackageTypes=".iapw,.iapd"; 
        private int InteractiveCheckExists=-1;
        bool findControlInMultipleControlStates=false; 
        int imageMatchConfidenceThreshold=80; 
        Stream pSourceImage=null; 
        private bool multiRotationTemplateMatching=false;
       


        
        public string ATRFileDirectory { get; set; }


        
        public AutomationFacade(System.Xml.XmlDocument automationConfigXML, bool LaunchApps, bool showAppStartingWaitBox = true, string firstApplicationToStart = "", bool highLightControl = false, bool multipleScaleTemplateMatching = true, bool waitForeverForImageTemplate = false)
        {
            ComputerVisionLib_Core.Utilities.SetDLLsPath();
            highlightElement = highLightControl;
            templateMachingInOriginalScale = !multipleScaleTemplateMatching;
            waitForeverForTemplate = waitForeverForImageTemplate;
            showWaitBox = showAppStartingWaitBox;
            firstAppToStart = firstApplicationToStart;

            

            using (LogHandler.TraceOperations(Logging.InformationMessages.RUNTIMEWRAPPER_ENTER, LogHandler.Layer.Business, Guid.Empty, className, Logging.Constants.PARAMDIRECTION_IN, automationConfigXML.Name))
            {

                LogHandler.LogInfo(Logging.InformationMessages.RUNTIMEWRAPPER_PARAMETERS, LogHandler.Layer.Business, "LaunchApps", Logging.Constants.PARAMDIRECTION_IN, LaunchApps.ToString());

                
                if (automationConfigXML != null)
                {
                    string xmlString = ComputerVisionLib_Core.Translation.XMLDocAsString(automationConfigXML);
                    if (!string.IsNullOrEmpty(xmlString))
                        TranslateAutomationConfig(xmlString, LaunchApps);
                }
            }
            LogHandler.LogInfo(Logging.InformationMessages.RUNTIMEWRAPPER_EXIT, LogHandler.Layer.Business, className, Logging.Constants.AUTOMATIONFACADE);
        }

        
        public AutomationFacade(string xMLPath, bool LaunchApps, bool showAppStartingWaitBox = true, string firstApplicationToStart = "", bool highLightControl = false, bool multipleScaleTemplateMatching = true, bool waitForeverForImageTemplate = false)
        {
            

            highlightElement = highLightControl;
            templateMachingInOriginalScale = !multipleScaleTemplateMatching;
            waitForeverForTemplate = waitForeverForImageTemplate;
            showWaitBox = showAppStartingWaitBox;
            firstAppToStart = firstApplicationToStart;
            string typeOfIAPPackage = "";
            
            using (LogHandler.TraceOperations(Logging.InformationMessages.RUNTIMEWRAPPER_ENTER, LogHandler.Layer.Business, Guid.Empty, className, Logging.Constants.AUTOMATIONFACADE))
            {
                
                
                ComputerVisionLib_Core.Utilities.ClearCache();

                LogHandler.LogInfo(Logging.InformationMessages.RUNTIMEWRAPPER_PARAMETERS, LogHandler.Layer.Business, "xMLPath", Logging.Constants.PARAMDIRECTION_IN, xMLPath);

                LogHandler.LogInfo(Logging.InformationMessages.RUNTIMEWRAPPER_PARAMETERS, LogHandler.Layer.Business, "LaunchApps", Logging.Constants.PARAMDIRECTION_IN, LaunchApps.ToString());

                
                PopulateRetrySettings();

                bool xmlpathFound = false;
                string xmlString = "";

                

                if (IsValidUrlFormat(xMLPath.Replace("$", "dollar")))
                {

                    #region next phase according to sid
                    
                    #endregion

                }

                else if (!string.IsNullOrEmpty(xMLPath) && !string.IsNullOrEmpty(typeOfIAPPackage = GetValidPackage(xMLPath)))
                {
                    xmlpathFound = true;
                    
                    string iapPackageLoc = xMLPath.Substring(0, xMLPath.IndexOf(typeOfIAPPackage) + typeOfIAPPackage.Length);
                    string atrLoc = xMLPath.Replace(xMLPath, iapPackageLoc).Replace(@"/", "\\");

                    
                    using (FileStream fileStream = new FileStream(iapPackageLoc, FileMode.Open, FileAccess.Read))
                    {
                        
                        ComputerVisionLib_Core.Utilities.IapwPackage = fileStream;

                        

                        ATRFileDirectory = System.IO.Path.GetDirectoryName(atrLoc).Substring(1);
                        if (fileStream != null && fileStream.Length > 0)
                        {
                            Stream atr = Packaging.ExtractFile(fileStream, atrLoc);
                            xmlString = StreamToString(atr);
                            Packaging.ClosePackage();
                        }



                        
                    }
                }

                else if (!string.IsNullOrEmpty(xMLPath) && System.IO.File.Exists(xMLPath))
                {
                    xmlpathFound = true;
                }

                else if (!string.IsNullOrEmpty(xMLPath))
                {
                    
                    xMLPath = GetProbableAbsolutePath(xMLPath);
                    if (System.IO.File.Exists(xMLPath))
                        xmlpathFound = true;
                }

               

                if (xmlpathFound && string.IsNullOrEmpty(xmlString))
                {
                    
                    ATRFileDirectory = System.IO.Path.GetDirectoryName(xMLPath);
                    xmlString = System.IO.File.ReadAllText(xMLPath);

                   

                }

                if (!string.IsNullOrEmpty(xmlString))
                {
                   

                    ComputerVisionLib_Core.Utilities.WriteLog("automation initialization started at " + DateTime.Now.ToString()); 
                    TranslateAutomationConfig(xmlString, LaunchApps);
                    ComputerVisionLib_Core.Utilities.WriteLog("automation initialization completed at " + DateTime.Now.ToString()); 

                    
                }
                else
                {
                    throw new System.Exception("Path provided for Automation Config file does not exists");
                }
            }
            LogHandler.LogInfo(Logging.InformationMessages.RUNTIMEWRAPPER_EXIT, LogHandler.Layer.Business, className, Logging.Constants.AUTOMATIONFACADE);
        }

       
        public AutomationFacade()
        {
            ComputerVisionLib_Core.Utilities.SetDLLsPath();
        }

        private Dictionary<string, ComputerVisionLib.Application> applications;
        public Dictionary<string, ComputerVisionLib.Application> Applications
        {
            get { return applications; }
            set { applications = value; }
        }

       
        public ComputerVisionLib.Control FindControl(string canonicalPath, string automationId, string automationName, Stream sourceImage = null)
        {
            getAllMatchingControls = false;
            using (LogHandler.TraceOperations(Logging.InformationMessages.RUNTIMEWRAPPER_ENTER, LogHandler.Layer.Business, Guid.Empty, className, Logging.Constants.FINDCONTROL))
            {
                
                LogHandler.LogInfo(Logging.InformationMessages.RUNTIMEWRAPPER_PARAMETERS, LogHandler.Layer.Business, "canonicalPath", Logging.Constants.PARAMDIRECTION_IN, canonicalPath);
                LogHandler.LogInfo(Logging.InformationMessages.RUNTIMEWRAPPER_PARAMETERS, LogHandler.Layer.Business, "automationId", Logging.Constants.PARAMDIRECTION_IN, automationId);
                LogHandler.LogInfo(Logging.InformationMessages.RUNTIMEWRAPPER_PARAMETERS, LogHandler.Layer.Business, "automationName", Logging.Constants.PARAMDIRECTION_IN, automationName);

                Control ctl = null;
                Application app = null;
                Screen screen = null;
                if (Applications != null)
                {
                    string[] canonicalPathParts = canonicalPath.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
                    if (canonicalPathParts != null && canonicalPathParts.Length >= 2) 
                    {
                        LogHandler.LogInfo(Logging.InformationMessages.RUNTIMEWRAPPER_VARIABLE_VALUE, LogHandler.Layer.Business, "canonicalPathParts", canonicalPathParts.Length.ToString());
                        bool stopTraversing = false;
                        for (int i = 0; i < canonicalPathParts.Length; i++)
                        {
                            if (stopTraversing)
                                break;
                            switch (i)
                            {
                                case 0: 
                                    app = Applications[canonicalPathParts[i]];
                                    break;
                                case 1: 
                                    if (app == null)
                                        stopTraversing = true;
                                    else if (app.Screens != null && app.Screens.ContainsKey(canonicalPathParts[i]))
                                    {
                                        screen = app.Screens[canonicalPathParts[i]];
                                        
                                    }
                                    else 
                                    {
                                        if (ctl == null) 
                                        {
                                            ctl = app.Controls[canonicalPathParts[i]];
                                            ctl.GetAllMatchingControls = false;
                                            
                                        }
                                        else 
                                        {
                                            ctl = ctl.Controls[canonicalPathParts[i]];
                                            ctl.GetAllMatchingControls = false;
                                            
                                        }
                                    }
                                    break;
                                default: 
                                    
                                    if (ctl == null) 
                                    {
                                        ctl = screen.Controls[canonicalPathParts[i]];
                                        ctl.GetAllMatchingControls = false;
                                        
                                    }
                                    else 
                                    {
                                        ctl = ctl.Controls[canonicalPathParts[i]];
                                        ctl.GetAllMatchingControls = false;
                                        
                                    }
                                    break;
                            }
                        }
                    }
                }
                
                if (ctl != null && app != null && screen != null)
                {
                    try
                    {
                        pSourceImage = sourceImage;
                        ctl = RefreshControl(app, screen, ctl);
                    }
                    finally
                    {
                        
                        pSourceImage = null;
                    }
                }

                
                if (highlightElement && ctl != null)
                {
                   
                }

                LogHandler.LogInfo(Logging.InformationMessages.RUNTIMEWRAPPER_EXIT, LogHandler.Layer.Business, className, Logging.Constants.FINDCONTROL);

                return ctl;
            }
        }

        
       

        public ComputerVisionLib.Control FindControl(string canonicalPath, Stream sourceImage = null)
        {
            return FindControl(canonicalPath, true, false, sourceImage);
        }
       

        public ComputerVisionLib.Control FindControl(string canonicalPath, bool allControlIdentifiersMustMatch, bool forceControlRefresh, Stream sourceImage = null)
        {
            getAllMatchingControls = false;
            using (LogHandler.TraceOperations(Logging.InformationMessages.RUNTIMEWRAPPER_ENTER, LogHandler.Layer.Business, Guid.Empty, className, Logging.Constants.FINDCONTROL))
            {
               
                LogHandler.LogInfo(Logging.InformationMessages.RUNTIMEWRAPPER_PARAMETERS, LogHandler.Layer.Business, "canonicalPath", Logging.Constants.PARAMDIRECTION_IN, canonicalPath);

                ComputerVisionLib.Control ctl = null;
                ComputerVisionLib.Application app = null;
                ComputerVisionLib.Screen screen = null;
                if (Applications != null)
                {
                    string[] canonicalPathParts = canonicalPath.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
                    if (canonicalPathParts != null && canonicalPathParts.Length >= 2) 
                    {
                        LogHandler.LogInfo(Logging.InformationMessages.RUNTIMEWRAPPER_VARIABLE_VALUE, LogHandler.Layer.Business, "canonicalPathParts", canonicalPathParts.Length.ToString());
                        bool stopTraversing = false;
                        try
                        {
                            for (int i = 0; i < canonicalPathParts.Length; i++)
                            {
                                if (stopTraversing)
                                    break;
                                switch (i)
                                {
                                    case 0: 
                                        app = Applications[canonicalPathParts[i]];
                                        break;
                                    case 1: 
                                        if (app == null)
                                            stopTraversing = true;
                                        else if (app.Screens != null && app.Screens.ContainsKey(canonicalPathParts[i]))
                                        {
                                            screen = app.Screens[canonicalPathParts[i]];
                                            
                                        }
                                        else 
                                        {
                                            if (ctl == null) 
                                            {
                                                ctl = app.Controls[canonicalPathParts[i]];
                                                ctl.GetAllMatchingControls = false;
                                                
                                            }
                                            else 
                                            {
                                                parentControl = ctl;
                                                ctl = ctl.Controls[canonicalPathParts[i]];
                                                ctl.GetAllMatchingControls = false;
                                                
                                            }
                                        }
                                        break;
                                    default: 
                                       
                                        if (ctl == null) 
                                        {
                                            ctl = screen.Controls[canonicalPathParts[i]];

                                            ctl.GetAllMatchingControls = false;
                                            
                                        }
                                        else 
                                        {
                                            parentControl = ctl;
                                            ctl = ctl.Controls[canonicalPathParts[i]];
                                            ctl.GetAllMatchingControls = false;
                                            
                                        }
                                        break;
                                }
                            }
                        }
                        catch (System.Collections.Generic.KeyNotFoundException ex)
                        {
                            throw new Exception("Incorrect name provided either for Application or Screen or Control in the canonical path.");
                        }
                    }
                }

                if (ctl != null && app != null && screen != null) 
                {
                    try
                    {
                        pSourceImage = sourceImage;
                        ctl = RefreshControl(app, screen, ctl);
                    }
                    finally
                    {
                        
                        pSourceImage = null;
                    }

                }
               

                LogHandler.LogInfo(Logging.InformationMessages.RUNTIMEWRAPPER_EXIT, LogHandler.Layer.Business, className, Logging.Constants.FINDCONTROL);

                return ctl;
            }
        }
       

        public List<ComputerVisionLib.Control> FindControls(string canonicalPath,Stream sourceImage=null) {
            return FindControls(canonicalPath,true,false,sourceImage);
        }

        

        public List<Control> FindControls(string canonicalPath,bool allControlIdentifiersMustMatch,bool forceControlRefresh,Stream sourceImage=null) {
            using(LogHandler.TraceOperations(Logging.InformationMessages.RUNTIMEWRAPPER_ENTER,LogHandler.Layer.Business,Guid.Empty,className,Logging.Constants.FINDCONTROLS)) {
                getAllMatchingControls=true;
               
                LogHandler.LogInfo(Logging.InformationMessages.RUNTIMEWRAPPER_PARAMETERS,LogHandler.Layer.Business,"canonicalPath",Logging.Constants.PARAMDIRECTION_IN,canonicalPath);
                List<Control> ctrls=new List<Control>();
                Control ctl=null;
                Application app=null;
                Screen screen=null;
                if (Applications != null)
                {
                    string[] canonicalPathParts = canonicalPath.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
                    if (canonicalPathParts != null && canonicalPathParts.Length >= 2) 
                    {
                        LogHandler.LogInfo(Logging.InformationMessages.RUNTIMEWRAPPER_VARIABLE_VALUE, LogHandler.Layer.Business, "canonicalPathParts", canonicalPathParts.Length.ToString());
                        bool stopTraversing = false;
                        for (int i = 0; i < canonicalPathParts.Length; i++)
                        {
                            if (stopTraversing)
                                break;
                            switch (i)
                            {
                                case 0: 
                                    app = Applications[canonicalPathParts[i]];
                                    break;
                                case 1: 
                                    if (app == null)
                                        stopTraversing = true;
                                    else if (app.Screens != null && app.Screens.ContainsKey(canonicalPathParts[i]))
                                    {
                                        screen = app.Screens[canonicalPathParts[i]];
                                        
                                    }
                                    else 
                                    {
                                        if (ctl == null) 
                                        {
                                            ctl = app.Controls[canonicalPathParts[i]];

                                            
                                            if (i + 1 == canonicalPathParts.Length)
                                                ctl.GetAllMatchingControls = true;
                                            
                                        }
                                        else 
                                        {
                                            ctl = ctl.Controls[canonicalPathParts[i]];
                                            if (i + 1 == canonicalPathParts.Length)
                                                ctl.GetAllMatchingControls = true;
                                            
                                        }
                                    }
                                    break;
                                default: 
                                   
                                    if (ctl == null) 
                                    {
                                        ctl = screen.Controls[canonicalPathParts[i]];
                                        if (i + 1 == canonicalPathParts.Length)
                                            ctl.GetAllMatchingControls = true;
                                        
                                    }
                                    else 
                                    {
                                        ctl = ctl.Controls[canonicalPathParts[i]];
                                        if (i + 1 == canonicalPathParts.Length)
                                            ctl.GetAllMatchingControls = true;
                                        
                                    }
                                    break;
                            }
                        }
                    }
                }
                if (ctl != null)
                {
                    if (ctl.DiscoveryMode == ElementDiscovertyMode.API || ctl.DiscoveryMode == ElementDiscovertyMode.APIAndImage)
                    {
                        
                        if (app != null && screen != null)
                        {
                            try
                            {
                                pSourceImage = sourceImage;
                                ctl = RefreshControl(app, screen, ctl);
                            }
                            finally
                            {
                                pSourceImage = null;
                            }
                        }
                        else
                        {
                            
                        }
                    }
                    else if (ctrls.Count == 0 && (ctl.DiscoveryMode == ElementDiscovertyMode.Image || ctl.DiscoveryMode == ElementDiscovertyMode.APIAndImage))
                    {
                        if (ctl.ImageReference != null && ctl.ImageReference.SupportedStates != null && ctl.ImageReference.SupportedStates.Count > 0)
                        {
                            

                            

                            foreach (var ct in ctl.ImageReference.SupportedStates)
                                    {
                                
                                        try
                                {
                                           
                                            var tempControl=FindControls(ct.ImagePath,true,sourceImage);
                                            if (tempControl != null && tempControl.Count > 0)
                                            {
                                                
                                                tempControl.ForEach(c1 =>
                                            {
                                                c1.ImageReference.CurrentState = ct.State;
                                            });
                                                for (int iCount = 0; iCount <= tempControl.Count - 1; iCount++)
                                                {
                                                    ctrls.Add(tempControl[iCount]);
                                                }
                                        if (!findControlInMultipleControlStates)
                                            break;
                                            }
                                           
                                        }
                                        catch (Exception ex)
                                        {
                                            
                                        }
                                    }
                            


                           

                        }
                    }
                }

                LogHandler.LogInfo(Logging.InformationMessages.RUNTIMEWRAPPER_EXIT, LogHandler.Layer.Business, className, Logging.Constants.FINDCONTROLS);

                return ctrls;
            }
        }

        
        
        private List<Control> FindControlsInParallel(Control ctl,Stream sourceImage) {
            List<Control> ctrls=new List<Control>();
            foreach(var item in ctl.ImageReference.SupportedStates) {
                /* Sid: 25 Dec 2017 - Updated logic to load all identified control in one state to a tempControl list
                and then load the controls instance. This fixes a bug in which controls identified across states were 
                overwriting the controls identified in previous state when the FindControlInMultipleControlStates property was
                set to true. */
                List<Control> tempControl=null;
                tempControl=FindControls(item.ImagePath,true,sourceImage);
                if(tempControl!=null && tempControl.Count>0) {
                    
                    tempControl.ForEach(c=> {
                        c.ImageReference.CurrentState=item.State;
                    });
                    for(int iCount=0;iCount<=tempControl.Count-1;iCount++) {
                        ctrls.Add(tempControl[iCount]);
                    }
                    if(!findControlInMultipleControlStates)
                        break;
                }
            }
            return ctrls;
        }

       
        public Control FindControl(string elementImagePath, bool imageRecog, Stream sourceImage = null)
        {
            getAllMatchingControls = false;
            LogHandler.TraceOperations(Logging.InformationMessages.RUNTIMEWRAPPER_ENTER, LogHandler.Layer.Business, Guid.Empty, className, Logging.Constants.FINDCONTROL);
            LogHandler.LogInfo(Logging.InformationMessages.RUNTIMEWRAPPER_PARAMETERS, LogHandler.Layer.Business, "elementImagePath", Logging.Constants.PARAMDIRECTION_IN, elementImagePath);
            Control ctl = null;
            if (!string.IsNullOrEmpty(elementImagePath) && File.Exists(elementImagePath))
            {
               
                ctl = new Control();
                ctl.ImageReference = new ControlImageReference();
                ctl.ImageReference.SupportedStates = new List<ControlStateReference>();

                ControlStateReference state = new ControlStateReference();
                state.ImagePath = elementImagePath;
                state.State = "";

                ctl.ImageReference.SupportedStates.Add(state);
                Core.Utilities.ScaleStep = imageMatchScaleStepSize;
                Core.Utilities.MaxScaleSteps = imageMatchMaxScaleStepCount;
                ctl.ImageReference = Core.Utilities.GetBoundingRectangle(ctl.ImageReference, templateMachingInOriginalScale, waitForeverForTemplate, useTrueColorTemplateMatching, null, ATRFileDirectory, sourceImage);
                ctl.ImageBoundingRectangle = ctl.ImageReference.CurrentBoundingRectangle;

            }
            else
                LogHandler.LogError(Logging.InformationMessages.RUNTIMEWRAPPER_INVALID_DATA, LogHandler.Layer.Business, "Element image path", elementImagePath);

           

            LogHandler.LogInfo(Logging.InformationMessages.RUNTIMEWRAPPER_EXIT, LogHandler.Layer.Business, className, Logging.Constants.FINDCONTROL);
            return ctl;
        }

       
        public List<Control> FindControls(string elementImagePath,bool ForImagePath,Stream sourceImage=null) {
            LogHandler.TraceOperations(Logging.InformationMessages.RUNTIMEWRAPPER_ENTER,LogHandler.Layer.Business,Guid.Empty,className,Logging.Constants.FINDCONTROLS);
            LogHandler.LogInfo(Logging.InformationMessages.RUNTIMEWRAPPER_PARAMETERS,LogHandler.Layer.Business,"elementImagePath",Logging.Constants.PARAMDIRECTION_IN,elementImagePath);
            int timeout=1000;
           
            double angle=0;
            if(WaitForever)
                timeout=-1;
            List<Control> controls=null;
            if(File.Exists(elementImagePath)) {
                controls=new List<Control>();
               
                List<TemplateMatching> rects=new List<TemplateMatching>();
                
                Core.Utilities.ScaleStep=imageMatchScaleStepSize;
                Core.Utilities.MaxScaleSteps=imageMatchMaxScaleStepCount;
                Core.Utilities.TemplateMatchMapBorderColor=templateMatchMappingBorderColor;
                Core.Utilities.TemplateMatchMapBorderThickness=templateMatchMappingBorderThickness;
                Core.Utilities.RotationStep=imageMatchRotationStepAngle;
               
                if(useTrueColorTemplateMatching)
                    
                    rects=Core.Utilities.FindAllInstancesInTrueColor(elementImagePath,out angle,timeout,imageMatchConfidenceThreshold,!templateMachingInOriginalScale,multiRotationTemplateMatching,sourceImage,enableTemplateMatchMapping);
                   
                else
                    rects=Core.Utilities.FindAllInstances(elementImagePath,out angle,timeout,imageMatchConfidenceThreshold,!templateMachingInOriginalScale,multiRotationTemplateMatching,sourceImage,enableTemplateMatchMapping);

                if(rects!=null && rects.Count>0) {
                    rects.ForEach(rect=> {
                        Control ctl=new Control();
                        ctl.ImageReference=new ControlImageReference();
                        ctl.ImageReference.SupportedStates=new List<ControlStateReference>();
                        ControlStateReference state=new ControlStateReference();
                        state.ImagePath=elementImagePath;
                        state.State="";
                        ctl.ImageReference.SupportedStates.Add(state);
                        ctl.ImageReference.CurrentBoundingRectangle=rect.BoundingBox;
                        ctl.ImageBoundingRectangle=rect.BoundingBox;
                        ctl.ImageReference.ConfidenceScore=rect.ConfidenceScore;
                        ctl.ImageReference.Angle=angle;
                        controls.Add(ctl);
                    });
                }
            }
            else
                LogHandler.LogError(Logging.InformationMessages.RUNTIMEWRAPPER_INVALID_DATA,LogHandler.Layer.Business,"element image path",elementImagePath);
            LogHandler.LogInfo(Logging.InformationMessages.RUNTIMEWRAPPER_EXIT,LogHandler.Layer.Business,className,Logging.Constants.FINDCONTROLS);
            return controls;
        }


      
        public Application FindApplication(string name, string windowsTitle = "", int timeOut = 0)
        {
            Application app = null;

            using (LogHandler.TraceOperations(Logging.InformationMessages.RUNTIMEWRAPPER_ENTER, LogHandler.Layer.Business, Guid.Empty, className, Logging.Constants.FIND_APPLICATION))
            {
                LogHandler.LogInfo(Logging.InformationMessages.RUNTIMEWRAPPER_PARAMETERS, LogHandler.Layer.Business, "application name", Logging.Constants.PARAMDIRECTION_IN, name);
                if (!string.IsNullOrEmpty(name) && Applications != null && Applications.ContainsKey(name))
                {
                    app = Applications[name];
                    
                    if (!string.IsNullOrEmpty(windowsTitle))
                    {
                        Applications[name].AutomationName = windowsTitle;
                    }

                    if (timeOut > 0)
                        Applications[name].TimeOut = timeOut;

                    
            }
            else
                {
                    LogHandler.LogError(Logging.InformationMessages.RUNTIMEWRAPPER_INVALID_DATA, LogHandler.Layer.Business, "application name", name);
                    throw new Exception(string.Format("Application- {0} provided is invalid", name));
                }
            }

            LogHandler.LogInfo(Logging.InformationMessages.RUNTIMEWRAPPER_EXIT, LogHandler.Layer.Business, className, Logging.Constants.FIND_APPLICATION);
            return app;
        }

        
        public Screen FindScreen(string applicationName, string screenName)
        {
            Application app = null;
            Screen screen = null;

            using (LogHandler.TraceOperations(Logging.InformationMessages.RUNTIMEWRAPPER_ENTER, LogHandler.Layer.Business, Guid.Empty, className, Logging.Constants.FIND_SCREEN))
            {
                LogHandler.LogInfo(Logging.InformationMessages.RUNTIMEWRAPPER_PARAMETERS, LogHandler.Layer.Business, "screen name", Logging.Constants.PARAMDIRECTION_IN, screenName);
                if (!string.IsNullOrEmpty(applicationName) && Applications != null && Applications.ContainsKey(applicationName))
                {
                    app = Applications[applicationName];
                    if (!string.IsNullOrEmpty(screenName) && app.Screens.ContainsKey(screenName))
                    {
                        screen = app.Screens[screenName];
                    }
                    else
                    {
                        LogHandler.LogError(Logging.InformationMessages.RUNTIMEWRAPPER_INVALID_DATA, LogHandler.Layer.Business, "screen name", screenName);
                        throw new Exception(string.Format("Screen- {0} provided is invalid", screenName));
                    }
                    
                }
                else
                {
                    LogHandler.LogError(Logging.InformationMessages.RUNTIMEWRAPPER_INVALID_DATA, LogHandler.Layer.Business, "application name", applicationName);
                    throw new Exception(string.Format("Application- {0} provided is invalid while calling FindScreen", applicationName));
                }
            }

            LogHandler.LogInfo(Logging.InformationMessages.RUNTIMEWRAPPER_EXIT, LogHandler.Layer.Business, className, Logging.Constants.FIND_APPLICATION);
            return screen;
        }

       
        private ComputerVisionLib.Control RefreshControl(ComputerVisionLib.Application app, ComputerVisionLib.Screen screen, ComputerVisionLib.Control ctl)
        {
            int counter = numOfTrials;
            if (ctl.DiscoveryMode == ElementDiscovertyMode.None)
            {
                ctl = null;
            }

           
            
            if (ctl.DiscoveryMode == ElementDiscovertyMode.Image && !getAllMatchingControls)
            {
                DateTime startTime = DateTime.Now;
                do
                {
                    
                    if (ComputerVisionLib_Core.Utilities.IsStopRequested())
                        throw new ComputerVisionLib_Core.CVExceptions.StopRequested();

                    counter--; 
                    
                    Rectangle searchRegion = Rectangle.Empty;
                    if (parentControl != null && parentControl.ImageReference != null)
                    {
                        ComputerVisionLib_Core.Utilities.ScaleStep = imageMatchScaleStepSize;
                        ComputerVisionLib_Core.Utilities.MaxScaleSteps = imageMatchMaxScaleStepCount;
                        parentControl.ImageReference = ComputerVisionLib_Core.Utilities.GetBoundingRectangle(parentControl.ImageReference, templateMachingInOriginalScale, waitForeverForTemplate, useTrueColorTemplateMatching, null, ATRFileDirectory, pSourceImage);
                        parentControl.ImageBoundingRectangle = parentControl.ImageReference.CurrentBoundingRectangle;
                        searchRegion = parentControl.ImageBoundingRectangle;
                        
                    }

                    
                    if (ctl != null && ctl.ImageReference != null)
                    {
                        ComputerVisionLib_Core.Utilities.ScaleStep = imageMatchScaleStepSize;
                        ComputerVisionLib_Core.Utilities.MaxScaleSteps = imageMatchMaxScaleStepCount;
                        ctl.ImageReference = ComputerVisionLib_Core.Utilities.GetBoundingRectangle(ctl.ImageReference, templateMachingInOriginalScale, waitForeverForTemplate, useTrueColorTemplateMatching, searchRegion, ATRFileDirectory, pSourceImage);
                        ctl.ImageBoundingRectangle = ctl.ImageReference.CurrentBoundingRectangle;
                        
                    }
                    else
                        ctl = null;
                    System.Threading.Thread.Sleep(timeGapBetweenTrials);
                }
                while ((ctl == null || (ctl != null && ctl.ImageBoundingRectangle == Rectangle.Empty)) && (DateTime.Now - startTime).TotalSeconds <= timeoutInSec);

                if (ctl.ImageBoundingRectangle == Rectangle.Empty)
                    ctl = null;
            }
            return ctl;
        }

        private void TranslateAutomationConfig(string xmlString, bool launchApps = false)
        {
            Applications = Core.Translation.PopulateApplications(xmlString, launchApps, showWaitBox, firstAppToStart);
        }

        
        private void PopulateRetrySettings()
        {

            string trialCount = "", timeGap = "";
            trialCount = System.Configuration.ConfigurationManager.AppSettings["NumberOfTrials"];
            if (!string.IsNullOrEmpty(trialCount))
                int.TryParse(trialCount, out numOfTrials);

            timeGap = System.Configuration.ConfigurationManager.AppSettings["TimeGapInMillisecondsBetweenTrials"];
            if (!string.IsNullOrEmpty(timeGap))
                int.TryParse(timeGap, out timeGapBetweenTrials);
        }

        


        private string GetProbableAbsolutePath(string relativePath)
        {
            
            return "";
        }

        public bool HighlightElement
        {
            get
            {
                return highlightElement;
            }
            set
            {
                highlightElement = value;
            }
        }
        public bool FindControlInMultipleControlStates
        {
            get
            {
                return findControlInMultipleControlStates;
            }
            set
            {
                findControlInMultipleControlStates = value;
            }
        }
        public bool MultipleScaleTemplateMatching
        {
            get
            {
                return !templateMachingInOriginalScale;
            }
            set
            {
                templateMachingInOriginalScale = !value;
            }
        }
        public int ImageRecognitionTimeout
        {
            get
            {
                return timeoutInSec;
            }
            set
            {
                timeoutInSec = value;
            }
        }
        public bool WaitForever
        {
            get
            {
                return waitForeverForTemplate;
            }
            set
            {
                waitForeverForTemplate = value;
            }
        }
        public bool UseTrueColorTemplateMatching
        {
            get
            {
                return useTrueColorTemplateMatching;
            }
            set
            {
                useTrueColorTemplateMatching = value;
            }
        }
        public int ImageMatchConfidenceThreshold
        {
            get
            {
                return imageMatchConfidenceThreshold;
            }
            set
            {
                imageMatchConfidenceThreshold = value;
            }
        }


        private double imageMatchScaleStepSize = 0.2;
        public double ImageMatchScaleStepSize
        {
            get
            {
                return imageMatchScaleStepSize;
            }
            set
            {
                imageMatchScaleStepSize = value;
            }
        }

        private int imageMatchMaxScaleStepCount = 20;
        public int ImageMatchMaxScaleStepCount
        {
            get
            {
                return imageMatchMaxScaleStepCount;
            }
            set
            {
                imageMatchMaxScaleStepCount = value;
            }
        }

        public bool ShowApplicationStartingWaitBox
        {
            get
            {
                return showWaitBox;
            }
            set
            {
                showWaitBox = value;
                if (Applications != null && Applications.Count > 0)
                {
                    foreach (string app in Applications.Keys)
                    {
                        Applications[app].ShowAppStartWaitBox = value;
                    }
                }
            }
        }

        

       

       

       

        public void Sleep(int seconds)
        {
            System.Threading.Thread.Sleep(seconds * 1000);
        }
        

        
        public string ReadTextArea(double x, double y, double height, double width, string filter = "", float imageResizeCoeff = 1)
        {
            using (LogHandler.TraceOperations(Logging.InformationMessages.RUNTIMEWRAPPER_ENTER, LogHandler.Layer.Business, Guid.Empty, className, Logging.Constants.READTEXTAREA))
            {
                string text = TextRecognitionManager.ReadTextArea(x, y, height, width, filter, imageResizeCoeff); 

                return text;
            }
        }

       
        public string ReadTextArea(double x, double y, double height, double width, TextType filter, float imageResizeCoeff = 1)
        {
            using (LogHandler.TraceOperations(Logging.InformationMessages.RUNTIMEWRAPPER_ENTER, LogHandler.Layer.Business, Guid.Empty, className, Logging.Constants.READTEXTAREA))
            {
                string text = TextRecognitionManager.ReadTextArea(x, y, height, width, filter, imageResizeCoeff);

                return text;
            }
        }

        

        private bool IsValidUrlFormat(string url)
        {
            Uri uriTryResult;
            bool isValid = Uri.TryCreate(url, UriKind.Absolute, out uriTryResult) && (uriTryResult.Scheme == Uri.UriSchemeHttp || uriTryResult.Scheme == Uri.UriSchemeHttps);
            return isValid;
        }

        private string StreamToString(Stream fileContent)
        {
            StreamReader reader = new StreamReader(fileContent);
            string fileString = reader.ReadToEnd();
            return fileString;
        }

        private string GetValidPackage(string path)
        {
            string[] iappackages = iapPackageTypes.Split(',');
            string validPackage = "";
            foreach (string iappackage in iappackages)
            {
                if (path.Contains(iappackage))
                {
                    validPackage = iappackage;
                    break;
                }
            }
            return validPackage;
        }

        public bool EnableTemplateMatchMapping
        {
            get
            {
                return enableTemplateMatchMapping;
            }
            set
            {
                enableTemplateMatchMapping = value;
            }
        }

        private bool enableTemplateMatchMapping = false;

        public byte[] TemplateMatchesMapScreen
        {
            get
            {
                return Core.Utilities.TemplateMatchMapScreen;
            }

        }
        public int TemplateMatchMappingBorderThickness
        {
            get
            {
                return templateMatchMappingBorderThickness;
            }
            set
            {
                templateMatchMappingBorderThickness = value;
            }
        }

        private int templateMatchMappingBorderThickness = 2;

        public Core.Utilities.ImageBgr TemplateMatchMappingBorderColor
        {
            get
            {
                return templateMatchMappingBorderColor;
            }
            set
            {
                templateMatchMappingBorderColor = value;
            }
        }

        private Core.Utilities.ImageBgr templateMatchMappingBorderColor;

        public bool MultiRotationTemplateMatching {
            get {
                return multiRotationTemplateMatching;
            }
            set {
                multiRotationTemplateMatching=value;
            }
        }

        private double imageMatchRotationStepAngle=2.0;
        public double ImageMatchRotationStepAngle {
            get {
                return imageMatchRotationStepAngle;
            }
            set {
                imageMatchRotationStepAngle=value;
            }
        }
    }
}
