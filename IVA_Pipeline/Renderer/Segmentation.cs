/*=============================================================================================================== *
 * Copyright 2025 Infosys Ltd.                                                                                    *
 * Use of this source code is governed by Apache License Version 2.0 that can be found in the LICENSE file or at  *
 * http://www.apache.org/licenses/                                                                                *
 * ===============================================================================================================*/
﻿using Emgu.CV.Structure;
using Emgu.CV;
using Infosys.Solutions.Ainauto.VideoAnalytics.BusinessEntity;
using Newtonsoft.Json;
using System.Drawing;
using Infosys.Solutions.Ainauto.VideoAnalytics.Resource.Entity.Queue;

namespace Infosys.Solutions.Ainauto.VideoAnalytics.Renderer
{
    public class Segmentation : IRender
    {
        public static int counter;

        public Segmentation()
        {
            counter++;
        }

        public Image<Bgr, byte> RenderFrame(List<Predictions> objectList, Image<Bgr, byte> image, int frameWidth, int frameHeight, string modelName, string info, DeviceDetails deviceDetails, string Ad, FrameDetails frameDetails)
        {
            Bgr bpcColor = new Bgr();
            Bgr tpcColor = new Bgr();
            Dictionary<string, string> segmentColors = JsonConvert.DeserializeObject<Dictionary<string, string>>(deviceDetails.SegmentColors);
            Dictionary<string, string> labelColors = JsonConvert.DeserializeObject<Dictionary<string, string>>(deviceDetails.LabelColor);
            if(deviceDetails.SegmentRendering.Equals("yes", StringComparison.InvariantCultureIgnoreCase))
            {
                for (int obj = 0; obj < objectList.Count; obj++)
                {
                    List<List<float>> bpc = objectList[obj].Bpc;
                    List<List<float>> tpc = objectList[obj].Tpc;
                    int h = image.Height;
                    int w = image.Width;

                    if (deviceDetails.PanopticSegmentation.Equals("yes", StringComparison.InvariantCultureIgnoreCase))
                    {
                        for (int i = 0; i < tpc.Count; i++)
                        {
                            int x = (int)Math.Round(tpc[i][0] * w);
                            int y = (int)Math.Round(tpc[i][1] * h);
                            string pickColor = segmentColors[(obj + 1).ToString()];
                            Color objectColor = System.Drawing.Color.FromName(pickColor);
                            tpcColor = new Bgr(objectColor);
                            image[y, x] = tpcColor;
                        }
                    }
                    else
                    {
                        for (int i = 0; i < tpc.Count; i++)
                        {
                            int x = (int)Math.Round(tpc[i][0] * w);
                            int y = (int)Math.Round(tpc[i][1] * h);
                           
                            string pickColor = labelColors[objectList[obj].Lb];
                            Color objectColor = System.Drawing.Color.FromName(pickColor);
                            tpcColor = new Bgr(objectColor);
                            image[y, x] = tpcColor;
                        }
                    }
                    for (int i = 0; i < bpc.Count; i++)
                    {
                        int x = (int)Math.Round(bpc[i][0] * w);
                        int y = (int)Math.Round(bpc[i][1] * h);
                        bpcColor = image[y, x];
                        bpcColor.Red = 255;
                        image[y, x] = bpcColor;
                    }
                }
            }
            return image;
        }
    }
}
