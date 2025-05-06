/*=============================================================================================================== *
 * Copyright 2025 Infosys Ltd.                                                                                    *
 * Use of this source code is governed by Apache License Version 2.0 that can be found in the LICENSE file or at  *
 * http://www.apache.org/licenses/                                                                                *
 * ===============================================================================================================*/
﻿using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Infosys.Solutions.Ainauto.VideoAnalytics.BusinessEntity;
using Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.Common;
using Infosys.Solutions.Ainauto.VideoAnalytics.Resource.Entity.Queue;
using System.Drawing;

namespace Infosys.Solutions.Ainauto.VideoAnalytics.Renderer
{
    public class MPlug : IRender
    {
        public Image<Bgr, byte> RenderFrame(List<Predictions> objectList, Image<Bgr, byte> image, int frameWidth, int frameHeight, string modelName, string info, DeviceDetails deviceDetails, string Ad, FrameDetails frameDetails)
        {
            Rectangle rectangle = new Rectangle();
            MCvScalar color = new MCvScalar();
            int RendererRectanglePointX = deviceDetails.RendererRectanglePointX;
            int RendererRectanglePointY = deviceDetails.RendererRectanglePointY;
            int RendererLabelPointX = deviceDetails.RendererLabelPointX;
            int RendererLabelPointY = deviceDetails.RendererLabelPointY;
            int RendererRectangleHeight = deviceDetails.RendererRectangleHeight;
            #region Added background color from Device.json
            Color clBgColor = Color.FromName(deviceDetails.BackgroundColor);
            Color clBgColorPredictCartList = Color.FromName(deviceDetails.RendererPredictCartListBackgroundColor);
            #endregion
            if(deviceDetails.Mplug.Equals("yes", StringComparison.InvariantCultureIgnoreCase))
            {
                try
                {
                    rectangle = new Rectangle(RendererRectanglePointX, RendererRectanglePointY, frameWidth, RendererRectangleHeight);

                    color = new MCvScalar(clBgColor.R, clBgColor.G, clBgColor.B);
                    CvInvoke.Rectangle(image, rectangle, color, deviceDetails.PenThickness);
                    CvInvoke.Rectangle(image, rectangle, color, -1);

                    Point point = new Point(RendererLabelPointX, RendererLabelPointY);

                    Color color3 = Color.FromName(deviceDetails.LabelFontColor);
                    color = new MCvScalar(color3.B, color3.G, color3.R);
                    CvInvoke.PutText(image, Ad, point, FontFace.HersheySimplex, deviceDetails.RendererFontScale, color);
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
