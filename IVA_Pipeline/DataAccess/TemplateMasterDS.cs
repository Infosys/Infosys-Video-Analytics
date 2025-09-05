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
using Infosys.Solutions.Ainauto.VideoAnalytics.Resource.Entity.VideoAnalytics;
using Infosys.Solutions.Ainauto.VideoAnalytics.Resource.IDataAccess;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using static Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.Common.ApplicationConstants;

namespace DataAccess
{
    public class TemplateMasterDS : IEntity<TemplateDetails>
    {

       
        public facemaskdetectionPTestContext dbCon;

        public TemplateMasterDS()
        {
            dbCon = new facemaskdetectionPTestContext();
        }

        public bool Delete(TemplateDetails entity)
        {
            throw new NotImplementedException();
        }

        public IList<TemplateDetails> GetAll(TemplateDetails Entity)
        {
            List<TemplateDetails> list = new List<TemplateDetails>();
            
            try
            {
                using (dbCon = new facemaskdetectionPTestContext())
                {


                    


                    list = (from rt in dbCon.Resourcetypes
                            join r in dbCon.Resources on rt.ResourceTypeId equals r.ResourceTypeId
                            join ra in dbCon.ResourceAttributes on r.ResourceId equals ra.ResourceId
                            join om in dbCon.ObservableResourceMaps on ra.ResourceId equals om.ResourceId
                            join otr in dbCon.Operators on om.OperatorId equals otr.OperatorId.ToString()
                            join os in dbCon.Observables on om.ObservableId equals os.ObservableId
                            where rt.ResourceTypeName == Entity.TemplateName
                            select new TemplateDetails
                            {
                                ResourceTypeName = rt.ResourceTypeName,
                                AttributeName = ra.AttributeName,
                                AttributeValue = ra.AttributeValue,
                                ObservableName = os.ObservableName,
                                Operator = otr.Operator1,
                                TemplateName = Entity.TemplateName

                            }
                                ).ToList();
                     
                }
            }
            catch (Exception ex)
            {
                return null;
            }
            return list;

            
            return null;
            
            throw new NotImplementedException();
        }

        public IList<TemplateDetails> GetAll()
        {
            throw new NotImplementedException();
        }

        public IQueryable<TemplateDetails> GetAny()
        {
            throw new NotImplementedException();
        }




        public TemplateDetails Insert(TemplateDetails entity)
        {
            
            throw new NotImplementedException();
        }

        public int GetCount(int feedId)
        {
           
            throw new NotImplementedException();
        }

        public IList<TemplateDetails> InsertBatch(IList<TemplateDetails> entities)
        {
            throw new NotImplementedException();
        }

        public TemplateDetails Update(TemplateDetails entity)
        {
            

            throw new NotImplementedException();
        }



        public TemplateDetails UpdatePersonCount(TemplateDetails entity)
        {
           
            throw new NotImplementedException();
        }

        public TemplateDetails GetRecord(TemplateDetails Entity)
        {
            
            throw new NotImplementedException();

        }

        public TemplateDetails GetOne(TemplateDetails entity)
        {
            
            throw new NotImplementedException();
        }




        public IList<TemplateDetails> UpdateBatch(IList<TemplateDetails> entities)
        {
            throw new NotImplementedException();
        }
    }
}
