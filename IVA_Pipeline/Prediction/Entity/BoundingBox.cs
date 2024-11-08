/*=============================================================================================================== *
 * Copyright 2024 Infosys Ltd.                                                                                    *
 * Use of this source code is governed by Apache License Version 2.0 that can be found in the LICENSE file or at  *
 * http://www.apache.org/licenses/                                                                                *
 * ===============================================================================================================*/


using Infosys.Solutions.Ainauto.VideoAnalytics.Services.MaskDetector.Contracts.Message;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.Serialization;

namespace Infosys.Solutions.Ainauto.VideoAnalytics.AIModels
{
    public class BoundingBoxDimensions
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float H { get; set; }
        public float W { get; set; } 
    }

    public class BoundingBox
    {
        public BoundingBoxDimensions Dm { get; set; } 
        public string Lb { get; set; } 
        public float Cs { get; set; }  

        private static readonly Color[] classColors = new Color[]
        {
            Color.Green, Color.Red 
        };

        public static Color GetColor(int index) => index < classColors.Length ? classColors[index] : classColors[index % classColors.Length];
        public string Uid { get; set; }

        public string Nobj { get; set; }

        public string Info { get; set; }

        public Dictionary<int, List<float>> Kp { get; set; }
    }

    public class ObjectDetectorAIResMsg
    {
        
   
        
        public int Rc { get; set; }
 
        public string Rm { get; set; }

        /**/
        #region New changes for IVA request/response structure 

 
        public string Did { get; set; } 
 
        public string Fid { get; set; }
 
        public string Tid { get; set; } 

 
        public string Ts { get; set; }

 
        public string Ts_ntp { get; set; }

 
        public string Msg_ver { get; set; }

 
        public string Inf_ver { get; set; }

  
        public string Ad { get; set; }


 
        public List<BoundingBox> Fs { get; set; }
 
        public List<Mtp> Mtp { get; set; }

 
        public string Ffp { get; set; } 
 
        public string Ltsize { get; set; }
     
        public string Lfp { get; set; }
        #endregion


    }
  

}
