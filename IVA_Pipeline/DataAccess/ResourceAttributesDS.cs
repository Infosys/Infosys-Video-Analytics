/*=============================================================================================================== *
 * Copyright 2025 Infosys Ltd.                                                                                    *
 * Use of this source code is governed by Apache License Version 2.0 that can be found in the LICENSE file or at  *
 * http://www.apache.org/licenses/                                                                                *
 * ===============================================================================================================*/
﻿

using Infosys.Solutions.Ainauto.VideoAnalytics.Resource.Entity.VideoAnalytics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.Common.ApplicationConstants;

namespace Infosys.Solutions.Ainauto.VideoAnalytics.Resource.DataAccess
{
    public class ResourceAttributesDS : IDataAccess.IEntity<ResourceAttribute>
    {
        public facemaskdetectionPTestContext dbCon;
        public ResourceAttributesDS()
        {
            dbCon = new facemaskdetectionPTestContext();
        }

        public bool Delete(ResourceAttribute entity)
        {
            throw new NotImplementedException();
        }

        public IList<ResourceAttribute> GetAll()
        {
            return dbCon.ResourceAttributes.ToList();
        }

        public string GetOfflineVideoDirectory(int tenantId, string ResourceId)
        {
            using (dbCon = new facemaskdetectionPTestContext())
            {
                string directory = dbCon.ResourceAttributes.Where(r => r.TenantId == tenantId && r.ResourceId == ResourceId && r.AttributeName == "OFFLINE_VIDEO_DIRECTORY").Select(r => r.AttributeValue).FirstOrDefault();
                return directory;
            }
        }

        public IList<ResourceAttribute> GetAll(ResourceAttribute Entity)
        {
            using (dbCon = new facemaskdetectionPTestContext())
            {
                var res = (from s in dbCon.ResourceAttributes
                           where s.ResourceId == Entity.ResourceId
                           && s.TenantId == Entity.TenantId
                           select s).ToList();
                return res;
            }
        }

        public List<string> GetDisplayNameOfPredictionModels()
        {
            using (dbCon = new facemaskdetectionPTestContext())
            {
                var res = (from s in dbCon.ResourceAttributes
                           where s.AttributeName == ProcessingStatus.PredictionModelDb
                           && s.DisplayName != null
                           select s.DisplayName).Distinct().ToList();
                return res;
            }
        }

        public IQueryable<ResourceAttribute> GetAny()
        {
            return dbCon.ResourceAttributes;
        }

        public ResourceAttribute GetOne(ResourceAttribute Entity)
        {
            using (dbCon = new facemaskdetectionPTestContext())
            {
                Entity = (from s in dbCon.ResourceAttributes where s.ResourceId == Entity.ResourceId && s.AttributeName == Entity.AttributeName select s).FirstOrDefault();
            }
            return Entity;
        }

        public string GetPredictionModelWithDisplayName(string displayName)
        {
            string predictionModel = "";
            using (dbCon = new facemaskdetectionPTestContext())
            {
                predictionModel = (from s in dbCon.ResourceAttributes where s.DisplayName == displayName select s.AttributeValue).FirstOrDefault();
            }
            return predictionModel;
        }

        public string GetDisplayNameWithPredictionModel(string modelName)
        {
            string predictionModel = "";
            using (dbCon = new facemaskdetectionPTestContext())
            {
                predictionModel = (from s in dbCon.ResourceAttributes where s.AttributeValue == modelName select s.DisplayName).FirstOrDefault();
            }
            return predictionModel;
        }

        public ResourceAttribute Insert(ResourceAttribute entity)
        {
            throw new NotImplementedException();
        }

        public IList<ResourceAttribute> InsertBatch(IList<ResourceAttribute> entities)
        {
            throw new NotImplementedException();
        }

        public bool UpdateClientStatus(ResourceAttribute entity)
        {
            ResourceAttribute result = (from s in dbCon.ResourceAttributes
                                          where s.ResourceId == entity.ResourceId &&
                                            s.AttributeName == entity.AttributeName &&
                                             s.TenantId == entity.TenantId
                                          select s).SingleOrDefault();


            result.AttributeValue = entity.AttributeValue;

            dbCon.SaveChanges();

            return true;
        }


        public bool UpdateResourceAttribute(ResourceAttribute entity)
        {
            ResourceAttribute result = (from s in dbCon.ResourceAttributes
                                          where s.ResourceId == entity.ResourceId &&
                                            s.AttributeName == entity.AttributeName &&
                                             s.TenantId == entity.TenantId
                                          select s).SingleOrDefault();
            result.AttributeName = entity.AttributeName;
            result.AttributeValue = entity.AttributeValue;
            result.ModifiedBy = entity.ModifiedBy;
            result.ModifiedDate = entity.ModifiedDate;
            dbCon.SaveChanges();
            return true;
        }


        public bool getClientStatus(ResourceAttribute entity)
        {
            ResourceAttribute result = (from s in dbCon.ResourceAttributes
                                          where s.ResourceId == entity.ResourceId &&
                                            s.AttributeName == entity.AttributeName &&
                                            s.TenantId == entity.TenantId
                                          select s).SingleOrDefault();


            if (result.AttributeValue.ToLower() == "yes")
            {
                return true;
            }

            return false;


        }
        public ResourceAttribute Update(ResourceAttribute entity)
        {
            throw new NotImplementedException();
        }

        public IList<ResourceAttribute> UpdateBatch(IList<ResourceAttribute> entities)
        {
            throw new NotImplementedException();
        }
    }
}
