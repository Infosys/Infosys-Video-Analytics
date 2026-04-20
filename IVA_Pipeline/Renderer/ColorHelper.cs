/*=============================================================================================================== *
 * Copyright 2025 Infosys Ltd.                                                                                    *
 * Use of this source code is governed by Apache License Version 2.0 that can be found in the LICENSE file or at  *
 * http://www.apache.org/licenses/                                                                                *
 * ===============================================================================================================*/
using OpenCvSharp;
using System;
using System.Collections.Generic;

namespace Infosys.Solutions.Ainauto.VideoAnalytics.Renderer
{
    /// <summary>
    /// Cross-platform replacement for System.Drawing.Color using OpenCvSharp Scalar (BGR format).
    /// </summary>
    public static class ColorHelper
    {
        private static readonly Dictionary<string, Scalar> _namedColors = new Dictionary<string, Scalar>(StringComparer.OrdinalIgnoreCase)
        {
            {"AliceBlue", new Scalar(255, 248, 240)},
            {"AntiqueWhite", new Scalar(215, 235, 250)},
            {"Aqua", new Scalar(255, 255, 0)},
            {"Aquamarine", new Scalar(212, 255, 127)},
            {"Azure", new Scalar(255, 255, 240)},
            {"Beige", new Scalar(220, 245, 245)},
            {"Bisque", new Scalar(196, 228, 255)},
            {"Black", new Scalar(0, 0, 0)},
            {"BlanchedAlmond", new Scalar(205, 235, 255)},
            {"Blue", new Scalar(255, 0, 0)},
            {"BlueViolet", new Scalar(226, 43, 138)},
            {"Brown", new Scalar(42, 42, 165)},
            {"BurlyWood", new Scalar(135, 184, 222)},
            {"CadetBlue", new Scalar(160, 158, 95)},
            {"Chartreuse", new Scalar(0, 255, 127)},
            {"Chocolate", new Scalar(30, 105, 210)},
            {"Coral", new Scalar(80, 127, 255)},
            {"CornflowerBlue", new Scalar(237, 149, 100)},
            {"Cornsilk", new Scalar(220, 248, 255)},
            {"Crimson", new Scalar(60, 20, 220)},
            {"Cyan", new Scalar(255, 255, 0)},
            {"DarkBlue", new Scalar(139, 0, 0)},
            {"DarkCyan", new Scalar(139, 139, 0)},
            {"DarkGoldenrod", new Scalar(11, 134, 184)},
            {"DarkGray", new Scalar(169, 169, 169)},
            {"DarkGreen", new Scalar(0, 100, 0)},
            {"DarkKhaki", new Scalar(107, 183, 189)},
            {"DarkMagenta", new Scalar(139, 0, 139)},
            {"DarkOliveGreen", new Scalar(47, 107, 85)},
            {"DarkOrange", new Scalar(0, 140, 255)},
            {"DarkOrchid", new Scalar(204, 50, 153)},
            {"DarkRed", new Scalar(0, 0, 139)},
            {"DarkSalmon", new Scalar(122, 150, 233)},
            {"DarkSeaGreen", new Scalar(143, 188, 143)},
            {"DarkSlateBlue", new Scalar(139, 61, 72)},
            {"DarkSlateGray", new Scalar(79, 79, 47)},
            {"DarkTurquoise", new Scalar(209, 206, 0)},
            {"DarkViolet", new Scalar(211, 0, 148)},
            {"DeepPink", new Scalar(147, 20, 255)},
            {"DeepSkyBlue", new Scalar(255, 191, 0)},
            {"DimGray", new Scalar(105, 105, 105)},
            {"DodgerBlue", new Scalar(255, 144, 30)},
            {"Firebrick", new Scalar(34, 34, 178)},
            {"FloralWhite", new Scalar(240, 250, 255)},
            {"ForestGreen", new Scalar(34, 139, 34)},
            {"Fuchsia", new Scalar(255, 0, 255)},
            {"Gainsboro", new Scalar(220, 220, 220)},
            {"GhostWhite", new Scalar(255, 248, 248)},
            {"Gold", new Scalar(0, 215, 255)},
            {"Goldenrod", new Scalar(32, 165, 218)},
            {"Gray", new Scalar(128, 128, 128)},
            {"Green", new Scalar(0, 128, 0)},
            {"GreenYellow", new Scalar(47, 255, 173)},
            {"Honeydew", new Scalar(240, 255, 240)},
            {"HotPink", new Scalar(180, 105, 255)},
            {"IndianRed", new Scalar(92, 92, 205)},
            {"Indigo", new Scalar(130, 0, 75)},
            {"Ivory", new Scalar(240, 255, 255)},
            {"Khaki", new Scalar(140, 230, 240)},
            {"Lavender", new Scalar(250, 230, 230)},
            {"LavenderBlush", new Scalar(245, 240, 255)},
            {"LawnGreen", new Scalar(0, 252, 124)},
            {"LemonChiffon", new Scalar(205, 250, 255)},
            {"LightBlue", new Scalar(230, 216, 173)},
            {"LightCoral", new Scalar(128, 128, 240)},
            {"LightCyan", new Scalar(255, 255, 224)},
            {"LightGoldenrodYellow", new Scalar(210, 250, 250)},
            {"LightGray", new Scalar(211, 211, 211)},
            {"LightGreen", new Scalar(144, 238, 144)},
            {"LightPink", new Scalar(193, 182, 255)},
            {"LightSalmon", new Scalar(122, 160, 255)},
            {"LightSeaGreen", new Scalar(170, 178, 32)},
            {"LightSkyBlue", new Scalar(250, 206, 135)},
            {"LightSlateGray", new Scalar(153, 136, 119)},
            {"LightSteelBlue", new Scalar(222, 196, 176)},
            {"LightYellow", new Scalar(224, 255, 255)},
            {"Lime", new Scalar(0, 255, 0)},
            {"LimeGreen", new Scalar(50, 205, 50)},
            {"Linen", new Scalar(230, 240, 250)},
            {"Magenta", new Scalar(255, 0, 255)},
            {"Maroon", new Scalar(0, 0, 128)},
            {"MediumAquamarine", new Scalar(170, 205, 102)},
            {"MediumBlue", new Scalar(205, 0, 0)},
            {"MediumOrchid", new Scalar(211, 85, 186)},
            {"MediumPurple", new Scalar(219, 112, 147)},
            {"MediumSeaGreen", new Scalar(113, 179, 60)},
            {"MediumSlateBlue", new Scalar(238, 104, 123)},
            {"MediumSpringGreen", new Scalar(154, 250, 0)},
            {"MediumTurquoise", new Scalar(204, 209, 72)},
            {"MediumVioletRed", new Scalar(133, 21, 199)},
            {"MidnightBlue", new Scalar(112, 25, 25)},
            {"MintCream", new Scalar(250, 255, 245)},
            {"MistyRose", new Scalar(225, 228, 255)},
            {"Moccasin", new Scalar(181, 228, 255)},
            {"NavajoWhite", new Scalar(173, 222, 255)},
            {"Navy", new Scalar(128, 0, 0)},
            {"OldLace", new Scalar(230, 245, 253)},
            {"Olive", new Scalar(0, 128, 128)},
            {"OliveDrab", new Scalar(35, 142, 107)},
            {"Orange", new Scalar(0, 165, 255)},
            {"OrangeRed", new Scalar(0, 69, 255)},
            {"Orchid", new Scalar(214, 112, 218)},
            {"PaleGoldenrod", new Scalar(170, 232, 238)},
            {"PaleGreen", new Scalar(152, 251, 152)},
            {"PaleTurquoise", new Scalar(238, 238, 175)},
            {"PaleVioletRed", new Scalar(147, 112, 219)},
            {"PapayaWhip", new Scalar(213, 239, 255)},
            {"PeachPuff", new Scalar(185, 218, 255)},
            {"Peru", new Scalar(63, 133, 205)},
            {"Pink", new Scalar(203, 192, 255)},
            {"Plum", new Scalar(221, 160, 221)},
            {"PowderBlue", new Scalar(230, 224, 176)},
            {"Purple", new Scalar(128, 0, 128)},
            {"Red", new Scalar(0, 0, 255)},
            {"RosyBrown", new Scalar(143, 143, 188)},
            {"RoyalBlue", new Scalar(225, 105, 65)},
            {"SaddleBrown", new Scalar(19, 69, 139)},
            {"Salmon", new Scalar(114, 128, 250)},
            {"SandyBrown", new Scalar(96, 164, 244)},
            {"SeaGreen", new Scalar(87, 139, 46)},
            {"SeaShell", new Scalar(238, 245, 255)},
            {"Sienna", new Scalar(45, 82, 160)},
            {"Silver", new Scalar(192, 192, 192)},
            {"SkyBlue", new Scalar(235, 206, 135)},
            {"SlateBlue", new Scalar(205, 90, 106)},
            {"SlateGray", new Scalar(144, 128, 112)},
            {"Snow", new Scalar(250, 250, 255)},
            {"SpringGreen", new Scalar(127, 255, 0)},
            {"SteelBlue", new Scalar(180, 130, 70)},
            {"Tan", new Scalar(140, 180, 210)},
            {"Teal", new Scalar(128, 128, 0)},
            {"Thistle", new Scalar(216, 191, 216)},
            {"Tomato", new Scalar(71, 99, 255)},
            {"Transparent", new Scalar(0, 0, 0, 0)},
            {"Turquoise", new Scalar(208, 224, 64)},
            {"Violet", new Scalar(238, 130, 238)},
            {"Wheat", new Scalar(179, 222, 245)},
            {"White", new Scalar(255, 255, 255)},
            {"WhiteSmoke", new Scalar(245, 245, 245)},
            {"Yellow", new Scalar(0, 255, 255)},
            {"YellowGreen", new Scalar(50, 205, 154)},
            // System colors
            {"ActiveBorder", new Scalar(212, 208, 180)},
            {"ActiveCaption", new Scalar(212, 175, 153)},
            {"AppWorkspace", new Scalar(171, 171, 171)},
            {"ButtonFace", new Scalar(240, 240, 240)},
            {"ButtonHighlight", new Scalar(255, 255, 255)},
            {"ButtonShadow", new Scalar(160, 160, 160)},
            {"Control", new Scalar(240, 240, 240)},
            {"ControlDark", new Scalar(160, 160, 160)},
            {"ControlDarkDark", new Scalar(105, 105, 105)},
            {"ControlLight", new Scalar(227, 227, 227)},
            {"ControlLightLight", new Scalar(255, 255, 255)},
            {"ControlText", new Scalar(0, 0, 0)},
            {"Desktop", new Scalar(0, 0, 0)},
            {"GradientActiveCaption", new Scalar(234, 209, 185)},
            {"GradientInactiveCaption", new Scalar(241, 226, 215)},
            {"GrayText", new Scalar(109, 109, 109)},
            {"Highlight", new Scalar(208, 163, 51)},
            {"HighlightText", new Scalar(255, 255, 255)},
            {"HotTrack", new Scalar(204, 102, 0)},
            {"InactiveBorder", new Scalar(252, 247, 244)},
            {"InactiveCaption", new Scalar(219, 205, 191)},
            {"InactiveCaptionText", new Scalar(0, 0, 0)},
            {"Info", new Scalar(225, 255, 255)},
            {"InfoText", new Scalar(0, 0, 0)},
            {"Menu", new Scalar(240, 240, 240)},
            {"MenuBar", new Scalar(240, 240, 240)},
            {"MenuHighlight", new Scalar(208, 163, 51)},
            {"MenuText", new Scalar(0, 0, 0)},
            {"ScrollBar", new Scalar(200, 200, 200)},
            {"Window", new Scalar(255, 255, 255)},
            {"WindowFrame", new Scalar(100, 100, 100)},
            {"WindowText", new Scalar(0, 0, 0)},
        };

        /// <summary>
        /// Returns a BGR Scalar from a named color (same behavior as System.Drawing.Color.FromName).
        /// </summary>
        public static Scalar ColorNameToScalar(string colorName)
        {
            if (_namedColors.TryGetValue(colorName, out var scalar))
                return scalar;
            return new Scalar(0, 0, 0); // Default to black
        }

        /// <summary>
        /// Returns all known color names (replacement for KnownColor enum iteration).
        /// </summary>
        public static Dictionary<int, string> GetAllKnownColorNames()
        {
            var result = new Dictionary<int, string>();
            int index = 0;
            foreach (var kvp in _namedColors)
            {
                result.Add(index, kvp.Key);
                index++;
            }
            return result;
        }
    }
}
