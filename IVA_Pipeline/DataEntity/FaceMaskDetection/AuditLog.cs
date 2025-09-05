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
    public partial class AuditLog
    {
        public int LogId { get; set; }
        public int? AnomalyId { get; set; }
        public string ResourceId { get; set; }
        public int ObservableId { get; set; }
        public int ActionId { get; set; }
        public string ActionParameters { get; set; }
        public DateTime? LogDate { get; set; }
        public string Status { get; set; }
        public string Output { get; set; }
        public int PlatformId { get; set; }
        public int TenantId { get; set; }
        public string TransactionId { get; set; }
        public string PortfolioId { get; set; }
        public string IncidentId { get; set; }

        public virtual Action Action { get; set; }
        public virtual Observable Observable { get; set; }
        public virtual Resource Resource { get; set; }
    }
}
