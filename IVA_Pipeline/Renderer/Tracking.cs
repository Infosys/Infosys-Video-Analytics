/*=============================================================================================================== *
 * Copyright 2025 Infosys Ltd.                                                                                    *
 * Use of this source code is governed by Apache License Version 2.0 that can be found in the LICENSE file or at  *
 * http://www.apache.org/licenses/                                                                                *
 * ===============================================================================================================*/
using OpenCvSharp;
using Infosys.Solutions.Ainauto.VideoAnalytics.BusinessEntity;
using Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.Common;
using Infosys.Solutions.Ainauto.VideoAnalytics.Resource.Entity.Queue;
using static Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.Common.ApplicationConstants;
using System.Drawing;
using System.Text.Json;
using Renderer;
using Newtonsoft.Json.Linq;

namespace Infosys.Solutions.Ainauto.VideoAnalytics.Renderer
{
    public class Tracking : IRender
    {
        public Mat RenderFrame(List<Predictions> objectList, Mat image, int frameWidth, int frameHeight, string modelName, string info, DeviceDetails deviceDetails, string Ad, FrameDetails frameDetails)
        {
            Rect rectangle = new Rect();
            Scalar color = new Scalar();
            #region Added background color from Device.json
            Color clBgColor = Color.FromName(deviceDetails.BackgroundColor);
            Color clBgColorPredictCartList = Color.FromName(deviceDetails.RendererPredictCartListBackgroundColor);
            #endregion
            string label = "";
            if (deviceDetails.Tracking.Equals("yes", StringComparison.InvariantCultureIgnoreCase))
            {
                try
                {
                    Color color2 = new Color();
                   
                    for (var i = 0; i < objectList.Count; i++)
                    {
                        Predictions face = objectList[i];
                        BoundingBox box = face.Dm;
                        if (face.Uid != null)
                        {
                            
                            label = face.Uid;
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
                        else if (label != null && int.TryParse(label, out int value))
                        {
                            try
                            {
                                pencolor = FrameRendererHelper.colornames[Convert.ToInt32(label)].ToString();
                                color2 = Color.FromName(pencolor);
                            }
                            catch (Exception ex)
                            {
                               
                                LogHandler.LogError("Error in rendering, Exception : {0}\nStackTrace : {1}", LogHandler.Layer.Business, ex.Message, ex.StackTrace);
                            }
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
                            Cv2.Rectangle(image, rectangle, color, deviceDetails.PenThickness);
                            /*
                            image.Data.SetValue((byte)105,new int[]{0,0,0});
                            image.Data.SetValue((byte)105,new int[]{0,0,1});
                            */
                            rectangle = new Rect(x, y - deviceDetails.LabelHeight, w, deviceDetails.LabelHeight);
                            Cv2.Rectangle(image, rectangle, color, deviceDetails.PenThickness);
                            Cv2.Rectangle(image, rectangle, color, -1);
                            OpenCvSharp.Point point3 = new OpenCvSharp.Point(x, y - deviceDetails.LabelHeight + 15);
                            Color color3 = Color.FromName(deviceDetails.LabelFontColor);
                            color = new Scalar(color3.B, color3.G, color3.R);
                            Cv2.PutText(image, label, point3, HersheyFonts.HersheySimplex, deviceDetails.RendererFontScale, color, deviceDetails.RendererFontThickness);
                        }

                    }
                }
                catch (Exception ex)
                {
                    LogHandler.LogError("Error in Tracking.RenderFrame, exception: {0}, inner exception: {1}, stack trace: {2}",
                        LogHandler.Layer.FrameRenderer, ex.Message, ex.InnerException, ex.StackTrace);
                }
            }
            return image;
        }
    }
}
