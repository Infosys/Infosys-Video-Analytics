/*=============================================================================================================== *
 * Copyright 2025 Infosys Ltd.                                                                                    *
 * Use of this source code is governed by Apache License Version 2.0 that can be found in the LICENSE file or at  *
 * http://www.apache.org/licenses/                                                                                *
 * ===============================================================================================================*/
﻿using Emgu.CV;
using Emgu.CV.Structure;
using Infosys.Solutions.Ainauto.VideoAnalytics.BusinessEntity;
using Infosys.Solutions.Ainauto.VideoAnalytics.Resource.Entity.Queue;
using Emgu.CV.CvEnum;
using Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.Common;
using Newtonsoft.Json.Linq;
using static Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.Common.ApplicationConstants;
using System.Drawing;

namespace Infosys.Solutions.Ainauto.VideoAnalytics.Renderer
{
    public class ObjectDetection : IRender
    {
        public Image<Bgr, byte> RenderFrame(List<Predictions> objectList, Image<Bgr, byte> image, int frameWidth,
        int frameHeight, string modelName, string info, DeviceDetails deviceDetails, string Ad, FrameDetails frameDetails)
        {
            Rectangle rectangle = new Rectangle();
            MCvScalar color = new MCvScalar();
            #region Added background color from Device.json
            Color clBgColor = Color.FromName(deviceDetails.BackgroundColor);
            Color clBgColorPredictCartList = Color.FromName(deviceDetails.RendererPredictCartListBackgroundColor);
            #endregion
            string label = "";
            if (deviceDetails.ObjectDetectionRendering.Equals("yes", StringComparison.InvariantCultureIgnoreCase))
            {
                try
                {
                    Color color2 = new Color();
                    for (var i = 0; i < objectList.Count; i++)
                    {
                        Predictions face = objectList[i];
                        BoundingBox box = face.Dm;
                        if (face.Lb != null)
                        {
                            label = face.Lb.ToLower();
                        }
                        else
                        {
                            label = face.Pid.ToLower();
                        }
                        
                        JObject colorJson = JObject.Parse(deviceDetails.BoxColor);
                        string pencolor;
                        if (colorJson[label] != null)
                        {
                            pencolor = colorJson[label].ToString();
                            color2 = Color.FromName(pencolor);
                        }
                        else
                        {
                            pencolor = string.Empty;
                            if (face.Np != null)
                            {
                                if (face.Np.ToLower() == "yes")
                                {
                                    pencolor = colorJson["new_person"].ToString();
                                    color2 = Color.FromName(pencolor);
                                }
                                else
                                {
                                    pencolor = colorJson["default"].ToString();
                                    color2 = Color.FromName(pencolor);
                                }
                            }
                            else
                            {
                                pencolor = colorJson["default"].ToString();
                                color2 = Color.FromName(pencolor);
                            }
                        }
                        color = new MCvScalar(color2.B, color2.G, color2.R);
                       
                        if (box != null)
                        {
                            int x = Convert.ToInt32(Math.Round(float.Parse(box.X) * image.Width));
                            int y = Convert.ToInt32(Math.Round(float.Parse(box.Y) * image.Height));
                            int w = Convert.ToInt32(Math.Round(float.Parse(box.W) * image.Width));
                            int h = Convert.ToInt32(Math.Round(float.Parse(box.H) * image.Height));
#if DEBUG
                            LogHandler.LogInfo(String.Format("v0:{0} v1:{1} v2:{2}", color.V0,
                            color.V1, color.V2), LogHandler.Layer.Business, null);
#endif
                            rectangle = new Rectangle(x, y, w, h);
                           
                            Bgr background = new Bgr(0, 0, 0);
                            Size size1 = image.Size;
                            if (modelName.ToLower() == "templatematching")
                            {
                                double angle = Convert.ToDouble(face.Info);
                                image = image.Rotate(angle, background, false);
                                
                            }
                            CvInvoke.Rectangle(image, rectangle, color, deviceDetails.PenThickness);
                            
                            rectangle = new Rectangle(x, y - deviceDetails.LabelHeight, w, deviceDetails.LabelHeight);
                            CvInvoke.Rectangle(image, rectangle, color, deviceDetails.PenThickness);
                            CvInvoke.Rectangle(image, rectangle, color, -1);
                            Point point3 = new Point(x, y - deviceDetails.LabelHeight + 12);
                            Color color3 = Color.FromName(deviceDetails.LabelFontColor);
                            color = new MCvScalar(color3.B, color3.G, color3.R);
                            CvInvoke.PutText(image, label, point3, FontFace.HersheySimplex, deviceDetails.RendererFontScale, color, deviceDetails.RendererFontThickness);
                            if (modelName.ToLower() == "templatematching")
                            {
                                double angle = Convert.ToDouble(face.Info);
                                image = image.Rotate(-angle, background, false);
                                
                                Size size2 = image.Size;
                                int x1 = (size2.Width - size1.Width) / 2;
                                int y1 = (size2.Height - size1.Height) / 2;
                                Rectangle roi = new Rectangle(x1, y1, size1.Width, size1.Height);
                                image.ROI = roi;
                               
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
