/*=============================================================================================================== *
 * Copyright 2024 Infosys Ltd.                                                                                    *
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
        /// <summary>
        /// Config
        /// </summary>
        private void Initialize()
        {
            if (!isConfigDataRead)
            {
                lock (syncObject)
                {
                    if (!isConfigDataRead)
                    {
                        // Read all config data into LISetttings object.
                        //liSettings = new LISettings();
                        //var config = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile(LI_FILENAME).Build();
                        //config.Bind(LI_CONFIGURATION, liSettings);

                        //isConfigDataRead = true;

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
            // Find the region details (region name, transport name and transport medium)
            for (int count = 0; count < liSettings.HostRegion.Region.Count; count++)
            {

                if (liSettings.HostRegion.Region[count].Name.Equals(hostRegion))
                {
                    region = liSettings.HostRegion.Region[count];
                    break;
                }
            }
            // If region does not exist then throw the exception
            if (region == null)
            {
                throw new LegacyException("Host region " + hostRegion + " does not exist in config file");
            }
            // Read TransportMedium, name and communication type into variable.
            string transportMedium = region.TransportMedium as string;
            string transportName = region.TransportName as string;
            // Transport medium is specified in region tag then throw the exception
            if (string.IsNullOrEmpty(transportMedium))
            {
                throw new LegacyException("Transport medium for the region = " + hostRegion + " is not specified");
            }

            // Transport medium is specified in region tag then throw the exception
            if (string.IsNullOrEmpty(transportName))
            {
                throw new LegacyException("Transport name for the region = " + hostRegion + " is not specified");
            }

            // Get the transport specific details
            PropertyInfo propertyInfo = liSettings.GetType().GetProperty(transportMedium);
            object transportSection = propertyInfo.GetValue(liSettings, null) as object;

            // TransportSection like MSMQ,MemoryQueue etc., is not specified then throw the legacyexception
            if (transportSection == null)
            {
                throw new LegacyException("TransportMedium " + transportMedium + " is not valid");
            }

            // Construct ListDictionary object containing details required by adapter.
            receiveAdapterDetails = new ListDictionary();
            receiveAdapterDetails.Add(REGION, region);
            receiveAdapterDetails.Add(TRANSPORT_SECTION, transportSection);
            // Find the dll path and type
            PropertyInfo adapterPropertyInfo = transportSection.GetType().GetProperty("DllPath");
            string dllPath = Path.GetFullPath(adapterPropertyInfo.GetValue(transportSection, null) as string);
            adapterPropertyInfo = transportSection.GetType().GetProperty("TypeName");
            string typeName = adapterPropertyInfo.GetValue(transportSection, null) as string;
            // Create an instance of adapter
            ObjectHandle objHandle = Activator.CreateInstanceFrom(dllPath, typeName);
            //AdapterBase transport;
            adapterBase = (IAdapter)objHandle.Unwrap();

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

                // Transport medium is specified in region tag then throw the exception
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
                // If other exception is thrown then wrap it into LegacyException type 
                // and re throw it.
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
                // Find the region details (region name, transport name and transport medium)
                
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

                // If region does not exist then throw the exception
                if (region == null)
                {
                    throw new LegacyException("Host region " + hostRegion + " does not exist in config file");
                }

                // Read TransportMedium, name and communication type into variable.
                string transportMedium = region.TransportMedium as string;
                string transportName = region.TransportName as string;

                // Transport medium is specified in region tag then throw the exception
                if (string.IsNullOrEmpty(transportMedium))
                {
                    throw new LegacyException("Transport medium for the region = " + hostRegion + " is not specified");
                }

                // Transport medium is specified in region tag then throw the exception
                if (string.IsNullOrEmpty(transportName))
                {
                    throw new LegacyException("Transport name for the region = " + hostRegion + " is not specified");
                }

                // Get the transport specific details
                PropertyInfo propertyInfo = liSettings.GetType().GetProperty(transportMedium);
                object transportSection = propertyInfo.GetValue(liSettings, null) as object;


                // TransportSection like IBMMQ. HIS is not specified then throw the legacyexception
                if (transportSection == null)
                {
                    throw new LegacyException("TransportMedium " + transportMedium + " is not valid");
                }

                // Construct ListDictionary object containing details required by adapter.
                ListDictionary adapterDetails = new ListDictionary();
                adapterDetails.Add(REGION, region);
                adapterDetails.Add(TRANSPORT_SECTION, transportSection);

                // Find the dll path and type
                PropertyInfo ibmMQpropertyInfo = transportSection.GetType().GetProperty("DllPath");
                string dllPath = Path.GetFullPath(ibmMQpropertyInfo.GetValue(transportSection, null) as string);
                ibmMQpropertyInfo = transportSection.GetType().GetProperty("TypeName");
                string typeName = ibmMQpropertyInfo.GetValue(transportSection, null) as string;
                // Create an instance of adapter
                ObjectHandle objHandle = Activator.CreateInstanceFrom(dllPath, typeName);

                IAdapter transport;

                transport = (IAdapter)objHandle.Unwrap();
                string response = string.Empty;
                // Invokde Send method.
                response = transport.Send(adapterDetails, message);
                return response;
            }
            catch (LegacyException exception)
            {
                throw exception;
            }
            catch (Exception exception)
            {
                // If other exception is thrown then wrap it into LegacyException type 
                // and re throw it.
                throw new LegacyException("Error in Execute method" + exception.ToString(), exception);
            }
        }

        public string Execute(Stream data, string hostRegion, NameValueCollection targetURLDetails)
        {
            try
            {
                Initialize();
                Region region = null;
                // Find the region details (region name, transport name and transport medium)
                for (int count = 0; count < liSettings.HostRegion.Region.Count; count++)
                {
                    if (liSettings.HostRegion.Region[count].Name.Equals(hostRegion))
                    {
                        region = liSettings.HostRegion.Region[count];
                        break;
                    }
                }

                // If region does not exist then throw the exception
                if (region == null)
                {
                    throw new LegacyException("Host region " + hostRegion + " does not exist in config file");
                }

                // Read TransportMedium, name and communication type into variable.
                string transportMedium = region.TransportMedium as string;
                string transportName = region.TransportName as string;

                // Transport medium is specified in region tag then throw the exception
                if (string.IsNullOrEmpty(transportMedium))
                {
                    throw new LegacyException("Transport medium for the region = " + hostRegion + " is not specified");
                }

                // Transport medium is specified in region tag then throw the exception
                if (string.IsNullOrEmpty(transportName))
                {
                    throw new LegacyException("Transport name for the region = " + hostRegion + " is not specified");
                }

                // Get the transport specific details
                PropertyInfo propertyInfo = liSettings.GetType().GetProperty(transportMedium);
                object transportSection = propertyInfo.GetValue(liSettings, null) as object;


                // TransportSection like IBMMQ. HIS is not specified then throw the legacyexception
                if (transportSection == null)
                {
                    throw new LegacyException("TransportMedium " + transportMedium + " is not valid");
                }

                // Construct ListDictionary object containing details required by adapter.
                ListDictionary adapterDetails = new ListDictionary();
                adapterDetails.Add(REGION, region);
                adapterDetails.Add(TRANSPORT_SECTION, transportSection);
                adapterDetails.Add(DATA, data);
                adapterDetails.Add(TARGETURLDETAILS, targetURLDetails);

                // Find the dll path and type
                PropertyInfo ibmMQpropertyInfo = transportSection.GetType().GetProperty("DllPath");
                string dllPath = Path.GetFullPath(ibmMQpropertyInfo.GetValue(transportSection, null) as string);
                ibmMQpropertyInfo = transportSection.GetType().GetProperty("TypeName");
                string typeName = ibmMQpropertyInfo.GetValue(transportSection, null) as string;
                // Create an instance of adapter
                ObjectHandle objHandle = Activator.CreateInstanceFrom(dllPath, typeName);
                IAdapter transport;
                transport = (IAdapter)objHandle.Unwrap();
                string response = string.Empty;
                // Invokde Send method.
                response = transport.Send(adapterDetails, null);
                return response;
            }
            catch (LegacyException exception)
            {
                throw exception;
            }
            catch (Exception exception)
            {
                // If other exception is thrown then wrap it into LegacyException type 
                // and re throw it.
                throw new LegacyException("Error in Execute method" + exception.ToString(), exception);
            }
        }
        #endregion

        #region Receive
        /// <summary>
        /// To read the message from the target legacy component.
        /// </summary>
        /// <param name="hostRegion">region to which it should connect</param>
        public void Receive(string hostRegion)
        {
            try
            {
                if (adapterBase == null)
                {
                    //adaptorBase = new AdapterBase();
                    InitAdapterBase(hostRegion);

                    adapterBase.GetType().GetEvent("Received").AddEventHandler(adapterBase, new ReceiveHandler(adaptorBase_Received));
                    //adaptorBase.Received += new AdapterBase.ReceiveHandler(adaptorBase_Received);
                }
                string response = string.Empty;
                // Invoke Receive method.

                adapterBase.Receive(receiveAdapterDetails);
            }
            catch (LegacyException exception)
            {
                throw exception;
            }
            catch (Exception exception)
            {
                // If other exception is thrown then wrap it into LegacyException type 
                // and re throw it.
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
                    //adaptorBase.Received += new AdapterBase.ReceiveHandler(adaptorBase_Received);
                }
                string response = string.Empty;
                // Invoke Receive method.
                receiveAdapterDetails.Add(TARGETURLDETAILS, targetURLDetails);

                adapterBase.Receive(receiveAdapterDetails);
            }
            catch (LegacyException exception)
            {
                throw exception;
            }
            catch (Exception exception)
            {
                // If other exception is thrown then wrap it into LegacyException type 
                // and re throw it.
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
        /// <summary>
        /// To be overwritten in the inheriting class. To explicitly delete the received message. This is to avoid the same message to be available in the 
        /// subsequent Receive operation. Mainly useful in case of communication with Queues, e.g. MSMQ, Azure queue, etc.
        /// IMP.- To be called after Receive and it needs to be called on the same adapter instance on which the Receive operation is called
        /// </summary>        
        /// <param name="messageId">the identifier of the message to be deleted</param>
        /// <returns>true if the delete is successful otherwise false</returns>
        public bool Delete(string messageId)
        {
            bool result = false;
            System.Collections.Specialized.ListDictionary messageDetails = new ListDictionary();
            messageDetails.Add("MessageIdentifier", messageId);
            if (adapterBase != null)
                result = adapterBase.Delete(messageDetails);
            return result;
        }


        /// <summary>
        /// Deletes item using adapter from given region and target details
        /// </summary>
        /// <param name="hostRegion">Name of region in LiSettings.config file</param>
        /// <param name="targetDetails">Dictionary of parameters accepted by the adapter</param>
        /// <returns></returns>
        public bool Delete(string hostRegion, NameValueCollection targetDetails)
        {
            InitAdapterBase(hostRegion);
            receiveAdapterDetails.Add(TARGETURLDETAILS, targetDetails);
            return adapterBase.Delete(receiveAdapterDetails);
        }
        #endregion
    }
}
