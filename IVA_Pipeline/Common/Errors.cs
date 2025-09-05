/*=============================================================================================================== *
 * Copyright 2025 Infosys Ltd.                                                                                    *
 * Use of this source code is governed by Apache License Version 2.0 that can be found in the LICENSE file or at  *
 * http://www.apache.org/licenses/                                                                                *
 * ===============================================================================================================*/
﻿

namespace Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.Common
{
    public class Errors
    {
        
        public enum ErrorCodes
        {
            Critical = 5000,
            Warning = 3000,
            Standard_Error = 1000,
            InvalidCharacter_Validation = 1040,
            Platform_Data_NotFound = 1041,
            RemediatioPlan_NotFound = 1042,
            SEE_Response_Null = 1043,
            Value_NullOrEmpty_Error = 1044,
            Method_Returned_Null = 1045,
            Empty_Frame = 1046,
            UnableToSendQueueMessage = 1047,
            BlobStorage_Failure = 1048,
            FrameGrabberInvalidConfig = 1049
        }
    }
}
