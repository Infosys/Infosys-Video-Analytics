/*=============================================================================================================== *
 * Copyright 2025 Infosys Ltd.                                                                                    *
 * Use of this source code is governed by Apache License Version 2.0 that can be found in the LICENSE file or at  *
 * http://www.apache.org/licenses/                                                                                *
 * ===============================================================================================================*/
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.ComputerVisionLib.Logging
{

    public class Constants
    {
        
        public const string PARAMDIRECTION_IN = "(In)";
        public const string PARAMDIRECTION_OUT = "(Out)";
        public const string PARAMDIRECTION_INANDOUT = "(InAndOut)";
        public const string VARIABLENAME_FORMAT = "v_{0}_{1}";

        public const string FIND_APPLICATION = "FindApplication";
        public const string FIND_SCREEN = "FindScreen";
        public const string FINDCONTROL = "FindControl";
        public const string READTEXTAREA = "ReadTextArea";
        public const string GETWINDOWSHANDLE = "GetWindowsHandle";

        
        public const string AUTOMATIONFACADE = "AutomationFacade constructor";        

        
        public const string APPLICATION = "Application constructor";
        public const string REFRESHAPPHANDLE = "RefreshAppHandle";       

        
        public const string CONTROL = "Control constructor";
        public const string FINDCONTROLS = "FindControls";
        public const string ADDCONTROL = "AddControl";
        public const string ISCONTROLAVAILABLE= "IsControlAvailable";
        public const string CLICK = "Click";
        public const string RIGHTCLICK = "RightClick";
        public const string DOUBLECLICK = "DoubleClick";
        public const string HOVER = "Hover";
        public const string KEYPRESS = "KeyPress";
        public const string REFRESHCONTROLHANDLE = "RefreshControlHandle";
        public const string MATCHFORAPPLICATIONTREEPATH = "MatchForApplicationTreePath";
        public const string GETAUTOMATIONELEMENTFROMWITHINBOUNDARY= "GetAutomationElementFromWithinBoundary";
        public const string SCANXAXIX ="ScanXaxix";
        
        public const string SCREEN = "Screen constructor";
        public const string REFRESHSCREENHANDLE = "RefreshScreenHandle";

        
        public const string BUTTON = "Button constructor";
        public const string CHECKBOX = "CheckBox constructor";
        public const string COMBOBOX = "ComboBox constructor";
        public const string EXPAND = "Expand";
        public const string COLLAPSE = "Collapse";
        public const string CUSTOM = "Custom constructor";
        public const string DOCUMENT = "Document constructor";
        public const string EDIT = "Edit constructor";
        public const string HYPERLINK = "Hyperlink constructor";
        public const string IMAGE = "Image constructor";
        public const string LIST = "List constructor";
        public const string LISTITEM = "ListItem constructor";
        public const string MENU = "Menu constructor";
        public const string RADIOBUTTON = "RadioButton constructor";
        public const string SELECT = "Select";
        public const string TEXTBOX = "TextBox constructor";
        public const string TREE = "Tree constructor";
        public const string TREEITEM = "TreeItem constructor";
        public const string TAB = "Tab constructor";
        public const string TABITEM = "Tab Item constructor";
        public const string DATAITEM = "Data Item constructor";
        public const string DATAGRID = "Data Grid constructor";
    }
}
