/*=============================================================================================================== *
 * Copyright 2024 Infosys Ltd.                                                                                    *
 * Use of this source code is governed by Apache License Version 2.0 that can be found in the LICENSE file or at  *
 * http://www.apache.org/licenses/                                                                                *
 * ===============================================================================================================*/

using Newtonsoft.Json;
using System;
using System.Runtime.Serialization.Json;
using System.Text;

namespace Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.Common
{
    public static class Utility
    {
        

       
        

        
        public static string SerialiseToJSON(this object objectToSerialize)
        {
            string serializedString = string.Empty;
            try
            {
                using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
                {
                    DataContractJsonSerializer serializer =
                        new DataContractJsonSerializer(objectToSerialize.GetType());
                    serializer.WriteObject(ms, objectToSerialize);
                    ms.Position = 0;

                    using (System.IO.StreamReader reader = new System.IO.StreamReader(ms))
                    {
                        serializedString = reader.ReadToEnd();
                    }
                }
               

            }
            catch (Exception  ex )
            {
                LogHandler.LogError("Exception in SerialiseToJSON while serialising obj : {0} , message : {1} , stack trace :{2}," +
                    "innerexception : {3}", LogHandler.Layer.Business , JsonConvert.SerializeObject(objectToSerialize),
                    ex.Message,ex.StackTrace,ex.InnerException);
            }
            return serializedString;
        }

        

        
       

        public static T DeserializeFromJSON<T>(this string serializedString)
        {
            
            if (string.IsNullOrWhiteSpace(serializedString))
            {
                return default(T);
            }

            try
            {
                
                using (System.IO.MemoryStream ms = new System.IO.MemoryStream(Encoding.Unicode.GetBytes(serializedString)))
                {
                    DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(T));
                    return (T)serializer.ReadObject(ms);
                }
                
            }
            catch (Exception)
            {
                
                return default(T);
            }
        }
    }
}

