/*=============================================================================================================== *
 * Copyright 2025 Infosys Ltd.                                                                                    *
 * Use of this source code is governed by Apache License Version 2.0 that can be found in the LICENSE file or at  *
 * http://www.apache.org/licenses/                                                                                *
 * ===============================================================================================================*/
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Collections;
using System.Collections.Specialized;
using System.Xml.Serialization;

namespace Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.ComputerVisionLib.Entities
{
    [XmlRoot]    
    [Serializable]
    public class UseCase : Base
    {
        public UseCase(){}

        [XmlElement("Activity")]
        public List<Activity> Activities { get; set; }
        [XmlAttribute]
        public string CreatedBy { get; set; }
              
        [XmlAttribute]
        public string MachineName { get; set; }
        [XmlAttribute]
        public string MachineIP { get; set; }
        [XmlAttribute]
        public string OS { get; set; }
        [XmlAttribute]
        public string Domain { get; set; }
        [XmlAttribute]
        public string MachineType { get; set; }
        [XmlAttribute]
        public string OSVersion { get; set; }
        [XmlAttribute]
        public string ScreenResolution { get; set; }
    }

    [Serializable]
    public class Activity : Base
    {
        public Activity() { }

        [XmlElement("Task")]
        public List<Task> Tasks { get; set; }
        [XmlElement]
        public ApplicationDetails TargetApplication { get; set; }
    }

    [Serializable]
    public class Task : Base
    {
        public Task() { }

        [XmlAttribute]
        public EventTypes Event { get; set; }
        [XmlAttribute]
        public string TriggeredPattern { get; set; }
        [XmlAttribute]
        public string CurrentState { get; set; }
        [XmlAttribute]
        public int Order { get; set; }
        [XmlAttribute]
        public string ControlId { get; set; }
        [XmlAttribute]
        public int SourceIndex { get; set; }
        [XmlAttribute]
        public string ControlName { get; set; }
        [XmlAttribute]
        public string XPath { get; set; }
        [XmlAttribute]
        public string ApplictionTreePath { get; set; }
        [XmlAttribute]
        public string ControlType { get; set; }
        [XmlAttribute]
        public string AccessKey { get; set; }
        [XmlAttribute]
        public ApplicationTypes ControlOnApplication { get; set; }
        [XmlElement]
        public List<NameValueAtribute> TargetControlAttributes { get; set; }
        [XmlAttribute]
        public string WindowTitle { get; set; }
        [XmlAttribute]
        public string GroupScriptId { get; set; }
        [XmlAttribute]
        public string ScreenId { get; set; }
        [XmlAttribute]
        public DateTime CapturedTime { get; set; }
    }

    [Serializable]
    public class ApplicationDetails 
    {
        public ApplicationDetails() { }

        [XmlAttribute]
        public string ApplicationType { get; set; }
        [XmlAttribute]
        public string ApplicationExe { get; set; }
        
        [XmlElement]
        public List<NameValueAtribute> TargetApplicationAttributes { get; set; }
    }
    [Serializable]
    public class NameValueAtribute
    {        
        public NameValueAtribute() { }

        public NameValueAtribute(string name, string value) {
            this.Name = name;
            this.Value = value;
        }
        [XmlAttribute]
        public string Name { get; set; }
        [XmlAttribute]
        public string Value { get; set; }
    }

    public enum EventTypes
    {
        MouseRightClick,
        MouseLeftClick,
        MouseMiddleClick,
        MouseDoubleClick,
        KeyboardKeyPress,
        Wait
    }

    public enum TaskNames
    {
        GroupedKeys,
        CollectiveKey,
        Pause,
        PauseForInput,
        ChildUsecase
    }

    public enum ApplicationTypes
    {
        Win32, 
        WinForm,
        WPF,
        WebApplication,
        JavaApplication,
        Others,
        None
    }

    public enum HTMLPattern
    {
        Invoke,
        Focus,
        Check,
        Radio
    }

    [Serializable]
    public class Base
    {
        [XmlAttribute]
        public string Id { get; set; }
        [XmlAttribute]
        public string Name { get; set; }
        [XmlAttribute]
        public string Description { get; set; }
        [XmlAttribute]
        public string ParentId { get; set; }
        [XmlAttribute]
        public string Tags { get; set; }
        [XmlAttribute]
        public DateTime CreatedOn { get; set; }
    }

    public class PositionInfo
    {
        public bool HasSibling { get; set; }
        public int Position { get; set; }
    }

    public class ActivityIdDetails
    {
        public string Id { get; set; }
        public string ParentId { get; set; }
    }

    public static class ManageActivity
    {
        static Hashtable processNActivityMapping = new Hashtable();
        public static ActivityIdDetails GetActivityId(int processId = 0)
        {
            string id = System.Guid.NewGuid().ToString();
            string parentId = "";
            if (processId != 0)
            {
                if (processNActivityMapping.ContainsKey(processId))
                {
                    parentId = processNActivityMapping[processId].ToString();
                    
                    processNActivityMapping[processId] = id;
                }
                else
                    processNActivityMapping.Add(processId, id);
            }
            return new ActivityIdDetails() { Id = id, ParentId = parentId };
        }        
    }
}
