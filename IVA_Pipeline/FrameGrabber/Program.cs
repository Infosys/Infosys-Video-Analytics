/*=============================================================================================================== *
 * Copyright 2025 Infosys Ltd.                                                                                    *
 * Use of this source code is governed by Apache License Version 2.0 that can be found in the LICENSE file or at  *
 * http://www.apache.org/licenses/                                                                                *
 * ===============================================================================================================*/
﻿using System;
using FG = Infosys.Solutions.Ainauto.VideoAnalytics.BusinessComponent.FrameGrabber;

namespace Infosys.Solutions.Ainauto.VideoAnalytics.FrameGrabber
{
    class Program
    {
        static void Main(string[] args)
        {

            FG.FrameGrabberProcess(true);
        }
    }
}
