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
    public partial class EnvironmentScanMetric
    {
        public EnvironmentScanMetric()
        {
            EnvironmentScanMetricDetails = new HashSet<EnvironmentScanMetricDetail>();
        }

        public int EnvironmentScanMetricId { get; set; }
        public int? Version { get; set; }
        public string ResourceId { get; set; }
        public int ObservableId { get; set; }
        public int TenantId { get; set; }
        public int PlatformId { get; set; }
        public DateTime GeneratedDate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }

        public virtual ICollection<EnvironmentScanMetricDetail> EnvironmentScanMetricDetails { get; set; }
    }
}
