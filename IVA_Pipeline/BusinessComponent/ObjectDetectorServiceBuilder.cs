/*=============================================================================================================== *
 * Copyright 2025 Infosys Ltd.                                                                                    *
 * Use of this source code is governed by Apache License Version 2.0 that can be found in the LICENSE file or at  *
 * http://www.apache.org/licenses/                                                                                *
 * ===============================================================================================================*/
﻿using System;
using Infosys.Solutions.Ainauto.VideoAnalytics.BusinessEntity;
using DE = Infosys.Solutions.Ainauto.VideoAnalytics.Resource.Entity.VideoAnalytics;
using FDDE = Infosys.Solutions.Ainauto.VideoAnalytics.Resource.Entity.Framedetail;
using DA = Infosys.Solutions.Ainauto.VideoAnalytics.Resource.DataAccess;
using System.Collections.Generic;
using Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.Common;
using System.Linq;
using Newtonsoft.Json;
using Infosys.Solutions.Ainauto.VideoAnalytics.Resource.DataAccess;
using static Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.Common.ApplicationConstants;
using System.IO;
using System.Text;
using System.Runtime.InteropServices;
using System.Net.Http;

namespace Infosys.Solutions.Ainauto.VideoAnalytics.BusinessComponent
{
    

    public class ObjectDetectorServiceBuilder
    {
        private static int IN_PROGRESS = 1;
        AppSettings appSettings = Config.AppSettings;
        public ConfigDetails GetDeviceConfigDetails(int tenantId, string deviceId)
        {
            ConfigDetails returnObj = new ConfigDetails();
            try
            {
                DE.Configuration configuration = new DE.Configuration()
                {
                    TenantId = tenantId,
                    ReferenceType = deviceId
                };

                DA.ConfigurationsDS configurationsDS = new DA.ConfigurationsDS();
                IList<DE.Configuration> res = configurationsDS.GetAll(configuration);
                returnObj = MapConfigDEtoModel(res, returnObj);
            }
            catch (Exception ex)
            {
                LogHandler.LogError("Exception Occured while fetching GetDeviceConfigDetails. Message : {0},Trace : {1}", LogHandler.Layer.Business, ex.Message,ex.StackTrace);
                throw ex;
            }
            return returnObj;
        }

        public ResourceAttributeDetails GetDeviceAttributeDetails(int tenantId, string deviceId)
        {
            
            ResourceAttributeDetails returnObj = new ResourceAttributeDetails()
            {
                DeviceId = deviceId,
                TenantId = tenantId
            };
            try
            {
                DE.ResourceAttribute resource_attributes = new DE.ResourceAttribute()
                {
                    ResourceId = deviceId,
                    TenantId = tenantId
                };
            DA.ResourceAttributesDS resourceAttributeDS = new DA.ResourceAttributesDS();
                var res = resourceAttributeDS.GetAll(resource_attributes).
                    Select(r => new Attributes
                    {
                        AttributeName = r.AttributeName,
                        AttributeValue = r.AttributeValue
                    }).ToList();
                returnObj.Attributes = res;


        }
            catch (Exception ex)
            {
                LogHandler.LogError("Exception Occured while fetching GetDeviceAttributeDetails. Message : {0},Trace : {1}", LogHandler.Layer.Business, JsonConvert.SerializeObject(ex.Message),JsonConvert.SerializeObject(ex.StackTrace));
                throw ex;
            }
            return returnObj;
        }

        public FeedProcessorMasterDetails GetFeedProcessorMasterWithVideoName(string videoName)
        {
            FeedProcessorMasterDetails feedProcessorMasterDetails = null;
            FeedProcessorMasterDS feedProcessorMasterDS = new FeedProcessorMasterDS();
            var obj = feedProcessorMasterDS.GetOneWithVideoSource(videoName);
            if (obj != null)
            {
                feedProcessorMasterDetails = MapFeedProcessorMasterDEtoBE(obj);
            }

            return feedProcessorMasterDetails;
        }
        static ConfigDetails MapConfigDEtoModel(IList<DE.Configuration> inpList, ConfigDetails retObj)
        {
            retObj.DeviceId = inpList.First().ReferenceType;
            retObj.TenantId = inpList.First().TenantId;
            foreach (var obj in inpList)
            {
                if (obj != null)
                {
                    switch (obj.ReferenceKey)
                    {
                        case "CAMERA_URL":
                            retObj.CameraURl = obj.ReferenceValue;
                            break;
                        case "STORAGE_BASE_URL":
                            retObj.StorageBaseUrl = obj.ReferenceValue;
                            break;
                        case "LOT_SIZE":
                            retObj.LotSize = Convert.ToInt32(obj.ReferenceValue) - 1;
                            break;
                        case "PREDICTION_MODEL":
                            retObj.ModelName = obj.ReferenceValue;
                            break;
                        case "VIDEO_FEED_TYPE":
                            retObj.VideoFeedType = obj.ReferenceValue;
                            break;
                        case "OFFLINE_VIDEO_DIRECTORY":
                            retObj.OfflineVideoDirectory = obj.ReferenceValue;
                            break;
                        case "ARCHIVE_LOCATION":
                            retObj.ArchiveDirectory = obj.ReferenceValue;
                            break;
                        case "ARCHIVE_ENABLED":
                            retObj.ArchiveEnabled = obj.ReferenceValue.Equals("Yes", StringComparison.InvariantCultureIgnoreCase) ? true : false;
                            break;
                        case "RENDERER_Q":
                            retObj.QueueName = obj.ReferenceValue;
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    
                    LogHandler.LogError("Configuration Value of {0} is Missing for Tenant ID: {1}, Device ID :{2}", LogHandler.Layer.Business, obj.ReferenceKey, obj.TenantId, obj.ReferenceType);

                    FaceMaskDetectionInvalidConfigException exception = new FaceMaskDetectionInvalidConfigException(String.Format("Configuration Value of {0} is Missing for Tenant ID: {0}, Device ID :{1}", obj.ReferenceKey, obj.TenantId, obj.ReferenceType));
                    throw exception;
                }

            }

            return retObj;
        }

        public bool UpdateResourceAttribute(ResourceAttributes resourceAttribute)
        {
            try
            {
                string userName = UserDetails.userName;
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    userName = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
                }
                
                ResourceAttributesDS resourceAttributesDS = new ResourceAttributesDS();
                DE.ResourceAttribute resource_entity = new DE.ResourceAttribute();
                resource_entity.TenantId = resourceAttribute.TenantId;
                resource_entity.ResourceId = resourceAttribute.ResourceId;
                resource_entity.AttributeName = resourceAttribute.AttributeName;
                resource_entity = resourceAttributesDS.GetOne(resource_entity);
                resource_entity.AttributeValue = resourceAttribute.AttributeValue;
                resource_entity.ModifiedBy = userName;
                resource_entity.ModifiedDate = DateTime.UtcNow;
                resourceAttributesDS.UpdateResourceAttribute(resource_entity);
                return true;
            }
            catch (Exception ex)
            {
                LogHandler.LogError("Exception Occured while Updating the status into Resource Attributes Table. Message : {0}", LogHandler.Layer.FrameGrabber, ex.Message);
                throw ex;
            }
        }

        public FeedStatusDetails GetFeedStatusDetail(string feedRequestId)
        {
#if DEBUG
            using (LogHandler.TraceOperations("ObjectDetectorServiceBuilder:GetProcessedFeedStatus", LogHandler.Layer.Business, Guid.NewGuid()))
            {
                LogHandler.LogInfo(String.Format(InfoMessages.Method_Execution_Start, "GetProcessedFeedStatus", "ObjectDetectorServiceBuilder"), LogHandler.Layer.Business, null);
#endif
                FeedStatusDetails feedStatusDetails = new FeedStatusDetails();
                ResourceAttributesDS resourceAttributesDS = new ResourceAttributesDS();
                FeedRequestDS feedRequestDS = new FeedRequestDS();
                var feedRequestEntity = feedRequestDS.GetOneWithRequestId(feedRequestId);
                FeedProcessorMasterDS feedProcessorMasterDS = new FeedProcessorMasterDS();

                if (feedRequestEntity != null)
                {
                    var feedMasterEntity = feedProcessorMasterDS.GetOne((int)feedRequestEntity.FeedProcessorMasterId);
                    if (feedMasterEntity != null)
                    {
                        feedStatusDetails.FeedId = feedRequestId;
                        
                        feedStatusDetails.VideoName = feedRequestEntity.VideoName;
                        if (feedMasterEntity.ProcessingEndTimeTicks != null)
                        {
                            feedStatusDetails.ProcessingEndTime = (long)feedMasterEntity.ProcessingEndTimeTicks;

                        }
                        if (feedMasterEntity.ProcessingStartTimeTicks != null)
                        {
                            feedStatusDetails.ProcessingStartTime = (long)feedMasterEntity.ProcessingStartTimeTicks;
                        }
                        if (feedRequestEntity.LastFrameProcessedTime != null)
                        {
                            DateTime feedDateTime = (DateTime)feedRequestEntity.LastFrameProcessedTime;
                            feedStatusDetails.LastPredictorTime = (long)feedDateTime.Ticks;
                        }

                    }
                    string configstring = JsonConvert.SerializeObject(GetDeviceAttributeDetails(feedRequestEntity.TenantId, feedRequestEntity.ResourceId));
                    var deviceConfig = Helper.AssignConfigValues(JsonConvert.DeserializeObject<AttributeDetailsResMsg>(configstring));
                    string URL = deviceConfig.MediaStreamingUrl;
                    HttpClient client = new HttpClient();
                    client.BaseAddress = new Uri(URL);
                    HttpResponseMessage response = client.GetAsync(URL).Result;
                    if (response.IsSuccessStatusCode && feedRequestEntity.Status == ProcessingStatus.inProgressStatus)
                    {

                        feedStatusDetails.Status = ProcessingStatus.inProgressStatus;
                    }
                    else if (feedRequestEntity.Status != ProcessingStatus.inProgressStatus)
                    {
                        feedStatusDetails.Status = feedRequestEntity.Status;
                    }
                    else if (!response.IsSuccessStatusCode && feedRequestEntity.Status == ProcessingStatus.inProgressStatus)
                    {

                        feedStatusDetails.Status = ProcessingStatus.RequestedStatus;
                    }

                    DE.ResourceAttribute resource_entity = new DE.ResourceAttribute();
                    resource_entity.TenantId = feedRequestEntity.TenantId;
                    resource_entity.ResourceId = feedRequestEntity.ResourceId;
                    resource_entity.AttributeName = ProcessingStatus.StreamingUrlAttributeName;
                    resource_entity = resourceAttributesDS.GetOne(resource_entity);
                    feedStatusDetails.StreamingUrl = resource_entity.AttributeValue;
                }

#if DEBUG
                LogHandler.LogInfo(String.Format(InfoMessages.Method_Execution_End, "GetProcessedFeedStatus", "ObjectDetectorServiceBuilder"), LogHandler.Layer.Business, null);
#endif
                return feedStatusDetails;
#if DEBUG
            }
#endif
        }

        public string InitiateFeedProcess(string fileName, string displayName)
        {
            string feedRequestId = String.Empty;
            List<string> allDevices = appSettings.Resources.Split(',').ToList();
            try
            {
                string predictionModels = appSettings.PredictionModel;
                string previewVideoFolder = appSettings.PreviewVideoFolder;
                string serviceVideoFolder = appSettings.ServiceFolder;
                FeedRequestDS feedRequestDS = new FeedRequestDS();
                FeedProcessorMasterDS feedProcessorMasterDS = new FeedProcessorMasterDS();
                ResourceAttributesDS resourceAttributesDS = new ResourceAttributesDS();
                List<string> availableResoures = feedRequestDS.GetAvailableDevices(allDevices);
                if (availableResoures.Count() <= 0)
                {
                    return ProcessingStatus.AllResourcesAreBusy;
                }
                var feedRequestEntity = feedRequestDS.GetOneWithRequestId(Path.GetFileNameWithoutExtension(fileName));
                feedRequestEntity.Model = resourceAttributesDS.GetPredictionModelWithDisplayName(displayName);
                string deviceId = availableResoures.FirstOrDefault();
                string configstring = JsonConvert.SerializeObject(GetDeviceAttributeDetails(feedRequestEntity.TenantId, deviceId));
                var deviceConfig = Helper.AssignConfigValues(JsonConvert.DeserializeObject<AttributeDetailsResMsg>(configstring));
                string sourceFile = Path.Combine(serviceVideoFolder, previewVideoFolder, fileName);
                string destinationFile = Path.Combine(deviceConfig.OfflineVideoDirectory, fileName);
                if (File.Exists(sourceFile))
                {
                    InsertUploadDetails(deviceId, feedRequestEntity, destinationFile);
                    File.Copy(sourceFile, destinationFile);
                }
                else
                {
                    return ProcessingStatus.FileNotFound;
                }
                return feedRequestEntity.RequestId;
            }


            catch (Exception ex)
            {
                LogHandler.LogError("Exception in InitiateFeedProcess method in ObjectDetectorServiceBuilder.cs : {0}", LogHandler.Layer.Business, ex.Message);
                throw ex;
            }

            return feedRequestId;
        }


        public string InsertUploadDetails(string deviceId,  FDDE.FeedRequest feedRequestEntity, string originalFileName)
        {

            FeedProcessorMasterDetails feedProcessorMasterDetails = new FeedProcessorMasterDetails()
            {
                ResourceId = deviceId,
                FileName = originalFileName, 
                FeedURI = originalFileName,
                ProcessingStartTimeTicks = DateTime.UtcNow.Ticks,
                CreatedBy = UserDetails.userName,
                CreatedDate = DateTime.UtcNow,
                TenantId = feedRequestEntity.TenantId,
                Status = 0,
                MachineName = System.Environment.MachineName,
            };
            FeedProcessorMasterDetails res = InsertFeedProcessorMaster(feedProcessorMasterDetails);
            int masterId = res.FeedProcessorMasterId;
            FeedRequestDS feedRequestDS = new FeedRequestDS();
            feedRequestEntity.Status = ProcessingStatus.RequestedStatus;
            feedRequestEntity.ResourceId = deviceId;
            feedRequestEntity.FeedProcessorMasterId = masterId;
            feedRequestDS.Update(feedRequestEntity);
            return feedRequestEntity.RequestId;
        }

        public static MediaMetadataDetail MapMediaMetaDataDEtoBE(FDDE.MediaMetadatum inpObj)
        {
            MediaMetadataDetail retObj = new MediaMetadataDetail();
            

            try
            {
                if (inpObj != null)
                {
                    if (inpObj.FeedProcessorMasterId != null)
                    {
                        retObj.FeedProcessorMasterId = (int)inpObj.FeedProcessorMasterId;

                    }
                    
                    retObj.CreatedBy = "";
                    retObj.CreatedDate = inpObj.CreatedDate;
                    retObj.ModifiedBy = inpObj.ModifiedBy;
                    retObj.ModifiedDate = inpObj.ModifiedDate;
                    retObj.TenantId = inpObj.TenantId;
                    
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
            return retObj;
        }


        public MediaMetadataDetail GetMediaMetadataWithMasterId(int masterId)
        {
            MediaMetadataDetail mediaMetadataDetail = null;
            MediaMetaDataDS mediaMetaDataDS = new MediaMetaDataDS();
            FDDE.MediaMetadatum mediametaData = new FDDE.MediaMetadatum();
            mediametaData.FeedProcessorMasterId = masterId;
            var obj = mediaMetaDataDS.GetOne(mediametaData);
            if (obj != null)
            {
                mediaMetadataDetail = MapMediaMetaDataDEtoBE(obj);
            }

            return mediaMetadataDetail;
        }

        public FeedProcessorMasterDetails GetFeedProcessorMasterWithMasterId(int masterId)
        {
            FeedProcessorMasterDetails feedProcessorMasterDetails = null;
            FeedProcessorMasterDS feedProcessorMasterDS = new FeedProcessorMasterDS();
            var obj = feedProcessorMasterDS.GetOneWithMasterId(masterId);
            if (obj != null)
            {
                feedProcessorMasterDetails = MapFeedProcessorMasterDEtoBE(obj);
            }

            return feedProcessorMasterDetails;
        }
        public FeedRequestRes GetFeedRequestWithMasterId(int masterId)
        {
            FeedRequestRes feedRequestDetails = null;
            FeedRequestDS feedRequestDS = new FeedRequestDS();
            var obj = feedRequestDS.GetOneWithMasterId(masterId);
            if (obj != null)
            {
                feedRequestDetails = MapFeedRequestDEtoBE(obj);
            }

            return feedRequestDetails;
        }

        public FeedRequestRes GetFeedRequestWithRequestId(string requestId)
        {
            FeedRequestRes feedRequestDetails = null;
            FeedRequestDS feedRequestDS = new FeedRequestDS();
            var obj = feedRequestDS.GetOneWithRequestId(requestId);
            if (obj != null)
            {
                feedRequestDetails = MapFeedRequestDEtoBE(obj);
            }

            return feedRequestDetails;
        }

        static FeedRequestRes MapFeedRequestDEtoBE(FDDE.FeedRequest inpObj)
        {
            FeedRequestRes retObj = new FeedRequestRes();
            try
            {
                retObj.CreatedBy = inpObj.CreatedBy;
                retObj.CreatedDate = inpObj.CreatedDate;
                retObj.FeedProcessorMasterId = inpObj.FeedProcessorMasterId;
                retObj.LastFrameGrabbedTime = inpObj.LastFrameGrabbedTime;
                retObj.LastFrameId = inpObj.LastFrameId;
                retObj.LastFrameProcessedTime = inpObj.LastFrameProcessedTime;
                retObj.ModifiedBy = inpObj.ModifiedBy;
                retObj.ModifiedDate = inpObj.ModifiedDate;
                retObj.RequestId = inpObj.RequestId;
                retObj.ResourceId = inpObj.ResourceId;
                retObj.Status = inpObj.Status;
                retObj.TenantId = inpObj.TenantId;
                retObj.VideoName = inpObj.VideoName;
                retObj.StartFrameProcessedTime = inpObj.StartFrameProcessedTime;
                retObj.Model = inpObj.Model;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return retObj;
        }

        public List<BusinessEntity.HistoryDetails> GetHistoryDetails(int tenantId, DateTime processingStartTime, DateTime processingEndTime)
        {
            try
            {

                FDDE.FeedRequest obj = new FDDE.FeedRequest()
                {
                    StartFrameProcessedTime = processingStartTime,
                    TenantId = tenantId,
                    LastFrameProcessedTime = processingEndTime
                };

                DA.FeedRequestDS feedRequestDS = new DA.FeedRequestDS();
                var res = MapFeedMasterFeedRequestDEtoBE(feedRequestDS.GetHistoryData(obj));
                return res;

            }
            catch (Exception ex)
            {
                LogHandler.LogError("Exception Occured while Getting the data into Feed Request Table. Message : {0}", LogHandler.Layer.FrameGrabber, ex.Message);
                throw ex;
            }
        }


        public static List<BusinessEntity.HistoryDetails> MapFeedMasterFeedRequestDEtoBE(List<DA.FeedRequestHistoryDetails> feedRequestHistoryDetails)
        {
            List<BusinessEntity.HistoryDetails> historyDetailsList = new List<BusinessEntity.HistoryDetails>();
            try
            {
                foreach (var inpObj in feedRequestHistoryDetails)
                {
                    BusinessEntity.HistoryDetails retObj = new BusinessEntity.HistoryDetails();
                    retObj.RequestId = inpObj.RequestId;
                    retObj.ResourceId = inpObj.ResourceId;
                    if (inpObj.FeedProcessorMasterId != 0)
                    {
                        retObj.FeedProcessorMasterId = inpObj.FeedProcessorMasterId;
                    }
                    if (inpObj.LastFrameId != null)
                    {
                        retObj.LastFrameId = inpObj.LastFrameId;
                    }
                    if (inpObj.LastFrameGrabbedTime != null)
                    {
                        retObj.LastFrameGrabbedTime = inpObj.LastFrameGrabbedTime;
                    }
                    if (inpObj.LastFrameProcessedTime != null)
                    {
                        retObj.LastFrameProcessedTime = inpObj.LastFrameProcessedTime.ToString();
                    }
                    if (inpObj.StartFrameProcessedTime != null)
                    {
                        retObj.StartFrameProcessedTime = inpObj.StartFrameProcessedTime.ToString();
                    }
                    
                    var mediaMetadata = JsonConvert.DeserializeObject<Infosys.Solutions.Ainauto.VideoAnalytics.Resource.DataAccess.MediaMetadata>(inpObj.VideoMetadata);
                    retObj.VideoMetaData = new BusinessEntity.MediaMetadata();
                    retObj.VideoMetaData.FileExtension = mediaMetadata.FileExtension;
                    retObj.VideoMetaData.FileSize = mediaMetadata.FileSize;
                    retObj.VideoMetaData.Format = mediaMetadata.Format;
                    retObj.VideoMetaData.Fps = mediaMetadata.Fps;
                    retObj.VideoMetaData.FrameSize = mediaMetadata.FrameSize;
                    retObj.VideoMetaData.VideoDuration = mediaMetadata.VideoDuration;
                    retObj.VideoMetaData.ColorModel = mediaMetadata.ColorModel;
                    retObj.VideoMetaData.BitRateKbs = mediaMetadata.BitRateKbs;
                    retObj.TenantId = inpObj.TenantId;
                    retObj.Status = inpObj.Status;
                    retObj.VideoName = Path.GetFileName(inpObj.VideoName);
                    retObj.FileName = inpObj.FileName;
                    retObj.FeedURI = inpObj.FeedURI;
                    retObj.ProcessingStartTimeTicks = inpObj.ProcessingStartTimeTicks;
                    retObj.ProcessingEndTimeTicks = inpObj.ProcessingEndTimeTicks;
                    retObj.MachineName = inpObj.MachineName;
                    retObj.VideoDuration = inpObj.VideoDuration;
                    retObj.Fps = inpObj.Fps;
                    retObj.FileSize = inpObj.FileSize;
                    retObj.FrameProcessedRate = inpObj.FrameProcessedRate;
                    retObj.TotalFrameProcessed = inpObj.TotalFrameProcessed;
                    retObj.TimeTaken = inpObj.TimeTaken;
                    ResourceAttributesDS resourceAttributesDS = new ResourceAttributesDS();
                    retObj.ModelName = resourceAttributesDS.GetDisplayNameWithPredictionModel(inpObj.ModelName);
                    historyDetailsList.Add(retObj);
                }



            }
            catch (Exception ex)
            {
                throw ex;
            }
            return historyDetailsList;
        }

        public UpdateResourceAttribute UploadVideo(int tenantId, Stream fileContents, string contentType)
        {
          
            string predictionModels = appSettings.PredictionModel;
            string previewVideoFolder = appSettings.PreviewVideoFolder;
            string serviceVideoFolder = appSettings.ServiceFolder;
            List<string> allDevices = appSettings.Resources.Split(',').ToList();
            string feedRequestId = Guid.NewGuid().ToString();
            FeedRequestDS feedRequestDS = new FeedRequestDS();
            FeedProcessorMasterDS feedProcessorMasterDS = new FeedProcessorMasterDS();
            List<string> availableResoures = feedRequestDS.GetAvailableDevices(allDevices);
            UpdateResourceAttribute uploadResponseMsg = new UpdateResourceAttribute();
            if (availableResoures.Count() <= 0)
            {
                uploadResponseMsg.Message = ProcessingStatus.AllResourcesAreBusy;
                return uploadResponseMsg;
            }
            try
            {
                int index = 2;
                if (!contentType.StartsWith("multipart/form-data"))
                {
                    throw new ArgumentException("Error, invalid Content-Type: " + contentType);
                }
                string boundary = contentType.Substring("multipart/form-data".Length);
                byte[] boundaryBytes = Encoding.ASCII.GetBytes(boundary);
                MemoryStream ms = new MemoryStream();
                fileContents.CopyTo(ms);
                byte[] requestBytes = ms.ToArray();
                string originalFileName = GetOriginalFileName(requestBytes, ref index);
                string previewFileName = Path.Combine(previewVideoFolder, feedRequestId + FileExtensions.mp4);
                string previewFullFilepath = Path.Combine(serviceVideoFolder, previewVideoFolder, feedRequestId + FileExtensions.mp4);
                byte[] file = new byte[requestBytes.Length - index];
                Array.Copy(requestBytes, index, file, 0, file.Length);
                File.WriteAllBytes(previewFullFilepath, file);
                string format = "";
               MediaMetaDataMsg mediaMetaDataMsgReq;
                (mediaMetaDataMsgReq, format) = Helper.ExtractVideoMetaData(previewFullFilepath, tenantId);

                MediaMetadataDetail retObj = new MediaMetadataDetail();
                var inpObj = mediaMetaDataMsgReq.MediaMetadataDetail;
                retObj.FeedProcessorMasterId = inpObj.FeedProcessorMasterId;
                retObj.CreatedBy = inpObj.CreatedBy;
                retObj.CreatedDate = inpObj.CreatedDate;
                retObj.ModifiedBy = inpObj.ModifiedBy;
                retObj.ModifiedDate = inpObj.ModifiedDate;
                retObj.TenantId = inpObj.TenantId;
                retObj.MetaData = inpObj.MetaData;
                retObj.RequestId = feedRequestId;
                InsertMediaMetaData(retObj);
                InsertRequestDetails(tenantId, originalFileName, feedRequestId);

                uploadResponseMsg.PreviewPath = previewFileName;
                uploadResponseMsg.VideoFormat = format;

                return uploadResponseMsg;
            }
            catch (Exception ex)
            {
                LogHandler.LogError("Exception in UploadVideoFile method in ObjectDetectorServiceBuilder.cs : {0}", LogHandler.Layer.Business, ex.Message);
                throw ex;
            }


        }

        public void InsertRequestDetails(int tenantId, string originalFileName, string feedRequestId)
        {

            FeedRequestDS feedRequestDS = new FeedRequestDS();
            FDDE.FeedRequest feed_Request_entity = new FDDE.FeedRequest()
            {
                RequestId = feedRequestId,
                VideoName = originalFileName,
                Status = ProcessingStatus.initiatedStatus,
                TenantId = tenantId,
            };
            feedRequestDS.Insert(feed_Request_entity);
        }

        public MediaMetaDataMsg InsertMediaMetaData(MediaMetadataDetail inpObj)
        {
            try
            {
                FDDE.MediaMetadatum obj = new FDDE.MediaMetadatum()
                {
                    CreatedBy = UserDetails.userName, 
                    CreatedDate = inpObj.CreatedDate, 
                    TenantId = inpObj.TenantId, 
                    FeedProcessorMasterId = inpObj.FeedProcessorMasterId,
                    MetaData = inpObj.MetaData,
                    RequestId = inpObj.RequestId,
                    ModifiedBy = inpObj.ModifiedBy,
                    ModifiedDate = inpObj.ModifiedDate,

                };

                DA.MediaMetaDataDS mediaMetaDataDS = new DA.MediaMetaDataDS();
                obj = mediaMetaDataDS.Insert(obj);
                MediaMetaDataMsg mediaMetaDataMsgRes = new MediaMetaDataMsg();
                return mediaMetaDataMsgRes;

            }
            catch (Exception ex)
            {
                LogHandler.LogError("Exception Occured while Inserting the data into Feed Processor Master Table. Message : {0}", LogHandler.Layer.FrameGrabber, ex.Message);
                throw ex;
            }
        }
        string GetOriginalFileName(byte[] buffer, ref int index)
        {
            StringBuilder sb = new StringBuilder();
            string fileName = string.Empty;
            while (true)
            {
                StringBuilder line_sb = new StringBuilder();
                while (index < buffer.Length - 1 && buffer[index] != 0x0D && buffer[index] != 0x0A)
                {

                    line_sb.Append((char)buffer[index]);
                    index++;
                }
                index += 2;
                if (line_sb.ToString() == "")
                {
                    break;
                }
                if (line_sb.ToString().Contains("filename"))
                {
                    string line = line_sb.ToString();
                    fileName = line.Split(';')[2].Replace("filename=", "").Replace("\"", "");
                }
                sb.Append(line_sb.ToString());
            }
            return fileName;
        }


        public bool UpdateMediaMetaData(MediaMetadataDetail inpObj)
        {
            try
            {


                DA.MediaMetaDataDS mediaMetaDataDS = new DA.MediaMetaDataDS();
                FDDE.MediaMetadatum obj = mediaMetaDataDS.UpdateFeedMasterId(inpObj.FeedProcessorMasterId, inpObj.RequestId);
                
                return true;

            }
            catch (Exception ex)
            {
                LogHandler.LogError("Exception Occured while Updating the data into Feed Processor Master Table. Message : {0}", LogHandler.Layer.FrameGrabber, ex.Message);
                throw ex;
            }
        }
        public bool UpdateFeedRequestMaster(FeedRequestRes feed_request)
        {
            try
            {
                DA.FeedRequestDS feedRequestDS = new DA.FeedRequestDS();
                
                FDDE.FeedRequest obj = new FDDE.FeedRequest()
                {
                    FeedProcessorMasterId = feed_request.FeedProcessorMasterId,
                    LastFrameGrabbedTime = feed_request.LastFrameGrabbedTime,
                    ModifiedBy = UserDetails.userName,
                    ModifiedDate = DateTime.UtcNow,
                    TenantId = feed_request.TenantId,
                    Status = feed_request.Status,
                    LastFrameId = feed_request.LastFrameId,
                    LastFrameProcessedTime = feed_request.LastFrameProcessedTime,
                    VideoName = feed_request.VideoName,
                    CreatedBy = UserDetails.userName,
                    CreatedDate = DateTime.UtcNow,
                    RequestId = feed_request.RequestId,
                    ResourceId = feed_request.ResourceId,
                    StartFrameProcessedTime = feed_request.StartFrameProcessedTime,
                    Model = feed_request.Model
                };


                obj = feedRequestDS.Update(obj);
                return true;

            }
            catch (Exception ex)
            {
                LogHandler.LogError("Exception Occured while Updating the data into Feed Processor Master Table. Message : {0}", LogHandler.Layer.FrameGrabber, ex.Message);
                throw ex;
            }
        }


        public ConfigData GetConfiguration(string tid, string jobName)
        {
            ConfigData returnObj = new ConfigData();
            try
            {
                
                DA.ConfigurationsDS configurationsDS = new DA.ConfigurationsDS();
                
                var response = configurationsDS.GetAll(new Resource.Entity.VideoAnalytics.Configuration()
                {
                    TenantId = Convert.ToInt32(tid),
                    ReferenceType = jobName
                }).ToList();

                Dictionary<string, string> dict = new Dictionary<string, string>();
                response.ForEach(x => dict.Add(x.ReferenceKey, x.ReferenceValue));

                if (dict.ContainsKey("BaseURI"))
                {
                    returnObj.BaseURI = dict["BaseURI"];
                }
                if (dict.ContainsKey("SuperBot_EndPoint"))
                {
                    returnObj.MetricIngestor_EndPoint = dict["SuperBot_EndPoint"];
                }
                if (dict.ContainsKey("Port"))
                {
                    returnObj.Port = dict["Port"];
                }

            }
            catch (Exception ex)
            {
                LogHandler.LogError("Exception Occured while fetching GetConfiguration. Message : {0}", LogHandler.Layer.Business, ex.Message);
                throw ex;
            }
            return returnObj;
        }

        static ConfigDetails MapDEtoModel(IList<DE.ResourceAttribute> inpList, ConfigDetails retObj)
        {
            foreach (var obj in inpList)
            {
                if (obj != null)
                {
                    switch (obj.AttributeName)
                    {
                        case "CAMERA_URL":
                            retObj.CameraURl = obj.AttributeValue;
                            break;
                        case "STORAGE_BASE_URL":
                            retObj.StorageBaseUrl = obj.AttributeValue;
                            break;
                        case "LOT_SIZE":
                            retObj.LotSize = Convert.ToInt32(obj.AttributeValue) - 1;
                            break;
                        case "PREDICTION_MODEL":
                            retObj.ModelName = obj.AttributeValue;
                            break;
                        case "VIDEO_FEED_TYPE":
                            retObj.VideoFeedType = obj.AttributeValue;
                            break;
                        case "OFFLINE_VIDEO_DIRECTORY":
                            retObj.OfflineVideoDirectory = obj.AttributeValue;
                            break;
                        case "ARCHIVE_LOCATION":
                            retObj.ArchiveDirectory = obj.AttributeValue;
                            break;
                        case "ARCHIVE_ENABLED":
                            retObj.ArchiveEnabled = obj.AttributeValue.Equals("Yes", StringComparison.InvariantCultureIgnoreCase) ? true : false;
                            break;
                        case "RENDERER_Q":
                            retObj.QueueName = obj.AttributeValue;
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                   
                    LogHandler.LogError("Configuration Value of {0} is Missing for Tenant ID: {1}, Device ID :{2}", LogHandler.Layer.Business, obj.AttributeName, obj.TenantId, obj.ResourceId);

                    FaceMaskDetectionInvalidConfigException exception = new FaceMaskDetectionInvalidConfigException(String.Format("Configuration Value of {0} is Missing for Tenant ID: {0}, Device ID :{1}", obj.AttributeName, obj.TenantId, obj.ResourceId));
                    List<ValidationError> validationErrors_List = new List<ValidationError>();
                    ValidationError validationErr = new ValidationError();
                    validationErr.Code = "1049";
                    validationErr.Description = String.Format("Configuration Value of {0} is Missing for Tenant ID: {0}, Device ID :{1}", obj.AttributeName, obj.TenantId, obj.ResourceId);
                    validationErrors_List.Add(validationErr);

                    if (validationErrors_List.Count > 0)
                    {
                        exception.Data.Add("DataNotFoundErrors", validationErrors_List);
                        throw exception;
                    }
                }

            }

            return retObj;
        }

       

        public bool getClientStatus(string deviceId, string tenantId)
        {
#if DEBUG
            using (LogHandler.TraceOperations("FrameRendererProcess:getClientStatus", LogHandler.Layer.Business, Guid.NewGuid(),null))
            {
                
#endif
                DA.ResourceAttributesDS resourceAttributesDS = new DA.ResourceAttributesDS();
                Infosys.Solutions.Ainauto.VideoAnalytics.Resource.Entity.VideoAnalytics.ResourceAttribute resource_entity = new Infosys.Solutions.Ainauto.VideoAnalytics.Resource.Entity.VideoAnalytics.ResourceAttribute();
                resource_entity.ResourceId = deviceId;
                resource_entity.AttributeName = ApplicationConstants.FrameRendererKey.clientActivationAttribute;
                resource_entity.TenantId = Convert.ToInt32(tenantId);
#if DEBUG
               
#endif
                return resourceAttributesDS.getClientStatus(resource_entity);
#if DEBUG
            }
#endif
        }

        public FeedProcessorMasterDetails InsertFeedProcessorMaster(FeedProcessorMasterDetails inpObj)
        {
            try
            {
                FDDE.FeedProcessorMaster obj = new FDDE.FeedProcessorMaster()
                {
                    ResourceId = inpObj.ResourceId, 
                    FileName = inpObj.FileName,
                    FeedUri = inpObj.FeedURI, 
                    ProcessingStartTimeTicks = inpObj.ProcessingStartTimeTicks, 
                    CreatedBy = inpObj.CreatedBy, 
                    CreatedDate = inpObj.CreatedDate,
                    TenantId = inpObj.TenantId, 
                    Status = inpObj.Status, 
                    MachineName = inpObj.MachineName 
                };

                DA.FeedProcessorMasterDS feedProcessorMasterDS = new DA.FeedProcessorMasterDS();
                obj = feedProcessorMasterDS.Insert(obj);
                var res = MapFeedProcessorMasterDEtoBE(obj);
                return res;

            }
            catch (Exception ex)
            {
                LogHandler.LogError("Exception Occured while Inserting the data into Feed Processor Master Table. Message : {0}", LogHandler.Layer.Business, ex.Message);
                throw ex;
            }
        }

        public FeedProcessorMasterDetails UpdateFeedProcessorMaster(FeedProcessorMasterDetails inpObj)
        {
            try
            {
                FDDE.FeedProcessorMaster obj = new FDDE.FeedProcessorMaster();
                obj.FeedProcessorMasterId = inpObj.FeedProcessorMasterId;
                obj.ProcessingEndTimeTicks = inpObj.ProcessingEndTimeTicks;
                obj.ModifiedBy = inpObj.ModifiedBy;
                obj.ModifiedDate = inpObj.ModifiedDate;
                obj.TenantId = inpObj.TenantId;
                obj.Status = inpObj.Status;

                DA.FeedProcessorMasterDS feedProcessorMasterDS = new DA.FeedProcessorMasterDS();
                obj = feedProcessorMasterDS.Update(obj);
                var res = MapFeedProcessorMasterDEtoBE(obj);
                return res;

            }
            catch (Exception ex)
            {
                LogHandler.LogError("Exception Occured while Updating the data into Feed Processor Master Table. Message : {0}", LogHandler.Layer.Business, ex.Message);
                throw ex;
            }
        }

        static FeedProcessorMasterDetails MapFeedProcessorMasterDEtoBE(FDDE.FeedProcessorMaster inpObj)
        {
            FeedProcessorMasterDetails feedProcessorMasterDetails = new FeedProcessorMasterDetails();
            try
            {
               
                
                if(inpObj != null)
                {
                   

                    feedProcessorMasterDetails.FeedProcessorMasterId = inpObj.FeedProcessorMasterId;
                    feedProcessorMasterDetails.ProcessingEndTimeTicks = inpObj.ProcessingEndTimeTicks;
                    feedProcessorMasterDetails.ModifiedBy = inpObj.ModifiedBy;
                    feedProcessorMasterDetails.ModifiedDate = inpObj.ModifiedDate;
                    feedProcessorMasterDetails.TenantId = inpObj.TenantId;
                    if (inpObj.TimeTaken != null)
                    {
                        feedProcessorMasterDetails.TimeTaken = (int)inpObj.TimeTaken;

                    }
                    if (inpObj.TotalFrameProcessed != null)
                    {
                        feedProcessorMasterDetails.TotalFrameProcessed = (int)inpObj.TotalFrameProcessed;

                    }
                    feedProcessorMasterDetails.ProcessingStartTimeTicks = inpObj.ProcessingStartTimeTicks;
                    feedProcessorMasterDetails.MachineName = inpObj.MachineName;
                    feedProcessorMasterDetails.FrameProcessedRate = inpObj.FrameProcessedRate;
                    feedProcessorMasterDetails.ResourceId = inpObj.ResourceId;
                    feedProcessorMasterDetails.FeedURI = inpObj.FeedUri;
                    feedProcessorMasterDetails.CreatedBy = inpObj.CreatedBy;
                    feedProcessorMasterDetails.CreatedDate = inpObj.CreatedDate;
                    feedProcessorMasterDetails.FileName = inpObj.FileName;
                    if (inpObj.Status != null)
                    {
                        feedProcessorMasterDetails.Status = (int)inpObj.Status;
                    }
                }
              

            }
            catch (Exception ex)
            {
                throw ex;
            }
            return feedProcessorMasterDetails;
        }

        public FeedProcessorMasterDetails GetInCompletedFramGrabberDetails(int tenantId, string deviceId)
        {
            try
            {
                FDDE.FeedProcessorMaster obj = new FDDE.FeedProcessorMaster()
                {
                    ResourceId = deviceId,
                    TenantId = tenantId,
                    Status = IN_PROGRESS
                };

                DA.FeedProcessorMasterDS feedProcessorMasterDS = new DA.FeedProcessorMasterDS();
                var res = MapFeedProcessorMasterDEtoBE(feedProcessorMasterDS.GetInProgressData(obj));
                return res;

            }
            catch (Exception ex)
            {
                LogHandler.LogError("Exception Occured while Getting the data into Feed Processor Master Table. Message : {0}", LogHandler.Layer.Business, ex.Message);
                throw ex;
            }
        }

      
    }
}
