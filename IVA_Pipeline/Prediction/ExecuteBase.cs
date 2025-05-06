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

using Infosys.Solutions.Ainauto.VideoAnalytics.BusinessEntity;
using System;
using System.Diagnostics;
using System.IO;

namespace Infosys.Solutions.Ainauto.VideoAnalytics.AIModels
{
    public abstract class ExecuteBase
    {
        public Process ProcessRunning;


        public virtual bool InitializeModel(string modelPath, string modelLabelPath)
        {
            return true;
        }

        public virtual bool InitializeModel(ModelParameters modeltoInfer)
        {
            return true;
        }

        
        public virtual bool InitializeModel()
        {
            return true;
        }
        public virtual string MakePrediction(Stream stream, string st, string baseUrl, string modelName)
        {
            return "string";
        }

        
        public virtual string MakePrediction(Stream st, string confidenceThreshold, float overlapThreshold, string baseUrl, string modelName)
        {
            return "string";
        }
        public virtual string MakePrediction(Stream st, string confidenceThreshold, float overlapThreshold, string baseUrl, string modelName, string predictionKey)
        {
            return "string";
        }
        public virtual string MakePrediction(Stream st, ModelParameters modelParameters)
        {
            return "string";
        }
        public virtual string MakePrediction(string st, string baseUrl, string modelName)
        {
            return "string";
        }

        public virtual string MakePrediction(Stream st, string confidenceThreshold, float overlapThreshold, string baseUrl, string modelName, string authenticationUrl, string host, double tokenCacheExpirationTime)
        {
            return "string";
        }
        public virtual string MakePrediction(Stream st, string confidenceThreshold, float overlapThreshold, string baseUrl, string modelName, string authenticationUrl, double tokenCacheExpirationTime)
        {
            return "string";
        }

        public virtual string MakePrediction(Stream stream, string st, string confidenceThreshold, float overlapThreshold, string baseUrl, string modelName, string authenticationUrl, string host, double tokenCacheExpirationTime)
        {
            return "string";
        }
        public virtual string MakePrediction(Stream stream, string st, string confidenceThreshold, float overlapThreshold, string baseUrl, string modelName, string authenticationUrl, double tokenCacheExpirationTime)
        {
            return "string";
        }

        public virtual string MakePrediction(Stream st, string deviceId, string tId, string Fid, DateTime Stime, string Src, DateTime Etime, string Ts, string Ts_ntp, string Msg_ver, string Inf_ver, string Per, string Ad, string confidenceThreshold, string baseUrl, string modelName, float overlapThreshold)
        {
            return "string";
        }

        
        public void Stop()
        {
            if (ProcessRunning != null && !ProcessRunning.HasExited)
            {
                ProcessRunning.Kill();
                ProcessRunning.WaitForExit();
                ProcessRunning = null;
            }
        }

    }
}
