/*=============================================================================================================== *
 * Copyright 2025 Infosys Ltd.                                                                                    *
 * Use of this source code is governed by Apache License Version 2.0 that can be found in the LICENSE file or at  *
 * http://www.apache.org/licenses/                                                                                *
 * ===============================================================================================================*/
﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Collections;
using System.Text;
using System.Runtime.Remoting;
using System.IO;
using System.Configuration;
using System.Reflection;
using System.Linq;
using Newtonsoft.Json;
using Infosys.Lif.LegacyCommon;
using Infosys.Lif.LegacyIntegratorService;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;

namespace Infosys.Lif.LegacyIntegrator
{
    public class AdapterManager
    {
        
        #region Private Members

       
        private static LISettings liSettings;
        private static bool isConfigDataRead;
        static object syncObject = new Object();
        IAdapter adapterBase;
        ISecretsAdapter secretsAdapter;
        ListDictionary receiveAdapterDetails;

        #endregion

        #region Public Members
        public delegate void AdapterReceiveHandler(ReceiveEventArgs eventArgs);
        public event AdapterReceiveHandler ResponseReceived;
        #endregion

        #region Constants
        private const string LI_FILENAME = "LiSettings.json";
        private const string LI_CONFIGURATION = "LISettings";
        private const string REGION = "Region";
        private const string TRANSPORT_SECTION = "TransportSection";
        private const string DATA = "Data";
        private const string TARGETURLDETAILS = "TargetURLDetails";

        #endregion

        #region Initialize
        
        private void Initialize()
        {
            if (!isConfigDataRead)
            {
                lock (syncObject)
                {
                    if (!isConfigDataRead)
                    {
                       

                        liSettings = new LISettings();
                        var appconfig = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json").Build();
                        var liSettingPath = appconfig.GetSection("LISettings").GetSection("Path").Value;
                            
                        var config = new ConfigurationBuilder().AddJsonFile(liSettingPath).Build();
                        config.Bind(LI_CONFIGURATION, liSettings);

                        isConfigDataRead = true;
                    }
                }
            }
        }

        private void InitAdapterBase(string hostRegion)
        {
            Initialize();
            Region region = null;
         
            for (int count = 0; count < liSettings.HostRegion.Region.Count; count++)
            {

                if (liSettings.HostRegion.Region[count].Name.Equals(hostRegion))
                {
                    region = liSettings.HostRegion.Region[count];
                    break;
                }
            }
           
            if (region == null)
            {
                throw new LegacyException("Host region " + hostRegion + " does not exist in config file");
            }
        
            string transportMedium = region.TransportMedium as string;
            string transportName = region.TransportName as string;
         
            if (string.IsNullOrEmpty(transportMedium))
            {
                throw new LegacyException("Transport medium for the region = " + hostRegion + " is not specified");
            }

            if (string.IsNullOrEmpty(transportName))
            {
                throw new LegacyException("Transport name for the region = " + hostRegion + " is not specified");
            }

           
            PropertyInfo propertyInfo = liSettings.GetType().GetProperty(transportMedium);
            object transportSection = propertyInfo.GetValue(liSettings, null) as object;

        
            if (transportSection == null)
            {
                throw new LegacyException("TransportMedium " + transportMedium + " is not valid");
            }

     
            receiveAdapterDetails = new ListDictionary();
            receiveAdapterDetails.Add(REGION, region);
            receiveAdapterDetails.Add(TRANSPORT_SECTION, transportSection);
            
            PropertyInfo adapterPropertyInfo = transportSection.GetType().GetProperty("DllPath");
            string dllPath = Path.GetFullPath(adapterPropertyInfo.GetValue(transportSection, null) as string);
            adapterPropertyInfo = transportSection.GetType().GetProperty("TypeName");
            string typeName = adapterPropertyInfo.GetValue(transportSection, null) as string;
      
            ObjectHandle objHandle = Activator.CreateInstanceFrom(dllPath, typeName);
            Assembly assembly = Assembly.LoadFrom(dllPath);
            Type type = assembly.GetType(typeName);
            var interfaces = type.GetInterfaces();
            if (interfaces.Contains(typeof(IAdapter)))
            {
                adapterBase = (IAdapter)objHandle.Unwrap();
            }
            else if (interfaces.Contains(typeof(ISecretsAdapter)))
            {
                secretsAdapter = (ISecretsAdapter)objHandle.Unwrap();
            }

        }
        #endregion


        public string GetTransportMedium(string hostRegion) 
        {
            try
            {
                Initialize();
                Region region = null;
                

                #region changes
                for (int count = 0; count < liSettings.HostRegion.Region.Count; count++)
                {
                    if (liSettings.HostRegion.Region[count].Name.Equals(hostRegion))
                    {
                        region = liSettings.HostRegion.Region[count];
                        break;
                    }
                }
                #endregion

           
                if (region == null)
                {
                    throw new LegacyException("Host region " + hostRegion + " does not exist in config file");
                }

               
                string transportMedium = region.TransportMedium as string;

           
                if (string.IsNullOrEmpty(transportMedium))
                {
                    throw new LegacyException("Transport GetTransportMedium for the region = " + hostRegion + " is not specified");
                }

                return transportMedium;
            }
            catch (LegacyException exception)
            {
                throw exception;
            }
            catch (Exception exception)
            {
             
                throw new LegacyException("Error in Execute method" + exception.ToString(), exception);
            }
        }
        #region Execute
        public string Execute(string message, string hostRegion)
        {
            try
            {
                Initialize();
                Region region = null;
            
                
                #region changes

                for (int count = 0; count < liSettings.HostRegion.Region.Count; count++)
                {
                    if (liSettings.HostRegion.Region[count].Name.Equals(hostRegion))
                    {
                        region = liSettings.HostRegion.Region[count];
                        break;
                    }
                }

                #endregion

            
                if (region == null)
                {
                    throw new LegacyException("Host region " + hostRegion + " does not exist in config file");
                }

               
                string transportMedium = region.TransportMedium as string;
                string transportName = region.TransportName as string;

                
                if (string.IsNullOrEmpty(transportMedium))
                {
                    throw new LegacyException("Transport medium for the region = " + hostRegion + " is not specified");
                }

               
                if (string.IsNullOrEmpty(transportName))
                {
                    throw new LegacyException("Transport name for the region = " + hostRegion + " is not specified");
                }

               
                PropertyInfo propertyInfo = liSettings.GetType().GetProperty(transportMedium);
                object transportSection = propertyInfo.GetValue(liSettings, null) as object;


                
                if (transportSection == null)
                {
                    throw new LegacyException("TransportMedium " + transportMedium + " is not valid");
                }

               
                ListDictionary adapterDetails = new ListDictionary();
                adapterDetails.Add(REGION, region);
                adapterDetails.Add(TRANSPORT_SECTION, transportSection);

              
                PropertyInfo ibmMQpropertyInfo = transportSection.GetType().GetProperty("DllPath");
                string dllPath = Path.GetFullPath(ibmMQpropertyInfo.GetValue(transportSection, null) as string);
                ibmMQpropertyInfo = transportSection.GetType().GetProperty("TypeName");
                string typeName = ibmMQpropertyInfo.GetValue(transportSection, null) as string;
               
                ObjectHandle objHandle = Activator.CreateInstanceFrom(dllPath, typeName);

                IAdapter transport;

                transport = (IAdapter)objHandle.Unwrap();
                string response = string.Empty;
            
                response = transport.Send(adapterDetails, message);
                return response;
            }
            catch (LegacyException exception)
            {
                throw exception;
            }
            catch (Exception exception)
            {
                
                throw new LegacyException("Error in Execute method" + exception.ToString(), exception);
            }
        }

        public string Execute(Stream data, string hostRegion, NameValueCollection targetURLDetails)
        {
            try
            {
                Initialize();
                Region region = null;
             
                for (int count = 0; count < liSettings.HostRegion.Region.Count; count++)
                {
                    if (liSettings.HostRegion.Region[count].Name.Equals(hostRegion))
                    {
                        region = liSettings.HostRegion.Region[count];
                        break;
                    }
                }

               
                if (region == null)
                {
                    throw new LegacyException("Host region " + hostRegion + " does not exist in config file");
                }

              
                string transportMedium = region.TransportMedium as string;
                string transportName = region.TransportName as string;

          
                if (string.IsNullOrEmpty(transportMedium))
                {
                    throw new LegacyException("Transport medium for the region = " + hostRegion + " is not specified");
                }

              
                if (string.IsNullOrEmpty(transportName))
                {
                    throw new LegacyException("Transport name for the region = " + hostRegion + " is not specified");
                }

              
                PropertyInfo propertyInfo = liSettings.GetType().GetProperty(transportMedium);
                object transportSection = propertyInfo.GetValue(liSettings, null) as object;


               
                if (transportSection == null)
                {
                    throw new LegacyException("TransportMedium " + transportMedium + " is not valid");
                }

           
                ListDictionary adapterDetails = new ListDictionary();
                adapterDetails.Add(REGION, region);
                adapterDetails.Add(TRANSPORT_SECTION, transportSection);
                adapterDetails.Add(DATA, data);
                adapterDetails.Add(TARGETURLDETAILS, targetURLDetails);

                
                PropertyInfo ibmMQpropertyInfo = transportSection.GetType().GetProperty("DllPath");
                string dllPath = Path.GetFullPath(ibmMQpropertyInfo.GetValue(transportSection, null) as string);
                ibmMQpropertyInfo = transportSection.GetType().GetProperty("TypeName");
                string typeName = ibmMQpropertyInfo.GetValue(transportSection, null) as string;
          
                ObjectHandle objHandle = Activator.CreateInstanceFrom(dllPath, typeName);
                IAdapter transport;
                transport = (IAdapter)objHandle.Unwrap();
                string response = string.Empty;
             
                response = transport.Send(adapterDetails, null);
                return response;
            }
            catch (LegacyException exception)
            {
                throw exception;
            }
            catch (Exception exception)
            {
                
                throw new LegacyException("Error in Execute method" + exception.ToString(), exception);
            }
        }
        #endregion

        #region Receive
       
        public void Receive(string hostRegion)
        {
            try
            {
                if (adapterBase == null)
                {
                    
                    InitAdapterBase(hostRegion);

                    adapterBase.GetType().GetEvent("Received").AddEventHandler(adapterBase, new ReceiveHandler(adaptorBase_Received));
                }
                string response = string.Empty;

                adapterBase.Receive(receiveAdapterDetails);
            }
            catch (LegacyException exception)
            {
                throw exception;
            }
            catch (Exception exception)
            {
              
                throw new LegacyException("Error in Receive method" + exception.ToString(), exception);
            }
        }

        public void Receive(string hostRegion, NameValueCollection targetURLDetails)
        {
            try
            {
                if (adapterBase == null)
                {
                    InitAdapterBase(hostRegion);
                    adapterBase.GetType().GetEvent("Received").AddEventHandler(adapterBase, new ReceiveHandler(adaptorBase_Received));
                }
                string response = string.Empty;
                receiveAdapterDetails.Add(TARGETURLDETAILS, targetURLDetails);

                adapterBase.Receive(receiveAdapterDetails);
            }
            catch (LegacyException exception)
            {
                throw exception;
            }
            catch (Exception exception)
            {
              
                throw new LegacyException("Error in Receive method" + exception.ToString(), exception);
            }
        }

        void adaptorBase_Received(ReceiveEventArgs eventArgs)
        {
            if (ResponseReceived != null)
            {
                ResponseReceived(eventArgs);
            }
        }

        #endregion

        #region Delete
        
        public bool Delete(string messageId)
        {
            bool result = false;
            System.Collections.Specialized.ListDictionary messageDetails = new ListDictionary();
            messageDetails.Add("MessageIdentifier", messageId);
            if (adapterBase != null)
                result = adapterBase.Delete(messageDetails);
            return result;
        }


     
        public bool Delete(string hostRegion, NameValueCollection targetDetails)
        {
            InitAdapterBase(hostRegion);
            receiveAdapterDetails.Add(TARGETURLDETAILS, targetDetails);
            return adapterBase.Delete(receiveAdapterDetails);
        }
        #endregion

        #region GetConfigurations
       
        public string GetSecrets(string hostRegion, Dictionary<string, string> targetDetails)
        {
            InitAdapterBase(hostRegion);
            receiveAdapterDetails.Add(TARGETURLDETAILS, targetDetails);
            return secretsAdapter.GetSecrets(receiveAdapterDetails).Result;
        }
        #endregion
    }
}
