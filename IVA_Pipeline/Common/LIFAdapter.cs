/*=============================================================================================================== *
 * Copyright 2025 Infosys Ltd.                                                                                    *
 * Use of this source code is governed by Apache License Version 2.0 that can be found in the LICENSE file or at  *
 * http://www.apache.org/licenses/                                                                                *
 * ===============================================================================================================*/
﻿using Infosys.Lif.LegacyIntegrator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.Common
{
    public class LIFAdapter
    {
        public string GetConfigurations(string transportRegion, Dictionary<string, string> secrets)
        {
            AdapterManager adapterManager = new AdapterManager();
            string result = adapterManager.GetSecrets(transportRegion, secrets);
            return result;
        }
    }
}
