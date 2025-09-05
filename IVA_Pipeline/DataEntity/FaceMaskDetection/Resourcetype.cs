/*=============================================================================================================== *
 * Copyright 2025 Infosys Ltd.                                                                                    *
 * Use of this source code is governed by Apache License Version 2.0 that can be found in the LICENSE file or at  *
 * http://www.apache.org/licenses/                                                                                *
 * ===============================================================================================================*/
﻿using System;
using System.Collections.Generic;

#nullable disable

namespace Infosys.Solutions.Ainauto.VideoAnalytics.Resource.Entity.VideoAnalytics
{
    public partial class Resourcetype
    {
        public Resourcetype()
        {
            ResourcetypeMetadata = new HashSet<ResourcetypeMetadatum>();
            ResourcetypeObservableActionMaps = new HashSet<ResourcetypeObservableActionMap>();
            ResourcetypeObservableMaps = new HashSet<ResourcetypeObservableMap>();
            ResourcetypeObservableRemediationPlanMaps = new HashSet<ResourcetypeObservableRemediationPlanMap>();
        }

        public int ResourceTypeId { get; set; }
        public string ResourceTypeName { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreateDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public DateTime ValidityStart { get; set; }
        public DateTime ValidityEnd { get; set; }
        public int TenantId { get; set; }
        public bool? IsMainEntiry { get; set; }
        public string Type { get; set; }
        public string PlatfromType { get; set; }

        public virtual ICollection<ResourcetypeMetadatum> ResourcetypeMetadata { get; set; }
        public virtual ICollection<ResourcetypeObservableActionMap> ResourcetypeObservableActionMaps { get; set; }
        public virtual ICollection<ResourcetypeObservableMap> ResourcetypeObservableMaps { get; set; }
        public virtual ICollection<ResourcetypeObservableRemediationPlanMap> ResourcetypeObservableRemediationPlanMaps { get; set; }
    }
}
