/*=============================================================================================================== *
 * Copyright 2025 Infosys Ltd.                                                                                    *
 * Use of this source code is governed by Apache License Version 2.0 that can be found in the LICENSE file or at  *
 * http://www.apache.org/licenses/                                                                                *
 * ===============================================================================================================*/
﻿using System;
using System.Collections;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Policy;
using System.Threading.Tasks;

using Infosys.Lif.LegacyCommon;
using Infosys.Lif.LegacyIntegratorService;

namespace Infosys.Lif
{
    public class IIS_DocAdapter : IAdapter
    {
        private const string REGION = "Region";
        private const string TRANSPORT_SECTION = "TransportSection";
        private const string DATA = "Data";
        private const string TARGETURLDETAILS = "TargetURLDetails";
        private const string SUCCESSFUL_DATA_SENT = "Document successfully uploaded.";
        private const string SUCCESSFUL_DATA_RECEIVED = "Document successfully downloaded.";
        private const string UNSUCCESSFUL_DATA_SENT = "Document couldn't be uploaded successfully. ";
        private const string UNSUCCESSFUL_DATA_RECEIVED = "Document couldn't be downloaded successfully. ";
        private const int SUCCESSFUL_STATUS_CODE = 0;
        private const int UNSUCCESSFUL_STATUS_CODE = 1000;

        private Stream dataStream = null;
        private NameValueCollection targetURLDetails;

        #region IAdapter Members

        public event ReceiveHandler Received;

        public string Send(System.Collections.Specialized.ListDictionary adapterDetails, string message)
        {
            string response = SUCCESSFUL_DATA_SENT;
            IISDoc transportSection = null;
            Region regionToBeUsed = null;
            try
            {
                LifLogHandler.LogDebug("IIS_Doc Adapter- Send called", LifLogHandler.Layer.IntegrationLayer);
                foreach (DictionaryEntry items in adapterDetails)
                {
                    if (items.Key.ToString() == REGION)
                    {
                        regionToBeUsed = items.Value as Region;
                    }
                    else if (items.Key.ToString() == TRANSPORT_SECTION)
                    {
                        transportSection = items.Value as IISDoc;
                    }
                    else if (items.Key.ToString() == DATA)
                    {
                        dataStream = items.Value as Stream;
                    }
                    else if (items.Key.ToString() == TARGETURLDETAILS)
                    {
                        targetURLDetails = items.Value as NameValueCollection;
                    }
                }

                if (dataStream == null && targetURLDetails == null)
                    throw new LegacyException("IIS_Doc Adapter- File Stream and metadata details both cannot be empty.");

                bool check = targetURLDetails.AllKeys.Contains("UriScheme")
                            && !string.IsNullOrWhiteSpace(targetURLDetails["UriScheme"]);
                if (!check)
                    throw new LegacyException("IIS_Doc Adapter- 'UriScheme' missing in metadata.");

                check = targetURLDetails.AllKeys.Contains("RootDNS")
                                && !string.IsNullOrWhiteSpace(targetURLDetails["RootDNS"]);
                if (!check)
                    throw new LegacyException("IIS_Doc Adapter- 'RootDNS' missing in metadata.");

                check = targetURLDetails.AllKeys.Contains("container_name")
                                && !string.IsNullOrWhiteSpace(targetURLDetails["container_name"]);
                if (!check)
                    throw new LegacyException("IIS_Doc Adapter- 'container_name' missing in metadata.");

                check = targetURLDetails.AllKeys.Contains("file_name")
                                && !string.IsNullOrWhiteSpace(targetURLDetails["file_name"]);
                if (!check)
                    throw new LegacyException("IIS_Doc Adapter- 'file_name' missing in metadata.");

                IISDocDetails docDetails = ValidateTransportName(transportSection, regionToBeUsed.TransportName);

                response = HandleStream(docDetails, DocAccessType.Send);
            }
            catch (LegacyException exception)
            {
                LifLogHandler.LogError("IIS_Doc Adapter- Send- exception raised, reason- " + exception.Message,
                    LifLogHandler.Layer.IntegrationLayer);
                throw exception;
            }
            catch (Exception exception)
            {
                LifLogHandler.LogError("IIS_Doc Adapter- Send- exception raised, reason- " + exception.Message,
                    LifLogHandler.Layer.IntegrationLayer);
                throw exception;
            }
            return response;
        }

        public void Receive(ListDictionary adapterDetails)
        {
            IISDoc transportSection = null;
            Region regionToBeUsed = null;
            try
            {
                LifLogHandler.LogDebug("IIS_Doc Adapter- Receive called", LifLogHandler.Layer.IntegrationLayer);
                foreach (DictionaryEntry items in adapterDetails)
                {
                    if (items.Key.ToString() == REGION)
                    {
                        regionToBeUsed = items.Value as Region;
                    }
                    else if (items.Key.ToString() == TRANSPORT_SECTION)
                    {
                        transportSection = items.Value as IISDoc;
                    }
                    else if (items.Key.ToString() == TARGETURLDETAILS)
                    {
                        targetURLDetails = items.Value as NameValueCollection;
                    }
                }

                if (targetURLDetails == null)
                    throw new LegacyException("IIS_Doc Adapter- Metadata details are not provided.");

                bool check = targetURLDetails.AllKeys.Contains("UriScheme")
                                && !string.IsNullOrWhiteSpace(targetURLDetails["UriScheme"]);
                if (!check)
                    throw new LegacyException("IIS_Doc Adapter- 'UriScheme' missing in metadata.");

                check = targetURLDetails.AllKeys.Contains("RootDNS")
                                && !string.IsNullOrWhiteSpace(targetURLDetails["RootDNS"]);
                if (!check)
                    throw new LegacyException("IIS_Doc Adapter- 'RootDNS' missing in metadata.");

                check = targetURLDetails.AllKeys.Contains("container_name")
                                && !string.IsNullOrWhiteSpace(targetURLDetails["container_name"]);
                if (!check)
                    throw new LegacyException("IIS_Doc Adapter- 'container_name' missing in metadata.");

                check = targetURLDetails.AllKeys.Contains("file_name")
                                && !string.IsNullOrWhiteSpace(targetURLDetails["file_name"]);
                if (!check)
                    throw new LegacyException("IIS_Doc Adapter- 'file_name' missing in metadata.");

                IISDocDetails docDetails = ValidateTransportName(transportSection, regionToBeUsed.TransportName);

                HandleStream(docDetails, DocAccessType.Receive);

            }
            catch (LegacyException exception)
            {
                LifLogHandler.LogError("IIS_Doc Adapter- Receive- exception raised, reason- " + exception.Message,
                    LifLogHandler.Layer.IntegrationLayer);
                throw exception;
            }
            catch (Exception exception)
            {
                LifLogHandler.LogError("IIS_Doc Adapter- Receive- exception raised, reason- " + exception.Message,
                    LifLogHandler.Layer.IntegrationLayer);
                throw exception;
            }
        }

        public bool Delete(ListDictionary messageDetails)
        {
            Infosys.Lif.LegacyIntegratorService.IISDoc transport = null;
            Infosys.Lif.LegacyIntegratorService.Region region = null;
            foreach (DictionaryEntry items in messageDetails)
            {
                if (items.Key.ToString() == REGION)
                {
                    region = items.Value as Region;
                }
                else if (items.Key.ToString() == TRANSPORT_SECTION)
                {
                    transport = items.Value as Infosys.Lif.LegacyIntegratorService.IISDoc;
                }
                else if (items.Key.ToString() == TARGETURLDETAILS)
                {
                    targetURLDetails = items.Value as NameValueCollection;
                }
            }
            if (region == null
                || transport == null
                || targetURLDetails == null)
            {
                LifLogHandler.LogError(
                    "IIS_Doc Adapter- Delete- One of Region, Transport section, Target details is not passed!",
                    LifLogHandler.Layer.IntegrationLayer);

                throw new ArgumentException(string.Format("{0}, {1}, {2} are required for delete operation!",
                    REGION, TRANSPORT_SECTION, TARGETURLDETAILS));
            }

            bool check = targetURLDetails.AllKeys.Contains("UriScheme")
                        && !string.IsNullOrWhiteSpace(targetURLDetails["UriScheme"]);
            if (!check)
                throw new LegacyException("IIS_Doc Adapter- 'UriScheme' missing in metadata.");
            check = targetURLDetails.AllKeys.Contains("RootDNS")
                            && !string.IsNullOrWhiteSpace(targetURLDetails["RootDNS"]);
            if (!check)
                throw new LegacyException("IIS_Doc Adapter- 'RootDNS' missing in metadata.");

            check = targetURLDetails.AllKeys.Contains("container_name")
                            && !string.IsNullOrWhiteSpace(targetURLDetails["container_name"]);
            if (!check)
                throw new LegacyException("IIS_Doc Adapter- 'container_name' missing in metadata.");

            check = targetURLDetails.AllKeys.Contains("company_id")
                            && !string.IsNullOrWhiteSpace(targetURLDetails["company_id"]);
            if (!check)
                throw new LegacyException("IIS_Doc Adapter- 'company_id' missing in metadata.");


            IISDocDetails docDetails = ValidateTransportName(transport, region.TransportName);

            if (docDetails == null)
                throw new LegacyException(
                    string.Format("IIS_Doc Adapter- Could not find transport by name: {0}", region.TransportName));

            var response = HandleStream(docDetails, DocAccessType.Delete, targetURLDetails);



            LifLogHandler.LogDebug("Response for delete: {0}", LifLogHandler.Layer.IntegrationLayer, response);


            return string.IsNullOrWhiteSpace(response);

        }

        #endregion

        private string HandleStream(
            IISDocDetails docDetails, DocAccessType accessType, NameValueCollection targetDetails = null)
        {
            LifLogHandler.LogDebug("IIS_Doc Adapter- HandleStream called for access type- " + accessType.ToString(),
                LifLogHandler.Layer.IntegrationLayer);
            string response = "";
            try
            {
                if (docDetails != null)
                {
                    switch (accessType)
                    {
                        case DocAccessType.Send:
                            response = SendDocument(docDetails, response);
                            break;
                        case DocAccessType.Receive:
                            response = ReceiveDocument(docDetails, response);
                            break;
                        case DocAccessType.Delete:
                            response = DeleteDocument(docDetails, targetDetails);
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                if (accessType == DocAccessType.Send)
                    response = UNSUCCESSFUL_DATA_SENT + ex.Message;
                else if (accessType == DocAccessType.Receive)
                    response = UNSUCCESSFUL_DATA_RECEIVED + ex.Message;
                else
                    response = ex.Message;

                if (ex.InnerException != null)
                {
                    response = response + ". Inner Error Message- " + ex.InnerException.Message;
                }

                LifLogHandler.LogError("IIS_Doc Adapter- HandleStream- exception raised, reason- {0}",
                    LifLogHandler.Layer.IntegrationLayer, ex);
                ReceiveEventArgs args = new ReceiveEventArgs();
                args.ResponseDetails = new ListDictionary();
                args.ResponseDetails.Add("Response", response);
                args.ResponseDetails.Add("StatusCode", UNSUCCESSFUL_STATUS_CODE);
                if (Received != null)
                {
                    Received(args);
                }
            }

            return response;
        }


       
        private string DeleteDocument(IISDocDetails docDetails, NameValueCollection targetDetails)
        {
          
            if (string.IsNullOrWhiteSpace(targetDetails["url_suffix"]))
                throw new Exception("url_suffix value missing in parameters.");

            
            var uri = new UriBuilder();
            switch (targetURLDetails["UriScheme"].ToLower())
            {
                case "https":
                    uri.Scheme = Uri.UriSchemeHttps;
                    uri.Port = int.Parse(targetURLDetails["Port"] ?? "443");
                    break;
                default:
                    uri.Scheme = Uri.UriSchemeHttp;
                    uri.Port = int.Parse(targetURLDetails["Port"] ?? "80");
                    break;

            }
            uri.Host = targetDetails["RootDNS"];
            uri.Path = Path.Combine(docDetails.DocumentsVirtualDirectoryFromRoot, targetDetails["url_suffix"]);

            WebRequest request = HttpWebRequest.Create(uri.Uri);
 
            request.UseDefaultCredentials = true;
            
            request.PreAuthenticate = true;
            request.Method = "DELETE";


            foreach (var key in targetDetails.AllKeys)
            {
                LifLogHandler.LogDebug("IIS_Doc Adapter- Adding Key: {0}\tValue: {1}",
                    LifLogHandler.Layer.IntegrationLayer,
                    key, targetDetails[key]);
                request.Headers.Add(key, targetDetails[key]);
            }

          
            LifLogHandler.LogDebug("IIS_Doc Adapter- HandleStream- trying to delete container...",
                LifLogHandler.Layer.IntegrationLayer);
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    LifLogHandler.LogDebug("IIS_Doc Adapter- HandleStream- file deleted",
                        LifLogHandler.Layer.IntegrationLayer);
                    return null;
                }
                else
                {
                    LifLogHandler.LogError("Server rejected delete request with status: {0} - {1}",
                        LifLogHandler.Layer.IntegrationLayer, response.StatusCode, response.StatusDescription);

                    return string.Format("{0}-{1}", response.StatusCode, response.StatusDescription);
                }
            }
        }

       

       

        private string ReceiveDocument(IISDocDetails docDetails, string response)
        {
           
            LifLogHandler.LogDebug(
                "IIS_Doc Adapter- ReceiveDocument- formating web request for RECEIVE operation...",
                LifLogHandler.Layer.IntegrationLayer);

            var uri = BuildUri(docDetails);

            using var httpclient = new HttpClient(BuildHttpclientHandler());
            using var httpReqMsg = BuildReqMsg(docDetails, uri, HttpMethod.Get);
            var r = httpclient.Send(httpReqMsg);

            if (r.IsSuccessStatusCode)
            {
                response = SUCCESSFUL_DATA_RECEIVED;
                LifLogHandler.LogDebug(
                    "IIS_Doc Adapter- ReceiveDocument- file received and accordingly raising Received event...",
                    LifLogHandler.Layer.IntegrationLayer);
                Stream inFileStream = r.Content.ReadAsStream();
                LifLogHandler.LogDebug(
                   "IIS_Doc Adapter- ReceiveDocument- Response Uri- {0}",
                   LifLogHandler.Layer.IntegrationLayer, r.RequestMessage.RequestUri.AbsoluteUri);
                LifLogHandler.LogDebug(
                    "IIS_Doc Adapter- ReceiveDocument- Content length- {0}",
                    LifLogHandler.Layer.IntegrationLayer, r.Content.Headers.ContentLength);

               
                ReceiveEventArgs args = new ReceiveEventArgs();
                args.ResponseDetails = new ListDictionary();
                args.ResponseDetails.Add("DataStream", inFileStream);
                args.ResponseDetails.Add("FileName", targetURLDetails["file_name"]);
                args.ResponseDetails.Add("Response", SUCCESSFUL_DATA_RECEIVED);
                args.ResponseDetails.Add("StatusCode", SUCCESSFUL_STATUS_CODE);
                if (Received != null)
                {
                    Received(args);
                }
            }
            else
            {
                using var sr = new StreamReader(r.Content.ReadAsStream());
                response = UNSUCCESSFUL_DATA_RECEIVED + ". " + sr.ReadToEnd();
                Exception exResponse = new Exception(response);
                throw exResponse;
            }


            return response;

        }

        private string SendDocument(IISDocDetails docDetails, string response)
        {
            Stream outFileStream = null;
            try
            {
                LifLogHandler.LogDebug(
                "IIS_Doc Adapter- HandleStream- formating web request for SEND operation...",
                LifLogHandler.Layer.IntegrationLayer);

                var uri = BuildUri(docDetails);

                
                LifLogHandler.LogDebug("IIS_Doc Adapter- HandleStream- trying to upload the file stream...",
                    LifLogHandler.Layer.IntegrationLayer);

                using var client = new HttpClient(BuildHttpclientHandler());
                using var request = BuildReqMsg(docDetails, uri, HttpMethod.Post);
                request.Content = new StreamContent(dataStream);

                var r = client.Send(request);

                if (r.IsSuccessStatusCode)
                {
                    response = SUCCESSFUL_DATA_SENT;
                    LifLogHandler.LogDebug("IIS_Doc Adapter- HandleStream- file uploaded",
                        LifLogHandler.Layer.IntegrationLayer);
                }
                else
                {
                    using var sr = new StreamReader(r.Content.ReadAsStream());
                    response = UNSUCCESSFUL_DATA_SENT + ". " + sr.ReadToEnd();
                    Exception exResponse = new Exception(response);
                    throw exResponse;
                }


            }
            catch (Exception ex)
            {
                LifLogHandler.LogError(ex.ToString(), LifLogHandler.Layer.IntegrationLayer);
            }
            finally
            {
                if (outFileStream != null)
                    outFileStream.Close();
                if (dataStream != null)
                    dataStream.Close();
            }

            return response;
        }

        private UriBuilder BuildUri(IISDocDetails docDetails)
        {
            var regionId = targetURLDetails["region_id"];

            var uri = new UriBuilder();
            switch (targetURLDetails["UriScheme"].ToLower())
            {
                case "https":
                    uri.Scheme = Uri.UriSchemeHttps;
                    uri.Port = int.Parse(targetURLDetails["Port"] ?? "443");
                    break;
                default:
                    uri.Scheme = Uri.UriSchemeHttp;
                    uri.Port = int.Parse(targetURLDetails["Port"] ?? "80");
                    break;

            }
            uri.Host = targetURLDetails["RootDNS"];
          
            uri.Path = string.IsNullOrWhiteSpace(regionId) ?
                Path.Combine(docDetails.DocumentsVirtualDirectoryFromRoot,
                                targetURLDetails["container_name"],
                                targetURLDetails["file_name"]) :
                Path.Combine(docDetails.DocumentsVirtualDirectoryFromRoot,
                                regionId,
                                targetURLDetails["container_name"],
                                targetURLDetails["file_name"]);

            return uri;
        }

        private HttpClientHandler BuildHttpclientHandler() =>
            new HttpClientHandler
            {
                UseDefaultCredentials = true,
                PreAuthenticate = true
            };


        private HttpRequestMessage BuildReqMsg(IISDocDetails docDetails, UriBuilder uri, HttpMethod verb)
        {
            var request = new HttpRequestMessage(verb, uri.Uri);

            
            LifLogHandler.LogDebug("IIS_Doc Adapter- Adding Key: {0}\tValue: {1}", LifLogHandler.Layer.IntegrationLayer,
                "application_type", "lif_document_handler_as_blob");

            request.Headers.Add("application_type", EncodeStringToBase64("lif_document_handler_as_blob"));

            LifLogHandler.LogDebug("IIS_Doc Adapter- Adding Key: {0}\tValue: {1}", LifLogHandler.Layer.IntegrationLayer,
                "block_size", docDetails.DataBlockSizeInKB);

            request.Headers.Add("block_size", EncodeStringToBase64(docDetails.DataBlockSizeInKB.ToString()));

            LifLogHandler.LogDebug("IIS_Doc Adapter- Adding Key: {0}\tValue: {1}", LifLogHandler.Layer.IntegrationLayer,
                "documents_VD_from_Root", docDetails.DocumentsVirtualDirectoryFromRoot);

            request.Headers.Add("documents_VD_from_Root",
                EncodeStringToBase64(docDetails.DocumentsVirtualDirectoryFromRoot));

            foreach (var key in targetURLDetails.AllKeys)
            {
                LifLogHandler.LogDebug("IIS_Doc Adapter- Adding Key: {0}\tValue: {1}", LifLogHandler.Layer.IntegrationLayer,
                    key, targetURLDetails[key]);
               
                if (string.IsNullOrWhiteSpace(targetURLDetails[key]))
                    continue;
                
                string value = EncodeStringToBase64(targetURLDetails[key]);
                request.Headers.Add(key, value);
            }

            return request;

        }

        private string EncodeStringToBase64(string key)
        {
            byte[] toEncodeAsBytes = System.Text.Encoding.Unicode.GetBytes(key);
            string value = System.Convert.ToBase64String(toEncodeAsBytes);
            value = "=?utf-8?B?" + value + "?=";
            return value;
        }

      
        private IISDocDetails ValidateTransportName(IISDoc transportSection,
            string transportName)
        {
            LifLogHandler.LogDebug("IIS_Doc Adapter- ValidateTransportName called...",
                LifLogHandler.Layer.IntegrationLayer);
            IISDocDetails blobDetails = null;
            bool isTransportNameExists = false;
            
            for (int count = 0; count < transportSection.IISDocDetails.Count; count++)
            {
                blobDetails = transportSection.IISDocDetails[count] as IISDocDetails;
                if (blobDetails.TransportName == transportName)
                {
                    isTransportNameExists = true;
                    break;
                }
            }
           
            if (!isTransportNameExists)
            {
                throw new LegacyException("IIS_Doc Adapter- " + transportName + " is not defined in MSMQDetails section");
            }
            return blobDetails;
        }
    }

    enum DocAccessType
    {
        Send,
        Receive,
        Delete
    }
}
