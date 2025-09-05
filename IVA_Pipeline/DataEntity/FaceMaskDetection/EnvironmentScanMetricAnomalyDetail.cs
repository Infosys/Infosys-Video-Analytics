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
    public partial class EnvironmentScanMetricAnomalyDetail
    {
        public int Id { get; set; }
        public string ResourceId { get; set; }
        public int ObservationId { get; set; }
        public int OldVersion { get; set; }
        public int NewVersion { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string MetricName { get; set; }
        public string MetricKey { get; set; }
        public int MetricId { get; set; }
        public string AttributeName { get; set; }
        public string AttributeValue { get; set; }
        public string AttributeStatus { get; set; }
        public string OldValue { get; set; }
        public int TenantId { get; set; }
        public int PlatformId { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreateDate { get; set; }
    }
}
