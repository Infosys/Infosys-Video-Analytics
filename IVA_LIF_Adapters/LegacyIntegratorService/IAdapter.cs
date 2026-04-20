/****************************************************************
 * This file is a part of the Legacy Integration Framework.
 * This file contains IAdapter.
 * Copyright (c) 2003 - 2005 Infosys Technologies Ltd. All Rights Reserved.
 ***************************************************************/
using System;
using System.Collections.Specialized;
using System.Text;
using System.Collections;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Infosys.Lif.LegacyIntegratorService
{   

    /// <summary>
	/// Different transport needs to be implement method this interface.
	/// </summary>
	public interface IAdapter
	{
        /// <summary>
        /// Event to be subscribed by the client calling the Receive operation
        /// </summary>
        event ReceiveHandler Received;
        
        /// <summary>
		/// Transport should implement send method.
		/// </summary>
		/// <param name="adapterDetails">Configuration details set in the config file</param>
		/// <param name="message">Message that needs to be sent to legacy system</param>
		/// <returns>return message from legacy system</returns>
		string Send(ListDictionary adapterDetails, string message);

        /// <summary>
        /// To read the message from the target legacy component.
        /// </summary>
        /// <param name="adapterDetails">the details for the adapter to be used to connect to the taget legacy component</param>
        void Receive(ListDictionary adapterDetails);

        /// <summary>
        /// To explicitly delete the received message. That is to avoid the same message to be available in the 
        /// subsequent Receive operation. Mainly useful in case of communication with Queues, e.g. MSMQ, Azure queue, etc.
        /// </summary>
        /// <param name="messageDetails"></param>
        /// <returns>true if the delete is successful otherwise false</returns>
        bool Delete(ListDictionary messageDetails);
	}

    public interface ISecretsAdapter
    {
        /// <summary>
        /// To read the configurations from the target legacy component.
        /// </summary>
        /// <param name="adapterDetails">the details for the adapter to be used to connect to the taget legacy component</param>
        Task<string> GetSecrets(ListDictionary adapterDetails);

    }

    public interface IEnvironmentAdapter
    {
        Task<Dictionary<string, string?>> GetEnvironmentVariables(ListDictionary adapterDetails);
    }

    /// <summary>
    /// The Receive operations are to be Asynchronous. Once the receive is completed by the concerned
    /// adapter, event is raised to the subscribed client passing an object of this class. The object has the
    /// required/expected result for the receive operation.
    /// </summary>
    public class ReceiveEventArgs : EventArgs
    {
        /// <summary>
        /// The response details are maintained as key-value pairs
        /// </summary>
        public ListDictionary ResponseDetails { get; set; }
    }

    public delegate void ReceiveHandler(ReceiveEventArgs eventArgs);
}
