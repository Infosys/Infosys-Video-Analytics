/*=============================================================================================================== *
 * Copyright 2025 Infosys Ltd.                                                                                    *
 * Use of this source code is governed by Apache License Version 2.0 that can be found in the LICENSE file or at  *
 * http://www.apache.org/licenses/                                                                                *
 * ===============================================================================================================*/
﻿using OpenCvSharp;
using Infosys.Solutions.Ainauto.VideoAnalytics.BusinessEntity;
using Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.Common;
using Infosys.Solutions.Ainauto.VideoAnalytics.Resource.Entity.Queue;

namespace Infosys.Solutions.Ainauto.VideoAnalytics.Renderer
{
    public class MPlug : IRender
    {
        public Mat RenderFrame(List<Predictions> objectList, Mat image, int frameWidth, int frameHeight, string modelName, string info, DeviceDetails deviceDetails, string Ad, FrameDetails frameDetails)
        {
            Rect rectangle = new Rect();
            Scalar color = new Scalar();
            int RendererRectanglePointX = deviceDetails.RendererRectanglePointX;
            int RendererRectanglePointY = deviceDetails.RendererRectanglePointY;
            int RendererLabelPointX = deviceDetails.RendererLabelPointX;
            int RendererLabelPointY = deviceDetails.RendererLabelPointY;
            int RendererRectangleHeight = deviceDetails.RendererRectangleHeight;
            #region Added background color from Device.json
            Scalar clBgColor = ColorHelper.ColorNameToScalar(deviceDetails.BackgroundColor);
            Scalar clBgColorPredictCartList = ColorHelper.ColorNameToScalar(deviceDetails.RendererPredictCartListBackgroundColor);
            #endregion
            if(deviceDetails.Mplug.Equals("yes", StringComparison.InvariantCultureIgnoreCase))
            {
                try
                {
                    rectangle = new Rect(RendererRectanglePointX, RendererRectanglePointY, frameWidth, RendererRectangleHeight);

                    color = new Scalar(clBgColor.Val0, clBgColor.Val1, clBgColor.Val2);
                    Cv2.Rectangle(image, rectangle, color, deviceDetails.PenThickness);
                    Cv2.Rectangle(image, rectangle, color, -1);

                    Point point = new Point(RendererLabelPointX, RendererLabelPointY);

                    Scalar color3 = ColorHelper.ColorNameToScalar(deviceDetails.LabelFontColor);
                    color = color3;
                    Cv2.PutText(image, Ad, point, HersheyFonts.HersheySimplex, deviceDetails.RendererFontScale, color);
                }
                catch (Exception ex)
                {
                    LogHandler.LogError("Error in Mplug.RenderFrame, exception: {0}, inner exception: {1}, stack trace: {2}",
                        LogHandler.Layer.FrameRenderer, ex.Message, ex.InnerException, ex.StackTrace);
                }
            }
            return image;
        }
    }
}
