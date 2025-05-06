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
 
using System.Runtime.Serialization;
using SE = Infosys.Solutions.Ainauto.VideoAnalytics.Services.MaskDetector.Contracts;
 
namespace Infosys.Solutions.Ainauto.VideoAnalytics.Resource.Entity.Framedetail
{
 public   partial class TemplateDetails
    {

        public int TemplateId { get; set; }
        
        public string TemplateName { get; set; }
   public string     AttributeName { get; set; }
   public string AttributeValue { get; set; }
        public string AttributeComparison { get; set; }
        public string AttributeApplicability { get; set; }
    public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
    }

    public partial class Template
    {

        public int TemplateId { get; set; }

        public string TemplateName { get; set; }
       public int IsActive { get; set; }
 
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
    }
}
