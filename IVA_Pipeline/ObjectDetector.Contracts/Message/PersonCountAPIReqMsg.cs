/*=============================================================================================================== *
 * Copyright 2025 Infosys Ltd.                                                                                    *
 * Use of this source code is governed by Apache License Version 2.0 that can be found in the LICENSE file or at  *
 * http://www.apache.org/licenses/                                                                                *
 * ===============================================================================================================*/
﻿using Infosys.Solutions.Ainauto.VideoAnalytics.Services.MaskDetector.Contracts.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infosys.Solutions.Ainauto.VideoAnalytics.Services.Contracts.Message
{
    public class PersonCountAPIReqMsg
    {
        public string Tid { get; set; }
        public string Did { get; set; }
        public string Fid { get; set; }
        public string Cs { get; set; }
        public PersonCount[] Per { get; set; }
        public string Tok { get; set; }
        public string Base64_image { get; set; }
    }
}
