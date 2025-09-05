/*=============================================================================================================== *
 * Copyright 2025 Infosys Ltd.                                                                                    *
 * Use of this source code is governed by Apache License Version 2.0 that can be found in the LICENSE file or at  *
 * http://www.apache.org/licenses/                                                                                *
 * ===============================================================================================================*/
﻿

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.Common
{
    public class ValidationUtility
    {
        public static bool InvalidCharacterValidator(string str)
        {
            string specialCharacters = @"%!@#$%^&*()?/><,:;'\|}]{[~`+=" + "\"";           
            char[] specialCharactersArray = specialCharacters.ToCharArray();
            int index = str.IndexOfAny(specialCharactersArray);
            
            if (index == -1)
                return false;
            else
                return true;
        }

        public static bool InvalidCharValidatorForFile(string fileName)  
        {
            if (fileName.IndexOfAny(System.IO.Path.GetInvalidFileNameChars()) > -1)
                return true;
            else
                return false;
        }

        public static bool InvalidCharValidatorForFolderPath(string path)  
        {
            if (path.IndexOfAny(System.IO.Path.GetInvalidPathChars()) > -1)
                return true;
            else
                return false;
        }

        public static bool InvalidCharValidatorForUserAlias(string alias)
        {
            if (System.Text.RegularExpressions.Regex.IsMatch(alias, "^[a-zA-Z0-9_.]+$"))
                return false;
            else
                return true;
        }        
    }
}
