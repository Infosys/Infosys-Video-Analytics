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
    public partial class EnvironmentScanMetricDetail
    {
        public int Id { get; set; }
        public int EnvironmentScanMetricId { get; set; }
        public string MetricName { get; set; }
        public int MetricId { get; set; }
        public string MetricKey { get; set; }
        public string AttributeName { get; set; }
        public string AttributeValue { get; set; }
        public string DisplayName { get; set; }
        public bool IsActive { get; set; }
        public int TenantId { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }

        public virtual EnvironmentScanMetric EnvironmentScanMetric { get; set; }
    }
}
