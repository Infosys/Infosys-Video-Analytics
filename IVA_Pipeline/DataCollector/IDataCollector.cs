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
using Infosys.Solutions.Ainauto.VideoAnalytics.BusinessEntity;
using Infosys.Solutions.Ainauto.VideoAnalytics.BusinessEntity.Queue;
using QueueEntity = Infosys.Solutions.Ainauto.VideoAnalytics.Resource.Entity.Queue;
namespace Infosys.Solutions.Ainauto.VideoAnalytics.DataCollector
{
    public interface IDataCollector
    {
        public bool LogCollector(QueueEntity.FrameCollectorMetadata metadata,string filepath);
    }
}
