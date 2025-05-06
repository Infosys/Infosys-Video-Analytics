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
    public class Classification : IRender
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
            if (deviceDetails.ClassificationRendering.Equals("yes", StringComparison.InvariantCultureIgnoreCase))
            {
                try
                {
                    if (objectList != null)
                    {
                        rectangle = new Rectangle(RendererRectanglePointX, RendererRectanglePointY, frameWidth, RendererRectangleHeight);
                        color = new MCvScalar(clBgColor.B, clBgColor.G, clBgColor.R);
                        CvInvoke.Rectangle(image, rectangle, color, -1);
                        Color color3 = Color.FromName(deviceDetails.LabelFontColor);
                        color = new MCvScalar(color3.B, color3.G, color3.R);
                        int x = RendererLabelPointX;
                        int y = RendererLabelPointY;
                        double cs;
                        for (int i = 0; i < objectList.Count; i++)
                        {

                            string[] words = objectList[i].Lb.Split(' ');
                            string currentLine = "";
                            int baseline = 0;
                            Size textSize = CvInvoke.GetTextSize(currentLine, FontFace.HersheySimplex, deviceDetails.RendererFontScale, deviceDetails.RendererFontThickness, ref baseline);
                            foreach (string word in words)
                            {
                                baseline = 0;
                                textSize = CvInvoke.GetTextSize(currentLine, FontFace.HersheySimplex, deviceDetails.RendererFontScale, deviceDetails.RendererFontThickness, ref baseline);
                                if (textSize.Width < image.Width * 0.8)
                                {
                                    currentLine = string.IsNullOrEmpty(currentLine) ? word : currentLine + " " + word;
                                }
                                else
                                {
                                    y += textSize.Height;
                                    Point point = new Point(x, y);
                                    CvInvoke.PutText(image, currentLine, point, FontFace.HersheySimplex, deviceDetails.RendererFontScale, color, deviceDetails.RendererFontThickness);
                                    currentLine = "";
                                }
                            }

                            if (objectList[i].Cs != "")
                            {
                                cs = Convert.ToDouble(objectList[i].Cs);
                                cs = Math.Round(cs, 2);
                                y += textSize.Height;
                                currentLine += " , " + cs.ToString();
                                Point point = new Point(x, y);
                                CvInvoke.PutText(image, currentLine, point, FontFace.HersheySimplex, deviceDetails.RendererFontScale, color, deviceDetails.RendererFontThickness);
                            }
                            else
                            {
                                y += textSize.Height;
                                Point point = new Point(x, y);
                                CvInvoke.PutText(image, currentLine, point, FontFace.HersheySimplex, deviceDetails.RendererFontScale, color, deviceDetails.RendererFontThickness);
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
                MCvScalar lbColor = new Bgr(255, 255, 255).MCvScalar;
                
                double fontScale = 1;
                int thickness = 3;

                
                Point position = new Point(10, 30);

              
                CvInvoke.PutText(image, objectList[0].Lb + "mph", position, FontFace.HersheySimplex, fontScale, lbColor, thickness);
            }
            return image;
        }
    }
}
