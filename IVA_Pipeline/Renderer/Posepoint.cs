/*=============================================================================================================== *
 * Copyright 2025 Infosys Ltd.                                                                                    *
 * Use of this source code is governed by Apache License Version 2.0 that can be found in the LICENSE file or at  *
 * http://www.apache.org/licenses/                                                                                *
 * ===============================================================================================================*/
﻿using Emgu.CV;
using Emgu.CV.Structure;
using System.Drawing;
using Infosys.Solutions.Ainauto.VideoAnalytics.BusinessEntity;
using Infosys.Solutions.Ainauto.VideoAnalytics.Resource.Entity.Queue;
using Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.Common;

namespace Infosys.Solutions.Ainauto.VideoAnalytics.Renderer
{
    public class Posepoint : IRender
    {
        public Image<Bgr, byte> RenderFrame(List<Predictions> objectList, Image<Bgr, byte> image, int frameWidth,
        int frameHeight, string modelName, string info, DeviceDetails deviceDetails, string Ad, FrameDetails frameDetails)
        {
            #region Added background color from Device.json
            Color clBgColor = Color.FromName(deviceDetails.BackgroundColor);
            Color clBgColorPredictCartList = Color.FromName(deviceDetails.RendererPredictCartListBackgroundColor);
            #endregion
            if (deviceDetails.PosePointRendering.Equals("yes", StringComparison.InvariantCultureIgnoreCase))
            {
                try
                {
                    if (objectList != null)
                    {
                        int objectListCount = objectList.Count;
                        for (int i = 0; i < objectListCount; i++)
                        {
                            var keypoints = objectList[i].Kp.Count;
                            for (int j = 1; j < keypoints; j++)
                            {
                                var point1 = Convert.ToInt32(objectList[i].Kp[j][0] * image.Width);
                                var point2 = Convert.ToInt32(objectList[i].Kp[j][1] * image.Height);
                                CvInvoke.Circle(image, new Point(point1, point2), 4, new MCvScalar(0, 0, 255), -1);
                            }
                        }

                        for (int i = 0; i < objectListCount; i++)
                        {
                            var keypoints = objectList[i].Kp.Count;
                            for (int j = 1; j < keypoints; j++)
                            {
                                for (int z = 1; z < deviceDetails.KpSkeleton.Count; z++)
                                {
                                    var Kskeletonpoint1 = deviceDetails.KpSkeleton[z][0];
                                    var Kskeletonpoint2 = deviceDetails.KpSkeleton[z][1];
                                    var kppoint1 = Convert.ToInt32(objectList[i].Kp[Kskeletonpoint1][0] * image.Width);
                                    var kppoint2 = Convert.ToInt32(objectList[i].Kp[Kskeletonpoint1][1] * image.Height);
                                    var kppoint3 = Convert.ToInt32(objectList[i].Kp[Kskeletonpoint2][0] * image.Width);
                                    var kppoint4 = Convert.ToInt32(objectList[i].Kp[Kskeletonpoint2][1] * image.Height);
                                    CvInvoke.Line(image, new Point(kppoint1, kppoint2), new Point(kppoint3, kppoint4), new MCvScalar(0, 255, 0), 2);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogHandler.LogError("Error in Posepoint.RenderFrame, exception: {0}, inner exception: {1}, stack trace: {2}",
                        LogHandler.Layer.Business, ex.Message, ex.InnerException, ex.StackTrace);
                }
            }
            return image;
        }
    }
}
