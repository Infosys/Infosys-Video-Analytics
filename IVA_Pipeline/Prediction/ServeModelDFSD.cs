/*=============================================================================================================== *
 * Copyright 2024 Infosys Ltd.                                                                                    *
 * Use of this source code is governed by Apache License Version 2.0 that can be found in the LICENSE file or at  *
 * http://www.apache.org/licenses/                                                                                *
 * ===============================================================================================================*/


using Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.Common;
using System;
using System.IO;
using SE = Infosys.Solutions.Ainauto.VideoAnalytics.Services.MaskDetector.Contracts;
using Newtonsoft.Json;
using Infosys.Solutions.Ainauto.VideoAnalytics.BusinessComponent;
using System.Collections.Generic;

namespace Infosys.Solutions.Ainauto.VideoAnalytics.AIModels
{
    public class ServeModelDFSD : ExecuteBase
    {
        public override bool InitializeModel()
        {
            return true;
        }

        public override string MakePrediction(Stream st, ModelParameters modelParameters)
        {
            string sstime = DateTime.UtcNow.ToString("yyy-MM-dd,HH:mm:ss.fff tt");

            List<SE.Message.Mtp> MtpData = new List<SE.Message.Mtp>()
                {
                    new SE.Message.Mtp(){ Etime = modelParameters.Etime, Src = modelParameters.Src, Stime=modelParameters.Stime},
                    new SE.Message.Mtp(){ Etime = "", Src = "Frame Processor", Stime=sstime},
                };
#if DEBUG
            using (LogHandler.TraceOperations("ServeModelDFSD:MakePrediction", LogHandler.Layer.MaskPrediction, Guid.NewGuid(), null))
            {
                LogHandler.LogUsage(String.Format("ServeModelDFSD MakePrediction is getting executed at : {0}", DateTime.UtcNow.ToLongTimeString()), null);
#endif
                string metadata = "";
                st.Position = 0;
                string base64_image = "";

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    st.CopyTo(memoryStream);
                    base64_image = Convert.ToBase64String(memoryStream.ToArray());
                    memoryStream.Dispose();
                }

                SE.Message.ObjectDetectorAPIReqMsg reqMsg = new SE.Message.ObjectDetectorAPIReqMsg()
                {
                    Did = modelParameters.deviceId,
                    Fid = modelParameters.Fid,
                    Mtp = MtpData,
                    Tid = modelParameters.tId,
                    Ts = modelParameters.Ts,
                    Ts_ntp = modelParameters.Ts_ntp,
                    Msg_ver = modelParameters.Msg_ver,
                    Inf_ver = modelParameters.Inf_ver,
                    Model = modelParameters.ModelName,
                    Per = null,
                    Ad = " ",
                    Base_64 = base64_image,
                    C_threshold = modelParameters.ConfidenceThreshold, 

                         Ffp = modelParameters.Ffp,
                    Ltsize = modelParameters.Ltsize,
                    Lfp = modelParameters.Lfp

                };

                
#if DEBUG
                LogHandler.LogUsage(String.Format("ServeModelDFSD MakePrediction finished execution at : {0}", DateTime.UtcNow.ToLongTimeString()), null);
#endif
                return metadata;
#if DEBUG
            }
#endif
        }

    }
}
