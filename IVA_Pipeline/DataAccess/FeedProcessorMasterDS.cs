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
using Infosys.Solutions.Ainauto.VideoAnalytics.Resource.Entity.Framedetail;
using static Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.Common.ApplicationConstants;

namespace Infosys.Solutions.Ainauto.VideoAnalytics.Resource.DataAccess
{
    public class FeedProcessorMasterDS : IDataAccess.IEntity<FeedProcessorMaster>
    {
        public framedetailsNewContext dbCon;
        public FeedProcessorMasterDS()
        {
            dbCon = new framedetailsNewContext();
        }
        public bool Delete(FeedProcessorMaster entity)
        {
            throw new NotImplementedException();
        }

        public IList<FeedProcessorMaster> GetAll()
        {
            throw new NotImplementedException();
        }

        public IList<FeedProcessorMaster> GetAll(FeedProcessorMaster Entity)
        {
            throw new NotImplementedException();
        }

        public IQueryable<FeedProcessorMaster> GetAny()
        {
            throw new NotImplementedException();
        }
        public FeedProcessorMaster GetOne(FeedProcessorMaster entity)
        {
            throw new NotImplementedException();
        }

      


        public FeedProcessorMaster GetOneWithVideoSource(string videoSource)
        {
            try
            {
                var res = (from s in dbCon.FeedProcessorMasters
                           where s.FileName.Contains(videoSource)
                           select s).FirstOrDefault();
                return res;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public FeedProcessorMaster GetOneWithMasterId(int masterId)
        {
            try
            {
                var res = (from s in dbCon.FeedProcessorMasters
                           where s.FeedProcessorMasterId == masterId
                           select s).FirstOrDefault();
                return res;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public int GetCount(int masterId)
        {
            try
            {
                var count = (from s in dbCon.FeedProcessorMasters
                             where s.FeedProcessorMasterId == masterId
                             select s).ToList().Count;
                return count;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public FeedProcessorMaster GetOne(int feedMasterId)
        {
            try
            {
                var res = (from s in dbCon.FeedProcessorMasters
                           where s.FeedProcessorMasterId == feedMasterId

                           select s).FirstOrDefault();
                return res;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public FeedProcessorMaster Insert(FeedProcessorMaster entity)
        {
            try
            {
                entity.CreatedDate = DateTime.UtcNow;
                var resEntity = dbCon.FeedProcessorMasters.Add(entity);
                dbCon.SaveChanges();
                return entity;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public IList<FeedProcessorMaster> InsertBatch(IList<FeedProcessorMaster> entities)
        {
            throw new NotImplementedException();
        }

        public FeedProcessorMaster Update(FeedProcessorMaster entity)
        {
            try
            {
                var resEntity = dbCon.FeedProcessorMasters.Find(entity.FeedProcessorMasterId);

                if (resEntity != null)
                {
                    if(entity.ProcessingEndTimeTicks!=0)
                        resEntity.ProcessingEndTimeTicks = entity.ProcessingEndTimeTicks;
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    {
                        resEntity.ModifiedBy = System.Security.Principal.WindowsIdentity.GetCurrent().Name;

                    }
                    else
                    {
                        resEntity.ModifiedBy = UserDetails.userName;
                    }

                    resEntity.ModifiedDate = DateTime.UtcNow;
                    resEntity.Status = entity.Status;
                    if (entity.TimeTaken != null)
                        resEntity.TimeTaken = entity.TimeTaken;
                    if ( entity.ProcessingStartTimeTicks != 0)
                        resEntity.ProcessingStartTimeTicks = entity.ProcessingStartTimeTicks;
                    if (entity.TotalFrameProcessed != null && entity.TotalFrameProcessed != 0)
                        resEntity.TotalFrameProcessed = entity.TotalFrameProcessed;
                    if ( entity.FrameProcessedRate != 0)
                        resEntity.FrameProcessedRate = entity.FrameProcessedRate;
                    if (entity.FeedUri != null && entity.FeedUri != "")
                        resEntity.FeedUri = entity.FeedUri;
                    dbCon.SaveChanges();
                }

                return resEntity;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public FeedProcessorMaster GetInProgressData(FeedProcessorMaster Entity)
        {
            try
            {
                var res = (from s in dbCon.FeedProcessorMasters
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


        public IList<FeedProcessorMaster> UpdateBatch(IList<FeedProcessorMaster> entities)
        {
            throw new NotImplementedException();
        }
    }
}
