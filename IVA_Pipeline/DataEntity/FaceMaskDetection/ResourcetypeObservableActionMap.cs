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
    public partial class ResourcetypeObservableActionMap
    {
        public int ResourceTypeId { get; set; }
        public int ObservableId { get; set; }
        public int ActionId { get; set; }
        public string Name { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreateDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public DateTime? ValidityStart { get; set; }
        public DateTime? ValidityEnd { get; set; }
        public int TenantId { get; set; }

        public virtual Action Action { get; set; }
        public virtual Observable Observable { get; set; }
        public virtual Resourcetype ResourceType { get; set; }
    }
}
