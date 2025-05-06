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
using Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.Common;
using Infosys.Solutions.Ainauto.VideoAnalytics.Resource.Entity;
using Infosys.Solutions.Ainauto.VideoAnalytics.Resource.IDataAccess;
using Infosys.Solutions.Ainauto.VideoAnalytics.Resource.Entity.Framedetail;
using System.Runtime.InteropServices;
using static Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.Common.ApplicationConstants;

namespace Infosys.Solutions.Ainauto.VideoAnalytics.Resource.DataAccess
{
    public class FramePredictedClassDetailsDS : IEntity<FramePredictedClassDetail>
    {

        public framedetailsNewContext dbCon;

        public FramePredictedClassDetailsDS()
        {
            dbCon = new framedetailsNewContext();
        }

        public bool Delete(FramePredictedClassDetail entity)
        {
            throw new NotImplementedException();
        }

        public IList<FramePredictedClassDetail> GetAll()
        {
            return dbCon.FramePredictedClassDetails.ToList();
        }

        public IList<FramePredictedClassDetail> GetAll(FramePredictedClassDetail Entity)
        {

            using (dbCon = new framedetailsNewContext())
            {
                return (from r in dbCon.FramePredictedClassDetails
                        where r.ResourceId == Entity.ResourceId
                        && r.FrameId == Entity.FrameId
                        && r.TenantId == Entity.TenantId
                        select r).ToList();
            }
        }



        public IQueryable<FramePredictedClassDetail> GetAny()
        {
            return dbCon.FramePredictedClassDetails;
        }



        public FramePredictedClassDetail Insert(FramePredictedClassDetail entity)
        {
            try
            {
                using (dbCon = new framedetailsNewContext())
                {
                    entity.CreatedDate = DateTime.UtcNow;
                    dbCon.FramePredictedClassDetails.Add(entity);
                    dbCon.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                LogHandler.LogError("Error while Inserting into FramePredictedClassDetail table. exception: {0}, inner exception: {1}, stack trace: {2}", 
                    LogHandler.Layer.Business, ex.Message, ex.InnerException, ex.StackTrace);
                return null;
            }

            return entity;
        }

        public IList<FramePredictedClassDetail> InsertBatch(IList<FramePredictedClassDetail> entities)
        {
            try
            {
                using (dbCon = new framedetailsNewContext())
                {
                    dbCon.FramePredictedClassDetails.AddRange(entities);
                    dbCon.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                LogHandler.LogError("Error while Batch Inserting the FramePredictedClassDetail table. message: {0}", LogHandler.Layer.Business, ex.Message);
                return null;
            }

            return entities;
        }

        public FramePredictedClassDetail Update(FramePredictedClassDetail entity)
        {
            try
            {
                string username = " ";
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    username = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
                }
                using (dbCon = new framedetailsNewContext())
                {
                    FramePredictedClassDetail dbEntity = dbCon.FramePredictedClassDetails.Where(t => entity.Id == t.Id).First();

                    dbEntity.PredictionType = entity.PredictionType;
                    dbEntity.ModifiedBy = username;
                    dbEntity.ModifiedDate = DateTime.UtcNow;

                    dbCon.SaveChanges();

                    return dbEntity;
                }
            }
            catch
            {
                LogHandler.LogError("Error while updating the FramePredictedClassDetail table", LogHandler.Layer.Business, null);
                return null;
            }
        }
        public FramePredictedClassDetail GetOne(FramePredictedClassDetail entity)
        {
            throw new NotImplementedException();
        }


        public FramePredictedClassDetail GetOneWithId(int id)
        {
            using (dbCon = new framedetailsNewContext())
            {
                return (from r in dbCon.FramePredictedClassDetails
                        where r.Id == id
                        select r).FirstOrDefault();
            }
        }




        public IList<FramePredictedClassDetail> UpdateBatch(IList<FramePredictedClassDetail> entities)
        {
            try
            {
                string username = UserDetails.userName;
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    username = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
                }
                using (dbCon = new framedetailsNewContext())
                {
                    List<int> ids = entities.Select(e => e.Id).ToList();
                    List<FramePredictedClassDetail> dbEntities = dbCon.FramePredictedClassDetails.Where(t => ids.Contains(t.Id)).ToList();
                    foreach (var ent in dbEntities)
                    {
                        ent.PredictionType = entities.Where(e => e.Id == ent.Id).Select(s => s.PredictionType).First();
                        ent.ModifiedBy = username;
                        ent.ModifiedDate = DateTime.UtcNow;
                    }
                    dbCon.SaveChanges();

                    return entities;
                }
            }
            catch
            {
                LogHandler.LogError("Error while updating Batch for the FramePredictedClassDetail table", LogHandler.Layer.Business, null);
                return null;
            }
        }
    }
}
