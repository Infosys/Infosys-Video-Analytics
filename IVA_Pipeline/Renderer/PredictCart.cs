/*=============================================================================================================== *
 * Copyright 2025 Infosys Ltd.                                                                                    *
 * Use of this source code is governed by Apache License Version 2.0 that can be found in the LICENSE file or at  *
 * http://www.apache.org/licenses/                                                                                *
 * ===============================================================================================================*/
using OpenCvSharp;
using Infosys.Solutions.Ainauto.VideoAnalytics.BusinessEntity;
using Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.Common;
using Infosys.Solutions.Ainauto.VideoAnalytics.Resource.Entity.Queue;
using Newtonsoft.Json.Linq;
using System.Drawing;

namespace Infosys.Solutions.Ainauto.VideoAnalytics.Renderer
{
    public class PredictCart : IRender
    {
        public Mat RenderFrame(List<Predictions> objectList, Mat image, int frameWidth, int frameHeight, string modelName, string info, DeviceDetails deviceDetails, string Ad, FrameDetails frameDetails)
        {
            Rect rectangle = new Rect();
            Scalar color = new Scalar();
            #region Added background color from Device.json
            Color clBgColor = Color.FromName(deviceDetails.BackgroundColor);
            Color clBgColorPredictCartList = Color.FromName(deviceDetails.RendererPredictCartListBackgroundColor);
            #endregion
            if (deviceDetails.SharedBlobStorage && deviceDetails.PredictCart.Equals("yes", StringComparison.InvariantCultureIgnoreCase))
            {
                try
                {
                    if (Ad != null)
                    {
                        #region Added the code as per new IVA schema starts 
                        JObject jObjects = JObject.Parse(Ad);
                        var outCome = jObjects["Outcome"].ToString();
                        var Obj = jObjects["Obj"].ToString();
                        #endregion
                        rectangle = new Rect(10, 10, 700, 35);

                        color = new Scalar(clBgColor.R, clBgColor.G, clBgColor.B);
                        Cv2.Rectangle(image, rectangle, color, deviceDetails.PenThickness);
                        Cv2.Rectangle(image, rectangle, color, -1);
                        rectangle = new Rect(10, 50, 200, 115);

                        color = new Scalar(clBgColorPredictCartList.R, clBgColorPredictCartList.G, clBgColorPredictCartList.B);
                        Cv2.Rectangle(image, rectangle, color, deviceDetails.PenThickness);
                        Cv2.Rectangle(image, rectangle, color, -1);
                        int width = 30;
                        OpenCvSharp.Point point1 = new OpenCvSharp.Point(10, width);
                        color = new Scalar(255, 255, 255);

                        Cv2.PutText(image, outCome, point1, HersheyFonts.HersheySimplex, deviceDetails.RendererFontScale, color, deviceDetails.RendererFontThickness);

                        color = new Scalar(0, 0, 0);
                        
                        JObject jObjects1 = JObject.Parse(Obj);

                        foreach (var jObject in jObjects1)
                        {
                            string message = jObject.Key + "  :  " + jObject.Value;
                            width = width + 30;
                            OpenCvSharp.Point point2 = new OpenCvSharp.Point(10, width);
                            Cv2.PutText(image, message, point2, HersheyFonts.HersheySimplex, deviceDetails.RendererFontScale, color, deviceDetails.RendererFontThickness);
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogHandler.LogError("Error in PredictCart.RenderFrame, exception: {0}, inner exception: {1}, stack trace: {2}",
                        LogHandler.Layer.FrameRenderer, ex.Message, ex.InnerException, ex.StackTrace);
                }
            }

            return image;
        }
    }
}
