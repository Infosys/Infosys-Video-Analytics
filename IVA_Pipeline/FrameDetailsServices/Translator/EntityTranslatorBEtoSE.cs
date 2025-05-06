/*=============================================================================================================== *
 * Copyright 2025 Infosys Ltd.                                                                                    *
 * Use of this source code is governed by Apache License Version 2.0 that can be found in the LICENSE file or at  *
 * http://www.apache.org/licenses/                                                                                *
 * ===============================================================================================================*/
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.Common;
using FrameDetailsServices.Models;
using BE = Infosys.Solutions.Ainauto.VideoAnalytics.BusinessEntity;
using Newtonsoft.Json;

namespace FrameDetailsServices.Translator
{
    public static class EntityTranslatorBEtoSE
    {
        public static GetDeviceConfigurationResMsg FrameGrabberConfigBEtoSE(BE.ConfigDetails objBE)
        {
            GetDeviceConfigurationResMsg resObj = new GetDeviceConfigurationResMsg();
            DeviceConfiguration deviceConfiguration = new DeviceConfiguration();
            try
            {
                deviceConfiguration.CameraURl = objBE.CameraURl;
                deviceConfiguration.ArchiveDirectory = objBE.ArchiveDirectory;
                deviceConfiguration.ArchiveEnabled = objBE.ArchiveEnabled;
                deviceConfiguration.DeviceId = objBE.DeviceId;
                deviceConfiguration.LotSize = objBE.LotSize;
                deviceConfiguration.ModelName = objBE.ModelName;
                deviceConfiguration.OfflineVideoDirectory = objBE.OfflineVideoDirectory;
                deviceConfiguration.QueueName = objBE.QueueName;
                deviceConfiguration.StorageBaseUrl = objBE.StorageBaseUrl;
                deviceConfiguration.TenantId = objBE.TenantId;
                deviceConfiguration.VideoFeedType = objBE.VideoFeedType;

                resObj.DeviceConfiguration = deviceConfiguration;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return resObj;
        }

        public static FeedMasterResMsg FeedMasterDetailsBEtoSE(BE.FeedProcessorMasterDetails objBE)
        {
            FeedMasterResMsg objSE = new FeedMasterResMsg();
            try
            {
                objSE.FeedMaster = FeedDetailsBEtoSE(objBE);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return objSE;
        }

        public static FeedMaster FeedDetailsBEtoSE(BE.FeedProcessorMasterDetails objBE)
        {
            FeedMaster objSE = new FeedMaster();
            try
            {
                string jsonString = JsonConvert.SerializeObject(objBE);
                objSE = JsonConvert.DeserializeObject<FeedMaster>(jsonString);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return objSE;
        }
    }
}