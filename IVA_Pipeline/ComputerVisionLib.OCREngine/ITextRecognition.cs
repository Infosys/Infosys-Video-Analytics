/*=============================================================================================================== *
 * Copyright 2025 Infosys Ltd.                                                                                    *
 * Use of this source code is governed by Apache License Version 2.0 that can be found in the LICENSE file or at  *
 * http://www.apache.org/licenses/                                                                                *
 * ===============================================================================================================*/
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infosys.IVA.ComputerVisionLib.OCREngine
{
    public interface ITextRecognition
    {
        string ReadTextArea(double x, double y, double height, double width, string filter,float imageResizeCoeff);
    }
}
