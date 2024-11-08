/*=============================================================================================================== *
 * Copyright 2024 Infosys Ltd.                                                                                    *
 * Use of this source code is governed by Apache License Version 2.0 that can be found in the LICENSE file or at  *
 * http://www.apache.org/licenses/                                                                                *
 * ===============================================================================================================*/

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
