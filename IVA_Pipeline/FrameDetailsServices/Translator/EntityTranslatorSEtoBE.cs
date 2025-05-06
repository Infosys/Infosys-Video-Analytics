/*=============================================================================================================== *
 * Copyright 2025 Infosys Ltd.                                                                                    *
 * Use of this source code is governed by Apache License Version 2.0 that can be found in the LICENSE file or at  *
 * http://www.apache.org/licenses/                                                                                *
 * ===============================================================================================================*/
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BE = Infosys.Solutions.Ainauto.VideoAnalytics.BusinessEntity;
using Newtonsoft.Json;
using FrameDetailsServices.Models;
using CD = Infosys.Solutions.Ainauto.VideoAnalytics.Services.MaskDetector.Contracts.Data;

namespace FrameDetailsServices.Translator
{
    public class EntityTranslatorSEtoBE
    {
        public static BE.FeedProcessorMasterDetails FeedProcessorMasterSEtoBE(FeedMaster inpObj)
        {
            BE.FeedProcessorMasterDetails retObj = new BE.FeedProcessorMasterDetails();

            string jsonString = JsonConvert.SerializeObject(inpObj);
            retObj = JsonConvert.DeserializeObject<BE.FeedProcessorMasterDetails>(jsonString);


            return retObj;
        }

        public static BE.FeedProcessorMasterDetails FeedProcessorMasterDetailsSEtoBE(CD.Feed_Processor_Master_Detail inpObj)
        {
            BE.FeedProcessorMasterDetails retObj = new BE.FeedProcessorMasterDetails();

            string jsonString = JsonConvert.SerializeObject(inpObj);
            retObj = JsonConvert.DeserializeObject<BE.FeedProcessorMasterDetails>(jsonString);


            return retObj;
        }
    }
}
