using Amazon;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using Infosys.Lif.LegacyCommon;
using Infosys.Lif.LegacyIntegratorService;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using NLog.Web.LayoutRenderers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Infosys.Lif
{
    public class AWSSecretsAdapter : ISecretsAdapter
    {
        private const string REGION = "Region";
        private const string TRANSPORT_SECTION = "TransportSection";
        private const string LI_CONFIGURATION = "LISettings";
        private static LISettings liSettings = new LISettings();


        public AWSSecretsAdapter() 
        {
            LifLogHandler.LogDebug("AWS Adapter- AWSSecretsAdapter static constructor executed", LifLogHandler.Layer.IntegrationLayer);

            // Read all config data into LISetttings object.
            var appconfig = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json").Build();
            var liSettingPath = appconfig.GetSection("LISettings").GetSection("Path").Value;
            var config = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile(liSettingPath).Build();
            config.Bind(LI_CONFIGURATION, liSettings);
        }

        public async Task<string> GetSecrets(ListDictionary adapterDetails)
        {
            AWSSecrets transportSection = null;
            Region regionToBeUsed = null;
            string response = string.Empty;
            Dictionary<string, string> secrets = new Dictionary<string, string>();
            try
            {
                foreach (DictionaryEntry items in adapterDetails)
                {
                    if (items.Key.ToString() == REGION)
                    {
                        regionToBeUsed = items.Value as Region;
                    }
                    else if (items.Key.ToString() == TRANSPORT_SECTION)
                    {
                        transportSection = items.Value as Infosys.Lif.LegacyIntegratorService.AWSSecrets;
                    }
                    else if (items.Key.ToString() == "TargetURLDetails")
                    {
                        secrets = items.Value as Dictionary<string, string>;
                    }
                }

                // Validates whether TransportName specified in the region, exists in MemoryQueueDetails section.
                AWSSecretsDetails AWSSecretsDetails = ValidateTransportName(transportSection, regionToBeUsed.TransportName);
                response = await GetSecretsByIdentityAsync(AWSSecretsDetails, secrets);
            }
            catch (Exception ex)
            {
                LifLogHandler.LogError($"Exception while getting secrets from AWS Secrets Manager using Identity. Exception: {ex.Message}, Inner Exception: {ex.InnerException}", LifLogHandler.Layer.IntegrationLayer);
            }
            return response;
        }

        public static AWSSecretsDetails ValidateTransportName(AWSSecrets transportSection, string transportName)
        {
            AWSSecretsDetails awsSecretsDetails = null;
            bool isTransportNameExists = false;
            for (int i = 0; i < transportSection.AWSSecretsDetails.Count; i++)
            {
                awsSecretsDetails = transportSection.AWSSecretsDetails[i] as AWSSecretsDetails;
                if (awsSecretsDetails.TransportName == transportName)
                {
                    isTransportNameExists = true;
                    break;
                }
            }
            if (!isTransportNameExists)
            {
                throw new LegacyException(transportName + " is not defined in AWSSecretsDetails section");
            }
            return awsSecretsDetails;
        }

        public async Task<string> GetSecretsByIdentityAsync(AWSSecretsDetails AWSSecretsDetails, Dictionary<string, string> secrets)
        {
            string result = string.Empty;
            int retry = 0;
            try
            {
                IAmazonSecretsManager client = new AmazonSecretsManagerClient(RegionEndpoint.GetBySystemName(AWSSecretsDetails.Region));
                foreach (string key in secrets.Keys)
                {
                    var response = await GetSecretAsync(client, key, AWSSecretsDetails.VersionStage, AWSSecretsDetails.Retry);
                    if (response is not null)
                    {
                        string secretValue = DecodeString(response);
                        secrets[key] = secretValue;
                    }
                }
                result = JsonConvert.SerializeObject(secrets);
            }
            catch (AmazonSecretsManagerException e)
            {
                LifLogHandler.LogError($"Exception while getting secrets from AWS Secrets Manager using Identity. Exception: {e.Message}, Inner Exception: {e.InnerException}", LifLogHandler.Layer.IntegrationLayer);
            }
            catch (Exception ex)
            {
                LifLogHandler.LogError($"Exception while getting secrets from AWS Secrets Manager using Identity. Exception: {ex.Message}, Inner Exception: {ex.InnerException}", LifLogHandler.Layer.IntegrationLayer);
            }
            
            
            return result;
        }

        public static async Task<GetSecretValueResponse> GetSecretAsync(IAmazonSecretsManager client, string secretName, string versionStage, int retry)
        {
            int attempts = 0;
            
            GetSecretValueRequest request = new GetSecretValueRequest()
            {
                SecretId = secretName,
                VersionStage = versionStage, // VersionStage defaults to AWSCURRENT if unspecified.
            };
            GetSecretValueResponse response = null;
            while (attempts < retry)
            {
                // For the sake of simplicity, this example handles only the most 
                // general SecretsManager exception. 
                try
                {
                    response = await client.GetSecretValueAsync(request);
                    if(response != null)
                    {
                        break;
                    }
                }
                catch (AmazonSecretsManagerException e)
                {
                    LifLogHandler.LogError($"Exception while getting secrets from AWS Secrets Manager using Identity. Exception: {e.Message}, Inner Exception: {e.InnerException}", LifLogHandler.Layer.IntegrationLayer);
                    attempts++;
                }
                catch (Exception ex)
                {
                    LifLogHandler.LogError($"Exception while getting secrets from AWS Secrets Manager using Identity. Exception: {ex.Message}, Inner Exception: {ex.InnerException}", LifLogHandler.Layer.IntegrationLayer);
                    attempts++;
                }
            }
            return response;
        }

        public static string DecodeString(GetSecretValueResponse response)
        {
            string result = string.Empty;
            // Decrypts secret using the associated AWS Key Management Service 
            // Customer Master Key (CMK.) Depending on whether the secret is a 
            // string or binary value, one of these fields will be populated. 
            try
            {
                if (response.SecretString is not null)
                {
                    result = response.SecretString;
                    return result;
                }
                else if (response.SecretBinary is not null)
                {
                    var memoryStream = response.SecretBinary;
                    StreamReader reader = new StreamReader(memoryStream);
                    result = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(reader.ReadToEnd()));
                    return result;
                }
            }
            catch (Exception ex)
            {
                LifLogHandler.LogError($"Exception while decoding secrets value fetched from AWS. Exception: {ex.Message}, Inner Exception: {ex.InnerException}", LifLogHandler.Layer.IntegrationLayer);
            }
            return result;
        }
    }
}
