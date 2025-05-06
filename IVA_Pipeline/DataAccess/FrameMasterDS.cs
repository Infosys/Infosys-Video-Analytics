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
using Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.Common;
using Infosys.Solutions.Ainauto.VideoAnalytics.Resource.Entity;
using Infosys.Solutions.Ainauto.VideoAnalytics.Resource.Entity.Framedetail;
using Infosys.Solutions.Ainauto.VideoAnalytics.Resource.IDataAccess;
using static Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.Common.ApplicationConstants;

namespace Infosys.Solutions.Ainauto.VideoAnalytics.Resource.DataAccess
{
    public class FrameMasterDS : IEntity<FrameMaster>
    {

        public framedetailsNewContext dbCon;

        public FrameMasterDS()
        {
            dbCon = new framedetailsNewContext();
        }

        public bool Delete(FrameMaster entity)
        {
            throw new NotImplementedException();
        }

        public IList<FrameMaster> GetAll()
        {
            throw new NotImplementedException();
        }

        public IList<FrameMaster> GetAll(FrameMaster Entity)
        {
            throw new NotImplementedException();
        }



        public IQueryable<FrameMaster> GetAny()
        {
            return dbCon.FrameMasters;
        }



        public FrameMaster Insert(FrameMaster entity)
        {
            try
            {
                using (dbCon = new framedetailsNewContext())
                {
                    if (entity.FrameGrabTime.Kind != DateTimeKind.Utc)
                    {
                        entity.FrameGrabTime = entity.FrameGrabTime.ToUniversalTime();
                    }
                    entity.CreatedDate = DateTime.UtcNow;
                    dbCon.FrameMasters.Add(entity);
                    dbCon.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("PRIMARY KEY constraint") || (ex.InnerException != null && ex.InnerException.InnerException.Message.Contains("PRIMARY KEY constraint")))
                {
                    LogHandler.LogError("error in insert metadata : message {0}", LogHandler.Layer.Business, ex.Message);
                    throw new DuplicateRecordException();
                }
                else
                {
                    throw ex;
                }
            }

            return entity;
        }

        public int GetCount(int feedId)
        {
            int count = 0;

            count = (from s in dbCon.FrameMasters
                     where s.FeedProcessorMasterId == feedId
                     select s).Count();
            return count;
        }

        public IList<FrameMaster> InsertBatch(IList<FrameMaster> entities)
        {
            throw new NotImplementedException();
        }

        public FrameMaster Update(FrameMaster entity)
        {
            using (dbCon = new framedetailsNewContext())

            {
                (from s in dbCon.FrameMasters
                 where s.ResourceId == entity.ResourceId && s.FrameId == entity.FrameId
                 select s)
                 .ToList().ForEach(x => { x.Status = entity.Status; x.ModifiedBy = entity.ModifiedBy; x.ModifiedDate = entity.ModifiedDate; });

                dbCon.SaveChanges();
            }
            return entity;
        }



        public FrameMaster UpdatePersonCount(FrameMaster entity)
        {
            using (dbCon = new framedetailsNewContext())
            {
                string username = UserDetails.userName;
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    username = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
                }
                var e = (from s in dbCon.FrameMasters
                         where s.ResourceId == entity.ResourceId
                         && s.FrameId == entity.FrameId
                         && s.TenantId == entity.TenantId
                         select s).FirstOrDefault();
                if (e != null)
                {
                    e.ClassPredictionCount = entity.ClassPredictionCount;
                    e.ModifiedBy = username;
                    e.ModifiedDate = DateTime.UtcNow;
                    dbCon.SaveChanges();
                    return e;
                }
                else
                    return null;
            }
        }

        public FrameMaster GetRecord(FrameMaster Entity)
        {
            try
            {
                using (dbCon = new framedetailsNewContext())
                {


                    Entity = (from s in dbCon.FrameMasters
                             .Where(k => k.ResourceId == Entity.ResourceId && k.FrameId == Entity.FrameId)
                              select (s)).FirstOrDefault();


                }
            }
            catch(Exception ex)
            {
                return null;
            }
            
            return Entity;
        }

        public FrameMaster GetOne(FrameMaster entity)
        {
            using (dbCon = new framedetailsNewContext())
            {
                entity = (from s in dbCon.FrameMasters
                          where s.ResourceId == entity.ResourceId
                          && s.FrameId == entity.FrameId
                          && s.TenantId == entity.TenantId
                          select s).FirstOrDefault();
            }
            return entity;
        }




        public IList<FrameMaster> UpdateBatch(IList<FrameMaster> entities)
        {
            throw new NotImplementedException();
        }
    }
}
