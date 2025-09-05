/*=============================================================================================================== *
 * Copyright 2025 Infosys Ltd.                                                                                    *
 * Use of this source code is governed by Apache License Version 2.0 that can be found in the LICENSE file or at  *
 * http://www.apache.org/licenses/                                                                                *
 * ===============================================================================================================*/
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Threading.Tasks;

namespace MemoryDocAdapter
{
     class MemoryCacheItem
    {
        public MemoryCache Cache { get; set; }
        public CacheItemPolicy CachePolicy { get; set; }

        public int GCTriggerExpireCount { get; set; }
    }
}
