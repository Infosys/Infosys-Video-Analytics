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
using DE = Infosys.Solutions.Ainauto.VideoAnalytics.Resource.Entity.VideoAnalytics;

namespace Infosys.Solutions.Ainauto.VideoAnalytics.Resource.DataAccess
{
    
    public class ResourceDS : IDataAccess.IEntity<DE.Resource>
    {
        public DE.facemaskdetectionPTestContext dbCon;     
        public ResourceDS()
        {
            dbCon = new DE.facemaskdetectionPTestContext();
        }
        public bool Delete(DE.Resource entity)
        {
            throw new NotImplementedException();
        }

        public IList<DE.Resource> GetAll()
        {
            using (dbCon = new DE.facemaskdetectionPTestContext())
            {
                return dbCon.Resources.ToList();
            }
        }

        public IList<DE.Resource> GetAll(DE.Resource Entity)
        {
            throw new NotImplementedException();
        }

        public IQueryable<DE.Resource> GetAny()
        {
            throw new NotImplementedException();
        }

        public DE.Resource GetOne(DE.Resource Entity)
        {
            using(dbCon=new DE.facemaskdetectionPTestContext())
            {
                Entity = (from c in dbCon.Resources
                          where c.ResourceId == Entity.ResourceId
                          select c).FirstOrDefault();

                return Entity;
            }
            
        }

        public DE.Resource Insert(DE.Resource entity)
        {
            throw new NotImplementedException();
        }

        public IList<DE.Resource> InsertBatch(IList<DE.Resource> entities)
        {
            throw new NotImplementedException();
        }

        public DE.Resource Update(DE.Resource entity)
        {
            throw new NotImplementedException();
        }

        public IList<DE.Resource> UpdateBatch(IList<DE.Resource> entities)
        {
            throw new NotImplementedException();
        }
    }
    public class ResourceDSExtn
    {
        public DE.facemaskdetectionPTestContext dbCon;
        public ResourceDSExtn()
        {
            dbCon = new DE.facemaskdetectionPTestContext();
        }
        public DE.Resource GetOne(DE.Resource Entity)
        {
            using (dbCon = new DE.facemaskdetectionPTestContext())
            {
                Entity = (from c in dbCon.Resources
                          where c.ResourceName == Entity.ResourceName
                          select c).FirstOrDefault();

                return Entity;
            }

        }
    }
}
