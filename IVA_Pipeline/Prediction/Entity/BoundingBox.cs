/*=============================================================================================================== *
 * Copyright 2025 Infosys Ltd.                                                                                    *
 * Use of this source code is governed by Apache License Version 2.0 that can be found in the LICENSE file or at  *
 * http://www.apache.org/licenses/                                                                                *
 * ===============================================================================================================*/
﻿/*
 *© 2019 Infosys Limited, Bangalore, India. All Rights Reserved. Infosys believes the information in this document is accurate as of its publication date; such information is subject to change without notice. Infosys acknowledges the proprietary rights of other companies to the trademarks, product names and such other intellectual property rights mentioned in this document. Except as expressly permitted, neither this document nor any part of it may be reproduced, stored in a retrieval system, or transmitted in any form or by any means, electronic, mechanical, printing, photocopying, recording or otherwise, without the prior permission of Infosys Limited and/or any named intellectual property rights holders under this document.   
 * 
 * © 2019 INFOSYS LIMITED. CONFIDENTIAL AND PROPRIETARY 
 */

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
        public BoundingBoxDimensions Dm { get; set; } // predicted face regions
        public string Lb { get; set; } // predicted class label
        public float Cs { get; set; }  // predicted confidence score

        private static readonly Color[] classColors = new Color[]
        {
            Color.Green, Color.Red  // bounding box color
        };

        public static Color GetColor(int index) => index < classColors.Length ? classColors[index] : classColors[index % classColors.Length];
        public string Uid { get; set; }

        public string Nobj { get; set; }

        public string Info { get; set; }

        public Dictionary<int, List<float>> Kp { get; set; }
        public string TaskType { get; set; }
    }

    public class ObjectDetectorAIResMsg
    {
        
   
        
        public int Rc { get; set; } // response code
 
        public string Rm { get; set; }

        /**/
        #region New changes for IVA request/response structure 

 
        public string Did { get; set; } // device id
 
        public string Fid { get; set; } // frame id
 
        public string Tid { get; set; } // tenant id

 
        public string Ts { get; set; }

 
        public string Ts_ntp { get; set; }

 
        public string Msg_ver { get; set; }

 
        public string Inf_ver { get; set; }

  
        public string Ad { get; set; }

        //[DataMember]
        //public PersonCountAPIResMsg[] Fs { get; set; }

        //[DataMember]
        //public PersonDetails[] Fs { get; set; }

 
        public List<BoundingBox> Fs { get; set; }
 
        public List<Mtp> Mtp { get; set; }

 
        public string Ffp { get; set; } //Added for IVA new request structure
 
        public string Ltsize { get; set; } //Added for IVA new request structure
     
        public string Lfp { get; set; }//Added for IVA new request structure
        #endregion
        public List<List<string>> Prompt { get; set; }
        public string Hp { get; set; }

    }
    


}
