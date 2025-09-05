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
    public partial class Action
    {
        public Action()
        {
            AuditLogs = new HashSet<AuditLog>();
            RemediationPlanActionMaps = new HashSet<RemediationPlanActionMap>();
            ResourceObservableActionMaps = new HashSet<ResourceObservableActionMap>();
            ResourcetypeObservableActionMaps = new HashSet<ResourcetypeObservableActionMap>();
        }

        public int ActionId { get; set; }
        public string ActionName { get; set; }
        public int ActionTypeId { get; set; }
        public string EndpointUri { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreateDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public DateTime? ValidityStart { get; set; }
        public bool? IsDeleted { get; set; }
        public DateTime? ValidityEnd { get; set; }
        public int TenantId { get; set; }
        public int? ScriptId { get; set; }
        public int? CategoryId { get; set; }
        public string CategoryName { get; set; }
        public int? AutomationEngineId { get; set; }
        public string AutomationEngineName { get; set; }

        public virtual ICollection<AuditLog> AuditLogs { get; set; }
        public virtual ICollection<RemediationPlanActionMap> RemediationPlanActionMaps { get; set; }
        public virtual ICollection<ResourceObservableActionMap> ResourceObservableActionMaps { get; set; }
        public virtual ICollection<ResourcetypeObservableActionMap> ResourcetypeObservableActionMaps { get; set; }
    }
}
