/*=============================================================================================================== *
 * Copyright 2025 Infosys Ltd.                                                                                    *
 * Use of this source code is governed by Apache License Version 2.0 that can be found in the LICENSE file or at  *
 * http://www.apache.org/licenses/                                                                                *
 * ===============================================================================================================*/
﻿using OpenCvSharp;
using Infosys.Solutions.Ainauto.VideoAnalytics.BusinessEntity;
using Infosys.Solutions.Ainauto.VideoAnalytics.Resource.Entity.Queue;
using Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.Common;
using Newtonsoft.Json.Linq;
using static Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.Common.ApplicationConstants;

namespace Infosys.Solutions.Ainauto.VideoAnalytics.Renderer
{
    public class ObjectDetection : IRender
    {
        public Mat RenderFrame(List<Predictions> objectList, Mat image, int frameWidth,
        int frameHeight, string modelName, string info, DeviceDetails deviceDetails, string Ad, FrameDetails frameDetails)
        {
            Rect rectangle = new Rect();
            Scalar color = new Scalar();
            #region Added background color from Device.json
            Scalar clBgColor = ColorHelper.ColorNameToScalar(deviceDetails.BackgroundColor);
            Scalar clBgColorPredictCartList = ColorHelper.ColorNameToScalar(deviceDetails.RendererPredictCartListBackgroundColor);
            #endregion
            string label = "";
            if (deviceDetails.ObjectDetectionRendering.Equals("yes", StringComparison.InvariantCultureIgnoreCase))
            {
                try
                {
                    Scalar color2 = new Scalar();
                    for (var i = 0; i < objectList.Count; i++)
                    {
                        Predictions face = objectList[i];
                        BoundingBox box = face.Dm;
                        if (face.Lb != null)
                        {
                            label = face.Lb.ToLower();
                        }
                        else if (face.Pid != null)
                        {
                            label = face.Pid.ToLower();
                        }
                        
                        JObject colorJson = JObject.Parse(deviceDetails.BoxColor);
                        string pencolor;
                        if (colorJson[label] != null)
                        {
                            pencolor = colorJson[label].ToString();
                            color2 = ColorHelper.ColorNameToScalar(pencolor);
                        }
                        else
                        {
                            pencolor = string.Empty;
                            if (face.Np != null)
                            {
                                if (face.Np.ToLower() == "yes")
                                {
                                    pencolor = colorJson["new_person"].ToString();
                                    color2 = ColorHelper.ColorNameToScalar(pencolor);
                                }
                                else
                                {
                                    pencolor = colorJson["default"].ToString();
                                    color2 = ColorHelper.ColorNameToScalar(pencolor);
                                }
                            }
                            else
                            {
                                pencolor = colorJson["default"].ToString();
                                color2 = ColorHelper.ColorNameToScalar(pencolor);
                            }
                        }
                        if (!string.IsNullOrEmpty(objectList[i].Info))
                        {
                            if (deviceDetails.DisplayPredictionInfo)
                            {
                                label = objectList[i].Info;
                            }
                        }
                        color = color2;
                       
                        if (box != null)
                        {
                            int x = Convert.ToInt32(Math.Round(float.Parse(box.X) * image.Width));
                            int y = Convert.ToInt32(Math.Round(float.Parse(box.Y) * image.Height));
                            int w = Convert.ToInt32(Math.Round(float.Parse(box.W) * image.Width));
                            int h = Convert.ToInt32(Math.Round(float.Parse(box.H) * image.Height));
#if DEBUG
                            LogHandler.LogInfo(String.Format("v0:{0} v1:{1} v2:{2}", color.Val0,
                            color.Val1, color.Val2), LogHandler.Layer.Business, null);
#endif
                            rectangle = new Rect(x, y, w, h);
                           
                            Scalar background = new Scalar(0, 0, 0);
                            Size size1 = image.Size();
                            if (modelName.ToLower() == "templatematching")
                            {
                                double angle = Convert.ToDouble(face.Info);
                                var center = new Point2f(image.Width / 2f, image.Height / 2f);
                                var rotMat = Cv2.GetRotationMatrix2D(center, angle, 1.0);
                                Cv2.WarpAffine(image, image, rotMat, image.Size());
                                
                            }
                            Mat overlay=image.Clone();
                            Cv2.Rectangle(image,rectangle,color,deviceDetails.PenThickness);
                            rectangle=new Rect(x,y-deviceDetails.LabelHeight,w,deviceDetails.LabelHeight);
                            Cv2.Rectangle(image,rectangle,color,deviceDetails.PenThickness);
                            Cv2.Rectangle(image,rectangle,color,-1);
                            double alpha=deviceDetails.RendererBackgroundTransparency;
                            if(alpha!=0) {
                                Cv2.AddWeighted(image,alpha,overlay,1-alpha,0,image);
                            }
                            overlay.Dispose();
                            Point point3=new Point(x,y-deviceDetails.LabelHeight+16);
                            Scalar color3 = ColorHelper.ColorNameToScalar(deviceDetails.LabelFontColor);
                            color = color3;
                            Cv2.PutText(image, label, point3, HersheyFonts.HersheySimplex, deviceDetails.RendererFontScale, color, deviceDetails.RendererFontThickness);
                            if (modelName.ToLower() == "templatematching")
                            {
                                double angle = Convert.ToDouble(face.Info);
                                var center = new Point2f(image.Width / 2f, image.Height / 2f);
                                var rotMat = Cv2.GetRotationMatrix2D(center, -angle, 1.0);
                                Cv2.WarpAffine(image, image, rotMat, image.Size());
                                
                                Size size2 = image.Size();
                                int x1 = (size2.Width - size1.Width) / 2;
                                int y1 = (size2.Height - size1.Height) / 2;
                                Rect roi = new Rect(x1, y1, size1.Width, size1.Height);
                                image = new Mat(image, roi);
                               
                            }
                            
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogHandler.LogError("Error in BoundingBox.RenderFrame, exception: {0}, inner exception: {1}, stack trace: {2}",
                        LogHandler.Layer.Business, ex.Message, ex.InnerException, ex.StackTrace);
                }
            }
            return image;
        }
    }
}
