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
    public partial class Observation
    {
        public int ObservationId { get; set; }
        public int PlatformId { get; set; }
        public string ResourceId { get; set; }
        public int ResourceTypeId { get; set; }
        public int ObservableId { get; set; }
        public string ObservableName { get; set; }
        public string ObservationStatus { get; set; }
        public string Value { get; set; }
        public DateTime? ObservationTime { get; set; }
        public string SourceIp { get; set; }
        public string Description { get; set; }
        public int? RemediationPlanExecId { get; set; }
        public string RemediationStatus { get; set; }
        public string EventType { get; set; }
        public string Source { get; set; }
        public string State { get; set; }
        public string IsNotified { get; set; }
        public DateTime? NotifiedTime { get; set; }
        public int TenantId { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreateDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string PortfolioId { get; set; }
        public string ConfigId { get; set; }
        public int? ObservationSequence { get; set; }
        public string IncidentId { get; set; }
        public string Application { get; set; }
        public string TransactionId { get; set; }
    }
}
