/*=============================================================================================================== *
 * Copyright 2024 Infosys Ltd.                                                                                    *
 * Use of this source code is governed by Apache License Version 2.0 that can be found in the LICENSE file or at  *
 * http://www.apache.org/licenses/                                                                                *
 * ===============================================================================================================*/


using Microsoft.ML.Transforms.Image;
using System.Drawing;
using Microsoft.ML.Data;

namespace Infosys.Solutions.Ainauto.VideoAnalytics.AIModels
{

    public struct ImageSettings
    {
        public const int imageHeight = 416;
        public const int imageWidth = 416;
    }

    public class ImageInput
    {
        [ImageType(ImageSettings.imageHeight, ImageSettings.imageWidth)]
        public Bitmap Image { get; set; }
    }

    public class ImagePredictions
    {
        [ColumnName("model_outputs0")]
        public float[] PredictedLabels { get; set; }
    }
}
