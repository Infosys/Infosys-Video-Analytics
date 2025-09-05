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
    public partial class ActionParam
    {
        public int ParamId { get; set; }
        public string Name { get; set; }
        public string FieldToMap { get; set; }
        public bool? IsMandatory { get; set; }
        public string DefaultValue { get; set; }
        public string ParamType { get; set; }
        public int ActionId { get; set; }
        public int? AutomationEngineParamId { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreateDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public int TenantId { get; set; }
    }
}
