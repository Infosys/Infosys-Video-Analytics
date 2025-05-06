/*=============================================================================================================== *
 * Copyright 2025 Infosys Ltd.                                                                                    *
 * Use of this source code is governed by Apache License Version 2.0 that can be found in the LICENSE file or at  *
 * http://www.apache.org/licenses/                                                                                *
 * ===============================================================================================================*/
﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.TCPChannelCommunication
{
    public class ServerTCPListen
    {
        protected readonly int AwaitTimeoutInMS; 
        protected readonly string Host;
        protected readonly int Port;
        protected readonly int MaxConcurrentListeners;
        protected readonly TcpListener Listener;
        protected bool IsRunning;
        protected List<Task> TcpClientTasks = new List<Task>();

        public delegate void ConnectionHandlerDelegate(NetworkStream connectedAutoDisposedNetStream);
        protected readonly ConnectionHandlerDelegate OnHandleConnection;


        private volatile bool _ExitSignal;
        public virtual bool ExitSignal
        {
            get => this._ExitSignal;
            set => this._ExitSignal = value;
        }


        public ServerTCPListen(ConnectionHandlerDelegate connectionHandler,
                            string host , int port, int maxConcurrentListeners = 10, int awaiterTimeoutInMS = 500)
        {

            this.OnHandleConnection = connectionHandler ?? throw new ArgumentNullException(nameof(connectionHandler));
            this.Host = host ?? throw new ArgumentNullException(nameof(host));
            this.Port = port;
            this.MaxConcurrentListeners = maxConcurrentListeners;
            this.AwaitTimeoutInMS = awaiterTimeoutInMS;
            this.Listener = new TcpListener(IPAddress.Parse(this.Host), this.Port);
        }

        public virtual void Run()
        {
            if (this.IsRunning)
                return; 

            this.IsRunning = true;
            this.Listener.Start();
            this.ExitSignal = false;
            
            while (!this.ExitSignal)
                this.ConnectionLooper();

            this.IsRunning = false;
        }

        

        protected virtual void ConnectionLooper()
        {
            while (this.TcpClientTasks.Count < this.MaxConcurrentListeners) //Maximum number of concurrent listeners
            {
                var AwaitTask = Task.Run(async () =>
                {

                    this.ProcessInBoundMessagesFromClient(await this.Listener.AcceptTcpClientAsync());
                });
                this.TcpClientTasks.Add(AwaitTask);
            }
            int RemoveAtIndex = Task.WaitAny(this.TcpClientTasks.ToArray(), this.AwaitTimeoutInMS); //Synchronously Waits up to 500ms for any Task completion
            if (RemoveAtIndex > 0) 
                this.TcpClientTasks.RemoveAt(RemoveAtIndex);
        }

        protected virtual void ProcessInBoundMessagesFromClient(TcpClient Connection)
        {
            using (Connection) 
            {
               
                if (!Connection.Connected) 
                    return;

                using (var netstream = Connection.GetStream()) 
                {
                    this.OnHandleConnection.Invoke(netstream);
                }
            }
         
        }

    }

}
