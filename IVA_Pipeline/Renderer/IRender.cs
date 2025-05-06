/*=============================================================================================================== *
 * Copyright 2025 Infosys Ltd.                                                                                    *
 * Use of this source code is governed by Apache License Version 2.0 that can be found in the LICENSE file or at  *
 * http://www.apache.org/licenses/                                                                                *
 * ===============================================================================================================*/
﻿using Emgu.CV;
using Emgu.CV.Structure;
using Infosys.Solutions.Ainauto.VideoAnalytics.BusinessEntity;
using QueueEntity = Infosys.Solutions.Ainauto.VideoAnalytics.Resource.Entity.Queue;

namespace Infosys.Solutions.Ainauto.VideoAnalytics.Renderer
{
    public interface IRender
    {
        public Image<Bgr, byte> RenderFrame(List<QueueEntity.Predictions> objectList, Image<Bgr, byte> image, int frameWidth,
        int frameHeight, string modelName, string info, DeviceDetails deviceDetails, string Ad, FrameDetails frameDetails);
    }
}
