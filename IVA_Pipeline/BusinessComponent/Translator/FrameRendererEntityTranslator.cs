/*=============================================================================================================== *
 * Copyright 2025 Infosys Ltd.                                                                                    *
 * Use of this source code is governed by Apache License Version 2.0 that can be found in the LICENSE file or at  *
 * http://www.apache.org/licenses/                                                                                *
 * ===============================================================================================================*/
﻿using Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BE = Infosys.Solutions.Ainauto.VideoAnalytics.BusinessEntity;
using QE = Infosys.Solutions.Ainauto.VideoAnalytics.Resource.Entity.Queue;
namespace Infosys.Solutions.Ainauto.VideoAnalytics.BusinessComponent.Translator
{
    public class FrameRendererEntityTranslator
    {
        public BE.FrameRendererData FrameRendererTranslator(QE.FrameRendererMetadata message)
        {
            BE.FrameRendererData frameRendererData = new BE.FrameRendererData();
            FaceMaskDetectionValidationException validateException = new FaceMaskDetectionValidationException(ErrorMessages.InvalidCharacter_Validation.ToString());
            List<ValidationError> validateErrs = new List<ValidationError>();
            if (message != null)
            {
                
                frameRendererData.TID = message.Tid;
                frameRendererData.DID = message.Did;
                frameRendererData.FID = message.Fid;
                frameRendererData.SequenceNumber = message.SequenceNumber;
                frameRendererData.Status = message.Status;
                frameRendererData.FrameNumber = message.FrameNumber;
                if (message.Fids != null)
                {
                    frameRendererData.FIDS = message.Fids;
                }
                if (message.Np != null)
                {
                    frameRendererData.NP = message.Np;
                }
                if (message.Pts != null)
                {
                    frameRendererData.PTS = message.Pts;
                }
                
                if (message.Class != null)
                {
                    frameRendererData.Class = message.Class;
                }
                frameRendererData.FeedId = message.FeedId;
                BE.PredictionsData[] BEPredArr = null;

               
                ValidationError validationErr = new ValidationError();
                validationErr.Code = Errors.ErrorCodes.Value_NullOrEmpty_Error.ToString();
                validationErr.Description = string.Format(ErrorMessages.Value_NullOrEmpty_Error, "FrameRendererTranslator");
                validateErrs.Add(validationErr);
                validateException.Data.Add("ValidationErrors", validateErrs);
                int i = 0;
                if (message.Fs != null)
                {
                    int sizeArr = message.Fs.Length;
                    BEPredArr = new BE.PredictionsData[sizeArr];
                    foreach (QE.Predictions DEPred in message.Fs)
                    {
                        
                        if (DEPred != null && DEPred?.Cs != null && (DEPred?.Lb != null || DEPred?.Pid != null) && DEPred?.Dm?.X != null
                        && DEPred?.Dm?.Y != null && DEPred?.Dm?.W != null && DEPred?.Dm?.H != null)
                        {
                            BE.PredictionsData BEPred = new BE.PredictionsData();
                            BEPred.DM = new BE.BoundingBoxData();
                            BEPred.CS = DEPred.Cs;
                            if (DEPred.Lb != null)
                            {
                                BEPred.LB = DEPred.Lb;
                            }
                            else if (DEPred.Pid != null)
                            {
                                BEPred.LB = DEPred.Pid ;
                                 if (DEPred.Np != null)
                                {
                                    BEPred.NP = DEPred.Np;
                                }
                            }
                            BEPred.DM.X = DEPred.Dm.X;
                            BEPred.DM.Y = DEPred.Dm.Y;
                            BEPred.DM.W = DEPred.Dm.W;
                            BEPred.DM.H = DEPred.Dm.H;
                            BEPredArr[i] = BEPred;
                            i++;
                        }
                        else
                        {

                            throw validateException;
                        }
                    }

                }
                else
                {

                    throw validateException;
                }
                frameRendererData.FS = BEPredArr;
            }
            return frameRendererData;
        }
        public BE.FramePreloaderData FramePreLoaderTranslator(QE.FramePreloaderMetadata message)
        {
            BE.FramePreloaderData framePreLoaderData = new BE.FramePreloaderData();
            if (message != null)
            {
                framePreLoaderData.TID = message.Tid;
                framePreLoaderData.DID = message.Did;
                framePreLoaderData.FID = message.Fid;
                framePreLoaderData.TE = message.TE;
                framePreLoaderData.SequenceNumber = message.SequenceNumber;
                framePreLoaderData.FrameNumber = message.FrameNumber;
            }
            return framePreLoaderData;
        }
    }   
}

