/*=============================================================================================================== *
 * Copyright 2025 Infosys Ltd.                                                                                    *
 * Use of this source code is governed by Apache License Version 2.0 that can be found in the LICENSE file or at  *
 * http://www.apache.org/licenses/                                                                                *
 * ===============================================================================================================*/
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Infosys.Solutions.Ainauto.VideoAnalytics.Resource.Entity;
using Infosys.Solutions.Ainauto.VideoAnalytics.Resource.Entity.Framedetail;
using Infosys.Solutions.Ainauto.VideoAnalytics.Resource.IDataAccess;
using static Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.Common.ApplicationConstants;

namespace Infosys.Solutions.Ainauto.VideoAnalytics.Resource.DataAccess
{
    public class FeedRequestDS : IEntity<FeedRequest>
    {

        public framedetailsNewContext dbCon;
        public FeedRequestDS()
        {
            dbCon = new framedetailsNewContext();
        }
        public bool Delete(FeedRequest entity)
        {
            throw new NotImplementedException();
        }

        public IList<FeedRequest> GetAll()
        {
            throw new NotImplementedException();
        }

        public IList<FeedRequest> GetAll(FeedRequest Entity)
        {
            throw new NotImplementedException();
        }

        public IQueryable<FeedRequest> GetAny()
        {
            throw new NotImplementedException();
        }

        public FeedRequest GetOne(FeedRequest entity)
        {
            try
            {
                var res = (from s in dbCon.FeedRequests
                           where s.FeedProcessorMasterId == entity.FeedProcessorMasterId
                           select s).FirstOrDefault();
                return res;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public List<string> GetAvailableDevices(List<string> deviceList)
        {
            try
            {
                var busyResources = (from s in dbCon.FeedRequests
                                     where s.Status != ProcessingStatus.completedStatus
                                     where deviceList.Contains(s.ResourceId)
                                     select s.ResourceId).Distinct().ToList();
                List<string> availableResources = deviceList.Except(busyResources).ToList();
                return availableResources;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public FeedRequest GetOneWithRequestId(string requestId)
        {
            try
            {
                var res = (from s in dbCon.FeedRequests
                           where s.RequestId == requestId
                           select s).FirstOrDefault();
                return res;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public FeedRequest GetOneWithVideoSource(string videoSource)
        {
            try
            {
                var res = (from s in dbCon.FeedRequests
                           where s.VideoName == videoSource
                           select s).FirstOrDefault();
                return res;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public FeedRequest GetOneWithMasterId(int masterId)
        {
            try
            {
                var res = (from s in dbCon.FeedRequests
                           where s.FeedProcessorMasterId == masterId
                           select s).FirstOrDefault();
                return res;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public FeedRequest Insert(FeedRequest entity)
        {
            try
            {
                entity.CreatedDate = DateTime.UtcNow;
                entity.CreatedBy = UserDetails.userName;
                var resEntity = dbCon.FeedRequests.Add(entity);
                dbCon.SaveChanges();
                return entity;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public IList<FeedRequest> InsertBatch(IList<FeedRequest> entities)
        {
            throw new NotImplementedException();
        }

        public FeedRequest Update(FeedRequest entity)
        {
            try
            {
                FeedRequest resEntity = null;
                if (entity.RequestId != null)
                {
                    resEntity = dbCon.FeedRequests.Find(entity.RequestId);

                }
                else
                {
                    resEntity = dbCon.FeedRequests.Where(s => s.FeedProcessorMasterId == entity.FeedProcessorMasterId).FirstOrDefault();
                }
                if (resEntity != null)
                {
                    resEntity.ModifiedBy = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
                    resEntity.ModifiedDate = DateTime.UtcNow;
                    resEntity.Status = entity.Status;
                    resEntity.LastFrameGrabbedTime = entity.LastFrameGrabbedTime;
                    resEntity.LastFrameId = entity.LastFrameId;
                    resEntity.StartFrameProcessedTime = entity.StartFrameProcessedTime;
                    resEntity.LastFrameProcessedTime = entity.LastFrameProcessedTime;
                    resEntity.FeedProcessorMasterId = entity.FeedProcessorMasterId;
                    resEntity.ResourceId = entity.ResourceId;
                    resEntity.Model = entity.Model;
                    dbCon.SaveChanges();
                }

                return resEntity;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }


        public List<FeedRequestHistoryDetails> GetHistoryData(FeedRequest Entity)
        {
            try
            {
                DateTime lastFrameProcessedTime = new DateTime();
                if (Entity.LastFrameProcessedTime == lastFrameProcessedTime)
                {
                    Entity.LastFrameProcessedTime = DateTime.UtcNow;
                }
                ResourceAttributesDS resourceAttributesDS = new ResourceAttributesDS();
                var res = (from feedReuest in dbCon.FeedRequests
                           from feedprocessormaster in dbCon.FeedProcessorMasters
                           from mediametadata in dbCon.MediaMetadata
                           where feedReuest.StartFrameProcessedTime >= Entity.StartFrameProcessedTime
                           && feedReuest.FeedProcessorMasterId == feedprocessormaster.FeedProcessorMasterId
                          && feedReuest.LastFrameProcessedTime <= Entity.LastFrameProcessedTime
                          && feedReuest.TenantId == Entity.TenantId
                          && mediametadata.FeedProcessorMasterId == feedprocessormaster.FeedProcessorMasterId
                           select new FeedRequestHistoryDetails
                           {
                               RequestId = feedReuest.RequestId,
                               ResourceId = feedReuest.ResourceId,
                               FeedProcessorMasterId = feedReuest.FeedProcessorMasterId,
                               VideoName = feedReuest.VideoName,
                               LastFrameId = feedReuest.LastFrameId,
                               LastFrameGrabbedTime = feedReuest.LastFrameGrabbedTime.ToString(),
                               LastFrameProcessedTime = feedReuest.LastFrameProcessedTime.ToString(),
                               StartFrameProcessedTime = feedReuest.StartFrameProcessedTime.ToString(),
                               TenantId = feedReuest.TenantId,
                               Status = feedReuest.Status,
                               ModelName = feedReuest.Model,
                               VideoMetadata = mediametadata.MetaData,
                              
                               FileName = feedprocessormaster.FileName,
                               FeedURI = feedprocessormaster.FeedUri,
                               ProcessingStartTimeTicks = feedprocessormaster.ProcessingStartTimeTicks,
                               ProcessingEndTimeTicks = feedprocessormaster.ProcessingEndTimeTicks,
                               MachineName = feedprocessormaster.MachineName
                           }).ToList();
                

                return res;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public FeedRequest GetInProgressData(FeedRequest Entity)
        {
            try
            {
                var res = (from s in dbCon.FeedRequests
                           where s.ResourceId == Entity.ResourceId
                           && s.TenantId == Entity.TenantId
                           && s.Status == Entity.Status
                           select s).FirstOrDefault();
                return res;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IList<FeedRequest> UpdateBatch(IList<FeedRequest> entities)
        {
            throw new NotImplementedException();
        }
    }
}

