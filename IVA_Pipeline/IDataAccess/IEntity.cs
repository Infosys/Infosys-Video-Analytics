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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infosys.Solutions.Ainauto.VideoAnalytics.Resource.IDataAccess
{
    public interface IEntity<T>
    {
        T GetOne(T Entity);
        IQueryable<T> GetAny();
        IList<T> GetAll();
        T Insert(T entity);
        T Update(T entity);
        bool Delete(T entity);
        IList<T> InsertBatch(IList<T> entities);
        IList<T> UpdateBatch(IList<T> entities);
        IList<T> GetAll(T Entity);
    }
}
