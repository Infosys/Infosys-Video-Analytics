/*=============================================================================================================== *
 * Copyright 2025 Infosys Ltd.                                                                                    *
 * Use of this source code is governed by Apache License Version 2.0 that can be found in the LICENSE file or at  *
 * http://www.apache.org/licenses/                                                                                *
 * ===============================================================================================================*/
﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FrameDetailsServices.Models;
using Infosys.Solutions.Ainauto.VideoAnalytics.BusinessComponent;
using Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.Common;
using CM=Infosys.Solutions.Ainauto.VideoAnalytics.Services.MaskDetector.Contracts.Message;

namespace FrameDetailsServices.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class FrameDetailsController : ControllerBase
    {
        [HttpPost]
        public InsertFeedDetailsResMsg InsertFeedDetails(InsertFeedDetailsReqMsg value)
        {
            InsertFeedDetailsResMsg retObj = new InsertFeedDetailsResMsg();
            ObjectDetectorServiceBuilder serviceBuilder = new ObjectDetectorServiceBuilder();
            try
            {
                var objBE = Translator.EntityTranslatorSEtoBE.FeedProcessorMasterSEtoBE(value.FeedMaster);
                retObj.MasterId = serviceBuilder.InsertFeedProcessorMaster(objBE).FeedProcessorMasterId;
                //retObj = Translator.EntityTranslatorBEtoSE.InsertFeedProcessorMasterBEtoSE(dataAccess.InsertFeedProcessorMaster(objBE));
                return retObj;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        [HttpPut]
        public UpdateFeedDetailsResMsg UpdateFeedDetails(UpdateFeedDetailsReqMsg value)
        {
            UpdateFeedDetailsResMsg retObj = new UpdateFeedDetailsResMsg();
            ObjectDetectorServiceBuilder serviceBuilder = new ObjectDetectorServiceBuilder();
            try
            {
                var objBE = Translator.EntityTranslatorSEtoBE.FeedProcessorMasterSEtoBE(value.FeedMaster);
                retObj.Status = serviceBuilder.UpdateFeedProcessorMaster(objBE) != null;
                return retObj;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        [HttpPut]
        public UpdateFeedDetailsResMsg UpdateAllFeedDetails(CM.FeedProcessorMasterMsg value)
        {
            UpdateFeedDetailsResMsg retObj = new UpdateFeedDetailsResMsg();
            ObjectDetectorServiceBuilder serviceBuilder = new ObjectDetectorServiceBuilder();
            try
            {
                var objBE = Translator.EntityTranslatorSEtoBE.FeedProcessorMasterDetailsSEtoBE(value.FeedProcessorMasterDetail);
                retObj.Status = serviceBuilder.UpdateFeedProcessorMaster(objBE) != null;
                return retObj;
            }
            catch (Exception ex)
            {
                return null;
            }
        }


        [HttpGet]
        public FeedMasterResMsg GetInCompleteFramGrabberDetails(int tid, string did)
        {
            ObjectDetectorServiceBuilder serviceBuilder = new ObjectDetectorServiceBuilder();
            try
            {
                FeedMasterResMsg feedMasterResMsg = Translator.EntityTranslatorBEtoSE.FeedMasterDetailsBEtoSE(serviceBuilder.GetInCompletedFramGrabberDetails(Convert.ToInt32(tid), did));
                return feedMasterResMsg;
            }
            catch (Exception ex)
            {
                LogHandler.LogError("Exception occured in GetInCompletedFramGrabberDetails Method of ObjectDetector service. Exception Message :{0}", LogHandler.Layer.WebServiceHost, ex.Message);
                return null;
            }
        }

    }
}
