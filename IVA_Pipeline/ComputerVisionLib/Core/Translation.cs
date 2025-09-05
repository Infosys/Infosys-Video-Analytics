/*=============================================================================================================== *
 * Copyright 2025 Infosys Ltd.                                                                                    *
 * Use of this source code is governed by Apache License Version 2.0 that can be found in the LICENSE file or at  *
 * http://www.apache.org/licenses/                                                                                *
 * ===============================================================================================================*/
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.ComputerVisionLib.Entities;
using System.Diagnostics;
using System.IO;
using System.Xml.Serialization;
using System.Xml;


namespace Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.ComputerVisionLib.Core
{
    public class Translation
    {
        static string baseImageDir = "";

        public static Dictionary<string, Application> PopulateApplications(string xmlString, bool LaunchApps = false, bool showAppStartWaitBox = true, string firstAppToStart = "")
        {
            Core.Utilities.WriteLog("populating applications"); 
            Dictionary<string, Application> applications = new Dictionary<string, Application>();
            
            AutomationConfig autoConfig = Deserialize(xmlString, typeof(AutomationConfig)) as AutomationConfig;
            Application app = null;
            if (autoConfig != null)
            {
                
                if (!string.IsNullOrEmpty(firstAppToStart))
                {
                    if (!autoConfig.AppConfigs.Any(a => a.AppName == firstAppToStart))
                        throw new Exception(string.Format("Application- {0} provided is invalid", firstAppToStart));
                }

                bool FocusSetToFirstApp = false;
                foreach (AppConfig appconfig in autoConfig.AppConfigs)
                {
                    if (LaunchApps || (!string.IsNullOrEmpty(firstAppToStart) && firstAppToStart == appconfig.AppName))
                    {
                        baseImageDir = appconfig.BaseImageDir;
                        string appType = appconfig.AppControlConfig.ApplicationType;
                        int processId = Core.Utilities.LaunchApplication(appconfig.AppControlConfig.ApplicationLocationPath, appType, appconfig.AppControlConfig.WebBrowser, showAppStartWaitBox);
                        try
                        {
                            app = new Application(processId);
                        }
                        catch (Exception ex)
                        {
                            Core.Utilities.WriteLog("instantiating application with control name");
                            app = new Application(appconfig.AppControlConfig.ControlName, true, appconfig.AppControlConfig.ControlClass);
                        }
                        
                    }                    
                    else
                        app = new Application(appconfig.AppControlConfig.ControlName, false, appconfig.AppControlConfig.ControlClass);

                    if (app != null)
                    {
                        
                        baseImageDir = appconfig.BaseImageDir;
                        app.Name = appconfig.AppName;
                        app.AppType = appconfig.AppControlConfig.ApplicationType;
                        app.AppLocationPath = appconfig.AppControlConfig.ApplicationLocationPath;
                        app.UIFwk = appconfig.AppControlConfig.UIFwk;
                        ScreensAndControlsUnderScreen data = PopulateApplicationScreens(appconfig, appconfig.AppControlConfig.ApplicationType, app.Name);
                        app.Screens = data.Screens;
                        app.Controls = data.ControlsUndderScreens;

                        app.WebBrowser = appconfig.AppControlConfig.WebBrowser;
                        app.WebBrowserVersion = appconfig.AppControlConfig.WebBrowserVersion;
                        app.ShowAppStartWaitBox = showAppStartWaitBox;
                    }
                    else
                        Core.Utilities.WriteLog(" failed to instantiate application as it is null");

                    applications.Add(appconfig.AppName, app); 
                }
            }
            Core.Utilities.WriteLog("applications populated");
            return applications;
        }

        private static ScreensAndControlsUnderScreen PopulateApplicationScreens(AppConfig appConfig, string applicationType, string appName)
        {
            Core.Utilities.WriteLog("populating screens"); 
            Dictionary<string, Screen> screens = new Dictionary<string, Screen>();
            Dictionary<string, Control> controls = new Dictionary<string, Control>(); 
            Screen screen = null;
            foreach (ScreenConfig screenConfig in appConfig.ScreenConfigs)
            {
                string appScreenQualifier = "";
                if (!string.IsNullOrEmpty(screenConfig.ScreenName))
                {
                    appScreenQualifier = appName + "." + screenConfig.ScreenName;
                    screen = new Screen(screenConfig.ScreenControlConfig.ControlName, screenConfig.ScreenControlConfig.ControlClass); 
                    screen.Name = screenConfig.ScreenName;
                    screen.Controls = PopulateControls(screenConfig, applicationType, appScreenQualifier);
                    
                    screens.Add(screenConfig.ScreenName, screen);
                }
                else
                {
                    appScreenQualifier = appName;
                    
                    controls.AddRange(PopulateControls(screenConfig, applicationType, appScreenQualifier));
                }
            }
            Core.Utilities.WriteLog("populated screens");
            return new ScreensAndControlsUnderScreen() { Screens = screens, ControlsUndderScreens = controls };
        }

        private static Dictionary<string, Control> PopulateControls(object objConfig, string applicationType, string ControlQualifier)
        {
            Core.Utilities.WriteLog("populating controls"); 
            Dictionary<string, Control> controls = new Dictionary<string, Control>();
            Control ctl = null;
            string fullControlQualifier = "";
            if (objConfig.GetType().Equals(typeof(ScreenConfig)))
            {
                ScreenConfig screenConfig = objConfig as ScreenConfig;
                Core.Utilities.WriteLog("controls populating under screen-" + screenConfig.ScreenName); 
                foreach (EntityConfig entityConfig in screenConfig.EntityConfigs)
                {

                    if (!string.IsNullOrEmpty(entityConfig.EntityName))
                    {
                        Core.Utilities.WriteLog("control being added- " + entityConfig.EntityName); 
                        fullControlQualifier = ControlQualifier + "." + entityConfig.EntityName;
                        
                        
                        
                        ctl = new Control(); 
                        if (entityConfig.EntityControlConfig.ControlClass == null || entityConfig.EntityControlConfig.ControlClass.ToLower() == "na" || entityConfig.EntityControlConfig.ControlClass.ToLower() == "none")
                            ctl.DiscoveryMode = ElementDiscovertyMode.Image;
                        else
                        ctl.DiscoveryMode = GetDiscoveryMode(entityConfig);
                        Core.Utilities.WriteLog("got discovery mode"); 
                        ctl.ImageReference = PopulateEntityImage(entityConfig.EntityImageConfig);
                        
                        ctl.Name = entityConfig.EntityName;
                       
                        if (entityConfig.EntityChildConfig != null && entityConfig.EntityChildConfig.Count > 0)
                        {
                            ctl.Controls = PopulateControls(entityConfig, applicationType, fullControlQualifier); 
                        }

                        
                        controls.Add(ctl.Name, ctl);
                        Core.Utilities.WriteLog("control added- " + ctl.Name);
                    }

                    else
                    {
                        controls.AddRange(PopulateControls(entityConfig, applicationType, fullControlQualifier));
                    }
                }
            }
            else if (objConfig.GetType().Equals(typeof(EntityConfig)))
            {
                EntityConfig entityConfig = objConfig as EntityConfig;
                Core.Utilities.WriteLog("controls populating under control-" + entityConfig.EntityName);
                foreach (EntityConfig entitychildConfig in entityConfig.EntityChildConfig)
                {
                    if (!string.IsNullOrEmpty(entitychildConfig.EntityName))
                    {
                        fullControlQualifier = ControlQualifier + "." + entitychildConfig.EntityName;
                        
                        
                        if (entityConfig.EntityControlConfig.ControlClass == null || entityConfig.EntityControlConfig.ControlClass.ToLower() == "na" || entityConfig.EntityControlConfig.ControlClass.ToLower() == "none")
                            ctl.DiscoveryMode = ElementDiscovertyMode.Image;
                        else
                        ctl.DiscoveryMode = GetDiscoveryMode(entitychildConfig);
                        ctl.ImageReference = PopulateEntityImage(entitychildConfig.EntityImageConfig);
                        
                        ctl.Name = entitychildConfig.EntityName;
                       
                        if (entitychildConfig.EntityChildConfig != null && entityConfig.EntityChildConfig.Count > 0)
                        {
                            ctl.Controls = PopulateControls(entitychildConfig, applicationType, fullControlQualifier);
                        }
                        
                        controls.Add(ctl.Name, ctl);
                    }
                    else
                        
                        controls.AddRange(PopulateControls(entitychildConfig, applicationType, fullControlQualifier));
                }
            }
            Core.Utilities.WriteLog("controls populated");
            return controls;
        }

        private static ControlImageReference PopulateEntityImage(ImageConfig entityImageConfig)
        {
            ControlImageReference imageRef = new ControlImageReference();
            if (entityImageConfig != null)
            {
                
                if (entityImageConfig != null && entityImageConfig.StateImageConfig != null && entityImageConfig.StateImageConfig.Count > 0)
                {
                    imageRef.SupportedStates = new List<ControlStateReference>();
                    entityImageConfig.StateImageConfig.ForEach(stateConfig =>
                    {
                        if (stateConfig.CenterImageName != null)
                        {
                            imageRef.SupportedStates.Add(new ControlStateReference() { State = stateConfig.State, ImagePath = baseImageDir + "\\" + stateConfig.CenterImageName.ImageName });
                        }
                        
                        else if (stateConfig.AboveImageName != null)
                        {
                            imageRef.SupportedStates.Add(new ControlStateReference() { State = stateConfig.State, ImagePath = baseImageDir + "\\" + stateConfig.AboveImageName.ImageName });
                        }
                        else if (stateConfig.LeftImageName != null)
                        {
                            imageRef.SupportedStates.Add(new ControlStateReference() { State = stateConfig.State, ImagePath = baseImageDir + "\\" + stateConfig.LeftImageName.ImageName });
                        }
                        else if (stateConfig.BelowImageName != null)
                        {
                            imageRef.SupportedStates.Add(new ControlStateReference() { State = stateConfig.State, ImagePath = baseImageDir + "\\" + stateConfig.BelowImageName.ImageName });
                        }
                        else if (stateConfig.RightImageName != null)
                        {
                            imageRef.SupportedStates.Add(new ControlStateReference() { State = stateConfig.State, ImagePath = baseImageDir + "\\" + stateConfig.RightImageName.ImageName });
                        }
                        else if (stateConfig.ValidationImageName != null)
                        {
                            imageRef.SupportedStates.Add(new ControlStateReference() { State = stateConfig.State, ImagePath = baseImageDir + "\\" + stateConfig.ValidationImageName.ImageName });
                        }
                    });
                }
            }
            return imageRef;
        }

        private static object Deserialize(string xmlObj, Type type)
        {
            StringReader stringReader = new StringReader(xmlObj);
            XmlSerializer serializer = new XmlSerializer(type);
            return serializer.Deserialize(stringReader);
        }

        public static string XMLDocAsString(System.Xml.XmlDocument xmlDoc)
        {
            StringWriter sw = new StringWriter();
            XmlTextWriter tx = new XmlTextWriter(sw);
            xmlDoc.WriteTo(tx);
            string strXmlText = sw.ToString();
            return strXmlText;
        }

        private static ElementDiscovertyMode GetDiscoveryMode(EntityConfig entityConfig)
        {
            ElementDiscovertyMode mode = ElementDiscovertyMode.None;
            if (entityConfig.EntityControlConfig != null && entityConfig.EntityImageConfig != null)
            {
                if ((!string.IsNullOrEmpty(entityConfig.EntityControlConfig.ControlPath) ||
                    !string.IsNullOrEmpty(entityConfig.EntityControlConfig.AutomationId) ||
                    !string.IsNullOrEmpty(entityConfig.EntityControlConfig.ControlName) ||
                    !string.IsNullOrEmpty(entityConfig.EntityControlConfig.ControlClass))
                    && (entityConfig.EntityImageConfig.StateImageConfig != null && entityConfig.EntityImageConfig.StateImageConfig.Count > 0))
                {
                    mode = ElementDiscovertyMode.APIAndImage;
                }
            }

            if (entityConfig.EntityControlConfig != null && mode == ElementDiscovertyMode.None)
            {
                if (!string.IsNullOrEmpty(entityConfig.EntityControlConfig.ControlPath) ||
                    !string.IsNullOrEmpty(entityConfig.EntityControlConfig.AutomationId) ||
                    !string.IsNullOrEmpty(entityConfig.EntityControlConfig.ControlName) ||
                    !string.IsNullOrEmpty(entityConfig.EntityControlConfig.ControlClass))
                    mode = ElementDiscovertyMode.API;
            }

            if (entityConfig.EntityImageConfig != null && mode == ElementDiscovertyMode.None)
            {
                if (entityConfig.EntityImageConfig.StateImageConfig != null && entityConfig.EntityImageConfig.StateImageConfig.Count > 0)
                    mode = ElementDiscovertyMode.Image;
            }
            return mode;
        }


        
    }

    static class CollectionAddRangeExtension
    {
        public static void AddRange<T>(this ICollection<T> targetCollection, IEnumerable<T> sourceCollection)
        {
            if (targetCollection == null)
                throw new ArgumentNullException("targetCollection");
            if (sourceCollection == null)
                throw new ArgumentNullException("sourceCollection");
            foreach (var element in sourceCollection)
            {
                try
                {
                    if (!targetCollection.Contains(element))
                        targetCollection.Add(element);

                }
                catch (ArgumentException) 
                {
                    
                }
            }
        }
    }

    class ScreensAndControlsUnderScreen
    {
       
        public Dictionary<string, Screen> Screens { get; set; }
        
        public Dictionary<string, Control> ControlsUndderScreens { get; set; }
    }
}
