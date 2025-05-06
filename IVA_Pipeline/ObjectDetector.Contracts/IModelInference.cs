/*=============================================================================================================== *
 * Copyright 2025 Infosys Ltd.                                                                                    *
 * Use of this source code is governed by Apache License Version 2.0 that can be found in the LICENSE file or at  *
 * http://www.apache.org/licenses/                                                                                *
 * ===============================================================================================================*/
﻿using System.Collections.Generic;
using System.ServiceModel;
using Infosys.Solutions.Ainauto.VideoAnalytics.Services.MaskDetector.Contracts.Data;
using Infosys.Solutions.Ainauto.VideoAnalytics.Services.MaskDetector.Contracts.Message;

namespace Infosys.Solutions.Ainauto.VideoAnalytics.Services.MaskDetector.Contracts
{
    
    public interface IModelInference
    {
        
        ObjectDetectorAPIResMsg GetDetectMask_t1(ObjectDetectorAPIReqMsg request);

        
        ObjectDetectorAPIResMsg GetDetectFaceAndDetectMask_t2(ObjectDetectorAPIReqMsg request);

       
        ObjectDetectorAPIResMsg GetDetectFaceAndDetectMask10m_t2(ObjectDetectorAPIReqMsg request);

        
        ObjectDetectorAPIResMsg GetDetectFaceAndClassifyMask_t3(ObjectDetectorAPIReqMsg request);

        
        ObjectDetectorAPIResMsg GetDetectMask_t5(ObjectDetectorAPIReqMsg request);

        
        ObjectDetectorAPIResMsg GetDetectMask10m_t5_16bit(ObjectDetectorAPIReqMsg request);
        
        
        ObjectDetectorAPIResMsg GetDetectMask10m_t5_hybrid(ObjectDetectorAPIReqMsg request);


        
        ObjectDetectorAPIResMsg GetDetectMask_t5_16bit(ObjectDetectorAPIReqMsg request);

        

        ObjectDetectorAPIResMsg GetDetectMask10m_t5(ObjectDetectorAPIReqMsg request);
        
       
        ObjectDetectorAPIResMsg GetDetectFaceAndDetectMask_t6(ObjectDetectorAPIReqMsg request);

       
        ObjectDetectorAPIResMsg GetDetectFaceAndDetectMask10m_t6(ObjectDetectorAPIReqMsg request);

      
        List<PersonCountAPIResMsg> GetDetectPersonCount_t7(PersonCountAPIReqMsg request);


        
        ObjectDetectorAPIResMsg DetectLicensePlate_t8(ObjectDetectorAPIReqMsg request);

       
        ObjectDetectorAPIResMsg DetectMask_t5_AI_Cloud(ObjectDetectorAPIReqMsg request);

       
        ObjectDetectorAPIResMsg GetFaceVerification(ObjectDetectorAPIReqMsg request);

        
        ObjectDetectorAPIResMsg DetectBasketBall(ObjectDetectorAPIReqMsg request);


       
        ObjectDetectorAPIResMsg GetDFSD(ObjectDetectorAPIReqMsg request);
        
       
        ObjectDetectorAPIResMsg GetGoalDetection(ObjectDetectorAPIReqMsg request);


       
        ObjectDetectorAPIResMsg GetFacialExpressionRecognition(ObjectDetectorAPIReqMsg request);

        
        ObjectDetectorAPIResMsg GetMobileNet(ObjectDetectorAPIReqMsg request);

        
        ObjectDetectorAPIResMsg GetResNet(ObjectDetectorAPIReqMsg request);

    }
}
