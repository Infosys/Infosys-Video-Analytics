/*=============================================================================================================== *
 * Copyright 2025 Infosys Ltd.                                                                                    *
 * Use of this source code is governed by Apache License Version 2.0 that can be found in the LICENSE file or at  *
 * http://www.apache.org/licenses/                                                                                *
 * ===============================================================================================================*/
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infosys.Solutions.Ainauto.VideoAnalytics.BusinessEntity
{
    public class ResourceAttributeDetails
    {
        public string DeviceId { get; set; }
        public int TenantId { get; set; }
        public List<Attributes> Attributes { get; set; }
    }
    public class Attributes
    {
        public string AttributeName { get; set; }
        public string AttributeValue { get; set; }
    }
}
