/*=============================================================================================================== *
 * Copyright 2025 Infosys Ltd.                                                                                    *
 * Use of this source code is governed by Apache License Version 2.0 that can be found in the LICENSE file or at  *
 * http://www.apache.org/licenses/                                                                                *
 * ===============================================================================================================*/
﻿

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infosys.Solutions.Ainauto.VideoAnalytics.Resource.Entity;
using Infosys.Solutions.Ainauto.VideoAnalytics.Resource.Entity.VideoAnalytics;

namespace Infosys.Solutions.Ainauto.VideoAnalytics.Resource.DataAccess
{
    public class ResourceDependencyMapDS : IDataAccess.IEntity<ResourceDependencyMap>
    {
        public facemaskdetectionPTestContext dbCon;
        public ResourceDependencyMapDS()
        {
            dbCon = new facemaskdetectionPTestContext();
        }
        public bool Delete(ResourceDependencyMap entity)
        {
            throw new NotImplementedException();
        }

        public IList<ResourceDependencyMap> GetAll()
        {
            using (dbCon = new facemaskdetectionPTestContext())
                return dbCon.ResourceDependencyMaps.ToList();
        }

        public IList<ResourceDependencyMap> GetAll(ResourceDependencyMap Entity)
        {
            var res = (from r in dbCon.ResourceDependencyMaps
                       where r.PortfolioId == Entity.PortfolioId
                       && r.TenantId == Entity.TenantId
                       select r).ToList();
            return res;
        }

        public IQueryable<ResourceDependencyMap> GetAny()
        {
           
                return dbCon.ResourceDependencyMaps;
        }

        public ResourceDependencyMap GetOne(ResourceDependencyMap Entity)
        {
            ResourceDependencyMap resourceDependencyMap = (from rdmds in dbCon.ResourceDependencyMaps
                       where rdmds.ResourceId == Entity.ResourceId
                       select rdmds).FirstOrDefault();
            return resourceDependencyMap;
        }

        public ResourceDependencyMap Insert(ResourceDependencyMap entity)
        {
            throw new NotImplementedException();
        }

        public IList<ResourceDependencyMap> InsertBatch(IList<ResourceDependencyMap> entities)
        {
            throw new NotImplementedException();
        }

        public ResourceDependencyMap Update(ResourceDependencyMap entity)
        {
            throw new NotImplementedException();
        }

        public IList<ResourceDependencyMap> UpdateBatch(IList<ResourceDependencyMap> entities)
        {
            throw new NotImplementedException();
        }
    }

    public class ResourceDependencyMapDSExtn
    {
        public facemaskdetectionPTestContext dbCon;
        public ResourceDependencyMapDSExtn()
        {
            dbCon = new facemaskdetectionPTestContext();
        }
        
    }
}
