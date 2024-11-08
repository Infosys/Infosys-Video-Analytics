/*=============================================================================================================== *
 * Copyright 2024 Infosys Ltd.                                                                                    *
 * Use of this source code is governed by Apache License Version 2.0 that can be found in the LICENSE file or at  *
 * http://www.apache.org/licenses/                                                                                *
 * ===============================================================================================================*/

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
