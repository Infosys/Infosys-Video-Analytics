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
    public partial class HealthcheckIterationTrackerDetail
    {
        public int TrackingDetailsId { get; set; }
        public string HealthcheckTrackingId { get; set; }
        public string ResourceId { get; set; }
        public string HealthcheckSource { get; set; }
        public int ObservableId { get; set; }
        public Guid? SeeTransactionId { get; set; }
        public int? Status { get; set; }
        public string Error { get; set; }
        public DateTime? StartTime { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public int TenantId { get; set; }
    }
}
