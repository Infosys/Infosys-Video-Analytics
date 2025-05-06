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

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using Common;
using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;
using Microsoft.Practices.EnterpriseLibrary.ExceptionHandling;
using Microsoft.Practices.EnterpriseLibrary.ExceptionHandling.Configuration;

namespace Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.Common
{
    [ConfigurationElementType(typeof(CustomHandlerData))]
    public class HTTPErrorExceptionHandler : IExceptionHandler
    {
        public HTTPErrorExceptionHandler(NameValueCollection ignore)
        {
        }


        public Exception HandleException(Exception exception, Guid handlingInstanceId)
        {
            System.Exception sr = new Exception();

            int statusCode = 500;
            string message = string.Empty;

           
            if (exception.Data["StatusCode"] != null)
            {
                if (((int)exception.Data["StatusCode"]) < 1000)
                {
                    statusCode = (int)exception.Data["StatusCode"];
                }

                if (!string.IsNullOrWhiteSpace(exception.Data["StatusDescription"] as string))
                {
                    message = exception.Data["StatusDescription"] as string;
                }
                else
                {
                    message = ErrorMessages.ResourceManager.GetString(
                        Enum.GetName(typeof(Errors.ErrorCodes), statusCode));
                }
            }
            else if (!string.IsNullOrWhiteSpace(exception.Message))
            {
                
                message = exception.Message;
            }
            else
            {
                message = ErrorMessages.Standard_Error;
            }

            List<ServiceFaultError> faults = new List<ServiceFaultError>();


            if (exception.GetType() == typeof(FaceMaskDetectionValidationException))
            {
                CollateFaults(exception, faults);
            }
            else if (exception.GetType() == typeof(SuperbotSecurityError))
            {
                CollateFaults(exception, faults);
               
            }
            else
            {

                ServiceFaultError fault = new ServiceFaultError
                {
                    Message = message,
                    ErrorCode = statusCode
                };
                faults.Add(fault);
            }


             return sr;
        }

        private void CollateFaults(Exception exception, List<ServiceFaultError> faults)
        {
            List<ValidationError> validationErrors = new List<ValidationError>();
            if (exception.Data["ValidationErrors"] != null)
            {
                validationErrors = exception.Data["ValidationErrors"] as List<ValidationError>;

                for (int iCount = 0; iCount < validationErrors.Count; iCount++)
                {
                    ServiceFaultError fault = new ServiceFaultError
                    {
                        Message = validationErrors[iCount].Description,
                        ErrorCode = Convert.ToInt32(validationErrors[iCount].Code)
                    };
                    faults.Add(fault);
                }
            }
        }
    }
    public class ServiceFaultError
    {
        public string Message { get; set; }
        public int ErrorCode { get; set; }

    }
    public class ValidationError
    {
        public string Code { get; set; }
        public string Description { get; set; }
        public string Key { get; set; }
    }
    public class SuperbotSecurityError
    {
        public string Code { get; set; }
        public string Description { get; set; }
        public string Key { get; set; }
    }

 
    public class CriticalError
    {
        public string Code { get; set; }
        public string Description { get; set; }
        public string Key { get; set; }
    }
}

