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
    public partial class HealthcheckTracker
    {
        public int HealthCheckId { get; set; }
        public string ConfigId { get; set; }
        public string IncidentId { get; set; }
        public string Status { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string Source { get; set; }
        public int TenantId { get; set; }

        public virtual HealthcheckMaster Config { get; set; }
    }
}
