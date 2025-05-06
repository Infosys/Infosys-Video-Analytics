/*=============================================================================================================== *
 * Copyright 2025 Infosys Ltd.                                                                                    *
 * Use of this source code is governed by Apache License Version 2.0 that can be found in the LICENSE file or at  *
 * http://www.apache.org/licenses/                                                                                *
 * ===============================================================================================================*/
﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infosys.Lif
{
    
    public class Bbox
    {
        public int topleftx { get; set; }
        public int toplefty { get; set; }
        public int bottomrightx { get; set; }
        public int bottomrighty { get; set; }
    }



    public class Coordinate
    {
        public double x { get; set; }
        public double y { get; set; }
        public double z { get; set; }
    }



    public class Event
    {
        public string id { get; set; }
        public string type { get; set; }
    }



    public class Location
    {
        public double lat { get; set; }
        public double lon { get; set; }
        public double alt { get; set; }
    }



    public class Object
    {
        public string stream_id { get; set; }
        public string frame_id { get; set; }
        public string tracking_id { get; set; }
        public Person person { get; set; }
        public Bbox bbox { get; set; }
        public Location location { get; set; }
        public Coordinate coordinate { get; set; }
    }



    public class Person
    {
        public string feature_map { get; set; }
        public string feature_shape { get; set; }
    }





    public class Sensor
    {
        public string id { get; set; }
        public string type { get; set; }
    }



}

