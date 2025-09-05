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

using Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.ProcessScheduler;
using Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.ProcessScheduler;

namespace Infosys.Solutions.Ainauto.VideoAnalytics.ProcessLoader
{
    public partial class ProcessScheduleRunner : ServiceBase
    {
        int _robotId;
        int _runInstanceId;
        string _LogMessage = string.Empty;

        public ProcessScheduleRunner()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                _LogMessage = "Windows Service Started";
                LogHandler.LogInfo(_LogMessage, LogHandler.Layer.Business, null);
                Tasks objTask = new Tasks();
                objTask.InitialiseComponent(_robotId, _runInstanceId, 1);
            }
            catch (Exception exProcess)
            {
                LogHandler.LogError("Error in RobotAllocationProcessClientSide", LogHandler.Layer.Business, null);
                Exception ex = new Exception();
                bool rethrow = ExceptionHandler.HandleException(exProcess, ApplicationConstants.SERVICE_EXCEPTIONHANDLING_POLICY, out ex);
            }
        }

        protected override void OnStop()
        {
        }
    }
}
