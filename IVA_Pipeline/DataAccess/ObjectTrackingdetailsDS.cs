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
    public class ObjectTrackingdetailsDS : IEntity<ObjectTrackingDetail>
    {

        public framedetailsNewContext dbCon;

        public ObjectTrackingdetailsDS()
        {
            dbCon = new framedetailsNewContext();
        }

        public bool Delete(ObjectTrackingDetail entity)
        {
            throw new NotImplementedException();
        }

        public IList<ObjectTrackingDetail> GetAll()
        {
            throw new NotImplementedException();
        }

        public IList<ObjectTrackingDetail> GetAll(ObjectTrackingDetail Entity)
        {

            throw new NotImplementedException();
        }



        public IQueryable<ObjectTrackingDetail> GetAny()
        {
            return dbCon.ObjectTrackingDetails;
        }



        public ObjectTrackingDetail Insert(ObjectTrackingDetail entity)
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
                    entity.CreatedDate = DateTime.UtcNow;
                    entity.CreatedBy = username;
                    dbCon.ObjectTrackingDetails.Add(entity);
                    dbCon.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                LogHandler.LogError("Error while Inserting into frame_predicted_class_details table. message: {0}", LogHandler.Layer.Business, ex.Message);
                return null;
            }

            return entity;
        }

        public IList<ObjectTrackingDetail> InsertBatch(IList<ObjectTrackingDetail> entities)
        {
            throw new NotImplementedException();
        }

        public ObjectTrackingDetail Update(ObjectTrackingDetail entity)
        {
            throw new NotImplementedException();
        }
        public ObjectTrackingDetail GetOne(ObjectTrackingDetail entity)
        {
            throw new NotImplementedException();
        }


        public IList<ObjectTrackingDetail> UpdateBatch(IList<ObjectTrackingDetail> entities)
        {
            throw new NotImplementedException();
        }
    }
}
