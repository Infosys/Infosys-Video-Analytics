using Infosys.Lif.LegacyIntegratorService;
using Infosys.Lif.LegacyCommon;
using System.Collections;
using Region = Infosys.Lif.LegacyIntegratorService.Region;
using Amazon;
using Amazon.S3;
using Amazon.S3.Transfer;
using Microsoft.Extensions.Configuration;
using System.Collections.Specialized;

using Amazon.S3.Model;
using Amazon.Runtime;
using Amazon.SecurityToken;
using Amazon.SecurityToken.Model;
using System.Diagnostics;
using NLog;
using System.Net;

namespace Infosys.Lif
{


    public class AWSS3Adapter : IAdapter
    {
        private const string REGION = "Region";
        private const string TRANSPORT_SECTION = "TransportSection";
        private const string TARGETURLDETAILS = "TargetURLDetails";
        private const string SUCCESSFUL_DATA_SENT = "Document successfully uploaded.";
        private const string SUCCESSFUL_DATA_RECEIVED = "Document successfully downloaded.";
        private const string UNSUCCESSFUL_DATA_SENT = "Document couldn't be uploaded successfully.";
        private const string UNSUCCESSFUL_DATA_RECEIVED = "Document couldn't be downloaded successfully.";
        private const string SUCCESSFUL_DATA_DELETED = "Document successfully Deleted.";
        private const string UNSUCCESSFUL_DATA_DELETED = "Document couldn't be deleted successfully. ";
        private const string LI_CONFIGURATION = "LISettings";
        private const string DATA = "Data";
        private static Credentials temporaryCredentials = new Credentials();
        private static AmazonS3Client s3Client;
        private NameValueCollection targetURLDetails;
        private readonly string sourceBucketName;
        private readonly string destinationBucketName;
        private const int SUCCESSFUL_STATUS_CODE = 0;
        private const int UNSUCCESSFUL_STATUS_CODE = 1000;

        private static readonly IConfigurationRoot appconfig = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

        public AWSS3Adapter() {
            LoadLISettings();
        }

        private static void GenerateS3Client(AWSS3DocDetails docDetails)
        {
            try
            {
                if (!string.IsNullOrEmpty(docDetails.SessionToken))
                {
                    var credentials = new Amazon.Runtime.SessionAWSCredentials(docDetails.Accesskey, docDetails.SecretKey, docDetails.SessionToken);
                    var awsConfig = new AmazonS3Config
                    {
                        RegionEndpoint = RegionEndpoint.GetBySystemName(docDetails.RegionName)
                    };
                    s3Client = new AmazonS3Client(credentials, awsConfig);
                }
                else if (!string.IsNullOrEmpty(docDetails.Accesskey) && !string.IsNullOrEmpty(docDetails.SecretKey))
                {
                    var creds = new BasicAWSCredentials(docDetails.Accesskey, docDetails.SecretKey);
                    var stsClient = new AmazonSecurityTokenServiceClient(creds, RegionEndpoint.GetBySystemName(docDetails.RegionName));
                    var getSessionTokenRequest = new GetSessionTokenRequest
                    {
                        DurationSeconds = docDetails.TokenValidityInSeconds
                    };
                    var sessionTokenResponse = stsClient.GetSessionTokenAsync(getSessionTokenRequest);
                    temporaryCredentials = sessionTokenResponse.Result.Credentials;
                    LifLogHandler.LogDebug("LifDebug - temporary credentials generated and valid till: {0}", LifLogHandler.Layer.IntegrationLayer, temporaryCredentials.Expiration);
                    var credentials = new Amazon.Runtime.SessionAWSCredentials(temporaryCredentials.AccessKeyId, temporaryCredentials.SecretAccessKey, temporaryCredentials.SessionToken);
                    var awsConfig = new AmazonS3Config
                    {
                        RegionEndpoint = RegionEndpoint.GetBySystemName(docDetails.RegionName)
                    };
                    s3Client = new AmazonS3Client(credentials, awsConfig);
                    LifLogHandler.LogDebug("LifDebug - s3Client object created successfully", LifLogHandler.Layer.IntegrationLayer);
                }
                else
                {
                    s3Client = new AmazonS3Client();
                }
            }
            catch (Exception e)
            {
                LifLogHandler.LogError("LifError - Exception while creating AWSS3Client object, exception message: {0}, inner exception: {1}, stack trace: {2}", LifLogHandler.Layer.IntegrationLayer, e.Message, e.InnerException, e.StackTrace);
                //throw e;
            }
        }

        private LISettings LoadLISettings()
        {
            string liSettingPath = appconfig.GetSection(LI_CONFIGURATION).GetSection("Path").Value;
            IConfigurationRoot config = new ConfigurationBuilder().AddJsonFile(liSettingPath).Build();

            LISettings liSettings = new LISettings();
            config.Bind(LI_CONFIGURATION, liSettings);

            return liSettings;

        }

        public event ReceiveHandler Received;

        public bool Delete(ListDictionary adapterDetails)
        {
            AWSS3Doc transport = null;
            Region region = null;

            foreach (DictionaryEntry items in adapterDetails)
            {
                if (items.Key.ToString() == REGION)
                    region = items.Value as Region;
                else if (items.Key.ToString() == TRANSPORT_SECTION)
                    transport = items.Value as AWSS3Doc;
                else if (items.Key.ToString() == TARGETURLDETAILS)
                    targetURLDetails = items.Value as NameValueCollection;
            }

            if (region == null || transport == null || targetURLDetails == null)
                throw new ArgumentException($"{REGION}, {TRANSPORT_SECTION}, {TARGETURLDETAILS} are required for delete operation!");

            ValidateTargetURLDetails();

            AWSS3DocDetails docDetails = ValidateTransportName(transport, region.TransportName);
            string response = HandleStream(docDetails, DocAccessType.Delete, null);


            return response == SUCCESSFUL_DATA_DELETED;
        }

        public async void Receive(ListDictionary adapterDetails)
        {
            AWSS3Doc transportSection = null;
            LegacyIntegratorService.Region regionToBeUsed = null;

            foreach (DictionaryEntry items in adapterDetails)
            {
                if (items.Key.ToString() == REGION)
                    regionToBeUsed = items.Value as LegacyIntegratorService.Region;
                else if (items.Key.ToString() == TRANSPORT_SECTION)
                    transportSection = items.Value as AWSS3Doc;
                else if (items.Key.ToString() == TARGETURLDETAILS)
                    targetURLDetails = items.Value as NameValueCollection;
            }

            if (targetURLDetails == null)
                throw new LegacyException("AWSS3Doc Adapter- Metadata details are not provided.");

            ValidateTargetURLDetails();

            AWSS3DocDetails docDetails = ValidateTransportName(transportSection, regionToBeUsed.TransportName);
            HandleStream(docDetails, DocAccessType.Receive, null);
        }

        public string Send(ListDictionary adapterDetails, string message)
        {
            AWSS3Doc transportSection = null;
            LegacyIntegratorService.Region regionToBeUsed = null;
            Stream dataStream = null;
            string response = "";

            foreach (DictionaryEntry items in adapterDetails)
            {
                if (items.Key.ToString() == REGION)
                    regionToBeUsed = items.Value as LegacyIntegratorService.Region;
                else if (items.Key.ToString() == TRANSPORT_SECTION)
                    transportSection = items.Value as AWSS3Doc;
                else if (items.Key.ToString() == DATA)
                    dataStream = items.Value as Stream;
                else if (items.Key.ToString() == TARGETURLDETAILS)
                    targetURLDetails = items.Value as NameValueCollection;
            }

            if (dataStream == null && targetURLDetails == null)
                throw new LegacyException("AWSS3Doc Adapter- File Stream and metadata details both cannot be empty.");

            try
            {
                ValidateTargetURLDetails();

                AWSS3DocDetails docDetails = ValidateTransportName(transportSection, regionToBeUsed.TransportName);
                
                response = HandleStream(docDetails, DocAccessType.Send, dataStream);
            }
            catch(Exception ex)
            {
                return ex.Message;
            }
            return response;
        }

        private void ValidateTargetURLDetails()
        {
            //if (!targetURLDetails.AllKeys.Contains("container_name") || string.IsNullOrWhiteSpace(targetURLDetails["container_name"]))
            //    throw new LegacyException("AWSS3Doc Adapter- 'container_name' missing in metadata.");

            //if (!targetURLDetails.AllKeys.Contains("file_name") || string.IsNullOrWhiteSpace(targetURLDetails["file_name"]))
            //    throw new LegacyException("AWSS3Doc Adapter- 'file_name' missing in metadata.");
        }

        private AWSS3DocDetails ValidateTransportName(AWSS3Doc transportSection, string transportName)
        {
            AWSS3DocDetails blobDetails = transportSection.AWSS3DocDetails.FirstOrDefault(d => d.TransportName == transportName);

            if (blobDetails == null)
                throw new LegacyException($"AWSS3Doc Adapter- Could not find transport by name: {transportName}");

            return blobDetails;
        }

        private string HandleStream(AWSS3DocDetails docDetails,
            DocAccessType accessType, Stream dataStream)
        {
            int retrys3Client = docDetails.RetryLimit;
            while ((s3Client == null || temporaryCredentials.Expiration <= DateTime.Now.AddSeconds(docDetails.TokenRefreshTimeBeforeExpiry)) && retrys3Client > 0)
            {
                Thread.Sleep(retrys3Client);
                GenerateS3Client(docDetails);
                retrys3Client--;
            }
            var fileTransferUtility = new TransferUtility(s3Client);
            string response = "";
            var fileName =  targetURLDetails["file_name"];
            var bucketName = docDetails.BucketName;
            var directory = docDetails.AWSDirectory;
            int retry = docDetails.RetryLimit;
            switch (accessType)
            {
                case DocAccessType.Send:
                    fileName = directory + Path.GetFileName(fileName);
                    var uploadRequest = new TransferUtilityUploadRequest
                    {
                        BucketName = bucketName,
                        Key = fileName
                    };
                    byte[] bytes;
                    using(MemoryStream ms = new MemoryStream())
                    {
                        dataStream.CopyTo(ms);
                        bytes = new byte[ms.ToArray().Length];
                        bytes = ms.ToArray();
                    }
                    while(retry > 0)
                    {
                        try
                        {
                            Stream fileStream = new MemoryStream(bytes);
                            uploadRequest.InputStream = fileStream;
                            fileTransferUtility.Upload(uploadRequest);
                            response = SUCCESSFUL_DATA_SENT;
                            break;
                        }
                        catch (Exception ex)
                        {
                            LifLogHandler.LogError("Error in LIF while uploading file: {0}, Exception message: {1}, inner exception: {2}, stack trace: {3}",
                                LifLogHandler.Layer.IntegrationLayer, fileName, ex.Message, ex.InnerException, ex.StackTrace);
                            retry--;
                            Thread.Sleep(docDetails.RetryWaitTime);
                        }
                        if (retry == 0)
                        {
                            response = UNSUCCESSFUL_DATA_RECEIVED;
                            break;
                        }
                    }
                    break;

                case DocAccessType.Receive:
                    List<string> s3Files = new List<string>();
                    if (string.IsNullOrEmpty(fileName))
                    {
                        ListObjectsV2Response files = GetS3BucketFiles(bucketName, directory);
                        s3Files = files.S3Objects.OrderBy(obj => obj.LastModified).Select(obj => Path.GetFileName(obj.Key)).Where(obj => !string.IsNullOrEmpty(obj)).ToList();
                    }
                    else
                    {
                        fileName = Path.GetFileName(fileName);
                        s3Files.Add(fileName);
                    }
                    foreach(string file in s3Files)
                    {
                        var getObjectRequest = new GetObjectRequest
                        {
                            BucketName = bucketName,
                            Key = directory + file
                        };
                        while (retry > 0)
                        {
                            try
                            {
                                using (var getObjectResponse = s3Client.GetObjectAsync(getObjectRequest))
                                {
                                    using (var responseStream = getObjectResponse.Result.ResponseStream)
                                    {
                                        var outDataStream = new MemoryStream();
                                        responseStream.CopyTo(outDataStream);
                                        outDataStream.Position = 0; // Reset the position to the beginning of the stream
                                        try
                                        {
                                            if (outDataStream != null)
                                            {
                                                ReceiveEventArgs args = new ReceiveEventArgs();
                                                args.ResponseDetails = new ListDictionary();
                                                args.ResponseDetails.Add("DataStream", outDataStream);
                                                args.ResponseDetails.Add("FileName", file);
                                                args.ResponseDetails.Add("Response", SUCCESSFUL_DATA_RECEIVED);
                                                args.ResponseDetails.Add("StatusCode", SUCCESSFUL_STATUS_CODE);
                                                if (Received != null)
                                                {
                                                    Received(args);
                                                }
                                                response = SUCCESSFUL_DATA_RECEIVED;
                                            }
                                            else
                                            {
                                                response = UNSUCCESSFUL_DATA_RECEIVED;
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            response = UNSUCCESSFUL_DATA_RECEIVED + ex.Message;
                                        }
                                        break;
                                    }

                                }
                                //break;
                            }
                            catch (Exception e)
                            {
                                LifLogHandler.LogError("Error in LIF while receiving file: {0}, Exception message: {1}, inner exception: {2}, stack trace: {3}",
                                    LifLogHandler.Layer.IntegrationLayer, fileName, e.Message, e.InnerException, e.StackTrace);
                                retry--;
                                Thread.Sleep(docDetails.RetryWaitTime);
                            }
                            if (retry == 0)
                            {
                                response = UNSUCCESSFUL_DATA_RECEIVED;
                                break;
                            }
                        }
                    }
                    break;

                case DocAccessType.Delete:
                    fileName = directory + Path.GetFileName(fileName);
                    while (retry > 0)
                    {
                        try
                        {
                            s3Client.DeleteObjectAsync(bucketName, fileName).Wait();
                            response = SUCCESSFUL_DATA_DELETED;
                            break;
                        }
                        catch (Exception ex)
                        {
                            LifLogHandler.LogError("Error in LIF while receiving file: {0}, Exception message: {1}, inner exception: {2}, stack trace: {3}",
                                LifLogHandler.Layer.IntegrationLayer, fileName, ex.Message, ex.InnerException, ex.StackTrace);
                            retry--;
                            Thread.Sleep(docDetails.RetryWaitTime);
                        }
                        if(retry == 0)
                        {
                            response = UNSUCCESSFUL_DATA_RECEIVED;
                            break;
                        }
                    }

                    break;
            }
            //s3Client.Dispose();
            return response;
        }

        public static ListObjectsV2Response GetS3BucketFiles(string bucketName, string folderPath)
        {
            LifLogHandler.LogDebug("Getting list of files available in : {0}/{1} - ThreadId {2}", LifLogHandler.Layer.IntegrationLayer, bucketName, folderPath, Thread.CurrentThread.ManagedThreadId);
            ListObjectsV2Response response = null;
            try
            {
                ListObjectsV2Request request = new ListObjectsV2Request
                {
                    BucketName = bucketName,
                    Prefix = folderPath
                };
                response = s3Client.ListObjectsV2Async(request).Result;
            }
            catch (Exception ex)
            {
                LifLogHandler.LogDebug("Exception in getting file from bucket: {0}, exception message: {1}, inner exception: {2}, stack trace: {3} - ThreadId: {4}", LifLogHandler.Layer.IntegrationLayer
                    , bucketName, ex.Message, ex.InnerException, ex.StackTrace, Thread.CurrentThread.ManagedThreadId);
                //throw ex;
            }
            if (response == null)
            {
                response = null;
            }
            return response;
        }

        enum DocAccessType
        {
            Send,
            Receive,
            Delete
        }
       
    }
}