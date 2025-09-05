/*=============================================================================================================== *
 * Copyright 2025 Infosys Ltd.                                                                                    *
 * Use of this source code is governed by Apache License Version 2.0 that can be found in the LICENSE file or at  *
 * http://www.apache.org/licenses/                                                                                *
 * ===============================================================================================================*/
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.ComputerVisionLib.Core.CVExceptions
{
    public class StopRequested : Exception
    {
        public StopRequested():base("Stop requested explicitly.")
        {
        }

        public StopRequested(string message)
            : base(message)
        {
        }

        public StopRequested(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }

    public class InteractiveCheckFailedException : Exception
    {
        public InteractiveCheckFailedException()
            : base("Automation stopped on user's confirmation")
        {
        }

        public InteractiveCheckFailedException(string message)
            : base(message)
        {
    }

        public InteractiveCheckFailedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
