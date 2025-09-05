/*=============================================================================================================== *
 * Copyright 2025 Infosys Ltd.                                                                                    *
 * Use of this source code is governed by Apache License Version 2.0 that can be found in the LICENSE file or at  *
 * http://www.apache.org/licenses/                                                                                *
 * ===============================================================================================================*/
﻿using Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.Common;
using System;

namespace Infosys.Solutions.Ainauto.VideoAnalytics.BusinessComponent
{
    public class PartitionKeyUtility
    {
        public int generatePartionKey(string tenantId,DateTime FrameGrabTime)
        {
            int partitionKey = 0;
            try
            {
                string month = FrameGrabTime.Month.ToString();
                string year = FrameGrabTime.Year.ToString();

                string stringPartitionKey = tenantId + month + year;



                 partitionKey = int.Parse(stringPartitionKey);

            }
            catch(Exception ex)
            {
                LogHandler.LogError(String.Format(ErrorMessages.Exception_Failed, "BusinessComponent", "PartitionKeyUtility"), LogHandler.Layer.Business, null);

            }


            return partitionKey;

        }
    }
}
