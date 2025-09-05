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
    public partial class Error
    {
        public int Id { get; set; }
        public string Systemname { get; set; }
        public string Parametername { get; set; }
        public string Errorcode { get; set; }
        public string Errordesc { get; set; }
        public DateTime Createdate { get; set; }
    }
}
