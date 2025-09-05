/*=============================================================================================================== *
 * Copyright 2025 Infosys Ltd.                                                                                    *
 * Use of this source code is governed by Apache License Version 2.0 that can be found in the LICENSE file or at  *
 * http://www.apache.org/licenses/                                                                                *
 * ===============================================================================================================*/
﻿

using System;
using System.Collections.Generic;
using Microsoft.Practices.EnterpriseLibrary.ExceptionHandling;

namespace Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.Common
{
    public class ExceptionHandler
    {
       
        public static bool HandleException(Exception exception, string exceptionHandlingPolicy, out Exception exceptionToThrow)
        {
            return ExceptionPolicy.HandleException(exception, exceptionHandlingPolicy, out exceptionToThrow);
        }

        
    }
}
