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
using Infosys.Solutions.Ainauto.VideoAnalytics.Services.MaskDetector.Contracts;


namespace Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.ServiceClientLibrary
{
    public class ModelInferenceAPI
    {
        string _serviceUrl; 
        string _baseUrl;
        public string ServiceUrl
        {
            get { return _serviceUrl; }
            set { _serviceUrl = value; }
        }
       
        public ModelInferenceAPI(string baseurl,string serviceUrl = "")
        {
            _serviceUrl = serviceUrl;
            _baseUrl = baseurl;

        }

        
    }
}
