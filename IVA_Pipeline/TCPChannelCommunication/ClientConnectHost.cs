/*=============================================================================================================== *
 * Copyright 2025 Infosys Ltd.                                                                                    *
 * Use of this source code is governed by Apache License Version 2.0 that can be found in the LICENSE file or at  *
 * http://www.apache.org/licenses/                                                                                *
 * ===============================================================================================================*/
﻿using Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.Common;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.TaskRoute;
using Infosys.Solutions.Ainauto.VideoAnalytics.BusinessEntity;

namespace Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.TCPSChannelCommunication
{

    public class ClientConnectHost
    {
        private readonly ClientTCPConnect Client;
        private readonly Thread ClientThread;
        private NetworkStream connectNetStreamGlobal;
        private string _host;
        public static AppSettings appSettings=Config.AppSettings;
        public static DeviceDetails deviceDetails=ConfigHelper.SetDeviceDetails(appSettings.TenantID.ToString(),appSettings.DeviceID,CacheConstants.ClientConnectHost);
        int waitingTime=deviceDetails.ClientConnectionWaitingTime;
        #region Constructor
        public ClientConnectHost(string Host, int Port)
        {
            _host = Host;
            if (!string.IsNullOrEmpty(Host))
                this.Client = new ClientTCPConnect(this.ConnectionHandler, Host, Port);
            else
                this.Client = new ClientTCPConnect(this.ConnectionHandler);

            this.ClientThread = new Thread(this.Client.Run);
        }
        #endregion
        #region Public Functions
        public virtual void RunClientThread()
        {
            this.ClientThread.Start();
        }

        public virtual void WaitClientThreadToStop()
        {
            this.Client.ExitSignal = true; 
            this.ClientThread.Join();

        }
        #endregion

        public  bool Send(byte[] payload)
        {
         
            if (connectNetStreamGlobal == null)
            {
                Thread.Sleep(waitingTime);
                if (connectNetStreamGlobal == null)
                {
                    throw new ClientNotConnectedException(String.Format("After wait time {0}, Connection is not established ",waitingTime));
                }
            }


            
                var StartTime = DateTime.Now;
                int i = 0;
                byte[] readResponse = new byte[10];
                if (!this.Client.ExitSignal)
                {

                    try
                    {

                    if (Client.ClientConnect.Connected == true)
                    {
                       
                        connectNetStreamGlobal.Write(payload, 0, payload.Length);

                     
                        int icount = connectNetStreamGlobal.Read(readResponse, 0, readResponse.Length);

                        string responseCode = Encoding.UTF8.GetString(readResponse, 0, readResponse.Length);
                    

                        if (responseCode != null && !responseCode.Contains("200")) 
                        {

                            LogHandler.LogError(String.Format("Server Connection is down , responce code is {0}", responseCode), LogHandler.Layer.TCPChannelCommunication);
                            return false;

                        }
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(_host))
                            _host = "localhost";
                       
                        throw new ClientDisconnectedException(String.Format("Client {0} has disconnected", _host));
                    }
                    }
                    catch (IOException ex)
                    {
                        throw ex;

                        
                    }
                   

                    i++;
                    var ElapsedTime = DateTime.Now - StartTime;
                    if (ElapsedTime.TotalMilliseconds >= 1000)
                    {
                        Console.WriteLine("Thread: " + Thread.CurrentThread.ManagedThreadId.ToString() + " Messages per second: " + i);
                        i = 0;
                        StartTime = DateTime.Now;
                    }

                }
                return true;

            
        }

        #region Protected Functions
        protected virtual void ConnectionHandler(NetworkStream connectedNetStream)
        {
            if (!connectedNetStream.CanRead && !connectedNetStream.CanWrite)
                return;

            connectNetStreamGlobal = connectedNetStream;

            while (!this.Client.ExitSignal) //Tight network message-loop (optional)
            {
                Thread.Sleep(10);
            }
        }
    }
    #endregion
}

