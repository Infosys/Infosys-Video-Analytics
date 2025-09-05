/*=============================================================================================================== *
 * Copyright 2025 Infosys Ltd.                                                                                    *
 * Use of this source code is governed by Apache License Version 2.0 that can be found in the LICENSE file or at  *
 * http://www.apache.org/licenses/                                                                                *
 * ===============================================================================================================*/
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Drawing;
using Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.Common;
using Infosys.IVA.ComputerVisionLib.OCREngine;
using Infosys.Solutions.Ainauto.VideoAnalytics.Services.MaskDetector.Contracts.Message;
using static Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.ComputerVisionLib.Core.Utilities;

namespace Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.ComputerVisionLib
{
    public class Control
    {
        
        private Dictionary<string, Control> _controls;
        private int[] _runtimeId;
        private string _automationControlName;

        private bool _highlightElement = false; 
      

        private string className = "Control";

        
        #region Event- PropertyHasChanged
        public class PropertyHasChangedArgs : EventArgs
        {
            public Control? Control { get; set; }
            public string? ChangedProperty { get; set; }
            public object? OldValue { get; set; }
            public object? NewValue { get; set; }
        }
        public delegate void PropertyHasChangedEventHandler(PropertyHasChangedArgs e);
        public event PropertyHasChangedEventHandler PropertyHasChanged;
        #endregion

        #region Event- StructureHasChanged
        public class StructureHasChangedArgs : EventArgs
        {
            public Control? Control { get; set; }
            public string? StructureChangeType { get; set; }
        }
        public delegate void StructureHasChangedEventHandler(StructureHasChangedArgs e);
        public event StructureHasChangedEventHandler StructureHasChanged;
        #endregion

        public Control()
        {

        }
        

       
        public Dictionary<string, Control> Controls
        {
            get { return _controls; }
            set { _controls = value; }
        }

        public bool HasChildren
        {
            get
            {
                return (_controls != null && _controls.Count > 0) ? true : false;
            }
        }

        

        public string AutomationId { get; set; }

        public string FullControlQualifier { get; set; }

        public string Name { get; set; }

        

        public string AutomationName
        {
            get { return _automationControlName; }
            set { _automationControlName = value; }

        }

        public string ApplicationType { get; set; }

        

        public string ControlPath { get; set; }

        

        
        public Rectangle ImageBoundingRectangle
        {
            get;
            set;
        }

        public ControlImageReference ImageReference
        {
            get;
            set;
        }

        public ElementDiscovertyMode DiscoveryMode { get; set; }

        
        

       
        public void SetRegion(int x, int y, int width, int height)
        {
            this.ImageBoundingRectangle = new Rectangle(x, y, width, height);
        }

        


        public void UpdateCondition(string automationId, string automationName)
        {
            this.ImageBoundingRectangle = Rectangle.Empty;
            if (automationId == null)
                automationId = "";
            if (automationName == null)
                automationName = "";
        }

        

        bool _GetAllMatchingControls = false; 
        public bool GetAllMatchingControls
        {
            get { return _GetAllMatchingControls; }
            set { _GetAllMatchingControls = value; }
        }

        
        public Control(string automationId, string automationName,
          string applicationTreePath, string applicationType, string fullControlQualifier)
        {
            this.AutomationId = automationId;
            this.AutomationName = automationName;
            this.ControlPath = applicationTreePath;
            this.ApplicationType = applicationType;
            this.FullControlQualifier = fullControlQualifier;


        }
        

       
        public string ReadTextArea(double offsetX, double offsetY, double height, double width, string filter = "", float imageResizeCoeff = 1)
        {
            using (LogHandler.TraceOperations(Logging.InformationMessages.RUNTIMEWRAPPER_ENTER, LogHandler.Layer.Business, Guid.Empty, className, Logging.Constants.RIGHTCLICK))
            {
                string text = null;

                double absoluteX;
                double absoluteY;
                double absoluteHeight;
                double absoluteWidth;
                CalculateImageAbsoluteCooridnates(offsetX, offsetY, height, width, out absoluteX, out absoluteY, out absoluteHeight, out absoluteWidth);

                text = TextRecognitionManager.ReadTextArea(absoluteX, absoluteY, absoluteHeight, absoluteWidth, filter, imageResizeCoeff);
                return text;
            }
        }

        private void CalculateImageAbsoluteCooridnates(double offsetX, double offsetY, double height, double width, out double absoluteX, out double absoluteY, out double absoluteHeight, out double absoluteWidth)
        {
            absoluteX = 0;
            absoluteY = 0;
            absoluteHeight = 0;
            absoluteWidth = 0;

            Rectangle rect = new Rectangle();

                          
            if (ImageBoundingRectangle != null)
            {
                rect = ImageBoundingRectangle;
            }
            

            
            if (offsetX == 0 && offsetY == 0)
            {
                absoluteX = rect.X;
                absoluteY = rect.Y;
            }
            else if (offsetX == 0 && offsetY > 0)
            {
                absoluteX = rect.X;
                absoluteY = rect.Y + offsetY;
            }
            else if (offsetX > 0 && offsetY == 0)
            {
                absoluteX = rect.X + offsetX;
                absoluteY = rect.Y;
            }
            else
            {
                absoluteX = offsetX + rect.X;
                absoluteY = offsetY + rect.Y;
            }

            if (height == 0 && width == 0)
            {
                absoluteHeight = rect.Height;
                absoluteWidth = rect.Width;
            }
            else if (height > 0 && width == 0)
            {
                absoluteHeight = height;
                absoluteWidth = rect.Width;
            }
            else if (height == 0 && width > 0)
            {
                absoluteHeight = rect.Height;
                absoluteWidth = width;
            }
            else
            {
                absoluteWidth = width;
                absoluteHeight = height;
            }
        }

        
        public string ReadTextArea(double offsetX, double offsetY, double height, double width, TextType filter, float imageResizeCoeff = 1)
        {
            using (LogHandler.TraceOperations(Logging.InformationMessages.RUNTIMEWRAPPER_ENTER, LogHandler.Layer.Business, Guid.Empty, className, Logging.Constants.RIGHTCLICK))
            {
                string text = null;

                double absoluteX = 0;
                double absoluteY = 0;
                double absoluteHeight = 0;
                double absoluteWidth = 0;

                CalculateImageAbsoluteCooridnates(offsetX, offsetY, height, width, out absoluteX, out absoluteY, out absoluteHeight, out absoluteWidth);

                text = TextRecognitionManager.ReadTextArea(absoluteX, absoluteY, absoluteHeight, absoluteWidth, filter, imageResizeCoeff);
                return text;
            }
        }

        
    }

    

    public class ControlImageReference {
        public List<ControlStateReference> SupportedStates {get;set;}
        public string CurrentState {get;set;}
        public Rectangle CurrentBoundingRectangle {get;set;}
        public double ConfidenceScore {get;set;}
        public double Angle {get;set;}
    }

    public class ControlStateReference
    {
        public string State { get; set; }
        public string ImagePath { get; set; }

    }

    public class TemplateMatching
    {
        public Rectangle BoundingBox { get; set; }
        public double ConfidenceScore { get; set; }
    }

   

    public enum ElementDiscovertyMode
    {
        Image, 
        API,
        APIAndImage,
        None
    }

    

}
