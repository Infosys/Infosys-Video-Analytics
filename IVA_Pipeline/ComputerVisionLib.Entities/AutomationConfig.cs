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
using Newtonsoft.Json;

namespace Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.ComputerVisionLib.Entities
{
    [XmlRoot(ElementName = "automationConfig")]    
    public class AutomationConfig
    {

        [XmlElement(ElementName = "appConfig")]
        [JsonProperty(PropertyName = "appConfig")]
        public List<AppConfig>? AppConfigs { get; set; }

        public string? ProjectMode { get; set; }
    }


    public class AppConfig
    {
        [XmlElement(ElementName = "name")]
        [JsonProperty(PropertyName = "name")]
        public string? AppName { get; set; }

        [XmlElement(ElementName = "baseImageDir")]
        [JsonProperty(PropertyName = "baseImageDir")]
        public string? BaseImageDir { get; set; }

        [XmlElement(ElementName = "appImageConfig")]
        [JsonProperty(PropertyName = "appImageConfig")]
        public ImageConfig? AppImageConfig { get; set; }

        [XmlElement(ElementName = "appControlConfig")]
        [JsonProperty(PropertyName = "appControlConfig")]
        public ControlConfig? AppControlConfig { get; set; }

        [XmlElement(ElementName = "screenConfig")]
        [JsonProperty(PropertyName = "screenConfig")]
        public List<ScreenConfig>? ScreenConfigs { get; set; }
    }

    public class ImageConfig
    {
        [XmlElement(ElementName = "stateImageConfig")]
        [JsonProperty(PropertyName = "stateImageConfig")]
        public List<StateImageConfig> StateImageConfig { get; set; }
    }

    public class ImageBase
    {
        [XmlElement(ElementName = "imageName")]
        [JsonProperty(PropertyName = "imageName")]
        public string ImageName { get; set; }

        [XmlElement(ElementName = "postTransformConfigList")]
        [JsonProperty(PropertyName = "postTransformConfigList")]
        public PostTransformConfigList PostTransform { get; set; }
    }

    public class TransformBase
    {
        [XmlElement(ElementName = "distance")]
        [JsonProperty(PropertyName = "distance")]
        public int Distance { get; set; }
    }

    public class StateImageConfig
    {
        [XmlElement(ElementName = "state")]
        [JsonProperty(PropertyName = "state")]
        public string State { get; set; }

        [XmlElement(ElementName = "validationImageSearchConfig")]
        [JsonProperty(PropertyName = "validationImageSearchConfig")]
        public ValidationImageSearchConfig ValidationImageName { get; set; }

        [XmlElement(ElementName = "centerImageSearchConfig")]
        [JsonProperty(PropertyName = "centerImageSearchConfig")]
        public CenterImageSearchConfig CenterImageName { get; set; }

        [XmlElement(ElementName = "rightImageSearchConfig")]
        [JsonProperty(PropertyName = "rightImageSearchConfig")]
        public RightImageSearchConfig RightImageName { get; set; }

        [XmlElement(ElementName = "leftImageSearchConfig")]
        [JsonProperty(PropertyName = "leftImageSearchConfig")]
        public LeftImageSearchConfig LeftImageName { get; set; }

        [XmlElement(ElementName = "aboveImageSearchConfig")]
        [JsonProperty(PropertyName = "aboveImageSearchConfig")]
        public AboveImageSearchConfig AboveImageName { get; set; }

        [XmlElement(ElementName = "belowImageSearchConfig")]
        [JsonProperty(PropertyName = "belowImageSearchConfig")]
        public BelowImageSearchConfig BelowImageName { get; set; }
    }

    public class PostTransformConfigList
    {
        [XmlElement(ElementName = "regionBelowTransformConfig")]
        [JsonProperty(PropertyName = "regionBelowTransformConfig")]
        public RegionBelowTransformConfig RegionBelowTransformConfig { get; set; }

        [XmlElement(ElementName = "regionAboveTransformConfig")]
        [JsonProperty(PropertyName = "regionAboveTransformConfig")]
        public RegionAboveTransformConfig RegionAboveTransformConfig { get; set; }

        [XmlElement(ElementName = "regionRightTransformConfig")]
        [JsonProperty(PropertyName = "regionRightTransformConfig")]
        public RegionRightTransformConfig RegionRightTransformConfig { get; set; }

        [XmlElement(ElementName = "regionLeftTransformConfig")]
        [JsonProperty(PropertyName = "regionLeftTransformConfig")]
        public RegionLeftTransformConfig RegionLeftTransformConfig { get; set; }

        [XmlElement(ElementName = "regionNearbyTransformConfig")]
        [JsonProperty(PropertyName = "regionNearbyTransformConfig")]
        public RegionNearbyTransformConfig RegionNearbyTransformConfig { get; set; }

        [XmlElement(ElementName = "regionClickOffsetTransformConfig")]
        [JsonProperty(PropertyName = "regionClickOffsetTransformConfig")]
        public RegionClickOffsetTransformConfig RegionClickOffsetTransformConfig { get; set; }

        [XmlElement(ElementName = "regionMorphTransformConfig")]
        [JsonProperty(PropertyName = "regionMorphTransformConfig")]
        public RegionMorphTransformConfig RegionMorphTransformConfig { get; set; }

        [XmlElement(ElementName = "regionLimitByTransformConfig")]
        [JsonProperty(PropertyName = "regionLimitByTransformConfig")]
        public RegionLimitByTransformConfig RegionLimitByTransformConfig { get; set; }
    }

    public class RegionBelowTransformConfig
    {
      
    }

    public class RegionAboveTransformConfig
    {

    }
    public class RegionRightTransformConfig
    {

    }
    public class RegionLeftTransformConfig
    {

    }
    public class RegionNearbyTransformConfig
    {

    }
    public class RegionClickOffsetTransformConfig
    {

    }
    public class RegionMorphTransformConfig
    {

    }
    public class RegionLimitByTransformConfig
    {

    }

    public class ValidationImageSearchConfig : ImageBase
    {
       
    }

    public class CenterImageSearchConfig : ImageBase
    {

    }
    public class RightImageSearchConfig : ImageBase
    {

    }
    public class LeftImageSearchConfig : ImageBase
    {

    }
    public class AboveImageSearchConfig : ImageBase
    {

    }
    public class BelowImageSearchConfig : ImageBase
    {

    }

    public class ControlConfig
    {
        [XmlElement(ElementName = "automationId")]
        [JsonProperty(PropertyName = "automationId")]
        public string AutomationId { get; set; }

        [XmlElement(ElementName = "controlName")]
        [JsonProperty(PropertyName = "controlName")]
        public string ControlName { get; set; }

        [XmlElement(ElementName = "controlClass")]
        [JsonProperty(PropertyName = "controlClass")]
        public string ControlClass { get; set; }

        [XmlElement(ElementName = "applicationType")]
        [JsonProperty(PropertyName = "applicationType")]
        public string ApplicationType { get; set; }

        private string _applocationpath;

        [XmlElement(ElementName = "appLocationPath")]
        [JsonProperty(PropertyName = "appLocationPath")]
        public string ApplicationLocationPath { get {return _applocationpath;} set{ _applocationpath = value;} }

        private string _ctrlpath;

        [XmlElement(ElementName = "controlPath")]
        [JsonProperty(PropertyName = "controlPath")]
        public string ControlPath { get { return _ctrlpath; } set { _ctrlpath=value;} }

        [XmlElement(ElementName = "uiFwk")]
        [JsonProperty(PropertyName = "uiFwk")]
        public string UIFwk { get; set; }

        [XmlElement(ElementName = "webBrowser")]
        [JsonProperty(PropertyName = "webBrowser")]
        public string WebBrowser
        { get; set; }

        [XmlElement(ElementName = "webBrowserVersion")]
        [JsonProperty(PropertyName = "webBrowserVersion")]
        public string WebBrowserVersion { get; set; }

    }

    public class ScreenConfig
    {
        [XmlElement(ElementName = "name")]
        [JsonProperty(PropertyName = "name")]
        public string ScreenName { get; set; }

        [XmlElement(ElementName = "screenImageConfig")]
        [JsonProperty(PropertyName = "screenImageConfig")]
        public ImageConfig ScreenImageConfig { get; set; }

        [XmlElement(ElementName = "screenControlConfig")]
        [JsonProperty(PropertyName = "screenControlConfig")]
        public ControlConfig ScreenControlConfig { get; set; }

        [XmlElement(ElementName = "entityConfig")]
        [JsonProperty(PropertyName = "entityConfig")]
        public List<EntityConfig> EntityConfigs { get; set; }
    }

    public class EntityBase
    {
        [XmlElement(ElementName = "name")]
        [JsonProperty(PropertyName = "name",Order=1)]
        public string EntityName { get; set; }

       
        [XmlElement(ElementName = "parent")]
        [JsonProperty(PropertyName = "parent", Order = 5)]
        public string Parent { get; set; }

        [XmlElement(ElementName = "entityControlConfig")]
        [JsonProperty(PropertyName = "entityControlConfig",Order=3)]
        public ControlConfig EntityControlConfig { get; set; }
    }


    public class EntityConfig : EntityBase
    {
        [XmlElement(ElementName = "entityImageConfig")]
        [JsonProperty(PropertyName = "entityImageConfig",Order=2)]
        public ImageConfig EntityImageConfig { get; set; }

        [XmlElement(ElementName = "entityConfig")]
        [JsonProperty(PropertyName = "entityConfig", Order = 4)]
        public List<EntityConfig> EntityChildConfig { get; set; }
    }


    public class EntityChildConfig : EntityBase
    {

    }

}

