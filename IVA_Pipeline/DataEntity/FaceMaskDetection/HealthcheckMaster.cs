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
    public partial class HealthcheckMaster
    {
        public HealthcheckMaster()
        {
            HealthcheckDetails = new HashSet<HealthcheckDetail>();
            HealthcheckTrackers = new HashSet<HealthcheckTracker>();
        }

        public string ConfigId { get; set; }
        public string ConfigurationName { get; set; }
        public string Description { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreateDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public DateTime? Validitystart { get; set; }
        public DateTime? Validityend { get; set; }
        public int TenantId { get; set; }

        public virtual ICollection<HealthcheckDetail> HealthcheckDetails { get; set; }
        public virtual ICollection<HealthcheckTracker> HealthcheckTrackers { get; set; }
    }
}
