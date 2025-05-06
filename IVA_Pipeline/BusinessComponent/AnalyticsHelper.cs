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
using DE = Infosys.Solutions.Ainauto.VideoAnalytics.Resource.Entity;
//using DE1=Infosys.Solutions.Ainauto.VideoAnalytics.Resource.Entity.Framedetail;
using QE = Infosys.Solutions.Ainauto.VideoAnalytics.Resource.Entity.Queue;
using Infosys.Solutions.Ainauto.VideoAnalytics.Resource.DataAccess;
using Newtonsoft.Json;
using Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.Common;
using BE = Infosys.Solutions.Ainauto.VideoAnalytics.BusinessEntity.Analytics;
using System.Configuration;

namespace Infosys.Solutions.Ainauto.VideoAnalytics.BusinessComponent
{
    public class AnalyticsHelper
    {
        public static string predictionType = string.Empty;
        public static bool UpdateFrameMasterDetail(QE.PersonCountQueueMsg message)
        {
            FrameMasterDS frameMasterDS = new FrameMasterDS();
            BE.FrameMasterPersonCount pc = new BE.FrameMasterPersonCount()
            {
                TotalPersonCount = message.Fs.Length,
                NewPersonCount = message.Fs.Where(f => f.Np == "Yes").ToList().Count
            };

            string jsonString = JsonConvert.SerializeObject(pc);
            jsonString = jsonString.Remove(jsonString.Length - 1, 1).Remove(0, 1).Insert(0, ",");

#if DEBUG
            LogHandler.LogDebug("Updating the FrameMaster table", LogHandler.Layer.Business, null);
#endif
            var entity = frameMasterDS.UpdatePersonCount(new DE.Framedetail.FrameMaster() { FrameId = message.Fid, ResourceId = message.Did, TenantId = Convert.ToInt32(message.Tid), ClassPredictionCount = jsonString });
            if (entity != null)
                return true;
            return false;
        }

        public static bool InsertFrameMetaDataDetail(QE.PersonCountQueueMsg message, int partitionKey, DateTime frameGrabTime)
        {
            FrameMetaDataDS frameMetaDataDS = new FrameMetaDataDS();
            string frameMetadata = JsonConvert.SerializeObject(message);
#if DEBUG
            LogHandler.LogDebug("Updating the FrameMetadata table.", LogHandler.Layer.Business, null);
#endif            
            
            var insEntity = frameMetaDataDS.Insert(new DE.Framedetail.FrameMetadatum()
            {
                ResourceId = message.Did,
                FeedProcessorMasterId = Convert.ToInt32(message.FeedId),
                FrameId = message.Fid,
                FrameGrabTime = frameGrabTime,
                MetaData = frameMetadata,
                PartitionKey = partitionKey,
                MachineName = System.Environment.MachineName,
                Status = "",
                TenantId = Convert.ToInt32(message.Tid),
                PredictionType=predictionType
                
            });

            if (insEntity != null)
                return true;
            else
                return false;
            
        }
        public static IList<DE.Framedetail.FramePredictedClassDetail> GetRegions(string frameId, string deviceId, int tenantId)
        {
#if DEBUG
            LogHandler.LogInfo(InfoMessages.Method_Execution_Start, LogHandler.Layer.Business, "GetRegions", "AnalyticsHelper");
            LogHandler.LogDebug("The GetRegions method of AnalyticsHelper class is getting executed with parameters frameId: {0}, deviceId: {1}, tenantId: {2}",
                LogHandler.Layer.Business, frameId, deviceId, tenantId);
#endif
            try
            {
                FramePredictedClassDetailsDS frameMetaDataDS = new FramePredictedClassDetailsDS();
                var retList = frameMetaDataDS.GetAll(new DE.Framedetail.FramePredictedClassDetail()
                {
                    FrameId = frameId,
                    ResourceId = deviceId,
                    TenantId = tenantId
                });
                
#if DEBUG
                LogHandler.LogInfo(InfoMessages.Method_Execution_End, LogHandler.Layer.Business, "GetRegions", "AnalyticsHelper");
#endif
                return retList;
            }
            catch (Exception ex)
            {
                LogHandler.LogError($"The GetRegions method of AnalyticsHelper threw an error for FrameId: {frameId}\t Device ID: {deviceId}\t Tenant ID: {tenantId}. Message : {ex.Message}",
                    LogHandler.Layer.Business,null);
                return null;
            }
        }
        public static IList<QE.BoundingBox> UpdateRegions(string frameId, string deviceId, int tenantId)
        {
            try
            {
                FramePredictedClassDetailsDS frameMetaDataDS = new FramePredictedClassDetailsDS();
                var retList = frameMetaDataDS.GetAll(new DE.Framedetail.FramePredictedClassDetail()
                {
                    FrameId = frameId,
                    ResourceId = deviceId,
                    TenantId = tenantId
                });
                List<QE.BoundingBox> regions = retList.Where(r => r.Region != "" && r.Region != null).Select(s => JsonConvert.DeserializeObject<QE.BoundingBox>(s.Region)).ToList();
                return regions;
            }
            catch (Exception ex)
            {
                LogHandler.LogError($"Error when getting the region details for FrameId: {frameId}\t Device ID: {deviceId}\t Tenant ID: {tenantId}. Message : {ex.Message}",
                    LogHandler.Layer.Business, null);
                return null;
            }
        }
    } 
}
