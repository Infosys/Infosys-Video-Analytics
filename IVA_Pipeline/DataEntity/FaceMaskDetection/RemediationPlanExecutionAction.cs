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
    public partial class RemediationPlanExecutionAction
    {
        public int RemediationPlanExecActionId { get; set; }
        public int RemediationPlanExecId { get; set; }
        public int RemediationPlanActionId { get; set; }
        public string CorrelationId { get; set; }
        public string Status { get; set; }
        public string Output { get; set; }
        public string Logdata { get; set; }
        public string OrchestratorDetails { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreateDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public int TenantId { get; set; }

        public virtual RemediationPlanExecution RemediationPlanExec { get; set; }
    }
}
