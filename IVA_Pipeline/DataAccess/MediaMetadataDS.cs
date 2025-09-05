/*=============================================================================================================== *
 * Copyright 2025 Infosys Ltd.                                                                                    *
 * Use of this source code is governed by Apache License Version 2.0 that can be found in the LICENSE file or at  *
 * http://www.apache.org/licenses/                                                                                *
 * ===============================================================================================================*/
﻿using Infosys.Solutions.Ainauto.VideoAnalytics.Resource.Entity;
using Infosys.Solutions.Ainauto.VideoAnalytics.Resource.IDataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infosys.Solutions.Ainauto.VideoAnalytics.Resource.Entity.Framedetail;

namespace Infosys.Solutions.Ainauto.VideoAnalytics.Resource.DataAccess
{
    public class MediaMetaDataDS : IEntity<MediaMetadatum>
    {

        public framedetailsNewContext dbCon;

        public MediaMetaDataDS()
        {
            dbCon = new framedetailsNewContext();
        }

        public bool Delete(MediaMetadatum entity)
        {
            throw new NotImplementedException();
        }

        public IList<MediaMetadatum> GetAll()
        {
            throw new NotImplementedException();
        }

        public IList<MediaMetadatum> GetAll(MediaMetadatum Entity)
        {
            throw new NotImplementedException();
        }



        public IQueryable<MediaMetadatum> GetAny()
        {
            return dbCon.MediaMetadata;
        }



        public MediaMetadatum Insert(MediaMetadatum entity)
        {
            try
            {
                using (dbCon = new framedetailsNewContext())
                {
                    entity.CreatedDate = DateTime.UtcNow;
                    dbCon.MediaMetadata.Add(entity);
                    dbCon.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return entity;
        }

        public IList<MediaMetadatum> InsertBatch(IList<MediaMetadatum> entities)
        {
            throw new NotImplementedException();
        }

        public MediaMetadatum Update(MediaMetadatum metaDataentity)
        {
            throw new NotImplementedException();
        }


        public MediaMetadatum UpdateFeedMasterId(int feedMasterId, string requestId)
        {
            MediaMetadatum resMetaDataentity = dbCon.MediaMetadata.Single(c => c.RequestId == requestId);
           
            if (resMetaDataentity != null)
            {
                resMetaDataentity.FeedProcessorMasterId = feedMasterId;
                resMetaDataentity.ModifiedBy = System.Security.Principal.WindowsIdentity.GetCurrent().Name; ;
                resMetaDataentity.ModifiedDate = DateTime.UtcNow;

                dbCon.SaveChanges();
            }

            return resMetaDataentity;
        }







        public MediaMetadatum GetOne(MediaMetadatum entity)
        {
            using (dbCon = new framedetailsNewContext())
            {
                entity = (from s in dbCon.MediaMetadata
                          where
                          s.MediaId == entity.MediaId
                          && s.TenantId == entity.TenantId
                          select s).FirstOrDefault();
            }
            return entity;
        }




        public IList<MediaMetadatum> UpdateBatch(IList<MediaMetadatum> entities)
        {
            throw new NotImplementedException();
        }
    }
}
