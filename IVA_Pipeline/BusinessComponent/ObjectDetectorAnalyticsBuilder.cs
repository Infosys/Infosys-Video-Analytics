/*=============================================================================================================== *
 * Copyright 2025 Infosys Ltd.                                                                                    *
 * Use of this source code is governed by Apache License Version 2.0 that can be found in the LICENSE file or at  *
 * http://www.apache.org/licenses/                                                                                *
 * ===============================================================================================================*/
﻿
using Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.Common;
using System;
using System.Collections.Generic;
using DA = Infosys.Solutions.Ainauto.VideoAnalytics.Resource.DataAccess;
using DE = Infosys.Solutions.Ainauto.VideoAnalytics.Resource.Entity;
using Infosys.Solutions.Ainauto.VideoAnalytics.BusinessEntity;
using Newtonsoft.Json;
using System.Linq;
using System.Globalization;
using System.Configuration;
using Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.TaskRoute;

namespace Infosys.Solutions.Ainauto.VideoAnalytics.BusinessComponent
{
    public class ObjectDetectorAnalyticsBuilder
    {
        private static AppSettings appSettings = Config.AppSettings;
        public static DeviceDetails deviceDetails=ConfigHelper.SetDeviceDetails(appSettings.TenantID.ToString(),appSettings.DeviceID,CacheConstants.ObjectDetectorAnalytics);
        string predictionType=deviceDetails.PredictionType;
        public List<ObjectDetectorAnalyticsRes> GetLocationBasedCount(ObjectDetectorAnalytics inpObj)
        {
            double maskCount = 0;
            double nomaskCount = 0;
            List<ObjectDetectorAnalyticsRes> resultList = new List<ObjectDetectorAnalyticsRes>();


            Dictionary<string, List<ObjectDetectorAnalyticsDetail>> locationDetails = GetLocationBasedDetails(inpObj);

            DateTime startTime = inpObj.StartTime;

            if (inpObj.TimeInterval != 0)
            {
                foreach (KeyValuePair<string, List<ObjectDetectorAnalyticsDetail>> item in locationDetails)
                {

                    List<DateTime> startTimeList = new List<DateTime>();
                    for (var startingTime = startTime; startingTime < inpObj.EndTime; startingTime = startingTime.AddMinutes(inpObj.TimeInterval))
                    {
                        startTimeList.Add(startingTime);
                    }


                    var intervalGroupedDetails = startTimeList.Select(intervalStartTime => GroupDetailsByTime(intervalStartTime, inpObj.TimeInterval, item.Value, null, inpObj.TenantId, item.Key, false, inpObj.UniquePersonCount));


                    resultList.AddRange(intervalGroupedDetails.Where(dataList => dataList.Details.Any()).ToList());

                }
            }
            else
            {

                foreach (KeyValuePair<string, List<ObjectDetectorAnalyticsDetail>> item in locationDetails)
                {
                    if (item.Value != null)
                    {

                        var intervalGroupedDetails = GroupDetails(startTime, item.Value, inpObj.DeviceId, inpObj.TenantId, item.Key, false, inpObj.UniquePersonCount);
                        resultList.Add(intervalGroupedDetails);
                    }
                }


            }


            return resultList;
        }

        public List<ObjectDetectorAnalyticsRes> GetDeviceBasedCount(ObjectDetectorAnalytics inpObj)
        {
            double maskCount = 0;
            double nomaskCount = 0;
            List<ObjectDetectorAnalyticsRes> resultList = new List<ObjectDetectorAnalyticsRes>();
            DateTime startTime = inpObj.StartTime;
            DateTime IntervalStartTime = new DateTime(startTime.Year, startTime.Month, startTime.Day, startTime.Hour, startTime.Minute, 0);
            List<ObjectDetectorAnalyticsDetail> countDetails = GetDeviceBasedDetails(inpObj);

            if (inpObj.TimeInterval != 0)
            {
                List<DateTime> startTimeList = new List<DateTime>();
                for (var startingTime = startTime; startingTime < inpObj.EndTime; startingTime = startingTime.AddMinutes(inpObj.TimeInterval))
                {
                    startTimeList.Add(startingTime);
                }
                var intervalGroupedDetails = startTimeList.Select(intervalStartTime => GroupDetailsByTime(intervalStartTime, inpObj.TimeInterval, countDetails, inpObj.DeviceId, inpObj.TenantId, null, true, inpObj.UniquePersonCount)).ToList();
                resultList = intervalGroupedDetails.Where(dataList => dataList.Details.Any()).ToList();
            }
            else
            {

                if (countDetails != null)
                {
                    List<ObjectDetectorAnalyticsDetail> dataInTimeRange = countDetails.Where(data => data.GrabberTime >= startTime && data.GrabberTime <= inpObj.EndTime).ToList();
                    var intervalGroupedDetails = GroupDetails(startTime, countDetails, inpObj.DeviceId, inpObj.TenantId, null, true, inpObj.UniquePersonCount);
                    resultList.Add(intervalGroupedDetails);
                }
            }
            return resultList;

        }

        public List<ObjectDetectorAnalyticsDetail> GetDeviceBasedDetails(ObjectDetectorAnalytics inpObj)
        {

            DA.FramePredictedClassDetailsDS framePredictedDetailsDS = new DA.FramePredictedClassDetailsDS();
            PartitionKeyUtility partitionKeyUtility = new PartitionKeyUtility();
            int partitionKey = partitionKeyUtility.generatePartionKey(inpObj.TenantId.ToString(), inpObj.StartTime);
            List<ObjectDetectorAnalyticsDetail> countDetails = new List<ObjectDetectorAnalyticsDetail>();
            string deviceId = inpObj.DeviceId;
            List<TrackingIds> idList = new List<TrackingIds>();
            if (inpObj.UniquePersonCount)
            {
                DA.ObjectTrackingdetailsDS objectTrackingdetails = new DA.ObjectTrackingdetailsDS();
                if (inpObj.DeviceId != null)
                {
                    idList = (from f in objectTrackingdetails.GetAny()
                              where f.DeviceId == inpObj.DeviceId
                             && f.TenantId == inpObj.TenantId
                              && f.FrameGrabTime >= inpObj.StartTime
                              && f.FrameGrabTime <= inpObj.EndTime
                              select new TrackingIds { ObjectDetectionId = f.ObjectDetectionId, ObjectTrackingId = f.ObjectTrackingId }
                        ).ToList();
                }
                else
                {
                    idList = (from f in objectTrackingdetails.GetAny()
                              where 
                             f.TenantId == inpObj.TenantId
                              && f.FrameGrabTime >= inpObj.StartTime
                              && f.FrameGrabTime <= inpObj.EndTime

                              select new TrackingIds { ObjectDetectionId = f.ObjectDetectionId, ObjectTrackingId = f.ObjectTrackingId }
                       ).ToList();
                }
                for (var i = 0; i < idList.Count; i++)
                {
                    var detectionId = idList[i].ObjectDetectionId;
                    var trackingId = idList[i].ObjectTrackingId;
                    var detectionObject = framePredictedDetailsDS.GetOneWithId(detectionId);
                    var trackingObject = framePredictedDetailsDS.GetOneWithId(trackingId);
                    ObjectDetectorAnalyticsDetail objectDetectorAnalytic = new ObjectDetectorAnalyticsDetail
                    {
                        GrabberTime = (DateTime)detectionObject.FrameGrabTime,
                        Name = detectionObject.PredictedClass,
                        PersonId = trackingObject.PredictedClass

                    };
                    countDetails.Add(objectDetectorAnalytic);
                }
            }
            else
            {
                var details = (from f in framePredictedDetailsDS.GetAny()
                               where f.TenantId == inpObj.TenantId
                               && f.FrameGrabTime >= inpObj.StartTime
                               && f.ResourceId == inpObj.DeviceId
                               && f.FrameGrabTime <= inpObj.EndTime
                               && f.PartitionKey == partitionKey
                               && f.PredictionType == "MaskDetection"
                               group f by new
                               {
                                   f.ResourceId,
                                   f.PredictedClass,
                                   f.FrameGrabTime,
                                   f.PredictedClassSequenceId
                               } into g
                               select new ObjectDetectorAnalyticsDetail
                               {
                                   GrabberTime = (DateTime)g.Key.FrameGrabTime,
                                   Name = g.Key.PredictedClass,
                                   

                               }
                    ).ToList();
                countDetails = details;
            }
            return countDetails;
        }


        private ObjectDetectorAnalyticsRes GroupDetailsByTime(DateTime startTime, int minutesInterval, List<ObjectDetectorAnalyticsDetail> details, string deviceId, int tenantId, string location, bool neededComplianceScore, bool uniquePersonCount)
        {
            double nomaskCount = 0;
            double maskCount = 0;
            List<ObjectDetectorAnalyticsData> intervalGroupedDetails = null;

            var intervalDetails = details.Where(detail => isInInterval(detail, startTime, startTime.AddMinutes(minutesInterval))).ToList();
            if (uniquePersonCount)
            {
                intervalDetails = intervalDetails.GroupBy(g => new { g.PersonId, g.Name }).Select(group => new ObjectDetectorAnalyticsDetail
                {
                    PersonId = group.Key.PersonId,
                    Name = group.Key.Name,

                }).ToList();
            }

            intervalGroupedDetails = intervalDetails.GroupBy(g => new { g.Name }).Select(group => new ObjectDetectorAnalyticsData
            {
                Name = group.Key.Name,
                Count = group.Count()
            }).ToList();


            if (neededComplianceScore)
            {
                nomaskCount = intervalGroupedDetails.Where(w => w.Name.ToLower().Contains("no")).Select(s => s.Count).SingleOrDefault();
                int mask1Count = intervalGroupedDetails.Where(w => w.Name.ToLower().Contains("1")).Select(s => s.Count).SingleOrDefault();
                int mask2Count = intervalGroupedDetails.Where(w => w.Name.ToLower().Contains("2")).Select(s => s.Count).SingleOrDefault();
                maskCount = mask1Count + mask2Count;
                if (maskCount == 0)
                {
                    maskCount = intervalGroupedDetails.Where(w => !w.Name.ToLower().Contains("no")).Select(s => s.Count).SingleOrDefault();
                }
                double complianceScore = (maskCount / (maskCount + nomaskCount)) * 100;
                complianceScore = Math.Round(complianceScore, 2);
                return new ObjectDetectorAnalyticsRes()
                {
                    Details = intervalGroupedDetails,
                    GrabberTime = startTime.ToString("M/dd/yyyy h:mm:00 tt", CultureInfo.InvariantCulture),
                    DeviceId = deviceId,
                    TenantId = tenantId,
                    Location = location,
                    CompliancePercentage = complianceScore
                };
            }
            else
            {
                return new ObjectDetectorAnalyticsRes()
                {
                    Details = intervalGroupedDetails,
                    GrabberTime = startTime.ToString("M/dd/yyyy h:mm:00 tt", CultureInfo.InvariantCulture),
                    DeviceId = deviceId,
                    TenantId = tenantId,
                    Location = location,

                };
            }
        }

        private ObjectDetectorAnalyticsRes GroupDetails(DateTime startTime, List<ObjectDetectorAnalyticsDetail> details, string deviceId, int tenantId, string location, bool neededComplianceScore, bool uniquePersonCount)
        {
            List<ObjectDetectorAnalyticsData> groupedDetails = null;
            double nomaskCount = 0;
            double maskCount = 0;

            if (uniquePersonCount)
            {
                details = details.GroupBy(g => new { g.PersonId, g.Name }).Select(group => new ObjectDetectorAnalyticsDetail
                {
                    PersonId = group.Key.PersonId,
                    Name = group.Key.Name,

                }).ToList();
            }

            groupedDetails = details.GroupBy(g => new { g.Name }).Select(group => new ObjectDetectorAnalyticsData
            {
                Name = group.Key.Name,
                Count = group.Count()
            }).ToList();

            if (neededComplianceScore)
            {

                nomaskCount = groupedDetails.Where(w => w.Name.ToLower().Contains("no")).Select(s => s.Count).SingleOrDefault();
                int mask1Count = groupedDetails.Where(w => w.Name.ToLower().Contains("1")).Select(s => s.Count).SingleOrDefault();
                int mask2Count = groupedDetails.Where(w => w.Name.ToLower().Contains("2")).Select(s => s.Count).SingleOrDefault();
                maskCount = mask1Count + mask2Count;
                if (maskCount == 0)
                {
                    maskCount = groupedDetails.Where(w => !w.Name.ToLower().Contains("no")).Select(s => s.Count).SingleOrDefault();
                }

                double complianceScore = (maskCount / (maskCount + nomaskCount)) * 100;
                complianceScore = Math.Round(complianceScore, 2);

                return new ObjectDetectorAnalyticsRes()
                {
                    Details = groupedDetails,
                    
                    DeviceId = deviceId,
                    TenantId = tenantId,
                    Location = location,
                    CompliancePercentage = complianceScore
                };
            }
            return new ObjectDetectorAnalyticsRes()
            {
                Details = groupedDetails,
                
                DeviceId = deviceId,
                TenantId = tenantId,
                Location = location
            };
        }


        private bool isInInterval(ObjectDetectorAnalyticsDetail detectorDetails, DateTime startTime, DateTime endTime)
        {
            return detectorDetails.GrabberTime <= endTime && detectorDetails.GrabberTime >= startTime;
        }

        public Dictionary<string, List<ObjectDetectorAnalyticsDetail>> GetLocationBasedDetails(ObjectDetectorAnalytics inpObj)
        {
            Dictionary<string, List<ObjectDetectorAnalyticsDetail>> locationDetails = new Dictionary<string, List<ObjectDetectorAnalyticsDetail>>();

            List<ObjectDetectorAnalyticsRes> resultList = new List<ObjectDetectorAnalyticsRes>();

            DA.ResourceDependencyMapDS rdmDS = new DA.ResourceDependencyMapDS();
            DA.ResourceDSExtn resourceDS = new DA.ResourceDSExtn();
            DA.FramePredictedClassDetailsDS framePredictedDetailsDS = new DA.FramePredictedClassDetailsDS();
            PartitionKeyUtility partitionKeyUtility = new PartitionKeyUtility();
            int partitionKey = partitionKeyUtility.generatePartionKey(inpObj.TenantId.ToString(), inpObj.StartTime);

            foreach (string location in inpObj.LocationId)
            {
                string deviceId = resourceDS.GetOne(new DE.VideoAnalytics.Resource() { ResourceName = location }).ResourceId;
                DE.VideoAnalytics.ResourceDependencyMap rdm = new DE.VideoAnalytics.ResourceDependencyMap()
                {
                    TenantId = inpObj.TenantId,
                    PortfolioId = deviceId
                };
                var deviceIdList = rdmDS.GetAll(rdm).Select(r => r.ResourceId).ToList();



                if (inpObj.UniquePersonCount)
                {
                    List<ObjectDetectorAnalyticsDetail> countDetails = new List<ObjectDetectorAnalyticsDetail>();
                    List<TrackingIds> idList = new List<TrackingIds>();
                    DA.ObjectTrackingdetailsDS objectTrackingdetails = new DA.ObjectTrackingdetailsDS();
                    idList = (from f in objectTrackingdetails.GetAny()
                              where 
                             f.TenantId == inpObj.TenantId
                              && f.FrameGrabTime >= inpObj.StartTime
                              && f.FrameGrabTime <= inpObj.EndTime

                              select new TrackingIds { ObjectDetectionId = f.ObjectDetectionId, ObjectTrackingId = f.ObjectTrackingId }
                       ).ToList();



                    for (var i = 0; i < idList.Count; i++)
                    {
                        var detectionId = idList[i].ObjectDetectionId;
                        var trackingId = idList[i].ObjectTrackingId;

                        var detectionObject = framePredictedDetailsDS.GetOneWithId(detectionId);
                        var trackingObject = framePredictedDetailsDS.GetOneWithId(trackingId);


                        ObjectDetectorAnalyticsDetail objectDetectorAnalytic = new ObjectDetectorAnalyticsDetail
                        {
                            GrabberTime = (DateTime)detectionObject.FrameGrabTime,
                            Name = detectionObject.PredictedClass,
                            PersonId = trackingObject.PredictedClass

                        };
                        countDetails.Add(objectDetectorAnalytic);
                    }

                    locationDetails[location] = countDetails;
                }
                else
                {
                    var details = (from f in framePredictedDetailsDS.GetAny()
                                   where deviceIdList.Contains(f.ResourceId)
                                   && f.TenantId == inpObj.TenantId
                                   && f.FrameGrabTime >= inpObj.StartTime
                                   && f.FrameGrabTime <= inpObj.EndTime
                                   && f.PartitionKey == partitionKey
                                   && f.PredictionType == predictionType
                                   
                                   select new ObjectDetectorAnalyticsDetail
                                   {
                                       Name = f.PredictedClass,
                                       GrabberTime = (DateTime)f.FrameGrabTime,
                                       FrameId = f.FrameId,

                                   }
                        ).ToList();

                    locationDetails[location] = details;
                }

            }

            return locationDetails;
        }

        public List<ObjectDetectorAnalyticsRes> GetLocationBasedComplianceScore(ObjectDetectorAnalytics inpObj)
        {
            double maskCount = 0;
            double nomaskCount = 0;
            List<ObjectDetectorAnalyticsRes> resultList = new List<ObjectDetectorAnalyticsRes>();


            Dictionary<string, List<ObjectDetectorAnalyticsDetail>> locationDetails = GetLocationBasedDetails(inpObj);

            if (inpObj.TimeInterval != 0)
            {
                DateTime startTime = inpObj.StartTime;
                List<DateTime> startTimeList = new List<DateTime>();

                for (var startingTime = startTime; startingTime < inpObj.EndTime; startingTime = startingTime.AddMinutes(inpObj.TimeInterval))
                {
                    startTimeList.Add(startingTime);
                }

                foreach (KeyValuePair<string, List<ObjectDetectorAnalyticsDetail>> item in locationDetails)
                {
                    List<ObjectDetectorAnalyticsDetail> dataInTimeRange = item.Value.Where(data => data.GrabberTime >= startTime && data.GrabberTime <= inpObj.EndTime).ToList();

                    foreach (ObjectDetectorAnalyticsDetail ObjectDetectorAnalyticsDetail in dataInTimeRange)
                    {
                        if (ObjectDetectorAnalyticsDetail.Name.ToLower().Contains("no"))
                        {
                            nomaskCount++;
                        }
                        else
                        {
                            maskCount++;
                        }
                    }

                    double complianceScore = (maskCount / (maskCount + nomaskCount)) * 100;
                    complianceScore = Math.Round(complianceScore, 2);


                    var intervalGroupedDetails = startTimeList.Select(intervalStartTime => GroupDetailsByTime(intervalStartTime, inpObj.TimeInterval, item.Value, null, inpObj.TenantId, item.Key, true, inpObj.UniquePersonCount));
                    resultList.AddRange(intervalGroupedDetails.Where(dataList => dataList.Details.Any()).ToList());
                }
            }
            else
            {
                DateTime startTime = inpObj.StartTime;
                foreach (KeyValuePair<string, List<ObjectDetectorAnalyticsDetail>> item in locationDetails)
                {
                    if (item.Value != null)
                    {
                        List<ObjectDetectorAnalyticsDetail> dataInTimeRange = item.Value.Where(data => data.GrabberTime >= startTime && data.GrabberTime <= inpObj.EndTime).ToList();

                        foreach (ObjectDetectorAnalyticsDetail ObjectDetectorAnalyticsDetail in dataInTimeRange)
                        {
                            if (ObjectDetectorAnalyticsDetail.Name.ToLower().Contains("no"))
                            {
                                nomaskCount++;
                            }
                            else
                            {
                                maskCount++;
                            }
                        }

                        double complianceScore = (maskCount / (maskCount + nomaskCount)) * 100;
                        complianceScore = Math.Round(complianceScore, 2);
                        var intervalGroupedDetails = GroupDetails(startTime, item.Value, inpObj.DeviceId, inpObj.TenantId, item.Key, true, inpObj.UniquePersonCount);
                        resultList.Add(intervalGroupedDetails);
                    }
                }


            }


            return resultList;

        }

        public List<ObjectDetectorAnalyticsRes> GetLocationBasedTimeIntervalDetails(ObjectDetectorAnalytics inpObj)
        {


            List<ObjectDetectorAnalyticsRes> resultList = new List<ObjectDetectorAnalyticsRes>();

            DA.ResourceDependencyMapDS rdmDS = new DA.ResourceDependencyMapDS();
            DA.ResourceDSExtn resourceDS = new DA.ResourceDSExtn();
            DA.FramePredictedClassDetailsDS framePredictedDetailsDS = new DA.FramePredictedClassDetailsDS();
            PartitionKeyUtility partitionKeyUtility = new PartitionKeyUtility();
            int partitionKey = partitionKeyUtility.generatePartionKey(inpObj.TenantId.ToString(), inpObj.StartTime);


            foreach (string location in inpObj.LocationId)
            {
                string deviceId = resourceDS.GetOne(new DE.VideoAnalytics.Resource() { ResourceName = location }).ResourceId;
                DE.VideoAnalytics.ResourceDependencyMap rdm = new DE.VideoAnalytics.ResourceDependencyMap()
                {
                    TenantId = inpObj.TenantId,
                    PortfolioId = deviceId
                };
                var deviceIdList = rdmDS.GetAll(rdm).Select(r => r.ResourceId).ToList();

                DA.ObjectTrackingdetailsDS objectTrackingdetails = new DA.ObjectTrackingdetailsDS();
                List<ObjectDetectorAnalyticsDetail> responseGroupBy = new List<ObjectDetectorAnalyticsDetail>();
                if (inpObj.UniquePersonCount)
                {
                    var idList = (from f in objectTrackingdetails.GetAny()
                                  where deviceIdList.Contains(f.DeviceId)
                                  && f.TenantId == inpObj.TenantId
                                  && f.FrameGrabTime >= inpObj.StartTime
                                  && f.FrameGrabTime <= inpObj.EndTime

                                  select new TrackingIds { ObjectDetectionId = f.ObjectDetectionId, ObjectTrackingId = f.ObjectTrackingId }
                        ).ToList();

                    List<ObjectDetectorAnalyticsDetail> response = new List<ObjectDetectorAnalyticsDetail>();

                    for (var i = 0; i < idList.Count; i++)
                    {
                        var detectionId = idList[i].ObjectDetectionId;
                        var trackingId = idList[i].ObjectTrackingId;

                        var detectionObject = framePredictedDetailsDS.GetOneWithId(detectionId);
                        var trackingObject = framePredictedDetailsDS.GetOneWithId(trackingId);


                        ObjectDetectorAnalyticsDetail objectDetectorAnalytic = new ObjectDetectorAnalyticsDetail
                        {
                            DeviceId = inpObj.DeviceId,
                            GrabberTime = Convert.ToDateTime(detectionObject.FrameGrabTime?.ToString("yyyy/MM/dd hh:mm", CultureInfo.InvariantCulture)),
                            Name = detectionObject.PredictedClass,
                            PersonId = trackingObject.PredictedClass

                        };

                        response.Add(objectDetectorAnalytic);

                    }
                    var responseGrp = (from res in response
                                       where ((res.GrabberTime.Minute) % inpObj.TimeInterval) == 0
                                       group res by new { res.Name, res.PersonId }
                                   into g
                                       select new ObjectDetectorAnalyticsDetail
                                       {
                                           Name = g.Key.Name,
                                           Count = g.Count(),
                                           
                                           PersonId = g.Key.PersonId


                                       }).ToList();

                    responseGroupBy = (from res in responseGrp

                                       group res by new { res.Name }
                                   into g
                                       select new ObjectDetectorAnalyticsDetail
                                       {
                                           Name = g.Key.Name,
                                           Count = g.Count(),


                                       }).ToList();
                }
                else
                {
                    var response = (from f in framePredictedDetailsDS.GetAll()
                                    where f.TenantId == inpObj.TenantId
                                    && f.FrameGrabTime >= inpObj.StartTime
                                    && f.FrameGrabTime <= inpObj.EndTime
                                    && f.PartitionKey == partitionKey
                                    && deviceIdList.Contains(f.ResourceId)
                                    && f.PredictionType ==predictionType
                                    select new ObjectDetectorAnalyticsDetail
                                    {
                                        DeviceId = inpObj.DeviceId,
                                        Name = f.PredictedClass,
                                        
                                        GrabberTime = Convert.ToDateTime(f.FrameGrabTime?.ToString("yyyy/MM/dd hh:mm", CultureInfo.InvariantCulture))

                                    }).ToList();

                    responseGroupBy = (from res in response
                                       where ((res.GrabberTime.Minute) % inpObj.TimeInterval) == 0
                                       group res by new { res.Name, res.GrabberTime }
                                         into g
                                       select new ObjectDetectorAnalyticsDetail
                                       {
                                           Name = g.Key.Name,
                                           Count = g.Count(),
                                           GrabberTime = g.Key.GrabberTime

                                       }).ToList();
                }

                List<ObjectDetectorAnalyticsData> analyticsDatas = null;

                Dictionary<string, List<ObjectDetectorAnalyticsData>> deviceDetails = new Dictionary<string, List<ObjectDetectorAnalyticsData>>();

                foreach (ObjectDetectorAnalyticsDetail analyticsDetail in responseGroupBy)
                {
                    string frameGrabTimeStr = analyticsDetail.GrabberTime.ToString();
                    ObjectDetectorAnalyticsData data = new ObjectDetectorAnalyticsData()
                    {
                        Name = analyticsDetail.Name,
                        Count = analyticsDetail.Count

                    };
                    if (deviceDetails.ContainsKey(frameGrabTimeStr))
                    {
                        analyticsDatas = deviceDetails[frameGrabTimeStr];
                        analyticsDatas.Add(data);
                        deviceDetails[frameGrabTimeStr] = analyticsDatas;
                    }
                    else
                    {
                        analyticsDatas = new List<ObjectDetectorAnalyticsData>();
                        analyticsDatas.Add(data);
                        deviceDetails[frameGrabTimeStr] = analyticsDatas;
                    }
                }

                foreach (KeyValuePair<string, List<ObjectDetectorAnalyticsData>> item in deviceDetails)
                {
                    ObjectDetectorAnalyticsRes resultObj = new ObjectDetectorAnalyticsRes()
                    {
                        GrabberTime = item.Key,
                        Details = item.Value,
                        Location = location,
                        TenantId = inpObj.TenantId
                    };
                    resultList.Add(resultObj);
                }


                
            }
            return resultList;
            
        }
        public List<ObjectDetectorAnalyticsRes> GetLocationBasedComplianceScoreDetails(ObjectDetectorAnalytics inpObj)
        {


            List<ObjectDetectorAnalyticsRes> resultList = new List<ObjectDetectorAnalyticsRes>();
            DA.ObjectTrackingdetailsDS objectTrackingdetails = new DA.ObjectTrackingdetailsDS();
            List<ObjectDetectorAnalyticsDetail> responseGroupBy = new List<ObjectDetectorAnalyticsDetail>();
            DA.ResourceDependencyMapDS rdmDS = new DA.ResourceDependencyMapDS();
            DA.ResourceDSExtn resourceDS = new DA.ResourceDSExtn();
            DA.FramePredictedClassDetailsDS framePredictedDetailsDS = new DA.FramePredictedClassDetailsDS();
            PartitionKeyUtility partitionKeyUtility = new PartitionKeyUtility();
            int partitionKey = partitionKeyUtility.generatePartionKey(inpObj.TenantId.ToString(), inpObj.StartTime);

            foreach (string location in inpObj.LocationId)
            {
                string deviceId = resourceDS.GetOne(new DE.VideoAnalytics.Resource() { ResourceName = location }).ResourceId;
                DE.VideoAnalytics.ResourceDependencyMap rdm = new DE.VideoAnalytics.ResourceDependencyMap()
                {
                    TenantId = inpObj.TenantId,
                    PortfolioId = deviceId
                };
                var deviceIdList = rdmDS.GetAll(rdm).Select(r => r.ResourceId).ToList();
                List<ObjectDetectorAnalyticsDetail> response = new List<ObjectDetectorAnalyticsDetail>();
                if (inpObj.UniquePersonCount)
                {
                    var idList = (from f in objectTrackingdetails.GetAny()
                                  where deviceIdList.Contains(f.DeviceId)
                                  && f.TenantId == inpObj.TenantId
                                  && f.FrameGrabTime >= inpObj.StartTime
                                  && f.FrameGrabTime <= inpObj.EndTime

                                  select new TrackingIds { ObjectDetectionId = f.ObjectDetectionId, ObjectTrackingId = f.ObjectTrackingId }
                        ).ToList();



                    for (var i = 0; i < idList.Count; i++)
                    {
                        var detectionId = idList[i].ObjectDetectionId;
                        var trackingId = idList[i].ObjectTrackingId;

                        var detectionObject = framePredictedDetailsDS.GetOneWithId(detectionId);
                        var trackingObject = framePredictedDetailsDS.GetOneWithId(trackingId);


                        ObjectDetectorAnalyticsDetail objectDetectorAnalytic = new ObjectDetectorAnalyticsDetail
                        {
                            DeviceId = inpObj.DeviceId,
                            GrabberTime = Convert.ToDateTime(detectionObject.FrameGrabTime?.ToString("yyyy/MM/dd hh:mm", CultureInfo.InvariantCulture)),
                            Name = detectionObject.PredictedClass,
                            PersonId = trackingObject.PredictedClass

                        };

                        response.Add(objectDetectorAnalytic);

                    }
                    var responseGrp = (from res in response
                                       where ((res.GrabberTime.Minute) % inpObj.TimeInterval) == 0
                                       group res by new { res.Name, res.PersonId }
                                   into g
                                       select new ObjectDetectorAnalyticsDetail
                                       {
                                           Name = g.Key.Name,
                                           Count = g.Count(),
                                           
                                           PersonId = g.Key.PersonId

                                       }).ToList();
                    responseGroupBy = (from res in responseGrp
                                           
                                       group res by new { res.Name }
                                   into g
                                       select new ObjectDetectorAnalyticsDetail
                                       {
                                           Name = g.Key.Name,
                                           Count = g.Count(),
                                           

                                       }).ToList();
                }
                else
                {
                    response = (from f in framePredictedDetailsDS.GetAll()
                                where f.TenantId == inpObj.TenantId
                                && f.FrameGrabTime >= inpObj.StartTime
                                && f.FrameGrabTime <= inpObj.EndTime
                                && f.PartitionKey == partitionKey
                                && deviceIdList.Contains(f.ResourceId)
                                && f.PredictionType == predictionType
                                select new ObjectDetectorAnalyticsDetail
                                {
                                    DeviceId = inpObj.DeviceId,
                                    Name = f.PredictedClass,
                                    GrabberTime = Convert.ToDateTime(f.FrameGrabTime?.ToString("yyyy/MM/dd hh:mm", CultureInfo.InvariantCulture))

                                }).ToList();

                    responseGroupBy = (from res in response
                                       where ((res.GrabberTime.Minute) % inpObj.TimeInterval) == 0
                                       group res by new { res.Name, res.GrabberTime }
                                        into g
                                       select new ObjectDetectorAnalyticsDetail
                                       {
                                           Name = g.Key.Name,
                                           Count = g.Count(),
                                           GrabberTime = g.Key.GrabberTime

                                       }).ToList();

                }
                List<ObjectDetectorAnalyticsData> analyticsDatas = null;

                Dictionary<string, List<ObjectDetectorAnalyticsData>> deviceDetails = new Dictionary<string, List<ObjectDetectorAnalyticsData>>();

                foreach (ObjectDetectorAnalyticsDetail analyticsDetail in responseGroupBy)
                {
                    string frameGrabTimeStr = analyticsDetail.GrabberTime.ToString();
                    ObjectDetectorAnalyticsData data = new ObjectDetectorAnalyticsData()
                    {
                        Name = analyticsDetail.Name,
                        Count = analyticsDetail.Count

                    };
                    if (deviceDetails.ContainsKey(frameGrabTimeStr))
                    {
                        analyticsDatas = deviceDetails[frameGrabTimeStr];
                        analyticsDatas.Add(data);
                        deviceDetails[frameGrabTimeStr] = analyticsDatas;
                    }
                    else
                    {
                        analyticsDatas = new List<ObjectDetectorAnalyticsData>();
                        analyticsDatas.Add(data);
                        deviceDetails[frameGrabTimeStr] = analyticsDatas;
                    }
                }

                foreach (KeyValuePair<string, List<ObjectDetectorAnalyticsData>> item in deviceDetails)
                {
                    double totalCount = 0;
                    foreach (ObjectDetectorAnalyticsData data in item.Value)
                    {
                        totalCount = totalCount + data.Count;
                    }
                    double compliancePercentage = 0;
                    double maskCount =0;
                    foreach (ObjectDetectorAnalyticsData data in item.Value)
                    {
                        if (data.Name.ToLower().StartsWith("mask"))
                        {
                            maskCount += data.Count;

                        }

                    }
                    compliancePercentage = Math.Round(((maskCount / totalCount) * (double)100), 2);
                    ObjectDetectorAnalyticsRes resultObj = new ObjectDetectorAnalyticsRes()
                    {
                        GrabberTime = item.Key,
                        Details = item.Value,
                        Location = location,
                        TenantId = inpObj.TenantId,
                        CompliancePercentage= compliancePercentage
                    };
                    resultList.Add(resultObj);
                }

            }

            return resultList;
        }

    }
    public class TrackingIds
    {
        public int ObjectDetectionId { get; set; }
        public int ObjectTrackingId { get; set; }
    }
}

