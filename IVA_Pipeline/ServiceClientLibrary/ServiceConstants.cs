/*=============================================================================================================== *
 * Copyright 2025 Infosys Ltd.                                                                                    *
 * Use of this source code is governed by Apache License Version 2.0 that can be found in the LICENSE file or at  *
 * http://www.apache.org/licenses/                                                                                *
 * ===============================================================================================================*/
﻿

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.ServiceClientLibrary
{
    public class ServiceConstants
    {

        public const string sMaskDetectorUrl = "{0}/{1}";
        
        public const string sModelInferenceUrl = "{0}";
    }

    public enum Services
    {
        Configuration,
        ObjectDetectorServices,
        facemaskdetection,
        videoanalytics,
        uniquepersondetection
    }
}
