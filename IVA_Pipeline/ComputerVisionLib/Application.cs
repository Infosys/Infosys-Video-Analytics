/*=============================================================================================================== *
 * Copyright 2025 Infosys Ltd.                                                                                    *
 * Use of this source code is governed by Apache License Version 2.0 that can be found in the LICENSE file or at  *
 * http://www.apache.org/licenses/                                                                                *
 * ===============================================================================================================*/
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.Common;

using System.Reflection;

namespace Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.ComputerVisionLib
{
    public class Application
    {
        
        private int _processId;
        private string _appName;
        private string _automationControlName;
        private string _automationClassName;
        private string _appType;
        private string _appLocationPath;
        private string _uiFwk;
        private Dictionary<string, Screen> _screens;
        Dictionary<string, Control> _controls;
        private bool appLaunched = true;
        private string _webBrowser;
        private string _webBrowserVersion;
        private int _getWindowsHandleTimeOut = 3;

        private string className = "Application";

       
        #region Event- PropertyHasChanged
        public class PropertyHasChangedArgs : EventArgs
        {
            public Control Control { get; set; }
            public string ChangedProperty { get; set; }
            public object OldValue { get; set; }
            public object NewValue { get; set; }
        }
        public delegate void PropertyHasChangedEventHandler(PropertyHasChangedArgs e);
        public event PropertyHasChangedEventHandler PropertyHasChanged;
        #endregion

        #region Event- StructureHasChanged
        public class StructureHasChangedArgs : EventArgs
        {
            public Control Control { get; set; }
            public string StructureChangeType { get; set; }
        }
        public delegate void StructureHasChangedEventHandler(StructureHasChangedArgs e);
        public event StructureHasChangedEventHandler StructureHasChanged;
        #endregion

       
        public Application(string applicationName, bool launchedApp = false, string applicationClassName = "")
        {
            Core.Utilities.SetDLLsPath();
            appLaunched = launchedApp;
             
            using (LogHandler.TraceOperations(Logging.InformationMessages.RUNTIMEWRAPPER_ENTER, LogHandler.Layer.Business, Guid.Empty, className, Logging.Constants.APPLICATION))
            {
               
            
            LogHandler.LogInfo(Logging.InformationMessages.RUNTIMEWRAPPER_PARAMETERS, LogHandler.Layer.Business, "applicationName", Logging.Constants.PARAMDIRECTION_IN, applicationName);
             
            _automationControlName = applicationName;
            _automationClassName = applicationClassName;
            

             
            } 
            LogHandler.LogInfo(Logging.InformationMessages.RUNTIMEWRAPPER_EXIT, LogHandler.Layer.Business, className, Logging.Constants.APPLICATION);
            
        }
        
        public Application(int pid)
        {
            Core.Utilities.WriteLog("instantiating application with process id");
            appLaunched = true;
            using (LogHandler.TraceOperations(Logging.InformationMessages.RUNTIMEWRAPPER_ENTER, LogHandler.Layer.Business, Guid.Empty, className, Logging.Constants.APPLICATION))
            {
                
                LogHandler.LogInfo(Logging.InformationMessages.RUNTIMEWRAPPER_PARAMETERS, LogHandler.Layer.Business, "pid", Logging.Constants.PARAMDIRECTION_IN, pid.ToString());

                _processId = pid;
                            
            }
            LogHandler.LogInfo(Logging.InformationMessages.RUNTIMEWRAPPER_EXIT, LogHandler.Layer.Business, className, Logging.Constants.APPLICATION);
        }


        public string Name
        {
            get { return _appName; }
            set { _appName = value; }

        }
        public string AutomationName
        {
            get { return _automationControlName; }
            set { _automationControlName = value; }
        }

        public string AutomationClassName
        {
            get { return _automationClassName; }
            set { _automationClassName = value; }
        }

        public int ProcessId
        {
            get { return _processId; }

        }


        public string AppType
        {
            get { return _appType; }
            set { _appType = value; }

        }

        public string AppLocationPath
        {
            get { return _appLocationPath; }
            set { _appLocationPath = value; }

        }

        public string UIFwk
        {
            get { return _uiFwk; }
            set { _uiFwk = value; }
        }

        public Dictionary<string, Control> Controls
        {
            get { return _controls; }
            set { _controls = value; }
        }

        public Dictionary<string, Screen> Screens
        {
            get { return _screens; }
            set { _screens = value; }
        }

        public string WebBrowser
        {
            get { return _webBrowser; }
            set { _webBrowser = value; }

        }

        public string WebBrowserVersion
        {
            get { return _webBrowserVersion; }
            set { _webBrowserVersion = value; }

        }

        public bool ShowAppStartWaitBox { get; set; }
       

       
        public bool StartApp()
        {
            bool startedSuccwessfully = true;
            _processId = Core.Utilities.LaunchApplication(this.AppLocationPath, AppType, this.WebBrowser, this.ShowAppStartWaitBox);
            appLaunched = true;
            
            return startedSuccwessfully;
        }
       
        public bool StartApp(string appArguement)
        {
            bool startedSuccwessfully = true;
            _processId = Core.Utilities.LaunchApplication(this.AppLocationPath, this.AppType, this.WebBrowser, this.ShowAppStartWaitBox, appArguement);
            appLaunched = true;
            
            return startedSuccwessfully;
        }
       
        public void CloseApp()
        {
            if (_processId > 0)
            {
                Process process = Process.GetProcessById(_processId);
                process.Kill();
            }
        }

        

        public bool AppLaunched
        {
            set
            {
                appLaunched = value;
            }
        }

        

        
        public int TimeOut
        {
            set
            {
                if (value > 0)
                    _getWindowsHandleTimeOut = value;
            }
        }
    }
}
