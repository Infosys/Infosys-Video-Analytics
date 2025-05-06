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
using Nest;
using System;
using Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.Common;

namespace Infosys.Solutions.Ainauto.VideoAnalytics.Resource.Entity
{
    public class ElasticsearchExtensions
    {
        string elasticsearchUrl = "";
        public ElasticClient client = null;
        public ElasticsearchExtensions(string index)
        {
            var appSettings = Config.AppSettings;
            if (appSettings.ElasticsearchUrl != null)
            {
                elasticsearchUrl = appSettings.ElasticsearchUrl;
            }

           
            ConnectionSettings settings = new ConnectionSettings(new Uri(elasticsearchUrl))
            .DefaultIndex(index);
            client = new ElasticClient(settings);


        }

    }
}
