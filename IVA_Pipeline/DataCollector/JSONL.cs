/*=============================================================================================================== *
 * Copyright 2025 Infosys Ltd.                                                                                    *
 * Use of this source code is governed by Apache License Version 2.0 that can be found in the LICENSE file or at  *
 * http://www.apache.org/licenses/                                                                                *
 * ===============================================================================================================*/
﻿
using Infosys.Solutions.Ainauto.VideoAnalytics.BusinessComponent;
using Infosys.Solutions.Ainauto.VideoAnalytics.BusinessEntity;
using Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.Common;
using Infosys.Solutions.Ainauto.VideoAnalytics.Resource.Entity.Queue;
using Newtonsoft.Json;
using System;
using System.Diagnostics.Metrics;
using System.Runtime.Caching;
namespace Infosys.Solutions.Ainauto.VideoAnalytics.DataCollector
{
    public class JSONL : IDataCollector
    {
        public string _logFilePath;
        private static readonly MemoryCache _cache = MemoryCache.Default;
        //private static int counter = 0;
        public bool LogCollector(FrameCollectorMetadata metadata, string filepath)
        {
            try
            {

                _logFilePath = filepath ?? throw new ArgumentNullException(nameof(filepath));
                string json = JsonConvert.SerializeObject(metadata);

                // Creating a cache policy with 60 minutes expiration
                var cacheItemPolicy = new CacheItemPolicy()
                {
                    AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(60)
                };

                // Using a unique key for each metadata (FrameId + timestamp)
                string cacheKey = $"metadata_{metadata.Fid}_{DateTime.UtcNow.Ticks}";
                _cache.Add(cacheKey, json, cacheItemPolicy);

                // Printing the number of items in the cache
                //LogHandler.LogInfo($"Current cache count: {_cache.GetCount()}", LogHandler.Layer.FrameRenderer, null);
                //++counter;
                //Console.WriteLine($"Total Count: {counter.ToString()}");

                if (metadata.Lfp == "1")
                {
                    using (StreamWriter sw = new StreamWriter(_logFilePath, append: true))
                    {
                        foreach (var item in _cache)
                        {
                            sw.WriteLine(item.Value);
                            //sw.WriteLine(json);
                        }
                    }
                   // _cache.Trim(100); // Clear the cache after writing
                }

                // using (StreamWriter sw = new StreamWriter(_logFilePath, append: true))
                // {
                //     sw.WriteLine(json);
                // }

            }
            catch (Exception ex)
            {
                LogHandler.LogError("Error in LogCollector.JSONL logs, exception: {0}, inner exception: {1}, stack trace: {2}",
                LogHandler.Layer.FrameRenderer, ex.Message, ex.InnerException, ex.StackTrace);
            }

            return true;
        }
    }
}
