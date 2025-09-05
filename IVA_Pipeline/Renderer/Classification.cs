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
    public class Classification : IRender
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
            Color clBgColor = Color.FromName(deviceDetails.BackgroundColor);
            Color clBgColorPredictCartList = Color.FromName(deviceDetails.RendererPredictCartListBackgroundColor);
            #endregion
            if (deviceDetails.ClassificationRendering.Equals("yes", StringComparison.InvariantCultureIgnoreCase))
            {
                try
                {
                    if (objectList != null)
                    {
                        rectangle = new Rect(RendererRectanglePointX, RendererRectanglePointY, frameWidth, RendererRectangleHeight);
                        color = new Scalar(clBgColor.B, clBgColor.G, clBgColor.R);
                        Cv2.Rectangle(image, rectangle, color, -1);
                        Color color3 = Color.FromName(deviceDetails.LabelFontColor);
                        color = new Scalar(color3.B, color3.G, color3.R);
                        int x = RendererLabelPointX;
                        int y = RendererLabelPointY;
                        double cs;
                        for (int i = 0; i < objectList.Count; i++)
                        {

                            string[] words = objectList[i].Lb.Split(' ');
                            string currentLine = "";
                            int baseline = 0;
                            OpenCvSharp.Size textSize = new OpenCvSharp.Size(0, 0);
                            //textSize = Cv2.GetTextSize(currentLine, HersheyFonts.HersheySimplex, deviceDetails.RendererFontScale, deviceDetails.RendererFontThickness, out baseline);
                            foreach (string word in words)
                            {
                                baseline = 0;
                                string tempLine = string.IsNullOrEmpty(currentLine) ? word : currentLine + " " + word;
                                textSize = Cv2.GetTextSize(tempLine, HersheyFonts.HersheySimplex, deviceDetails.RendererFontScale, deviceDetails.RendererFontThickness, out baseline);
                                if (textSize.Width < image.Width * 0.8)
                                {
                                    currentLine = string.IsNullOrEmpty(currentLine) ? word : currentLine + " " + word;
                                }
                                else
                                {
                                    y += textSize.Height;
                                    OpenCvSharp.Point point = new OpenCvSharp.Point(x, y);
                                    Cv2.PutText(image, currentLine, point, HersheyFonts.HersheySimplex, deviceDetails.RendererFontScale, color, deviceDetails.RendererFontThickness);
                                    currentLine = "";
                                }
                            }

                            if (objectList[i].Cs != "")
                            {
                                cs = Convert.ToDouble(objectList[i].Cs);
                                cs = Math.Round(cs, 2);
                                y += textSize.Height;
                                currentLine += " , " + cs.ToString();
                                OpenCvSharp.Point point = new OpenCvSharp.Point(x, y);
                                Cv2.PutText(image, currentLine, point, HersheyFonts.HersheySimplex, deviceDetails.RendererFontScale, color, deviceDetails.RendererFontThickness);
                            }
                            else
                            {
                                y += textSize.Height;
                                OpenCvSharp.Point point = new OpenCvSharp.Point(x, y);
                                Cv2.PutText(image, currentLine, point, HersheyFonts.HersheySimplex, deviceDetails.RendererFontScale, color, deviceDetails.RendererFontThickness);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogHandler.LogError("Error in Classification.RenderFrame, exception: {0}, inner exception: {1}, stack trace: {2}",
                        LogHandler.Layer.FrameRenderer, ex.Message, ex.InnerException, ex.StackTrace);
                }
            }
            if(deviceDetails.SpeedDetection.Equals("yes", StringComparison.InvariantCultureIgnoreCase))
            {
                Scalar lbColor = new Scalar(255, 255, 255);
                
                double fontScale = 1;
                int thickness = 3;

                
                OpenCvSharp.Point position = new OpenCvSharp.Point(10, 30);

              
                Cv2.PutText(image, objectList[0].Lb + "mph", position, HersheyFonts.HersheySimplex, fontScale, lbColor, thickness);
            }
            return image;
        }
    }
}
