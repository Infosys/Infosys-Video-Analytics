/*=============================================================================================================== *
 * Copyright 2025 Infosys Ltd.                                                                                    *
 * Use of this source code is governed by Apache License Version 2.0 that can be found in the LICENSE file or at  *
 * http://www.apache.org/licenses/                                                                                *
 * ===============================================================================================================*/
﻿
using Infosys.IVA.ComputerVisionLib.OCREngine;
using System;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Xml.Linq;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
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

        

        public string ReadTextArea(Image image, string filter, float imageResizeCoeff)
        {


            



           
            string text = String.Empty;
            

           
            if (imageResizeCoeff  >1 || imageResizeCoeff<-1)
                {
                  int incrWidth;
                  int incrHeight;
                  if(imageResizeCoeff > 1)
                  { 
                    incrWidth = (int)(image.Width * imageResizeCoeff);
                    incrHeight = (int)(image.Height * imageResizeCoeff);
                   }
                  else
                  {
                      incrWidth = (int)(image.Width / imageResizeCoeff);
                      incrHeight = (int)(image.Height / imageResizeCoeff);
                  }

                  image = ResizeImage(image, incrWidth, incrHeight);
                  
                }

            System.IO.Stream stream = Tesseract.ImageToTxt((Bitmap)image, languages: new[] { Language.English});
            using (var reader = new StreamReader(stream, Encoding.UTF8))
            {
                text = reader.ReadToEnd();
            }

             
            

            return text.Trim();
        }

       
    public static Bitmap ResizeImage(Image image, int width, int height)
    {
        var destRect = new Rectangle(0, 0, width, height);
        var destImage = new Bitmap(width, height);

        destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

        using (var graphics = Graphics.FromImage(destImage))
        {
            graphics.CompositingMode = CompositingMode.SourceCopy;
            graphics.CompositingQuality = CompositingQuality.HighQuality;
            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            graphics.SmoothingMode = SmoothingMode.HighQuality;
            graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

            using (var wrapMode = new ImageAttributes())
            {
                wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                graphics.DrawImage(image, destRect, 0, 0, image.Width,image.Height, GraphicsUnit.Pixel, wrapMode);
            }
        }

        return destImage;
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
