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
using Infosys.Solutions.Ainauto.VideoAnalytics.Resource.Entity.VideoAnalytics;
using Microsoft.EntityFrameworkCore;

namespace Infosys.Solutions.Ainauto.VideoAnalytics.Resource.DataAccess
{
    public class ConfigurationsDS : IDataAccess.IEntity<Configuration>
    {
        public facemaskdetectionPTestContext dbCon;
        public ConfigurationsDS()
        {
            dbCon = new facemaskdetectionPTestContext();
        }
        public bool Delete(Configuration entity)
        {
            throw new NotImplementedException();
        }

        public IList<Configuration> GetAll()
        {
            return dbCon.Configurations.ToList();
        }

        public IList<Configuration> GetAll(Configuration Entity)
        {
            using (dbCon = new facemaskdetectionPTestContext())
            {
                var res = (from c in dbCon.Configurations
                           where c.TenantId == Entity.TenantId
                           && c.ReferenceType == Entity.ReferenceType
                           select c).ToList();
                return res;
            }
        }

        public IQueryable<Configuration> GetAny()
        {
            return dbCon.Configurations;
        }

        public Configuration GetOne(Configuration Entity)
        {
            throw new NotImplementedException();
        }

        public Configuration Insert(Configuration entity)
        {
            throw new NotImplementedException();
        }

        public IList<Configuration> InsertBatch(IList<Configuration> entities)
        {
            throw new NotImplementedException();
        }

        public Configuration Update(Configuration entity)
        {
            throw new NotImplementedException();
        }

        public IList<Configuration> UpdateBatch(IList<Configuration> entities)
        {
            throw new NotImplementedException();
        }
    }
}
