/*=============================================================================================================== *
 * Copyright 2025 Infosys Ltd.                                                                                    *
 * Use of this source code is governed by Apache License Version 2.0 that can be found in the LICENSE file or at  *
 * http://www.apache.org/licenses/                                                                                *
 * ===============================================================================================================*/
/* 
 * © 2012-2013 Infosys Limited, Bangalore, India. All Rights Reserved.
 * Version: 1.0 b
 * Except for any open source software components embedded in this Infosys proprietary software program ("Program"),
 * this Program is protected by copyright laws, international treaties and other pending or existing intellectual
 * property rights in India, the United States and other countries. Except as expressly permitted, any unauthorized
 * reproduction, storage, transmission in any form or by any means (including without limitation electronic, mechanical,
 * printing, photocopying, recording or otherwise), or any distribution of this Program, or any portion of it, may
 * results in severe civil and criminal penalties, and will be prosecuted to the maximum extent possible under the law.
 */

namespace Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.ProcessScheduler.Framework
{
    public interface IProcessHandler
    {
       
        void Start(Drive[] drives, ModeType mode, string entityName, string id , int robotId, int runInstanceId, int robotTaskMapId);
    }
}
