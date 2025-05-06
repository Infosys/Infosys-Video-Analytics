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
using static Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.Common.ApplicationConstants;

namespace Infosys.Solutions.Ainauto.VideoAnalytics.Resource.DataAccess
{
    public class FrameMetaDataDS : IDataAccess.IEntity<FrameMetadatum>
    {
        public framedetailsNewContext dbCon;
        public FrameMetaDataDS()
        {
            dbCon = new framedetailsNewContext();
        }

        public bool Delete(FrameMetadatum entity)
        {
            throw new NotImplementedException();
        }

        public IList<FrameMetadatum> GetAll()
        {
            return dbCon.FrameMetadata.ToList();
        }

        public IList<FrameMetadatum> GetAll(FrameMetadatum Entity)
        {
            throw new NotImplementedException();
        }

        public IQueryable<FrameMetadatum> GetAny()
        {
            return dbCon.FrameMetadata;
        }
        public FrameMetadatum GetRecord(FrameMetadatum Entity)
        {
            using (dbCon = new framedetailsNewContext())
            {

                
                Entity = (from s in dbCon.FrameMetadata
                         .Where(k => k.ResourceId == Entity.ResourceId && k.FrameId == Entity.FrameId)
                          select (s)).FirstOrDefault();


            }

            return Entity;
        }

        public IList<FrameMetadatum> GetnRecords(FrameMetadatum Entity, int n)
        {


            IList<FrameMetadatum> frame_processed_List = new List<FrameMetadatum>();
            using (dbCon = new framedetailsNewContext())
            {

                frame_processed_List = (from s in dbCon.FrameMetadata
                                        orderby s.SequenceId ascending
                                        where s.SequenceId > Entity.SequenceId  && s.PredictionType == Entity.PredictionType
                                        select s).Take(n).ToList();

                return frame_processed_List;
            }


        }

        public FrameMetadatum GetOneAnalytics(FrameMetadatum Entity)
        {
            using (dbCon = new framedetailsNewContext())
            {
                Entity = (from s in dbCon.FrameMetadata 
                          where s.FrameId == Entity.FrameId
                          && s.ResourceId == Entity.ResourceId
                          && s.TenantId == Entity.TenantId
                          select s).FirstOrDefault();
            }
            return Entity;
        }
        public FrameMetadatum GetOne(FrameMetadatum Entity)
        {
            using (dbCon = new framedetailsNewContext())
            {
                Entity = (from s in dbCon.FrameMetadata where s.SequenceId == Entity.SequenceId select s).FirstOrDefault();
            }
            return Entity;
        }
        public FrameMetadatum GetTopRecord(FrameMetadatum Entity)
        {
            using (dbCon = new framedetailsNewContext())
            {

                
                Entity = (from s in dbCon.FrameMetadata
                         .Where(k => k.Status == null).OrderByDescending(u => u.FrameGrabTime)
                          select (s)).FirstOrDefault();


            }

            return Entity;
        }

        public FrameMetadatum Insert(FrameMetadatum entity)
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
                    entity.CreatedBy = username;
                    entity.CreatedDate = DateTime.UtcNow;
                    dbCon.FrameMetadata.Add(entity);
                    dbCon.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("PRIMARY KEY constraint") || ex.InnerException.InnerException.Message.Contains("PRIMARY KEY constraint"))
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
        public FrameMetadatum GetList(FrameMetadatum Entity)
        {
            using (dbCon = new framedetailsNewContext())
            {
                Entity = (from s in dbCon.FrameMetadata where s.SequenceId == Entity.SequenceId select s).FirstOrDefault();
            }
            return Entity;

        }

        public FrameMetadatum InsertList(FrameMetadatum entity)
        {
            try
            {
                using (dbCon = new framedetailsNewContext())
                {
                    entity.CreatedDate = DateTime.UtcNow;
                    dbCon.FrameMetadata.Add(entity);
                    dbCon.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return entity;
        }



        public IList<FrameMetadatum> InsertBatch(IList<FrameMetadatum> entities)
        {
            throw new NotImplementedException();
        }

        public FrameMetadatum Update(FrameMetadatum entity)
        {
            using (dbCon = new framedetailsNewContext())
            {
                string username = UserDetails.userName;
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    username = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
                }
                var res = (from s in dbCon.FrameMetadata
                             where s.ResourceId == entity.ResourceId
                             && s.FrameId == entity.FrameId
                             && s.TenantId == entity.TenantId
                             select s).FirstOrDefault();
                if (res != null)
                {
                    res.PredictionType = entity.PredictionType;
                    res.ModifiedBy = username;
                    res.ModifiedDate = DateTime.UtcNow;
                    dbCon.SaveChanges();
                    return res;
                }
                else
                    return null;                
            }
        }
        public FrameMetadatum UpdateStatus(FrameMetadatum entity)
        {
            using (dbCon = new framedetailsNewContext())
            {



                (from s in dbCon.FrameMetadata
                 where s.ResourceId == entity.ResourceId && s.FrameId == entity.FrameId
                 select s)
                 .ToList().ForEach(x => x.Status = entity.Status);

                dbCon.SaveChanges();
            }
            return entity;
        }
        public FrameMetadatum UpdateFrameMetadata(FrameMetadatum entity)
        {
            using (dbCon = new framedetailsNewContext())
            {



                (from s in dbCon.FrameMetadata
                 where s.ResourceId == entity.ResourceId && s.FrameId == entity.FrameId
                 select s)
                 .ToList().ForEach(x => x.Status = entity.Status);

                dbCon.SaveChanges();
            }
            return entity;
        }

        public IList<FrameMetadatum> UpdateBatch(IList<FrameMetadatum> entities)
        {
            throw new NotImplementedException();
        }
    }
}
