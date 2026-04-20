/*=============================================================================================================== *
 * Copyright 2025 Infosys Ltd.                                                                                    *
 * Use of this source code is governed by Apache License Version 2.0 that can be found in the LICENSE file or at  *
 * http://www.apache.org/licenses/                                                                                *
 * ===============================================================================================================*/
﻿using Infosys.Solutions.Ainauto.VideoAnalytics.BusinessEntity.Queue;
using Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.ProcessScheduler.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infosys.Solutions.Ainauto.VideoAnalytics.Renderer;

namespace Renderer
{
    public static class FrameRendererHelper
    {
        public static Dictionary<int, string> colornames = new Dictionary<int, string>();

        static FrameRendererHelper()
        {
            colornames = ColorHelper.GetAllKnownColorNames();
        }
    }
}
