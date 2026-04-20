/*=============================================================================================================== *
 * Copyright 2025 Infosys Ltd.                                                                                    *
 * Use of this source code is governed by Apache License Version 2.0 that can be found in the LICENSE file or at  *
 * http://www.apache.org/licenses/                                                                                *
 * ===============================================================================================================*/
﻿
using Infosys.IVA.ComputerVisionLib.OCREngine;
using OpenCvSharp;
using System;
using System.Configuration;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Xml.Linq;
using TesseractSharp;
using System.Text;

namespace Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.ComputerVisionLib.TessaractWrapper
{
    public class TessaractWrapper : ITextRecognition
    {
        
        public string ReadTextArea(double x, double y, double height, double width, string filter, float imageResizeCoeff)
        {
            
            throw new NotImplementedException();
        }

        

        public string ReadTextArea(Mat image, string filter, float imageResizeCoeff)
        {
            string text = String.Empty;

            Mat processImage = image;
            if (imageResizeCoeff > 1 || imageResizeCoeff < -1)
            {
                int incrWidth;
                int incrHeight;
                if (imageResizeCoeff > 1)
                {
                    incrWidth = (int)(image.Width * imageResizeCoeff);
                    incrHeight = (int)(image.Height * imageResizeCoeff);
                }
                else
                {
                    incrWidth = (int)(image.Width / imageResizeCoeff);
                    incrHeight = (int)(image.Height / imageResizeCoeff);
                }

                processImage = new Mat();
                Cv2.Resize(image, processImage, new OpenCvSharp.Size(incrWidth, incrHeight), 0, 0, InterpolationFlags.Cubic);
            }

            // Save to temp file for cross-platform Tesseract processing
            string tempFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".png");
            try
            {
                Cv2.ImWrite(tempFile, processImage);
                System.IO.Stream stream = Tesseract.ImageToTxt(tempFile, languages: new[] { Language.English });
                using (var reader = new StreamReader(stream, Encoding.UTF8))
                {
                    text = reader.ReadToEnd();
                }
            }
            finally
            {
                if (File.Exists(tempFile))
                    File.Delete(tempFile);
                if (processImage != image)
                    processImage?.Dispose();
            }

            return text.Trim();
        }
        

        private bool CheckSaveOCRImages()
        {
            bool save = false;
                     
            string directory = Directory.GetCurrentDirectory();
            string settingsFilePath = directory + @"\Infosys.ATR.WinUIAutomationRuntimeWrapper.xml";
            if (File.Exists(settingsFilePath))
            {
                XElement settings = XElement.Load(settingsFilePath);
                if (settings != null)
                {
                    IEnumerable<XElement> elements = settings.Elements().Where(e => e.Name.LocalName == "SaveOCRImages");
                    if (elements.Count() != 0)
                    {
                        string val = settings.Elements().Where(e => e.Name.LocalName == "SaveOCRImages").Single().Value;
                        if (val.ToLower() == "true")
                            save = true;
                    }
                }
            }
            return save;
        }
        
    }
}
