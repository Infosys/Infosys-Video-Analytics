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
    public partial class Ticket
    {
        public int Id { get; set; }
        public string Ticketingsystemname { get; set; }
        public string Ticketid { get; set; }
        public string Shortdesc { get; set; }
        public DateTime Createdate { get; set; }
        public string Resolvingparty { get; set; }
        public short? Autoresolutionattempted { get; set; }
        public string Status { get; set; }
        public string Comments { get; set; }
        public DateTime? Resolutiondate { get; set; }
    }
}
