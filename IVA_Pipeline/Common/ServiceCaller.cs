/*=============================================================================================================== *
 * Copyright 2025 Infosys Ltd.                                                                                    *
 * Use of this source code is governed by Apache License Version 2.0 that can be found in the LICENSE file or at  *
 * http://www.apache.org/licenses/                                                                                *
 * ===============================================================================================================*/
﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.Common
{
    public class ServiceCaller
    {
        private static readonly HttpClient _httpClient = new HttpClient();

        static long framecount = 0;
        
        public T GenerateApi<T, U>(string URL, HttpMethod method, U postData)
        {
            HttpClient client = new HttpClient();

            return (T)Convert.ChangeType("", typeof(T));
        }

        public static async Task<string> ApiCaller(dynamic reqMsg, string URL, string methodType)
        {
            string result = "";
            try
            {
                string input = JsonConvert.SerializeObject(reqMsg);
                LogHandler.LogDebug("Sending input to the api {0}, with payload: {1}", LogHandler.Layer.FrameProcessor, URL, input);
                if (methodType.ToUpper() != "GET")
                {
                    var content = new StringContent(input, encoding: Encoding.UTF8, "application/json");
                    HttpResponseMessage response = await _httpClient.PostAsync(URL, content);
                    response.EnsureSuccessStatusCode();
                    result = await response.Content.ReadAsStringAsync();
                }
                else
                {
                    HttpResponseMessage response = await _httpClient.GetAsync(URL);
                    response.EnsureSuccessStatusCode();
                    result = await response.Content.ReadAsStringAsync();
                }
                LogHandler.LogDebug("Result from the api: {0}", LogHandler.Layer.FrameProcessor, result);
                return result;
            }
            catch (HttpRequestException e)
            {
                throw e;
            }
            catch (Exception ex)
            {
                LogHandler.LogError("Error in APICaller, exception message:{0}\nStackTrace:{1}", LogHandler.Layer.Business, ex.Message, ex.StackTrace);
                throw ex;
            }      
            
        }

        public static string ServiceCall(dynamic reqMsg, string URL, string methodType)
        {
            string result = "";
            try
            {
            using (var client = new WebClient())   
            {
                client.Headers.Add("Content-Type:application/json"); 
                client.Headers.Add("Content-Type:text/plain");
                client.Headers.Add("Accept:application/json");
                client.Headers.Add("Accept:text/plain");
                    var input = JsonConvert.SerializeObject(reqMsg);
#if DEBUG
                    framecount++;
                    LogHandler.LogInfo("Request message for frame{0} is :\n{1}", LogHandler.Layer.Business, framecount, input);
#endif
                if (methodType.ToUpper() != "GET")
                    result = client.UploadString(URL, methodType, JsonConvert.SerializeObject(reqMsg));
                else
                    result = client.DownloadString(URL);  
#if DEBUG
                    LogHandler.LogInfo("Response message for frame{0} is :\n{1}", LogHandler.Layer.Business, framecount, result);
#endif
                }
            }
            catch (Exception ex)
            {
                LogHandler.LogError("Error in APICaller, exception message:{0}\nStackTrace:{1}", LogHandler.Layer.Business, ex.Message, ex.StackTrace);
            }
            return result;
        }

        public static string GetOAuth2TokenAsString(byte[] tokenObj)
        {
            var oAuth2TokenObj = System.Text.Encoding.Default.GetString(tokenObj);
            JObject authResponse = (JObject)JsonConvert.DeserializeObject(oAuth2TokenObj);
            string accessTok = ((JValue)authResponse["access_token"]).ToString();
            return accessTok;
        }

        public static byte[] GetOAuth2Token(AccessTokenSend tokenObj)
        {
            using (var webClient = new WebClient())
            {
                webClient.Headers.Add("Content-Type:application/x-www-form-urlencoded");
                NameValueCollection parameters = ConvertFromObjectToDictionary(tokenObj);
                var result = webClient.UploadValues(tokenObj.accessTokenURL, "POST", parameters);
                return result;
            }
        }

        public static NameValueCollection ConvertFromObjectToDictionary(object arg)
        {
            var json = JsonConvert.SerializeObject(arg);
            var dictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
            return dictionary.Aggregate(new NameValueCollection(),
                (seed, current) =>
                {
                    seed.Add(current.Key, current.Value);
                    return seed;
                });
        }
        public enum HttpMethod
        {
            Get,
            Post,
            Put,
            Delete
        }

        public static ApiResponse HttpClientApiCaller(dynamic reqMsg, string URL, string methodType)
        {
          

            HttpClient client = new HttpClient();
           
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            
            HttpContent content = null;
            if (reqMsg != null)
            {
                string resString = JsonConvert.SerializeObject(reqMsg);
                var buffer = System.Text.Encoding.UTF8.GetBytes(resString);
                content = new ByteArrayContent(buffer);
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
               
            }

            HttpResponseMessage response = new HttpResponseMessage();
            switch (methodType)
            {
                case "GET":
                    response = client.GetAsync(URL).Result;  
                    break;

                case "POST":
                    response = client.PostAsync(URL,content).Result;  
                    break;
                case "PUT":
                    response = client.PutAsync(URL, content).Result; 
                    break;
                case "DELETE":
                    response = client.DeleteAsync(URL).Result;  
                    break;

            }


            string responseString = string.Empty;
            
            if (response.IsSuccessStatusCode)
            {
                responseString = response.Content.ReadAsStringAsync().Result;
            }
            ApiResponse apiResponse = new ApiResponse();
            apiResponse.Response = responseString;
            apiResponse.StatusCode = (int)response.StatusCode;
            apiResponse.StatusMessage = response.ReasonPhrase;
            return apiResponse;


        }

        public static ApiResponse HttpClientApiCaller(dynamic reqMsg, string URL, string methodType, string accessMethod, string accessToken)
        {

            HttpResponseMessage response = PostToServiceAPI(reqMsg, URL, methodType, accessToken);
            string responseString = string.Empty;

            if (response.IsSuccessStatusCode)
            {
                 responseString = response.Content.ReadAsStringAsync().Result;
            }

            ApiResponse apiResponse = new ApiResponse();
            apiResponse.Response = responseString;
            apiResponse.StatusCode = (int)response.StatusCode;
            apiResponse.StatusMessage = response.ReasonPhrase;
            return apiResponse;

        }

        public static HttpResponseMessage PostToServiceAPI(dynamic reqMsg, string URL, string methodType, string accessToken)
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            HttpContent content = null;
            if (reqMsg != null)
            {
                string resString = JsonConvert.SerializeObject(reqMsg);
                var buffer = System.Text.Encoding.UTF8.GetBytes(resString);
                content = new ByteArrayContent(buffer);
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            }

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(accessToken);
            HttpResponseMessage response = new HttpResponseMessage();
            switch (methodType)
            {
                case "GET":
                    response = client.GetAsync(URL).Result;  
                    break;

                case "POST":
                    response = client.PostAsync(URL, content).Result;  
                    break;
                case "PUT":
                    response = client.PutAsync(URL, content).Result;  
                    break;
                case "DELETE":
                    response = client.DeleteAsync(URL).Result; 
                    break;

            }
            return response;
        }

        
    }
    public class ApiResponse
    {
        public int StatusCode { get; set; }
        public string StatusMessage { get; set; }
        public string Response { get; set; }

    }

    public class AccessTokenSend
    {
        public string accessTokenURL { get; set; }
        public string tokenName { get; set; }
        public string client_authentication { get; set; }
        public string client_id { get; set; }
        public string client_secret { get; set; }
        public string code { get; set; }
        public string grant_type { get; set; }
        public string redirect_url { get; set; }
    }
}
    
      
    

    




       





        
        




