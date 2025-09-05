/*=============================================================================================================== *
 * Copyright 2025 Infosys Ltd.                                                                                    *
 * Use of this source code is governed by Apache License Version 2.0 that can be found in the LICENSE file or at  *
 * http://www.apache.org/licenses/                                                                                *
 * ===============================================================================================================*/
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

namespace Infosys.Lif.LegacyIntegratorService
{   

    
	public interface IAdapter
	{
       
        event ReceiveHandler Received;
        
        
		string Send(ListDictionary adapterDetails, string message);

       
        void Receive(ListDictionary adapterDetails);

        
        bool Delete(ListDictionary messageDetails);
	}

    public interface ISecretsAdapter
    {
       
        Task<string> GetSecrets(ListDictionary adapterDetails);

    }

   
    public class ReceiveEventArgs : EventArgs
    {
       
        public ListDictionary ResponseDetails { get; set; }
    }

    public delegate void ReceiveHandler(ReceiveEventArgs eventArgs);
}
