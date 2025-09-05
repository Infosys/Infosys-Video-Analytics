/*=============================================================================================================== *
 * Copyright 2025 Infosys Ltd.                                                                                    *
 * Use of this source code is governed by Apache License Version 2.0 that can be found in the LICENSE file or at  *
 * http://www.apache.org/licenses/                                                                                *
 * ===============================================================================================================*/
using OpenCvSharp;
using Infosys.Solutions.Ainauto.VideoAnalytics.BusinessEntity;
using Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.Common;
using Infosys.Solutions.Ainauto.VideoAnalytics.Resource.Entity.Queue;
using System.Drawing.Imaging;
using System.Drawing;
using SE = Infosys.Solutions.Ainauto.VideoAnalytics.Services.MaskDetector.Contracts;
using Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.PythonLoader;
using Infosys.Solutions.Ainauto.VideoAnalytics.BusinessComponent;

namespace Infosys.Solutions.Ainauto.VideoAnalytics.Renderer
{
    public class HeatMap : IRender
    {
        public Mat RenderFrame(List<Predictions> objectList, Mat image, int frameWidth, int frameHeight, string modelName, string info, DeviceDetails deviceDetails, string Ad, FrameDetails frameDetails)
        {
            Scalar color = new Scalar();
            if (deviceDetails.CrowdCounting.Equals("yes", StringComparison.InvariantCultureIgnoreCase))
            {
                try
                {
                    Predictions face = objectList[0];
#if DEBUG
                    LogHandler.LogInfo(String.Format("v0:{0} v1:{1} v2:{2}", color.Val0,
                        color.Val1, color.Val2), LogHandler.Layer.Business, null);
#endif
                    var x = new int[face.Tpc.Count];
                    var y = new int[face.Tpc.Count];
                    for (var z = 0; z < face.Tpc.Count; z++)
                    {
                        var point1 = Convert.ToInt32(face.Tpc[z][0] * image.Width);
                        var point2 = Convert.ToInt32(face.Tpc[z][1] * image.Height);
                        x[z] = point1;
                        y[z] = point2;
                    }
                    string base64_image = "";

                    Cv2.ImEncode(".jpeg", image, out byte[] imageByteData);
                    base64_image = Convert.ToBase64String(imageByteData);
                    
                    SE.Message.CrowdCounting reqMsg = new SE.Message.CrowdCounting()
                    {
                        x = x,
                        y = y,
                        Base_64 = base64_image
                    };
                    PythonNet pNet = new PythonNet();
                    pNet = PythonNet.GetInstance;
                    var val = "";
                   
                    val = "";

                    string base64_return_image = "";
                    base64_return_image = val.ToString();
                    byte[] bytes = Convert.FromBase64String(base64_return_image);

                    System.IO.Stream imageStream = new MemoryStream();
                    using (MemoryStream ms = new MemoryStream())
                    {
                        imageStream.CopyTo(ms);
                        byte[] imageBytes = bytes.ToArray();
                        FrameGrabberHelper.CompressImage(imageBytes, ms, 100).CopyTo(ms);
                        FrameGrabberHelper.UploadToBlob(ms, frameDetails.FrameId + ApplicationConstants.FileExtensions.jpg);
                        ms.Dispose();
                    }

                }
                catch (Exception ex)
                {
                    LogHandler.LogError("Error in HeatMap.RenderFrame, exception: {0}, inner exception: {1}, stack trace: {2}",
                        LogHandler.Layer.FrameRenderer, ex.Message, ex.InnerException, ex.StackTrace);
                    throw ex;
                }
            }
            return image;
        }
    }
}
