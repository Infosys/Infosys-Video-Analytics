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

    public class FrameRendererData

    {
        public string Id { get; set; }
        public string TID { get; set; }
        public string DID { get; set; }
        public string FID { get; set; }
        public List< string> FIDS { get; set; }
        public string PTS { get; set; }
        public string NP { get; set; }
        public string FeedId { get; set; } 
        public string Class { get; set; }
        public PredictionsData[] FS { get; set; }
        public string SequenceNumber { get; set; }
        public string Status { get; set; } 
        public string FrameNumber { get; set; } 
    }





    public class PredictionsData
    {
        public BoundingBoxData DM { get; set; }
        public string CS { get; set; }
        public string LB { get; set; }

        public string NP { get; set; }

        
    }
    public class BoundingBoxData
    {
        public string X { get; set; }
        public string Y { get; set; }
        public string H { get; set; }
        public string W { get; set; }
    }

    public class FramePreloaderData

    {
        public string TID { get; set; }
        public string DID { get; set; }
        public string FID { get; set; }
        public Dictionary<string, List<string>> TE { get; set; } 
        public string SequenceNumber { get; set; } 
        public string FrameNumber { get; set; } 
    }

}



