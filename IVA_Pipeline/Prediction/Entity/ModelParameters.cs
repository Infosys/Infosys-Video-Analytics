/*=============================================================================================================== *
 * Copyright 2025 Infosys Ltd.                                                                                    *
 * Use of this source code is governed by Apache License Version 2.0 that can be found in the LICENSE file or at  *
 * http://www.apache.org/licenses/                                                                                *
 * ===============================================================================================================*/
﻿using Infosys.Solutions.Ainauto.VideoAnalytics.Services.MaskDetector.Contracts.Message;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.Json;


namespace Infosys.Solutions.Ainauto.VideoAnalytics.AIModels
{
    [Serializable]
    public class ModelParameters : ICloneable
    {   
        public string tId { get; set; }
        public string deviceId { get; set; }
        public string keyPrefix { get; set; }

        public string ModelName { get; set; }
        public string TaskType { get; set; }
        public string MethodName { get; set; }
        public float ConfidenceThreshold { get; set; }
        public float OverlapThreshold { get; set; }

        public double TokenCacheExpirationTime { get; set; }

        public string ModelType { get; set; }

        public string PredictionKey { get; set; }

        public string AuthenticationUrl { get; set; }
        public string Host { get; set; }
        public string BaseUrl { get; set; }
        public string ModelPath { get; set; }
        public string ModelLabelPath { get; set; }
        public string CanonicalPath { get; set; }
        public string ImagePixelExtractionOrder { get; set; }
        public string GPUDeviceId { get; set; }
        public string CPUFallbackValue { get; set; }
        public string Fid { get; set; } 
        public string Etime { get; set; }
        public string Src { get; set; }
        public string Stime { get; set; }
        public string Ts { get; set; }
        public string Ts_ntp { get; set; }
        public string Msg_ver { get; set; }
        public string Inf_ver { get; set; }
        public string Per { get; set; }
        public string Ad { get; set; }
        public byte[] Pcd {get;set;}
        public string Ffp { get; set; } 
        public string Ltsize { get; set; } 
        public string Lfp { get; set; }

        public long FrameNumber { get; set; }

        public string videoFileName { get; set; }

        public string AWSEndpointName { get; set; }
        public string AWSRegionName { get; set; }
        public string AWSAccessKey { get; set; }
        public string AWSSecretKey { get; set; }
        public string AWSSessionToken { get; set; }
        public string Prompt { get; set; }
        public List<string> Msk_img { get; set; }
        public List<string> Rep_img { get; set; }
        public List<PersonDetails> Fs { get; set; }
        public string ExplainerURL { get; set; }
        public string Hp { get; set; }
      

        #region ICloneable Members
        public object Clone() {
            using(MemoryStream stream=new MemoryStream()) {
                if(this.GetType().IsSerializable) {           
                    /* //BinaryFormatter is obsolete in .net 8
                    BinaryFormatter formatter=new BinaryFormatter();
                    formatter.Serialize(stream,this);
                    stream.Position=0;
                    return formatter.Deserialize(stream);
                    */

                    // Serialize to JSON and write to the memory stream
                    JsonSerializer.Serialize(stream, this, this.GetType());

                    // Reset stream position to read from the beginning
                    stream.Position = 0;

                    // Deserialize the object from the memory stream
                    return JsonSerializer.Deserialize(stream, this.GetType());
                }
                return null;
            }
        }

        #endregion
    }

  }
