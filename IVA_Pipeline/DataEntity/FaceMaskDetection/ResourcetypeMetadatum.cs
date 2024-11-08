/*=============================================================================================================== *
 * Copyright 2024 Infosys Ltd.                                                                                    *
 * Use of this source code is governed by Apache License Version 2.0 that can be found in the LICENSE file or at  *
 * http://www.apache.org/licenses/                                                                                *
 * ===============================================================================================================*/

﻿using System;
using System.Collections.Generic;

#nullable disable

namespace Infosys.Solutions.Ainauto.VideoAnalytics.Resource.Entity.VideoAnalytics
{
    public partial class ResourcetypeMetadatum
    {
        public int ResourceTypeId { get; set; }
        public string AttributeName { get; set; }
        public string DefaultValue { get; set; }
        public string AttributeType { get; set; }
        public bool IsMandatory { get; set; }
        public int? Sequence { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreateDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public int TenantId { get; set; }
        public string Description { get; set; }
        public string DisplayName { get; set; }

        public virtual Resourcetype ResourceType { get; set; }
    }
}
