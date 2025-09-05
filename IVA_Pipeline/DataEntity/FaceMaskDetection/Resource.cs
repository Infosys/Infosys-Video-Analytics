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
    public partial class Resource
    {
        public Resource()
        {
            AuditLogs = new HashSet<AuditLog>();
            HealthcheckDetails = new HashSet<HealthcheckDetail>();
            ObservableResourceMaps = new HashSet<ObservableResourceMap>();
            ResourceAttributes = new HashSet<ResourceAttribute>();
            ResourceDependencyMaps = new HashSet<ResourceDependencyMap>();
            ResourceObservableActionMaps = new HashSet<ResourceObservableActionMap>();
            ResourceObservableRemediationPlanMaps = new HashSet<ResourceObservableRemediationPlanMap>();
        }

        public string ResourceId { get; set; }
        public string ResourceName { get; set; }
        public int ResourceTypeId { get; set; }
        public string Source { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreateDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public DateTime ValidityStart { get; set; }
        public DateTime ValidityEnd { get; set; }
        public int TenantId { get; set; }
        public string ResourceRef { get; set; }
        public int? PlatformId { get; set; }
        public string VersionNumber { get; set; }
        public string Comments { get; set; }
        public bool? IsActive { get; set; }

        public virtual ICollection<AuditLog> AuditLogs { get; set; }
        public virtual ICollection<HealthcheckDetail> HealthcheckDetails { get; set; }
        public virtual ICollection<ObservableResourceMap> ObservableResourceMaps { get; set; }
        public virtual ICollection<ResourceAttribute> ResourceAttributes { get; set; }
        public virtual ICollection<ResourceDependencyMap> ResourceDependencyMaps { get; set; }
        public virtual ICollection<ResourceObservableActionMap> ResourceObservableActionMaps { get; set; }
        public virtual ICollection<ResourceObservableRemediationPlanMap> ResourceObservableRemediationPlanMaps { get; set; }
    }
}
