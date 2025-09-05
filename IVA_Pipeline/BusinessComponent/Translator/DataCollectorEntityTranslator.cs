/*=============================================================================================================== *
 * Copyright 2025 Infosys Ltd.                                                                                    *
 * Use of this source code is governed by Apache License Version 2.0 that can be found in the LICENSE file or at  *
 * http://www.apache.org/licenses/                                                                                *
 * ===============================================================================================================*/
﻿
using Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.Common;
using System;
using QE = Infosys.Solutions.Ainauto.VideoAnalytics.Resource.Entity.Queue;
using BE = Infosys.Solutions.Ainauto.VideoAnalytics.BusinessEntity;
using Infosys.Solutions.Ainauto.VideoAnalytics.Services.MaskDetector.Contracts.Message;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Infosys.Solutions.Ainauto.VideoAnalytics.BusinessComponent.Translator
{
    public class DataCollectorEntityTranslator
    {
        public BE.Queue.FrameCollectorMetadata DataCollectorTranslator(QE.FrameCollectorMetadata message) {
            BE.Queue.FrameCollectorMetadata frameProcessor=new BE.Queue.FrameCollectorMetadata();
            try {
                if(message!=null) {
                    frameProcessor.Tid=message.Tid;
                    frameProcessor.Did=message.Did;
                    frameProcessor.Fid=message.Fid;
                    frameProcessor.Pts=message.Pts;
                    frameProcessor.FeedId=message.FeedId;
                    frameProcessor.SequenceNumber=message.SequenceNumber;
                    frameProcessor.Status=message.Status;
                    frameProcessor.FrameNumber=message.FrameNumber;
                    frameProcessor.FileName=message.videoFileName;
                    frameProcessor.Obase_64=message.Obase_64;
                    frameProcessor.Img_url=message.Img_url;
                    frameProcessor.Prompt = message.Prompt;
                    frameProcessor.Mtp = message.Mtp;
                    frameProcessor.Hp = message.Hp;
                    if (message.Fs!=null) {
                        int length=message.Fs.Count;
                        BE.Queue.Predictions[] BEPredArr=new BE.Queue.Predictions[length];
                        int i=0;
                        foreach(QE.Predictions DEPred in message.Fs) {
                            BE.Queue.Predictions BEPred=new BE.Queue.Predictions();
                            BE.Queue.BoundingBox boundingBox=new BE.Queue.BoundingBox();
                            BEPred.Info = DEPred.Info;
                            BEPred.Cs=DEPred.Cs;
                            BEPred.Lb=DEPred.Lb;
                            BEPred.TaskType = DEPred.TaskType;
                            if(DEPred.Dm!=null) {
                                boundingBox.X=DEPred.Dm.X;
                                boundingBox.Y=DEPred.Dm.Y;
                                boundingBox.W=DEPred.Dm.W;
                                boundingBox.H=DEPred.Dm.H;
                                
                                BEPred.Dm=boundingBox;
                            }
                            else {
                                BEPred.Dm=null;
                            }
                            if(DEPred.Kp!=null) {
                                BEPred.Kp=DEPred.Kp;
                            }
                            if(DEPred.Tpc!=null) {
                                BEPred.Tpc=DEPred.Tpc;
                            }
                            if(DEPred.Bpc!=null) {
                                BEPred.Bpc=DEPred.Bpc;
                            }
                            BEPredArr[i]=BEPred;
                            i++;
                        }
                        frameProcessor.Fs=BEPredArr;
                    }
                }
                return frameProcessor;
            }
            catch(Exception ex) {
                LogHandler.LogError(String.Format(ErrorMessages.Exception_Failed,"DataCollectorEntityTranslator","DataCollectorTranslator"),LogHandler.Layer.Business,null);
                throw ex;
            }
        }

        public   BE.Queue.FrameExplainerModeMetaData FaceMaskExplainerBEToDE(QE.FrameCollectorMetadata dataMessage)
        {
            BE.Queue.FrameExplainerModeMetaData deMessage = new BE.Queue.FrameExplainerModeMetaData();

            if (dataMessage != null)
            {
                deMessage.Fids = new List<string>();
                
                deMessage.Tid = dataMessage.Tid;
                deMessage.Did = dataMessage.Did;
              
                deMessage.Pts = DateTime.UtcNow.Ticks.ToString();
                deMessage.TE = dataMessage.TE;
                deMessage.Fids = dataMessage.Fids;
                deMessage.FeedId = dataMessage.FeedId;
                deMessage.SequenceNumber = dataMessage.SequenceNumber;
                deMessage.FrameNumber = dataMessage.FrameNumber;
                deMessage.Mtp = dataMessage.Mtp;
                deMessage.Ad = dataMessage.Ad;
                deMessage.videoFileName = dataMessage.videoFileName;
 
                deMessage.Obase_64 = dataMessage.Obase_64;
                deMessage.Img_url = dataMessage.Img_url;
                deMessage.Explainer_Metadata = dataMessage.Explainer_Metadata;
                deMessage.ExpToRun = dataMessage.ExpToRun;
                deMessage.ModelName = dataMessage.ModelName;
                deMessage.ExpVer = dataMessage.ExpVer;
                deMessage.I_Fn = dataMessage.videoFileName;

            }
            
            return deMessage;
        }
    }
}
