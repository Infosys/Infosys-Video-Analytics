/*=============================================================================================================== *
 * Copyright 2025 Infosys Ltd.                                                                                    *
 * Use of this source code is governed by Apache License Version 2.0 that can be found in the LICENSE file or at  *
 * http://www.apache.org/licenses/                                                                                *
 * ===============================================================================================================*/
/****************************************************************
 * This file is a part of the Legacy Integration Framework.
 * This file contains enum definition.
 * Copyright (c) 2003 - 2005 Infosys Technologies Ltd. All Rights Reserved.
 ***************************************************************/
using System;
using System.Collections.Generic;
using System.Text;

namespace Infosys.Lif.LegacyIntegratorService
{
	
	public enum CommunicationType
	{
		//Synchronous 
		Sync,
		//Asynchronous
		Async
	}

	
	public enum ConnectionModelType
	{
		
		ConnectionPool,
		
		None
	}

	
	public enum MessagePersistence
	{
	
		Persistent,
		
		NonPersistent,
		
		Default
	}

    [Serializable]
    public enum MSMQType
    {
        Public,        
        Private
    }

    [Serializable]
    public enum MSMQReadType
    {
        Receive,
        Peek
    }

    [Serializable]
    public enum MSMQReadMode
    {
        Async,
        Sync
    }

    [Serializable]
    public enum MSMQSendPattern
    {
        None,
        RoundRobin,
        QueueLoad,
        BroadCast
    }

    public enum MSMQOperationType
    {
        Send, Receive, Peek
    }

}
