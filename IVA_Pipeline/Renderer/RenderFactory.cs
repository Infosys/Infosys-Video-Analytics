/*=============================================================================================================== *
 * Copyright 2025 Infosys Ltd.                                                                                    *
 * Use of this source code is governed by Apache License Version 2.0 that can be found in the LICENSE file or at  *
 * http://www.apache.org/licenses/                                                                                *
 * ===============================================================================================================*/
﻿

namespace Infosys.Solutions.Ainauto.VideoAnalytics.Renderer
{
    public static class RenderFactory
    {
        public static IRender? CreateInstance(string renderType)
        {
            IRender? render = null;
            if(!string.IsNullOrEmpty(renderType))
            {
                string rendernamespace = "Infosys.Solutions.Ainauto.VideoAnalytics.Renderer";
                Type? type = Type.GetType(rendernamespace + "." + renderType);
                render = (IRender?)Activator.CreateInstance(type);
            }
            return render;
        }
    }
}
