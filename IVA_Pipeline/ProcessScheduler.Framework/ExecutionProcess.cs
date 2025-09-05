/*=============================================================================================================== *
 * Copyright 2025 Infosys Ltd.                                                                                    *
 * Use of this source code is governed by Apache License Version 2.0 that can be found in the LICENSE file or at  *
 * http://www.apache.org/licenses/                                                                                *
 * ===============================================================================================================*/
/* 
 * © 2012-2013 Infosys Limited, Bangalore, India. All Rights Reserved.
 * Version: 1.0 b
 * Except for any open source software components embedded in this Infosys proprietary software program ("Program"),
 * this Program is protected by copyright laws, international treaties and other pending or existing intellectual
 * property rights in India, the United States and other countries. Except as expressly permitted, any unauthorized
 * reproduction, storage, transmission in any form or by any means (including without limitation electronic, mechanical,
 * printing, photocopying, recording or otherwise), or any distribution of this Program, or any portion of it, may
 * results in severe civil and criminal penalties, and will be prosecuted to the maximum extent possible under the law.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.ProcessScheduler.Framework
{
    public class SerializationOfProcess
    {
        public static String UTF8ByteArrayToString(Byte[] characters)
        {
            UTF8Encoding encoding = new UTF8Encoding();
            String constructedString = encoding.GetString(characters);
            return (constructedString);
        }
        public static Byte[] StringToUTF8ByteArray(String XmlString)
        {
            UTF8Encoding encoding = new UTF8Encoding();
            Byte[] byteArray = encoding.GetBytes(XmlString);
            return byteArray;
        }
    }
    
    public class Processes
    {
        ExecutionProcess[] processes2Execute;
        [XmlElement("Process")]
        public ExecutionProcess[] Processes2Execute
        {
            get { return processes2Execute; }
            set { processes2Execute = value; }
        }
    }
    
    [XmlRootAttribute("Process")]
    public class ExecutionProcess 
    {
        string id;
        [XmlAttribute]
        public string Id
        {
            get { return id; }
            set { id = value; }
        }

        string dll;
        [XmlAttribute]
        public string Dll
        {
            get { return dll; }
            set { dll = value; }
        }

        string fullClassName;
        [XmlAttribute]
        public string FullClassName
        {
            get { return fullClassName; }
            set { fullClassName = value; }
        }

       

        ModeType mode;
        [XmlAttribute]
        public ModeType Mode
        {
            get { return mode; }
            set { mode = value; }
        }

        string entityToBeWatched;
        [XmlAttribute]
        public string EntityToBeWatched
        {
            get { return entityToBeWatched; }
            set { entityToBeWatched = value; }
        }

        Drive[] drives;

        public Drive[] Drives
        {
            get { return drives; }
            set { drives = value; }
        }
    }
    public class Drive
    {
        string id;
        [XmlAttribute]
        public string Id
        {
            get { return id; }
            set { id = value; }
        }

        string letter;
        [XmlAttribute]
        public string Letter
        {
            get { return letter; }
            set { letter = value; }
        }

        string vhd;
        [XmlAttribute]
        public string Vhd
        {
            get { return vhd; }
            set { vhd = value; }
        }

        DriveType typeOfDrive;
        [XmlAttribute]
        public DriveType TypeOfDrive
        {
            get { return typeOfDrive; }
            set { typeOfDrive = value; }
        }
    }

    public enum DriveType
    {
        ForAll, ForService, ForDataEntity, ForDAL, ForMergeAndCompress, ForBuildAndDeploy
    }
    public enum DriveID
    {
        de, wcf, asmx, dal, mnc, bnd
    }
    public enum ProcessID
    {
        dataentitygeneration, wcfservicegeneration, asmxservicegeneration, dalgeneration, mergeandcompress, buildanddeploy, codegenstatuswatch, publishcheck
    }
    public enum ModeType
    {
        Table, Queue
    }
}
