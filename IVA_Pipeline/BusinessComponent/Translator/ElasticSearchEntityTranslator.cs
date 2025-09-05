/*=============================================================================================================== *
 * Copyright 2025 Infosys Ltd.                                                                                    *
 * Use of this source code is governed by Apache License Version 2.0 that can be found in the LICENSE file or at  *
 * http://www.apache.org/licenses/                                                                                *
 * ===============================================================================================================*/
﻿
using Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.Common;
using System;
using QE = Infosys.Solutions.Ainauto.VideoAnalytics.Resource.Entity.Queue;
using DE = Infosys.Solutions.Ainauto.VideoAnalytics.Resource.Entity.Index;
using Nest;
using System.Collections.Generic;
using System.Buffers.Text;

namespace Infosys.Solutions.Ainauto.VideoAnalytics.BusinessComponent.Translator
{
    public class ElasticSearchEntityTranslator
    {
        public DE.FrameElasticSearchMetadata DataCollectorTranslator(QE.FrameElasticSearchMetaData message) 
        {
            DE.FrameElasticSearchMetadata frameElasticSearch = new DE.FrameElasticSearchMetadata();

            try
            {

                if (message != null)
                {

                    frameElasticSearch.Tid = message.Tid;
                    frameElasticSearch.Did = message.Did;
                    frameElasticSearch.Fid = message.Fid;
                    frameElasticSearch.Pts = message.Pts;
                    frameElasticSearch.FeedId = message.FeedId;
                    frameElasticSearch.SequenceNumber = message.SequenceNumber;
                    frameElasticSearch.Status = message.Status;
                    frameElasticSearch.FrameNumber = message.FrameNumber;
                    frameElasticSearch.FileName = message.videoFileName;
                    frameElasticSearch.Mtp = message.Mtp;
                    if (message.Fs != null)
                    {
                        int length = message.Fs.Count;
                        DE.Predictions[] BEPredArr = new DE.Predictions[length];
                        int i = 0;
                        foreach (QE.Predictions DEPred in message.Fs)
                        {
                            DE.Predictions BEPred = new DE.Predictions();
                            DE.BoundingBox boundingBox = new DE.BoundingBox();
                            BEPred.Cs = DEPred.Cs;
                            BEPred.Lb = DEPred.Lb;
                            if (DEPred.Dm != null)
                            {
                                boundingBox.X = DEPred.Dm.X;
                                boundingBox.Y = DEPred.Dm.Y;
                                boundingBox.W = DEPred.Dm.W;
                                boundingBox.H = DEPred.Dm.H;
                                
                                BEPred.Dm = boundingBox;
                            }
                            else
                            {
                                BEPred.Dm = null;
                            }
                            if (DEPred.Kp != null)
                            {
                                BEPred.Kp = DEPred.Kp;
                            }
                            if (DEPred.Tpc != null)
                            {
                                BEPred.Tpc = DEPred.Tpc;
                            }
                            if (DEPred.Bpc != null)
                            {
                                BEPred.Bpc = DEPred.Bpc;
                            }
                            BEPredArr[i] = BEPred;
                            i++;
                        }
                        frameElasticSearch.Fs = BEPredArr;
                    }


                }

                return frameElasticSearch;
            }
            catch (Exception ex)
            {
                LogHandler.LogError(String.Format(ErrorMessages.Exception_Failed, "DataCollectorEntityTranslator", "DataCollectorTranslator"), LogHandler.Layer.Business, null);

                throw ex;
            }
        }

        public DE.FrameElasticSearchMetadata DataCollectorTranslatorRenderer(QE.FrameRendererMetadata message, string Raw_base64_image, string Rendered_base64_image) //DataCollector Part when elasticsearch execute alone
        {
            List<DE.Mtp> MtpData = new List<DE.Mtp>()
                {
                    new DE.Mtp(){ Etime = message.Mtp[0].Etime, Src = message.Mtp[0].Src, Stime=message.Mtp[0].Stime},
                    new DE.Mtp(){ Etime = message.Mtp[1].Etime, Src = message.Mtp[1].Src, Stime=message.Mtp[1].Stime}
                };
            DE.FrameElasticSearchMetadata frameElasticSearch = new DE.FrameElasticSearchMetadata();

            try
            {

                if (message != null)
                {

                    frameElasticSearch.Tid = message.Tid;
                    frameElasticSearch.Did = message.Did;
                    frameElasticSearch.Fid = message.Fid;
                    frameElasticSearch.Pts = message.Pts;
                    frameElasticSearch.FeedId = message.FeedId;
                    frameElasticSearch.SequenceNumber = message.SequenceNumber;
                    frameElasticSearch.Status = message.Status;
                    frameElasticSearch.FrameNumber = message.FrameNumber;
                    frameElasticSearch.FileName = message.videoFileName;
                    frameElasticSearch.Mtp = MtpData;//message.Mtp;
                    frameElasticSearch.Raw_base64_image = Raw_base64_image;
                    frameElasticSearch.Rendered_base64_image = Rendered_base64_image;
                    if (message.Fs != null)
                    {
                        int length = message.Fs.Length;
                        DE.Predictions[] BEPredArr = new DE.Predictions[length];
                        int i = 0;
                        foreach (QE.Predictions DEPred in message.Fs)
                        {
                            DE.Predictions BEPred = new DE.Predictions();
                            DE.BoundingBox boundingBox = new DE.BoundingBox();
                            BEPred.Cs = DEPred.Cs;
                            BEPred.Lb = DEPred.Lb;
                            if (DEPred.Dm != null)
                            {
                                boundingBox.X = DEPred.Dm.X;
                                boundingBox.Y = DEPred.Dm.Y;
                                boundingBox.W = DEPred.Dm.W;
                                boundingBox.H = DEPred.Dm.H;
                                
                                BEPred.Dm = boundingBox;
                            }
                            else
                            {
                                BEPred.Dm = null;
                            }
                            if (DEPred.Kp != null)
                            {
                                BEPred.Kp = DEPred.Kp;
                            }
                            if (DEPred.Tpc != null)
                            {
                                BEPred.Tpc = DEPred.Tpc;
                            }
                            if (DEPred.Bpc != null)
                            {
                                BEPred.Bpc = DEPred.Bpc;
                            }
                            BEPredArr[i] = BEPred;
                            i++;
                        }
                        frameElasticSearch.Fs = BEPredArr;
                    }


                }

                return frameElasticSearch;
            }
            catch (Exception ex)
            {
                LogHandler.LogError(String.Format(ErrorMessages.Exception_Failed, "DataCollectorEntityTranslator", "DataCollectorTranslator"), LogHandler.Layer.Business, null);

                throw ex;
            }
        }
    }
}
