/*=============================================================================================================== *
 * Copyright 2025 Infosys Ltd.                                                                                    *
 * Use of this source code is governed by Apache License Version 2.0 that can be found in the LICENSE file or at  *
 * http://www.apache.org/licenses/                                                                                *
 * ===============================================================================================================*/
using OpenCvSharp;
using Infosys.Solutions.Ainauto.VideoAnalytics.BusinessEntity;
using Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.Common;
using Infosys.Solutions.Ainauto.VideoAnalytics.Resource.Entity.Queue;
using System.Drawing;

namespace Infosys.Solutions.Ainauto.VideoAnalytics.Renderer
{
    public class CrowdCounting : IRender
    {
        public Mat RenderFrame(List<Predictions> objectList, Mat image, int frameWidth, int frameHeight, string modelName, string info, DeviceDetails deviceDetails, string Ad, FrameDetails frameDetails)
        {
            Scalar color = new Scalar();
            #region Added background color from Device.json
            Color clBgColor = Color.FromName(deviceDetails.BackgroundColor);
            Color clBgColorPredictCartList = Color.FromName(deviceDetails.RendererPredictCartListBackgroundColor);
            #endregion
            int RendererRectanglePointX = deviceDetails.RendererRectanglePointX;
            int RendererRectanglePointY = deviceDetails.RendererRectanglePointY;
            int RendererLabelPointX = deviceDetails.RendererLabelPointX;
            int RendererLabelPointY = deviceDetails.RendererLabelPointY;
            int RendererRectangleHeight = deviceDetails.RendererRectangleHeight;
            string label = "";
            if (deviceDetails.CrowdCounting.Equals("yes", StringComparison.InvariantCultureIgnoreCase))
            {
                try
                {
                    Predictions face = objectList[0];
                    if (face.Lb != null)
                    {
                        label = face.Lb + ":" + face.Info;
                    }
#if DEBUG
                    LogHandler.LogInfo(String.Format("v0:{0} v1:{1} v2:{2}", color.Val0,
                        color.Val1, color.Val2), LogHandler.Layer.Business, null);
#endif
                    OpenCvSharp.Point point3 = new OpenCvSharp.Point(RendererLabelPointX, RendererLabelPointY);
                    Color color3 = Color.FromName(deviceDetails.LabelFontColor);
                    color = new Scalar(color3.B, color3.G, color3.R);
                    Cv2.PutText(image, label, point3, HersheyFonts.HersheySimplex, deviceDetails.RendererFontScale, color, deviceDetails.RendererFontThickness);

                    for (var z = 0; z < face.Tpc.Count; z++)
                    {
                        var point1 = Convert.ToInt32(face.Tpc[z][0] * image.Width);
                        var point2 = Convert.ToInt32(face.Tpc[z][1] * image.Height);
                        Cv2.Circle(image, new OpenCvSharp.Point(point1, point2), 4, new Scalar(0, 0, 255), -1);

                    }
                    string? ext = new ImageFormatConverter().ConvertToString(image.ImEncode(".jpg"));
                }
                catch (Exception ex)
                {
                    LogHandler.LogError("Error in CrowdCounting.RenderFrame, exception: {0}, inner exception: {1}, stack trace: {2}",
                        LogHandler.Layer.FrameRenderer, ex.Message, ex.InnerException, ex.StackTrace);
                }
            }
            return image;
        }
    }
}
