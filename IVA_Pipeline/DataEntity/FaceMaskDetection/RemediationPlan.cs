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
    public partial class RemediationPlan
    {
        public RemediationPlan()
        {
            RemediationPlanActionMaps = new HashSet<RemediationPlanActionMap>();
            ResourceObservableRemediationPlanMaps = new HashSet<ResourceObservableRemediationPlanMap>();
            ResourcetypeObservableRemediationPlanMaps = new HashSet<ResourcetypeObservableRemediationPlanMap>();
        }

        public int RemediationPlanId { get; set; }
        public string RemediationPlanName { get; set; }
        public string RemediationPlanDescription { get; set; }
        public bool IsUserDefined { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreateDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public int TenantId { get; set; }
        public bool? IsDeleted { get; set; }

        public virtual ICollection<RemediationPlanActionMap> RemediationPlanActionMaps { get; set; }
        public virtual ICollection<ResourceObservableRemediationPlanMap> ResourceObservableRemediationPlanMaps { get; set; }
        public virtual ICollection<ResourcetypeObservableRemediationPlanMap> ResourcetypeObservableRemediationPlanMaps { get; set; }
    }
}
