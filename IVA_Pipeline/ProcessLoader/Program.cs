/*=============================================================================================================== *
 * Copyright 2025 Infosys Ltd.                                                                                    *
 * Use of this source code is governed by Apache License Version 2.0 that can be found in the LICENSE file or at  *
 * http://www.apache.org/licenses/                                                                                *
 * ===============================================================================================================*/
﻿using Elastic.CommonSchema.NLog;
using Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.ProcessScheduler;
using NLog.Targets;
using NLog;
using System;
using NLog.Config;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace ProcessLoader
{
    class Program
    {
        static void Main(string[] args)
        {  

            Console.WriteLine("Initiating Process Loader");
            Dictionary<string,string> dict=null;
            if(args.Length>0) {
                dict=JsonConvert.DeserializeObject<Dictionary<string,string>>(args[0]);
                if(dict!=null) {
                    foreach(var pair in dict) {
                        Console.WriteLine(pair.Key+": "+pair.Value);
                    }
                }
            }
            Tasks objTask = new Tasks();
            objTask.InitialiseComponent(1,1,1,dict);
            Console.WriteLine("Completed Process Loader");
        }
    }
}
