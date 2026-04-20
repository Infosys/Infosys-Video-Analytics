/*=============================================================================================================== *
 * Copyright 2025 Infosys Ltd.                                                                                    *
 * Use of this source code is governed by Apache License Version 2.0 that can be found in the LICENSE file or at  *
 * http://www.apache.org/licenses/                                                                                *
 * ===============================================================================================================*/
﻿using OpenCvSharp;
using Infosys.Solutions.Ainauto.VideoAnalytics.BusinessEntity;
using Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.Common;
using Infosys.Solutions.Ainauto.VideoAnalytics.Resource.Entity.Queue;
using static Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.Common.ApplicationConstants;
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
            Scalar clBgColor = ColorHelper.ColorNameToScalar(deviceDetails.BackgroundColor);
            Scalar clBgColorPredictCartList = ColorHelper.ColorNameToScalar(deviceDetails.RendererPredictCartListBackgroundColor);
            #endregion
            string label = "";
            if (deviceDetails.Tracking.Equals("yes", StringComparison.InvariantCultureIgnoreCase))
            {
                try
                {
                    Scalar color2 = new Scalar();
                   
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
                        /* switch (label.Contains("mask"))
                        {
                            case true:
                                if (label.StartsWith("mask"))
                                {
                                    color2 = Color.FromName(deviceDetails.MaskPenColor);
                                    if (label.Contains("2"))
                                    {
                                        LogHandler.CollectPerformanceMetric(ApplicationConstants.FRPerfMonCategories.CategoryName,
                                        ApplicationConstants.FRPerfMonCounters.Mask2ObjectDetected,
                                        counterInstanceName,1,false,false);
                                    }
                                    else if (label.Contains("1"))
                                    {
                                        LogHandler.CollectPerformanceMetric(ApplicationConstants.FRPerfMonCategories.CategoryName, 
                                        ApplicationConstants.FRPerfMonCounters.Mask1ObjectDetected,
                                        counterInstanceName,1,false,false);
                                    }
                                    label = FrameRendererKey.maskLabel;
                                }
                                else if (label.StartsWith("no"))
                                {
                                    color2 = Color.FromName(deviceDetails.NoMaskPenColor);
                                    label = FrameRendererKey.noMaskLabel;
                                    LogHandler.CollectPerformanceMetric(ApplicationConstants.FRPerfMonCategories.CategoryName, 
                                    ApplicationConstants.FRPerfMonCounters.NoMaskObjectDetected,
                                    counterInstanceName,1,false,false);
                                }
                                color = new MCvScalar(color2.B, color2.G, color2.R);
                                break;
                            case false: */
                        JObject colorJson = JObject.Parse(deviceDetails.BoxColor);
                        string pencolor;
                        if (colorJson[label] != null)
                        {
                            pencolor = colorJson[label].ToString();
                            color2 = ColorHelper.ColorNameToScalar(pencolor);
                        }
                        else if (label != null && int.TryParse(label, out int value))
                        {
                            try
                            {
                                pencolor = FrameRendererHelper.colornames[Convert.ToInt32(label)].ToString();
                                color2 = ColorHelper.ColorNameToScalar(pencolor);
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
                        color = color2;
                        /* string counterName;
                        if(deviceDetails.PredictionClassType=="dynamic") {
                            counterName=string.Format(ApplicationConstants.FRPerfMonCounters.CounterTemplate,deviceDetails.PredictionModel);
                        }
                        else {
                            counterName=string.Format(ApplicationConstants.FRPerfMonCounters.CounterTemplate,
                            label);
                        }
                        LogHandler.CollectPerformanceMetric(ApplicationConstants.FRPerfMonCategories.CategoryName, 
                        counterName,counterInstanceName,1,false,false);
                        break;
                        }
                        LogHandler.CollectPerformanceMetric(ApplicationConstants.FRPerfMonCategories.CategoryName, 
                        ApplicationConstants.FRPerfMonCounters.TotalObjectDetected,
                        counterInstanceName,1,false,false); */
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
                            Mat overlay=image.Clone();
                            Cv2.Rectangle(image, rectangle, color, deviceDetails.PenThickness);
                            /*
                            image.Data.SetValue((byte)105,new int[]{0,0,0});
                            image.Data.SetValue((byte)105,new int[]{0,0,1});
                            */
                            rectangle = new Rect(x, y - deviceDetails.LabelHeight, w, deviceDetails.LabelHeight);
                            Cv2.Rectangle(image, rectangle, color, deviceDetails.PenThickness);
                            Cv2.Rectangle(image, rectangle, color, -1);
                            double alpha=deviceDetails.RendererBackgroundTransparency;
                            if(alpha!=0) {
                                Cv2.AddWeighted(image,alpha,overlay,1-alpha,0,image);
                            }
                            overlay.Dispose();
                            Point point3=new Point(x,y-deviceDetails.LabelHeight+17);
                            Scalar color3 = ColorHelper.ColorNameToScalar(deviceDetails.LabelFontColor);
                            color = color3;
                            Cv2.PutText(image, label, point3, HersheyFonts.HersheySimplex, deviceDetails.RendererFontScale, color, deviceDetails.RendererFontThickness);
                            /*
                            image.Data.SetValue((byte)35,new int[]{0,0,0});
                            image.Data.SetValue((byte)35,new int[]{0,0,2});
                            */
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
