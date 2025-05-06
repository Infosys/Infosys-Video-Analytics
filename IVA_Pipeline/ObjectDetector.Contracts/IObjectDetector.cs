/*=============================================================================================================== *
 * Copyright 2025 Infosys Ltd.                                                                                    *
 * Use of this source code is governed by Apache License Version 2.0 that can be found in the LICENSE file or at  *
 * http://www.apache.org/licenses/                                                                                *
 * ===============================================================================================================*/
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using Infosys.Solutions.Ainauto.VideoAnalytics.Services.MaskDetector.Contracts.Data;
using Infosys.Solutions.Ainauto.VideoAnalytics.Services.MaskDetector.Contracts.Message;

namespace Infosys.Solutions.Ainauto.VideoAnalytics.Services.Contracts
{
    
    public interface IObjectDetector
    {
        
        GetDeviceConfigurationResMsg GetDeviceConfiguration(int tenantId, string deviceId);

        
        Attribute_Details_ResMsg GetDeviceAttributes(int tenantId, string deviceId);

        
        InsertFeedDetailsResMsg InsertFeedDetails(InsertFeedDetailsReqMsg value);

       
        UpdateFeedDetailsResMsg UpdateFeedDetails(UpdateFeedDetailsReqMsg value);


        
        UpdateResourceAttributeResMsg UpdateResourceAttribute(UpdateResourceAttributeReqMsg value);

       
        FeedMasterResMsg GetInCompletedFramGrabberDetails(int tenantId, string deviceId);


        
        Boolean GetClientStatus(string deviceId, string tenantId);

    }

}
