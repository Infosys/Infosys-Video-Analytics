/*=============================================================================================================== *
 * Copyright 2025 Infosys Ltd.                                                                                    *
 * Use of this source code is governed by Apache License Version 2.0 that can be found in the LICENSE file or at  *
 * http://www.apache.org/licenses/                                                                                *
 * ===============================================================================================================*/
﻿using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.Common;
using Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.TaskRoute;
using Infosys.Solutions.Ainauto.VideoAnalytics.BusinessEntity;

namespace Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.TCPSChannelCommunication
{
    public class ClientTCPConnect
    {
        protected readonly int ConnectAttemptDelayInMsec; 
        protected bool IsRunning;
        protected readonly string Host;
        protected readonly int Port;
        public TcpClient ClientConnect;

        private volatile bool _ExitSignal;
        public bool ExitSignal
        {
            get => this._ExitSignal;
            set => this._ExitSignal = value;
        }

        public delegate void ConnectionHandlerDelegate(NetworkStream connectedNetStream);
        protected readonly ConnectionHandlerDelegate OnHandleConnection;

        public ClientTCPConnect(ConnectionHandlerDelegate connectionHandler, string host = "127.0.0.1",
            int port = 8080, int ConnectAttemptDelayInMsec = 2000)
        {

            this.OnHandleConnection = connectionHandler ?? throw new ArgumentNullException(nameof(connectionHandler));
            this.Host = host ?? throw new ArgumentNullException(nameof(host));
            this.Port = port;
            this.ConnectAttemptDelayInMsec = ConnectAttemptDelayInMsec;
        }


        public virtual void Run(object deviceDetails) {
            if (this.IsRunning)
                return; 

            this.IsRunning = true;
            this.ExitSignal = false;

            while (!this.ExitSignal)
                this.ConnectionLoop((DeviceDetails)deviceDetails);

            this.IsRunning = false;
        }

        protected virtual void ConnectionLoop(DeviceDetails deviceDetails) {

            using (var Client = new TcpClient())
            {
                try
                {
                    Client.Connect(this.Host, this.Port);
                    ClientConnect = Client;
                }
                catch (SocketException ex)
                {

                    Thread.Sleep(this.ConnectAttemptDelayInMsec);
                    return;
                }

                if (!Client.Connected) 
                    return;

                using (var netstreamClient = Client.GetStream())
                {
                    int timeOut = 5000;
                    if(deviceDetails.DataStreamTimeOut!=null) {
                        timeOut=int.Parse(deviceDetails.DataStreamTimeOut);
                    }
                    netstreamClient.ReadTimeout = timeOut;
                    netstreamClient.WriteTimeout = timeOut;
                    this.OnHandleConnection.Invoke(netstreamClient);
                }
            }
        }

    }


}
