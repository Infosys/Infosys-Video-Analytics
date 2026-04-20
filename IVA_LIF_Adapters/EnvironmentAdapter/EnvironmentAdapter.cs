using Infosys.Lif.LegacyIntegratorService;
using Infosys.Lif.LegacyCommon;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace Infosys.Lif
{
    public class EnvironmentAdapter : IEnvironmentAdapter
    {
        private const string REGION = "Region";
        private const string TRANSPORT_SECTION = "TransportSection";
        private const string LI_CONFIGURATION = "LISettings";
        private static LISettings liSettings = new LISettings();
        public Task<Dictionary<string, string?>> GetEnvironmentVariables(ListDictionary adapterDetails)
        {
            Environments? transportSection = null;
            Region regionToBeUsed = null;
            List<string> environmentVariables = new List<string>();
            Dictionary<string, string?> environmentValues = new Dictionary<string, string?>();
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
                        transportSection = items.Value as Infosys.Lif.LegacyIntegratorService.Environments;
                    }
                }

                // Validates whether TransportName specified in the region, exists in MemoryQueueDetails section.
                EnvironmentDetails environmentDetails = ValidateTransportName(transportSection, regionToBeUsed.TransportName);
                EnvironmentVariableTarget envTarget = EnvironmentVariableTarget.User;
                Enum.TryParse(environmentDetails.EnvironmentTarget, true, out envTarget);
                foreach(DictionaryEntry variable in Environment.GetEnvironmentVariables(envTarget))
                {
                    environmentValues.Add(variable.Key.ToString(), variable.Value?.ToString());
                }
            }
            catch (Exception ex)
            {
                LifLogHandler.LogError($"Exception occurred in GetEnvironmentVariables, Exception message: {ex.Message}, inner exception: {ex.InnerException}", LifLogHandler.Layer.IntegrationLayer);
            }
            return Task.FromResult(environmentValues);
        }

        public static EnvironmentDetails ValidateTransportName(Environments transportSection, string transportName)
        {
            EnvironmentDetails environmentDetails = null;
            bool isTransportNameExists = false;
            for (int i = 0; i < transportSection.EnvironmentDetails.Count; i++)
            {
                environmentDetails = transportSection.EnvironmentDetails[i] as EnvironmentDetails;
                if (environmentDetails.TransportName == transportName)
                {
                    isTransportNameExists = true;
                    break;
                }
            }
            if (!isTransportNameExists)
            {
                throw new LegacyException(transportName + " is not defined in EnvironmentDetails section");
            }
            return environmentDetails;
        }
    }
}
