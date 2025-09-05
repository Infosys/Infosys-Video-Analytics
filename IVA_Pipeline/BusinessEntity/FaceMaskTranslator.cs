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

using System;
using Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.Common;
using DE = Infosys.Solutions.Ainauto.VideoAnalytics.Resource.Entity.Queue;
using BE = Infosys.Solutions.Ainauto.VideoAnalytics.BusinessEntity.Queue;
using Newtonsoft.Json;
using Infosys.Solutions.Ainauto.VideoAnalytics.Services.MaskDetector.Contracts.Message;
using System.Linq;
using System.ComponentModel;
using System.Collections.Generic;

namespace Infosys.Solutions.Ainauto.VideoAnalytics.BusinessEntity
{
    public class FaceMaskTranslator
    {

        public static BE.FrameProcessorMetaData FaceMaskDEToBE(DE.FrameProcessorMetaData message)
        {
            BE.FrameProcessorMetaData beMessage = new BE.FrameProcessorMetaData();
            try
            {
                if (message != null)
                {
                    beMessage.Tid = message.Tid;
                    beMessage.Did = message.Did;
                    beMessage.Fid = message.Fid;
                    beMessage.Sbu = message.Sbu;
                    beMessage.Mod = message.Mod;
                    beMessage.TE = message.TE;
                    beMessage.FeedId = message.FeedId;
                    beMessage.Fids = message.Fids;
                    beMessage.SequenceNumber = message.SequenceNumber;
                    beMessage.FrameNumber = message.FrameNumber;
                    beMessage.Src = message.Src;
                    beMessage.Stime = message.Stime;
                    beMessage.Etime = message.Etime;
                    
                    beMessage.Ffp = message.Ffp;
                    beMessage.Ltsize=message.Ltsize;
                    beMessage.Lfp = message.Lfp;
                    beMessage.videoFileName = message.videoFileName;
                    beMessage.Pcd=message.Pcd;
                    beMessage.Msk_img = message.Msk_img;
                    beMessage.Rep_img = message.Rep_img;
                    beMessage.Prompt = message.Prompt;
                    beMessage.Hp = message.Hp;
                    if (message.Fs != null)
                    {
                        if (beMessage.Fs == null)
                        {
                            beMessage.Fs = new List<DE.PersonDetails>();
                        }
                        beMessage.Fs.AddRange(message.Fs);
                    }
                }
            }
            catch (Exception ex)
            {
                LogHandler.LogError("Error in FaceMaskTranslator.FaceMaskDEToBE, exception: {0}, inner exception: {1}, stack trace: {2}",
                    LogHandler.Layer.FrameProcessor, ex.Message, ex.InnerException, ex.StackTrace);
            }

            return beMessage;
        }

        public static DE.FrameRendererMetadata FaceMaskRendererBEToDE(string predictionMessage,DE.FrameProcessorMetaData dataMessage) {
            DE.FrameRendererMetadata deMessage=new DE.FrameRendererMetadata();
            var msg=JsonConvert.DeserializeObject<DE.ObjectDetectorAPIResMessage>(predictionMessage);
            if(msg!=null) {
                if(dataMessage.Fs == null)
                {
                    dataMessage.Fs = new List<DE.PersonDetails>();
                }
                dataMessage.Fs.AddRange(msg.Fs);
                deMessage.Fs = new DE.Predictions[dataMessage.Fs.Count];
                for (var i = 0; i < dataMessage.Fs.Count; i++)
                {
                    deMessage.Fs[i] = new();
                    deMessage.Fs[i].TaskType = dataMessage.Fs[i].TaskType;
                    if (dataMessage.Fs[i].Dm != null)
                    {
                        if (dataMessage.Fs[i].Dm.X != null && dataMessage.Fs[i].Dm.Y != null && dataMessage.Fs[i].Dm.W != null && dataMessage.Fs[i].Dm.H != null)
                        {
                            deMessage.Fs[i].Dm = dataMessage.Fs[i].Dm;
                            deMessage.Fs[i].Dm.X = dataMessage.Fs[i].Dm.X;
                            deMessage.Fs[i].Dm.Y = dataMessage.Fs[i].Dm.Y;
                            deMessage.Fs[i].Dm.W = dataMessage.Fs[i].Dm.W;
                            deMessage.Fs[i].Dm.H = dataMessage.Fs[i].Dm.H;
                        }
                    }
                    /* Add Kp value */
                    if(dataMessage.Fs[i].Kp!=null) {
                        deMessage.Fs[i].Kp=dataMessage.Fs[i].Kp;
                    }
                    if(dataMessage.Fs[i].Tpc!=null) {
                        deMessage.Fs[i].Tpc=dataMessage.Fs[i].Tpc;
                    }
                    if(dataMessage.Fs[i].Bpc!=null) {
                        deMessage.Fs[i].Bpc=dataMessage.Fs[i].Bpc;
                    }
                    deMessage.Fs[i].Info=dataMessage.Fs[i].Info;
                    deMessage.Fs[i].Cs=dataMessage.Fs[i].Cs;
                    deMessage.Fs[i].NoObj=dataMessage.Fs[i].Nobj;
                    deMessage.Fs[i].Uid=dataMessage.Fs[i].Uid;
                    deMessage.Fs[i].Lb=dataMessage.Fs[i].Lb;
                }
                deMessage.videoFileName = msg.I_fn;
                deMessage.Status = msg.Rm;
                deMessage.Mtp = msg.Mtp;
                deMessage.Obase_64 = msg.Obase_64;
                deMessage.Img_url = msg.Img_url;
                deMessage.Ad = msg.Ad;
            }
            deMessage.Tid = dataMessage.Tid;
            deMessage.Did = dataMessage.Did;
            deMessage.Fid = dataMessage.Fid;
            deMessage.Fids = dataMessage.Fids;
            deMessage.Fp = dataMessage.Fp;
            deMessage.Mtp = dataMessage.Mtp;
            
            deMessage.Sbu = dataMessage.Sbu;
            deMessage.SequenceNumber = dataMessage.SequenceNumber;
            deMessage.Pts = DateTime.UtcNow.Ticks.ToString();
            deMessage.TE = dataMessage.TE;
            deMessage.Fids = dataMessage.Fids;
            deMessage.FeedId = dataMessage.FeedId;
            deMessage.SequenceNumber = dataMessage.SequenceNumber;
            deMessage.FrameNumber = dataMessage.FrameNumber;
            
            deMessage.Ffp = dataMessage.Ffp;
            deMessage.Ltsize = dataMessage.Ltsize;
            deMessage.Lfp = dataMessage.Lfp;
            deMessage.Prompt = string.IsNullOrEmpty(dataMessage.Prompt) ? null : dataMessage.Prompt;
            deMessage.videoFileName = dataMessage.videoFileName;
            deMessage.Hp = dataMessage.Hp;
            
            return deMessage;
        }

        public static DE.FrameProcessorMetaData FaceMaskProcessorBEToDE(string predictionMessage, DE.FrameProcessorMetaData dataMessage)
        {
            var msg = JsonConvert.DeserializeObject<DE.ObjectDetectorAPIResMessage>(predictionMessage);
            if (msg != null && msg.Fs != null)
            {
                if (dataMessage.Fs == null)
                {
                    dataMessage.Fs = new List<DE.PersonDetails>();
                }
                dataMessage.Fs.AddRange(msg.Fs);
            }
            dataMessage.videoFileName = dataMessage.videoFileName;
            
            dataMessage.Mtp = msg.Mtp;
            
            dataMessage.Prompt = dataMessage.Prompt;
            return dataMessage;
        }

        public static DE.FrameCollectorMetadata FaceMaskCollectorBEToDE(string message, DE.FrameProcessorMetaData message1)
        {

            DE.FrameCollectorMetadata deMessage = new DE.FrameCollectorMetadata();

            var msg = JsonConvert.DeserializeObject<DE.ObjectDetectorAPIResMessage>(message);
            deMessage.Fs = new List<DE.Predictions>();

            if (message != null)
            {
                
                for (var i = 0; i < msg.Fs.Count; i++)
                {
                    deMessage.Fs[i] = new();

                    if (msg.Fs[i].Dm.X != null && msg.Fs[i].Dm.Y != null && msg.Fs[i].Dm.W != null && msg.Fs[i].Dm.H != null)
                    {
                        deMessage.Fs[i].Dm = msg.Fs[i].Dm;
                        
                        deMessage.Fs[i].Dm.X = msg.Fs[i].Dm.X;
                        deMessage.Fs[i].Dm.Y = msg.Fs[i].Dm.Y;
                        deMessage.Fs[i].Dm.W = msg.Fs[i].Dm.W;
                        deMessage.Fs[i].Dm.H = msg.Fs[i].Dm.H;
                    }

                    
                    if (msg.Fs[i].Kp != null)
                    {
                        deMessage.Fs[i].Kp = msg.Fs[i].Kp;
                    }

                    if (msg.Fs[i].Tpc != null)
                    {
                        deMessage.Fs[i].Tpc = msg.Fs[i].Tpc;
                    }
                    if (msg.Fs[i].Bpc != null)
                    {
                        deMessage.Fs[i].Bpc = msg.Fs[i].Bpc;
                    }

                    deMessage.Fs[i].Info = msg.Fs[i].Info;
                    deMessage.Fs[i].Cs = msg.Fs[i].Cs;
                    deMessage.Fs[i].NoObj = msg.Fs[i].Nobj;
                    deMessage.Fs[i].Uid = msg.Fs[i].Uid;
                    deMessage.Fs[i].Lb = msg.Fs[i].Lb;

                }

                deMessage.Tid = message1.Tid;
                deMessage.Did = message1.Did;
                deMessage.Fid = message1.Fid;
                deMessage.Pts = DateTime.Now.Ticks.ToString();
               
            }

            return deMessage;
        }

        public static BE.PersonCountMetaData PersonCountDEToBE(DE.PersonCountMetaData message)
        {
            BE.PersonCountMetaData beMessage = new BE.PersonCountMetaData();

            if (message != null)
            {
                beMessage.Tid = message.Tid;
                beMessage.Did = message.Did;
                beMessage.Fid = message.Fid;
                beMessage.Mod = message.Mod;
                beMessage.TE = message.TE;
                beMessage.FeedId = message.FeedId;
                beMessage.SequenceNumber = message.SequenceNumber;
                beMessage.FrameNumber = message.FrameNumber;                   
                            
      
           
                beMessage.Fids = message.Fids;
              
               
                beMessage.Src = message.Src;
                beMessage.Stime = message.Stime;
                beMessage.Etime = message.Etime;
                
                beMessage.Ffp = message.Ffp;
                beMessage.Ltsize = message.Ltsize;
                beMessage.Lfp = message.Lfp;
                beMessage.videoFileName = message.videoFileName;
                beMessage.Hp = message.Hp;
            }

            return beMessage;
        }

        public static DE.FrameRendererMetadata PersonCountBEToDE(string predictionMessage,DE.PersonCountMetaData dataMessage) {
            DE.FrameRendererMetadata deMessage=new DE.FrameRendererMetadata();
            
            var msg=JsonConvert.DeserializeObject<DE.ObjectDetectorAPIResMessage>(predictionMessage);
            if(msg!=null) {
                deMessage.Fs=new DE.Predictions[msg.Fs.Count];
                for(var i=0;i<msg.Fs.Count;i++) {
                    deMessage.Fs[i]=new();
                    if(msg.Fs[i].Dm.X!=null && msg.Fs[i].Dm.Y!=null && msg.Fs[i].Dm.W!=null && msg.Fs[i].Dm.H!=null) {
                        deMessage.Fs[i].Dm=msg.Fs[i].Dm;
                        
                        deMessage.Fs[i].Dm.X=msg.Fs[i].Dm.X;
                        deMessage.Fs[i].Dm.Y=msg.Fs[i].Dm.Y;
                        deMessage.Fs[i].Dm.W=msg.Fs[i].Dm.W;
                        deMessage.Fs[i].Dm.H=msg.Fs[i].Dm.H;
                    }
                    deMessage.Fs[i].Info=msg.Fs[i].Info;
                    deMessage.Fs[i].Cs=msg.Fs[i].Cs;
                    deMessage.Fs[i].NoObj=msg.Fs[i].Nobj;
                    deMessage.Fs[i].Uid=msg.Fs[i].Uid;
                    deMessage.Fs[i].Lb=msg.Fs[i].Lb;
                    deMessage.Fs[i].TaskType = msg.Fs[i].TaskType;
                }
                deMessage.Tid=msg.Tid;
                deMessage.Did=msg.Did;
                deMessage.Fid=msg.Fid;
                deMessage.Pts=DateTime.UtcNow.Ticks.ToString();
                deMessage.TE=dataMessage.TE;
                deMessage.Fids=dataMessage.Fids;
                deMessage.FeedId=dataMessage.FeedId;
                deMessage.SequenceNumber=dataMessage.SequenceNumber;
                deMessage.FrameNumber=dataMessage.FrameNumber;
                deMessage.Ffp=dataMessage.Ffp;
                deMessage.Ltsize=dataMessage.Ltsize;
                deMessage.Lfp=dataMessage.Lfp;
                deMessage.Ad=msg.Ad;
                deMessage.videoFileName=msg.I_fn;
                deMessage.Status=msg.Rm;
                deMessage.Mtp=msg.Mtp;
                deMessage.Obase_64=msg.Obase_64;
                deMessage.Img_url=msg.Img_url;
                deMessage.Hp = msg.Hp;
            }
            return deMessage;
        }


        public static DE.FrameRendererMetadata PersonCountFailureBEToDE(DE.PersonCountMetaData message)
        {
            DE.FrameRendererMetadata deMessage = new DE.FrameRendererMetadata();
            deMessage.Tid = message.Tid;
            deMessage.Did = message.Did;
            deMessage.Fid = message.Fid;
            deMessage.Pts = DateTime.UtcNow.Ticks.ToString();
            deMessage.Fs = new DE.Predictions[0];
            deMessage.SequenceNumber = message.SequenceNumber;
            deMessage.Status = ApplicationConstants.ProcessingStatus.FailedToPredict;
            deMessage.FrameNumber = message.FrameNumber;
            deMessage.Hp = message.Hp;
            return deMessage;
        }

        public static DE.FrameExplainerModeMetaData FaceMaskExplainerBEToDE(string predictionMessage, DE.FrameExplainerModeMetaData dataMessage)
        {
            DE.FrameExplainerModeMetaData deMessage = new DE.FrameExplainerModeMetaData();
            var msg = JsonConvert.DeserializeObject<ObjectDetectorAPIResMsgExp>(predictionMessage);
            if (msg != null)
            {
                deMessage.Fids = new List<string>();
                
                deMessage.Tid = msg.Tid;
                deMessage.Did = msg.Did;
              
                deMessage.Pts = DateTime.UtcNow.Ticks.ToString();
                deMessage.TE = dataMessage.TE;
                deMessage.Fids = msg.Fid;
                deMessage.FeedId = dataMessage.FeedId;
                deMessage.SequenceNumber = dataMessage.SequenceNumber;
                deMessage.FrameNumber = dataMessage.FrameNumber;
                
                deMessage.Ad = msg.Ad;
                deMessage.videoFileName = msg.I_fn;
                
                deMessage.Obase_64 = msg.Obase_64;
                deMessage.Img_url = msg.Img_url;
                deMessage.Explainer_Metadata = msg.Explainer_Metadata;
                deMessage.ExpToRun = msg.ExplainerToRun;
                deMessage.ModelName = msg.ModelName;
                deMessage.ExpVer = msg.ExpVersion;
                deMessage.Hp = msg.Hp;

            }
            
            return deMessage;
        }
    }
}
