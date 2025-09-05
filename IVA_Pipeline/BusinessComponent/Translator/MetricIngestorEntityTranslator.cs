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

using Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.Common;

using QE = Infosys.Solutions.Ainauto.VideoAnalytics.Resource.Entity.Queue;
using BE = Infosys.Solutions.Ainauto.VideoAnalytics.BusinessEntity;

namespace Infosys.Solutions.Ainauto.VideoAnalytics.BusinessComponent.Translator
{
   public class MetricIngestorEntityTranslator
    {
        public BE.Queue.MetricIngestorMetadata MetricEntityTranslator(QE.MetricIngestorMetadata message)
        {
            BE.Queue.MetricIngestorMetadata metricIngestorMetadata = new BE.Queue.MetricIngestorMetadata();

            try
            {

                if (message != null)
                {

                    metricIngestorMetadata.Tid = message.Tid;
                    metricIngestorMetadata.Did = message.Did;
                    metricIngestorMetadata.Fid = message.Fid;
                    metricIngestorMetadata.Pts = message.Pts;
                    metricIngestorMetadata.SequenceNumber = message.SequenceNumber;

                    if (message.Fs != null)
                    {
                        int length = message.Fs.Count;
                        BE.Queue.Predictions[] BEPredArr = new BE.Queue.Predictions[length];
                        int i = 0;
                        foreach (QE.Predictions DEPred in message.Fs)
                        {
                            BE.Queue.Predictions BEPred = new BE.Queue.Predictions();
                            BE.Queue.BoundingBox boundingBox = new BE.Queue.BoundingBox();
                            BEPred.Cs = DEPred.Cs;
                            BEPred.Lb = DEPred.Lb;
                            boundingBox.X = DEPred.Dm.X;
                            boundingBox.Y = DEPred.Dm.Y;
                            boundingBox.W = DEPred.Dm.W;
                            boundingBox.H = DEPred.Dm.H;
                            BEPred.Dm = boundingBox;
                            BEPredArr[i] = BEPred;
                            i++;
                        }
                        metricIngestorMetadata.Fs = BEPredArr;
                    }


                }

                return metricIngestorMetadata;
            }
            catch (Exception ex)
            {
                LogHandler.LogError(String.Format(ErrorMessages.Exception_Failed, "DataCollectorEntityTranslator", "DataCollectorTranslator"), LogHandler.Layer.Business, null);

                throw ex;
            }
        }
    }
}
