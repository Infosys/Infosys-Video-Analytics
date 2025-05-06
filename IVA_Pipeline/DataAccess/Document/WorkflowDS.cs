/*=============================================================================================================== *
 * Copyright 2025 Infosys Ltd.                                                                                    *
 * Use of this source code is governed by Apache License Version 2.0 that can be found in the LICENSE file or at  *
 * http://www.apache.org/licenses/                                                                                *
 * ===============================================================================================================*/
﻿using System;
using System.Collections.Specialized;
using System.Threading;
using Infosys.Lif.LegacyIntegrator;
using Infosys.Lif.LegacyIntegratorService;
using Infosys.Solutions.Ainauto.VideoAnalytics.Resource.IDataAccess;
using DocumentEntity = Infosys.Solutions.Ainauto.VideoAnalytics.Resource.Entity.Document;
using Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.Common;

namespace Infosys.Solutions.Ainauto.VideoAnalytics.Resource.DataAccess.Document
{

    
    public class WorkflowDS : IDocument<DocumentEntity.Workflow>
    {
        string SuccessMessage = "Document successfully uploaded.";
        string SUCCESSFUL_DATA_DELETED = "Document successfully Deleted.";
        int statusCode = 0;
        private System.IO.Stream _presentation = null;
        private string response = "";
        private string frameId = "";
        
        public DocumentEntity.Workflow Upload(DocumentEntity.Workflow workflowRequest)
        {
            if (workflowRequest.File?.Length > 0)
            {
                using (LogHandler.TraceOperations("Document.PresentationDS:Upload",
               LogHandler.Layer.Resource, System.Guid.Empty, null))
                {
                    AdapterManager adapterManager = new AdapterManager();

                    string msgResponse = "";

                    NameValueCollection dictionary = new NameValueCollection();

                   
                    Uri uri = new Uri(workflowRequest.StorageBaseURL);
                    dictionary.Add("UriScheme", uri.Scheme);
                    dictionary.Add("RootDNS", uri.DnsSafeHost);
                    dictionary.Add("Port", uri.Port.ToString());

                    dictionary.Add("container_name", ConstructContainerName(workflowRequest.TenantId, workflowRequest.DeviceId)); 
                    dictionary.Add("file_name", workflowRequest.FrameId); 

                   
                    dictionary.Add("device_id", workflowRequest.DeviceId);
                                                                           
                    dictionary.Add("tenant_id", workflowRequest.TenantId.ToString());


                    if (!string.IsNullOrWhiteSpace(workflowRequest.UploadedBy))
                        dictionary.Add("uploaded_by", workflowRequest.UploadedBy);


                    
                    LogHandler.LogDebug(
                        "Document.WorkflowDS: Document {0} message to be posted. The document is for device with id {1} for company with Id {3}",
                        LogHandler.Layer.Resource,
                        workflowRequest.FrameId,
                        workflowRequest.DeviceId,
                        workflowRequest.TenantId);

                    msgResponse = adapterManager.Execute(workflowRequest.File,
                        ApplicationConstants.DOCUMENTSTORE_KEY, dictionary);

                    workflowRequest.StatusMessage = msgResponse;

                    if (workflowRequest.StatusMessage == SuccessMessage)
                    {
                        workflowRequest.StatusCode = 0;
                        LogHandler.LogDebug(
                            "Document.PresentationDS: Document {0} message successfully posted. The document is for device with id {1} for company with Id {2}",
                            LogHandler.Layer.Resource,
                            workflowRequest.DeviceId,
                            workflowRequest.TenantId);
                    }
                    else
                    {
                        
                        workflowRequest.StatusCode = -1;
                        LogHandler.LogError(
                            "Document.PresentationDS: Document {0} message failed to be uploaded. The document is for device with id {1} for company with Id {2}. Failure Message- {3}",
                            LogHandler.Layer.Resource,
                            workflowRequest.FrameId,
                            workflowRequest.DeviceId,
                            workflowRequest.TenantId,
                            workflowRequest.StatusMessage);
                    }

                    return workflowRequest;
                }
            }
            return null;
        }


       
        public DocumentEntity.Workflow Download(DocumentEntity.Workflow workflowRequest)
        {
            using (LogHandler.TraceOperations("Document.WorkflowDS:Download",
                LogHandler.Layer.Resource, System.Guid.Empty, null))
            {
                AutoResetEvent arEvent = new AutoResetEvent(false);

                AdapterManager adapterManager = new AdapterManager();

                
                adapterManager.ResponseReceived +=
                    new AdapterManager.AdapterReceiveHandler((ea) => adapterManager_ResponseReceived(ea, arEvent));

                NameValueCollection dictionary = new NameValueCollection();
                string frameId = workflowRequest.FrameId;
                
                Uri uri = new Uri(workflowRequest.StorageBaseURL);
                dictionary.Add("UriScheme", uri.Scheme);
                dictionary.Add("RootDNS", uri.DnsSafeHost);
                dictionary.Add("Port", uri.Port.ToString());

                dictionary.Add("container_name", ConstructContainerName(workflowRequest.TenantId, workflowRequest.DeviceId));
                dictionary.Add("file_name", workflowRequest.FrameId);

                
                dictionary.Add("device_id", workflowRequest.DeviceId);
                dictionary.Add("tenant_id", workflowRequest.TenantId.ToString());

                LogHandler.LogDebug("Download Presentation by using httpAdapter for Device Id {0}",
                    LogHandler.Layer.Resource, workflowRequest.DeviceId);

                adapterManager.Receive(ApplicationConstants.DOCUMENTSTORE_KEY, dictionary);

                
                arEvent.WaitOne();

                workflowRequest.File = _presentation;
                workflowRequest.StatusMessage = response;
                workflowRequest.StatusCode = statusCode;
                workflowRequest.FrameId = frameId;

                if (workflowRequest.StatusCode == 0)
                {
                    LogHandler.LogDebug(
                        "Document.PresentationDS: Document Id {0} message successfully downloaded. The document is for device Id  {1} for company with Id {2}",
                        LogHandler.Layer.Resource,
                        frameId,
                        workflowRequest.DeviceId,
                        workflowRequest.TenantId);
                }
                else
                {
                    
                    workflowRequest.StatusCode = statusCode;
                    LogHandler.LogDebug(
                    "Document.PresentationDS: Document Id {0} message failed to be downloaded. The document is for device Id  {1} for company with Id {2}",
                        LogHandler.Layer.Resource,
                        frameId,
                        workflowRequest.DeviceId,
                        workflowRequest.TenantId);
                    LogHandler.LogError(
                        "Document.PresentationDS: Document Id {0} message failed to be downloaded. The document is for device Id  {1} for company with Id {2}. Status code = {3}, Status Message = {4}",
                        LogHandler.Layer.Resource,
                        frameId,
                        workflowRequest.DeviceId,
                        workflowRequest.TenantId,
                        statusCode,
                        workflowRequest.StatusMessage);
                }
                return workflowRequest;
            }
        }




        void adapterManager_ResponseReceived(ReceiveEventArgs eventArgs, AutoResetEvent arEvent)
        {
            statusCode = (int)eventArgs.ResponseDetails["StatusCode"];
            LogHandler.LogDebug(
                "Download File (adapterManager_ResponseReceived) by using httpAdapter called. Status code = {0}",
                    LogHandler.Layer.Resource, statusCode);
            if (statusCode == 0)
            {
                LogHandler.LogDebug(
                 "Download File (adapterManager_ResponseReceived) by using httpAdapter called. Success status code",
                    LogHandler.Layer.Resource, statusCode);
                frameId = eventArgs.ResponseDetails["FileName"] as string;

                System.IO.Stream presentation = eventArgs.ResponseDetails["DataStream"] as System.IO.Stream;

                System.IO.MemoryStream outStream = new System.IO.MemoryStream();
                byte[] buffer = new byte[1024];
                int bytesRead = 0;
                while ((bytesRead = presentation.Read(buffer, 0, buffer.Length)) != 0)
                {
                    outStream.Write(buffer, 0, bytesRead);
                }

                presentation.Close();

                _presentation = outStream;

                response = eventArgs.ResponseDetails["Response"] as string;

                if (string.IsNullOrWhiteSpace(response) || string.IsNullOrWhiteSpace(frameId))
                {
                    LogHandler.LogWarning("Invalid response received from the server", LogHandler.Layer.Resource);
                    statusCode = -1;
                    response = "Invalid response received from the server";
                }
            }
            else
            {
                LogHandler.LogDebug(
                    "Download File (adapterManager_ResponseReceived) by using httpAdapter called. Failed status code",
                     LogHandler.Layer.Resource, statusCode);
                response = eventArgs.ResponseDetails["Response"] as string;
            }

            LogHandler.LogDebug(
                "Download File (adapterManager_ResponseReceived) by using httpAdapter call completed",
                LogHandler.Layer.Resource);

            
            arEvent.Set();
        }
    
        string ConstructContainerName(int tenantId, string deviceId)
        {
            string containerName = string.Concat(tenantId,"_",deviceId);
            return containerName;
        }

        public void Delete(DocumentEntity.Workflow workflowRequest)
        {
            using (LogHandler.TraceOperations("Document.PresentationDS:Upload",
               LogHandler.Layer.Resource, System.Guid.Empty, null))
            {
                AdapterManager adapterManager = new AdapterManager();



                NameValueCollection dictionary = new NameValueCollection();

                
                Uri uri = new Uri(workflowRequest.StorageBaseURL);
                dictionary.Add("UriScheme", uri.Scheme);
                dictionary.Add("RootDNS", uri.DnsSafeHost);
                dictionary.Add("Port", uri.Port.ToString());

                dictionary.Add("container_name", ConstructContainerName(workflowRequest.TenantId, workflowRequest.DeviceId)); 
                dictionary.Add("file_name", workflowRequest.FrameId); 

                
                dictionary.Add("device_id", workflowRequest.DeviceId); 

                dictionary.Add("tenant_id", workflowRequest.TenantId.ToString()); 


                
                LogHandler.LogDebug(
                    "Document.WorkflowDS: Document {0} message to be Deleted. The document is for device with id {1} for company with Id {2}",
                    LogHandler.Layer.Resource,
                    workflowRequest.FrameId,
                    workflowRequest.DeviceId,
                    workflowRequest.TenantId);

                adapterManager.Delete(
                   ApplicationConstants.DOCUMENTSTORE_KEY, dictionary);


            }

        }
    }
}
