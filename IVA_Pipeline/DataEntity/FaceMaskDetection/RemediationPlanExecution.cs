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
    public partial class RemediationPlanExecution
    {
        public RemediationPlanExecution()
        {
            RemediationPlanExecutionActions = new HashSet<RemediationPlanExecutionAction>();
        }

        public int RemediationPlanExecId { get; set; }
        public int RemediationPlanId { get; set; }
        public string ResourceId { get; set; }
        public int ObservableId { get; set; }
        public int ObservationId { get; set; }
        public string NodeDetails { get; set; }
        public string ExecutedBy { get; set; }
        public DateTime? ExecutionStartDateTime { get; set; }
        public DateTime? ExecutionEndDateTime { get; set; }
        public int? ExecutionPercentage { get; set; }
        public string Status { get; set; }
        public bool? IsNotified { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreateDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool? IsPicked { get; set; }
        public int TenantId { get; set; }

        public virtual ICollection<RemediationPlanExecutionAction> RemediationPlanExecutionActions { get; set; }
    }
}
