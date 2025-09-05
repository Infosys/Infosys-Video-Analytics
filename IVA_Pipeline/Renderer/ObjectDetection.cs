/*=============================================================================================================== *
 * Copyright 2025 Infosys Ltd.                                                                                    *
 * Use of this source code is governed by Apache License Version 2.0 that can be found in the LICENSE file or at  *
 * http://www.apache.org/licenses/                                                                                *
 * ===============================================================================================================*/
using OpenCvSharp;
using Infosys.Solutions.Ainauto.VideoAnalytics.BusinessEntity;
using Infosys.Solutions.Ainauto.VideoAnalytics.Resource.Entity.Queue;
using Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.Common;
using Newtonsoft.Json.Linq;
using static Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.Common.ApplicationConstants;
using System.Drawing;

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
                        color = new Scalar(color2.B, color2.G, color2.R);
                       
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
                            OpenCvSharp.Size size1 = image.Size();
                            if (modelName.ToLower() == "templatematching")
                            {
                                double angle = Convert.ToDouble(face.Info);
                                //image = image.Rotate(angle, background, false);
                                Point2f center = new Point2f(image.Width / 2.0f, image.Height / 2.0f);
                                InputArray rotationMatrix = Cv2.GetRotationMatrix2D(center, angle, 1.0);
                                Mat rotatedImage = new Mat();
                                Cv2.WarpAffine(image, rotatedImage, rotationMatrix, image.Size());
                                image = rotatedImage;
                            }
                            Cv2.Rectangle(image, rectangle, color, deviceDetails.PenThickness);
                            
                            rectangle = new Rect(x, y - deviceDetails.LabelHeight, w, deviceDetails.LabelHeight);
                            Cv2.Rectangle(image, rectangle, color, deviceDetails.PenThickness);
                            Cv2.Rectangle(image, rectangle, color, -1);
                            OpenCvSharp.Point point3 = new OpenCvSharp.Point(x, y - deviceDetails.LabelHeight + 12);
                            Color color3 = Color.FromName(deviceDetails.LabelFontColor);
                            color = new Scalar(color3.B, color3.G, color3.R);
                            Cv2.PutText(image, label, point3, HersheyFonts.HersheySimplex, deviceDetails.RendererFontScale, color, deviceDetails.RendererFontThickness);
                            if (modelName.ToLower() == "templatematching")
                            {
                                double angle = Convert.ToDouble(face.Info);
                                Point2f center = new Point2f(image.Width / 2.0f, image.Height / 2.0f);
                                //image = image.Rotate(-angle, background, false);

                                OpenCvSharp.Size size2 = image.Size();
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
