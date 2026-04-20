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
	/// <summary>
	/// Communication type 
	/// </summary>
	public enum CommunicationType
	{
		//Synchronous 
		Sync,
		//Asynchronous
		Async
	}

	/// <summary>
	/// Represent type of connection model used.
	/// </summary>
	public enum ConnectionModelType
	{
		// Connection pooling
		ConnectionPool,
		// No connection pooling
		None
	}

	/// <summary>
	/// Represent persistent property for a message
	/// </summary>
	public enum MessagePersistence
	{
		// Persistent 
		Persistent,
		// Non persistent 
		NonPersistent,
		// as per queue definition
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
