using Azure.Core;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Infosys.Lif.LegacyCommon;
using Infosys.Lif.LegacyIntegratorService;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using System.Linq;
//using System.Runtime.Caching;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Xml;
using Region = Infosys.Lif.LegacyIntegratorService.Region;

namespace Infosys.Lif
{
    public class AzureVaultAdapter : ISecretsAdapter 
    {
        private const string REGION = "Region";
        private const string TRANSPORT_SECTION = "TransportSection";
        private const string LI_CONFIGURATION = "LISettings";
        private static LISettings liSettings = new LISettings();

        public AzureVaultAdapter() 
        {
            
            try
            {
                LifLogHandler.LogDebug("Azure Adapter- AzureVaultAdapter static constructor executed", LifLogHandler.Layer.IntegrationLayer);

                // Read all config data into LISetttings object.
                var appconfig = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json").Build();
                var liSettingPath = appconfig.GetSection("LISettings").GetSection("Path").Value;
                var config = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile(liSettingPath).Build();
                config.Bind(LI_CONFIGURATION, liSettings);
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        public async Task<string> GetSecrets(ListDictionary adapterDetails)
        {
            AzureVault transportSection = null;
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
                        transportSection = items.Value as Infosys.Lif.LegacyIntegratorService.AzureVault;
                    }
                    else if (items.Key.ToString() == "TargetURLDetails")
                    {
                        secrets = items.Value as Dictionary<string, string>;
                    }
                }

                // Validates whether TransportName specified in the region, exists in MemoryQueueDetails section.
                AzureVaultDetails azureVaultDetails = ValidateTransportName(transportSection, regionToBeUsed.TransportName);
                if (!string.IsNullOrEmpty(azureVaultDetails.CertificatePath))
                {
                    response = await GetSecretsByCertificateAsync(azureVaultDetails, secrets);
                }
                else
                {
                    response = await GetSecretsByIdentityAsync(azureVaultDetails, secrets);
                }
            }
            catch (Exception ex)
            {
                LifLogHandler.LogError($"Exception occurred in GetConfigurations, Exception message: {ex.Message}, inner exception: {ex.InnerException}", LifLogHandler.Layer.IntegrationLayer);
            }
            return response;
        }

        public static AzureVaultDetails ValidateTransportName(AzureVault transportSection, string transportName)
        {
            AzureVaultDetails azureVaultDetails = null;
            bool isTransportNameExists = false;
            for(int i = 0; i < transportSection.AzureVaultDetails.Count; i++)
            {
                azureVaultDetails = transportSection.AzureVaultDetails[i] as AzureVaultDetails;
                if(azureVaultDetails.TransportName == transportName)
                {
                    isTransportNameExists = true;
                    break;
                }
            }
            if(!isTransportNameExists)
            {
                throw new LegacyException(transportName + " is not defined in AzureVaultDetails section");
            }
            return azureVaultDetails;
        }

        public async Task<string> GetSecretsByCertificateAsync(AzureVaultDetails azureVaultDetails, Dictionary<string, string> secrets)
        {
            try
            {
                ClientCertificateCredential credential = new ClientCertificateCredential(azureVaultDetails.TenantId, azureVaultDetails.ClientId, azureVaultDetails.CertificatePath);

                SecretClientOptions options = new SecretClientOptions()
                {
                    Retry = {
                            Delay= TimeSpan.FromSeconds(azureVaultDetails.Delay),
                            MaxDelay = TimeSpan.FromSeconds(azureVaultDetails.MaxDelay),
                            MaxRetries = azureVaultDetails.MaxRetries,
                            Mode = RetryMode.Exponential
                        }
                };

                SecretClient client = new SecretClient(new Uri(azureVaultDetails.VaultUrl), credential, options);
                foreach (string key in secrets.Keys)
                {
                    KeyVaultSecret secretValue = await client.GetSecretAsync(key);
                    secrets[key] = secretValue.Value;
                }
            }
            catch(Exception ex)
            {
                LifLogHandler.LogError($"Exception occurred in GetSecretsByCertificateAsync, Exception message: {ex.Message}, inner exception: {ex.InnerException}", LifLogHandler.Layer.IntegrationLayer);
            }
            string result = JsonConvert.SerializeObject(secrets);
            return result;
        }

        public async Task<string> GetSecretsByIdentityAsync(AzureVaultDetails azureVaultDetails, Dictionary<string, string> secrets)
        {
            try
            {
                //Identity - works only on Azure services, need to establish identity relation with key vault //
                if (!string.IsNullOrEmpty(azureVaultDetails.VaultName) && !string.IsNullOrEmpty(azureVaultDetails.VaultUrl))
                {
                    SecretClientOptions options = new SecretClientOptions()
                    {
                        Retry = {
                            Delay= TimeSpan.FromSeconds(azureVaultDetails.Delay),
                            MaxDelay = TimeSpan.FromSeconds(azureVaultDetails.MaxDelay),
                            MaxRetries = azureVaultDetails.MaxRetries,
                            Mode = RetryMode.Exponential
                        }
                    };

                    var client = new SecretClient(new Uri(azureVaultDetails.VaultUrl), new DefaultAzureCredential(), options);

                    foreach (string key in secrets.Keys)
                    {
                        KeyVaultSecret secretValue = await client.GetSecretAsync(key);
                        secrets[key] = secretValue.Value;
                    }
                }
            }
            catch(Exception ex)
            {
                LifLogHandler.LogError($"Exception occurred in GetSecretsByIdentityAsync, Exception message: {ex.Message}, inner exception: {ex.InnerException}", LifLogHandler.Layer.IntegrationLayer);
            }
            string result = JsonConvert.SerializeObject(secrets);
            return result;
        }
    }
}
