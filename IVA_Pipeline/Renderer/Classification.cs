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
        /// <summary>
        /// Tracks the last rendered label text so consecutive similar labels
        /// can be deduplicated. Managed explicitly by the renderer — not shared
        /// with the TTS SemanticSimilarity instance.
        /// </summary>
        private static string? _previousRenderedText;
        private static readonly object _prevLock = new object();

        // ── Rendering constants ──
        private const int TextPadX = 6;
        private const int TextPadY = 4;
        private const double TextBgOpacity = 0.7;
        private const int MaxLines = 3;
        private const double MaxWidthRatio = 0.95;

        public Mat RenderFrame(List<Predictions> objectList, Mat image, int frameWidth, int frameHeight, string modelName, string info, DeviceDetails deviceDetails, string Ad, FrameDetails frameDetails)
        {
            if (deviceDetails.ClassificationRendering.Equals("yes", StringComparison.InvariantCultureIgnoreCase))
            {
                try
                {
                    if (objectList != null)
                    {
                        // ── Colors computed once ──
                        Color clFontColor = Color.FromName(deviceDetails.LabelFontColor);
                        Scalar fontColor = new Scalar(clFontColor.B, clFontColor.G, clFontColor.R);
                        Color clLabelBg = Color.FromName(deviceDetails.RendererPredictCartListBackgroundColor);
                        Scalar labelBgColor = new Scalar(clLabelBg.B, clLabelBg.G, clLabelBg.R);
                        double fontScale = deviceDetails.RendererFontScale;
                        int fontThickness = deviceDetails.RendererFontThickness;
                        int maxTextWidth = (int)(image.Width * MaxWidthRatio) - deviceDetails.RendererLabelPointX;

                        int x = deviceDetails.RendererLabelPointX;
                        int y = deviceDetails.RendererLabelPointY;
                        int lineCount = 0;
                        bool truncated = false;

                        for (int i = 0; i < objectList.Count && !truncated; i++)
                        {
                            // ── Similarity-based label selection when TTS is enabled ──
                            string labelToRender = objectList[i].Lb ?? "";

                            // ── Append confidence to the label text ──
                            if (!string.IsNullOrEmpty(objectList[i].Cs)
                                && double.TryParse(objectList[i].Cs, out double cs))
                            {
                                labelToRender = string.Concat(labelToRender, " , ", Math.Round(cs, 2).ToString());
                            }

                            // ── Word-wrap with 3-line max + truncation ──
                            string[] words = labelToRender.Split(' ');
                            string currentLine = "";

                            foreach (string word in words)
                            {
                                if (truncated) break;

                                string candidate = string.IsNullOrEmpty(currentLine) ? word : currentLine + " " + word;
                                var sz = Cv2.GetTextSize(candidate, HersheyFonts.HersheySimplex, fontScale, fontThickness, out _);

                                if (sz.Width <= maxTextWidth || string.IsNullOrEmpty(currentLine))
                                {
                                    currentLine = candidate;
                                }
                                else
                                {
                                    // Emit the completed line
                                    lineCount++;
                                    if (lineCount >= MaxLines)
                                    {
                                        currentLine += " ...";
                                        var szT = Cv2.GetTextSize(currentLine, HersheyFonts.HersheySimplex, fontScale, fontThickness, out _);
                                        y += szT.Height + TextPadY;
                                        DrawTextWithBackground(image, currentLine, x, y, fontScale, fontThickness, fontColor, labelBgColor);
                                        truncated = true;
                                        break;
                                    }
                                    var szL = Cv2.GetTextSize(currentLine, HersheyFonts.HersheySimplex, fontScale, fontThickness, out _);
                                    y += szL.Height + TextPadY;
                                    DrawTextWithBackground(image, currentLine, x, y, fontScale, fontThickness, fontColor, labelBgColor);
                                    currentLine = word;
                                }
                            }

                            // Emit remaining text on the last partial line
                            if (!truncated && !string.IsNullOrEmpty(currentLine))
                            {
                                lineCount++;
                                if (lineCount >= MaxLines)
                                {
                                    currentLine += " ...";
                                    truncated = true;
                                }
                                var szR = Cv2.GetTextSize(currentLine, HersheyFonts.HersheySimplex, fontScale, fontThickness, out _);
                                y += szR.Height + TextPadY;
                                DrawTextWithBackground(image, currentLine, x, y, fontScale, fontThickness, fontColor, labelBgColor);
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
            if (deviceDetails.SpeedDetection != null
                && deviceDetails.SpeedDetection.Equals("yes", StringComparison.InvariantCultureIgnoreCase)
                && objectList != null && objectList.Count > 0)
            {
                Scalar lbColor = new Scalar(255, 255, 255);
                DrawTextWithBackground(image, objectList[0].Lb + "mph",
                    10, 30, 1, 3, lbColor, new Scalar(50, 50, 50));
            }
            return image;
        }

        /// <summary>
        /// Draws text with a semi-transparent background rectangle for readability.
        /// Uses Cv2.AddWeighted on the ROI subMat for an efficient alpha blend.
        /// </summary>
        private static void DrawTextWithBackground(
            Mat image, string text, int x, int y,
            double fontScale, int thickness,
            Scalar textColor, Scalar bgColor)
        {
            var textSize = Cv2.GetTextSize(text, HersheyFonts.HersheySimplex,
                                           fontScale, thickness, out int baseline);

            int bgX1 = Math.Max(0, x - TextPadX);
            int bgY1 = Math.Max(0, y - textSize.Height - TextPadY);
            int bgX2 = Math.Min(image.Width - 1, x + textSize.Width + TextPadX);
            int bgY2 = Math.Min(image.Height - 1, y + baseline + TextPadY);

            if (bgX2 > bgX1 && bgY2 > bgY1)
            {
                var roi = new Rect(bgX1, bgY1, bgX2 - bgX1, bgY2 - bgY1);
                using var subMat = new Mat(image, roi);
                using var overlay = new Mat(subMat.Size(), subMat.Type(), bgColor);
                Cv2.AddWeighted(subMat, 1.0 - TextBgOpacity, overlay, TextBgOpacity, 0, subMat);
            }

            Cv2.PutText(image, text, new OpenCvSharp.Point(x, y),
                        HersheyFonts.HersheySimplex, fontScale, textColor, thickness);
        }
    }
}
