/*=============================================================================================================== *
 * Copyright 2025 Infosys Ltd.                                                                                    *
 * Use of this source code is governed by Apache License Version 2.0 that can be found in the LICENSE file or at  *
 * http://www.apache.org/licenses/                                                                                *
 * ===============================================================================================================*/
using Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.ProcessScheduler;
using Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.ProcessScheduler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Infosys.Solutions.Ainauto.VideoAnalytics.ProcessLoader
{
    static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            //Application.SetHighDpiMode(HighDpiMode.SystemAware);
            //Application.EnableVisualStyles();
            //Application.SetCompatibleTextRenderingDefault(false);
            //Application.Run(new Form1());

            //ServiceBase[] ServicesToRun;
            //ServicesToRun = new ServiceBase[]
            //{
            //    new ProcessScheduleRunner()
            //};
            //ServiceBase.Run(ServicesToRun);

            Tasks objTask = new Tasks();
            objTask.InitialiseComponent(1, 1, 1);
        }
    }
}
