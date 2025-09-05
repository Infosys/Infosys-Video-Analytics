/*=============================================================================================================== *
 * Copyright 2025 Infosys Ltd.                                                                                    *
 * Use of this source code is governed by Apache License Version 2.0 that can be found in the LICENSE file or at  *
 * http://www.apache.org/licenses/                                                                                *
 * ===============================================================================================================*/
﻿using Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.TCPChannelCommunication
{
    public class ServerHost
    {
        protected readonly ServerTCPListen Server;
        protected readonly Thread ServerThread;

        public delegate void PayloadHandlerDelegate(byte[] payload);
        public delegate void LogCommunicationDelegate(string message);
        protected readonly PayloadHandlerDelegate OnHandlePayload; 
        protected readonly LogCommunicationDelegate OnHandleLog;
        public ServerHost(PayloadHandlerDelegate payloadHandler, LogCommunicationDelegate logCommunicationHandler, string ipAddress, int port)
        {
            this.OnHandlePayload = payloadHandler ?? throw new ArgumentNullException(nameof(payloadHandler));
            this.OnHandleLog = logCommunicationHandler ?? throw new ArgumentNullException(nameof(logCommunicationHandler));
            this.Server = new ServerTCPListen(this.ConnectionHandler, ipAddress, port); 
            this.ServerThread = new Thread(this.Server.Run);
        }

        public virtual void RunServerThread()
        {
            this.ServerThread.Start();
        }

        public virtual void WaitServerThreadToStop()
        {
            this.Server.ExitSignal = true; 
            this.ServerThread.Join();

        }



        protected virtual void ConnectionHandler(NetworkStream connectedAutoDisposedNetStream)
        {
            try
            {
                if (!connectedAutoDisposedNetStream.CanRead && !connectedAutoDisposedNetStream.CanWrite)
                    return;

                var writerStream = new StreamWriter(connectedAutoDisposedNetStream) { AutoFlush = true };
                var readerStream = new StreamReader(connectedAutoDisposedNetStream);

                var StartTime = DateTime.Now;
                int i = 0;

                while (!this.Server.ExitSignal)
                {

                    Int32 bytesRead;
                    byte[] payloadbytes = new byte[3000000];
                    byte[] totalPayloadRead = new byte[0];
                    byte[] tempData = new byte[0];
                    int sizeOfTotalBytes = 0;
                    while (true)
                    {
                        bytesRead = readerStream.BaseStream.Read(payloadbytes, 0, payloadbytes.Length);
                        
                        this.OnHandleLog.Invoke(String.Format("Received bytes: {0}", bytesRead));

                        if (bytesRead > 0)
                        {
                            if (bytesRead == 4)
                            {
                                this.OnHandleLog.Invoke("4 bytes received");
                                string payloadString = Encoding.UTF8.GetString(payloadbytes.Take(4).ToArray());
                                if (payloadString == "ping")
                                {
                                    this.OnHandleLog.Invoke("ping received");
                                    break;
                                }
                            }

                            byte[] firstTenBytes = new byte[10];
                            firstTenBytes = payloadbytes.Take(10).ToArray<byte>();
                            
                            string sizeString = Encoding.UTF8.GetString(firstTenBytes);
                            this.OnHandleLog.Invoke(String.Format("firstTenBytes: {0}", sizeString));
                            if (int.TryParse(sizeString, out int n))
                            {

                                string firstCharInJson = Encoding.UTF8.GetString(new[] { payloadbytes[10] });
                                this.OnHandleLog.Invoke(String.Format("firstCharInJson: {0}", firstCharInJson));
                                if (firstCharInJson == "{")
                                {
                                    sizeOfTotalBytes = int.Parse(sizeString) + 10;
                                    this.OnHandleLog.Invoke(String.Format("sizeOfTotalBytes in if: {0}", sizeOfTotalBytes));
                                    totalPayloadRead = new byte[0];
                                }

                            }
                           
                            this.OnHandleLog.Invoke(String.Format("sizeOfTotalBytes : {0}", sizeOfTotalBytes));
                            
                            Array.Resize(ref tempData, bytesRead);
                            tempData = payloadbytes.Take(bytesRead).ToArray<byte>();
                            
                            totalPayloadRead = JoinArrays(totalPayloadRead, tempData);
                            
                            
                        }

                        if (totalPayloadRead.Length >= sizeOfTotalBytes && sizeOfTotalBytes > 0)
                        {
                            this.OnHandleLog.Invoke("breaking , sizeOfTotalBytes : " + sizeOfTotalBytes);
                            
                            break;
                        }
                    }
                    this.OnHandleLog.Invoke(String.Format("Sending payload to client , length: {0}", totalPayloadRead.Length));
                    
                    this.OnHandlePayload.Invoke(totalPayloadRead);


                    writerStream.WriteLine("200"); 

                }

                i++;
                var ElapsedTime = DateTime.Now - StartTime;
                if (ElapsedTime.TotalMilliseconds >= 1000)
                {
                    
                    i = 0;
                    StartTime = DateTime.Now;
                }
            }

            catch (Exception ex)
            {
                throw ex;
            }

        }

        private byte[] JoinArrays(byte[] totalData, byte[] tempData)
        {
            
            byte[] outputBytes = totalData.Concat(tempData).ToArray<byte>();
            
            return outputBytes;
        }

    }




}