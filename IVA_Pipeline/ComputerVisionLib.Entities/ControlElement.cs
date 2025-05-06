/*=============================================================================================================== *
 * Copyright 2025 Infosys Ltd.                                                                                    *
 * Use of this source code is governed by Apache License Version 2.0 that can be found in the LICENSE file or at  *
 * http://www.apache.org/licenses/                                                                                *
 * ===============================================================================================================*/
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.ComputerVisionLib.Entities
{
    public class ControlElement
    {
        private string _name ="";
        public string Name { get { return _name; } set { _name = value;} }
        public string ControlType { get; set; }
        private string _ctrlName = "";
        public string ControlName { get { return _ctrlName; } set { _ctrlName = value; } }

    }

    public class Script
    {
        public string ClassName { get; set; }        
        public string ClassType { get; set; }
        private List<Import> _imports = new List<Import>();
        public List<Import> Imports { get { return _imports; } set { _imports = value; } }
        private List<ControlElement> _elements = new List<ControlElement>();
        public List<ControlElement> CtrlElements { get { return _elements; } set { _elements =value ;} }
        private List<ControlClass> _classes = new List<ControlClass>();
        public List<ControlClass> CtrlClasses { get { return _classes; } set { _classes = value ;} }
    }
  
    public class ControlClass
    {
        public string ControlName { get; set; }
        public string ControlClassName { get; set; }        
    }
    
    public class Import
    {
        private string _importClass = "";
        public string ImportClass { get { return _importClass; } set { _importClass = value; } }
        public string ImportClass_1 { get { return _importClass; } set { _importClass = value; } }
    }

    public class Application
    {
        public Control Control;
    }

    public class Control
    {
        public Win32 Win32 { get; set; }
        public Image Images { get; set; }
        public List<Control> Children { get; set; }
    }

    public class Win32
    {
        public string Name { get; set; }
        public string Id { get; set; }
    }

    public class Image
    {
        public string Center { get; set; }
        public String Top { get; set; }
        public String Down { get; set; }
        public string Right { get; set; }
        public string Left { get; set; }
        public String Validate { get; set; }
    }

    [Serializable]
    public class Desktop
    {
        [XmlElement("Application")]
        public List<UIElement> Applications = new List<UIElement>();
       
        [XmlAttribute]
        public string DisplayText = "desktop";
      
    }

    [Serializable]
    public class UIElement
    {
        [XmlElement]
        public BaseControl ElementAttributes { get; set; }
        [XmlAttribute]
        public string ElementImageFile { get; set; }
        [XmlAttribute]
        public string HierarchicalImageLoc { get; set; }
        [XmlAttribute]
        public string DisplayText { get; set; }
        [XmlElement("ChildElement")]
        public List<UIElement> Children { get; set; }
        [XmlElement]
        public CtrlImage Images { get; set; }
    }

   
    public class CtrlImage
    {
        private string _center = "";
        private string _right = "";
        private string _left = "";
        private string _up = "";
        private string _down = "";
        private string _validation = "";

      
        public string Center
        {
            get
            {
                return _center;
            }
            set
            {
                _center = value;
            }
        }
      
        public string Left { get { return _left; } set { _left = value; } }
      
        public string Right { get { return _right; } set { _right = value; } }
      
        public string Up { get { return _up; } set { _up = value; } }
      
        public string Down { get { return _down; } set { _down = value; } }

        public string Validation { get { return _validation; } set { _validation = value; } }
    }

    [Serializable]
    public class WindowControl : BaseControl
    {
        public string Name { get; set; }
        public string AutomationId { get; set; }
        public string ControlType { get; set; }
        public string LocalizedControlType { get; set; }
        public string AccessKey { get; set; }
        public string UIFramework { get; set; }
        public string BoundingRectangle { get; set; }
        public string WindowHandle { get; set; }
        public string ClassName { get; set; }
        public string ProcessId { get; set; }
        public string ControlPatterns { get; set; }
        public string ApplicationTreePath { get; set; }
    }


    public class BaseControl
    { }
}
