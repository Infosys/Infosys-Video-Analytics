/*=============================================================================================================== *
 * Copyright 2025 Infosys Ltd.                                                                                    *
 * Use of this source code is governed by Apache License Version 2.0 that can be found in the LICENSE file or at  *
 * http://www.apache.org/licenses/                                                                                *
 * ===============================================================================================================*/
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.Common;

namespace Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.ComputerVisionLib
{
    public class Screen
    {   
        int _winHandleId;
        IntPtr _winHandle;
        private string _screenName;
        private string _automationControlName;
        private string _automationClassName;
        Dictionary<string, Control> _controls;

        private string className = "Screen";

        public Screen()
        {
        }

        public Screen(string screenNameInput, string className = "")
        {
             
            using (LogHandler.TraceOperations(Logging.InformationMessages.RUNTIMEWRAPPER_ENTER, LogHandler.Layer.Business, Guid.Empty, className, Logging.Constants.SCREEN))
            {
               
                LogHandler.LogInfo(Logging.InformationMessages.RUNTIMEWRAPPER_PARAMETERS, LogHandler.Layer.Business, "screenNameInput", Logging.Constants.PARAMDIRECTION_IN, screenNameInput);
            
            _automationControlName = screenNameInput;
            _automationClassName = className;

            }
         
            LogHandler.LogInfo(Logging.InformationMessages.RUNTIMEWRAPPER_EXIT, LogHandler.Layer.Business, className, Logging.Constants.SCREEN);
        }
        

        
        public string Name
        {
            get { return _screenName; }
            set { _screenName = value; }
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
        public Dictionary<string, Control> Controls
        {
            get { return _controls; }
            set { _controls = value; }
        }

        public IntPtr? WindowsHandle
        {
            get { return _winHandle; }
        }
        
        public bool IsAvailable
        {
            get
            {
                if (WindowsHandle == IntPtr.Zero || WindowsHandle == null)
                    return false;
                else
                    return true;
            }
        }
    }
}
