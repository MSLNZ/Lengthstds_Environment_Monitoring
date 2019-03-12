﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Windows.Forms;
using System.Threading;

namespace Temperature_Monitor
{
    public class ClientSocket
    {
        private TcpClient client;
        private NetworkStream stream;
        private int timeout = 10000; //The default timeout
        bool just_connected = false;

        public ClientSocket()
        {
            client = new TcpClient();
        }

        public int Timeout
        {
            get { return timeout; }
            set { timeout = value; }
        }
        public string GetHostnameFromIp(string ipAddress)
        {
            try
            {
                IPHostEntry entry = Dns.GetHostEntry(ipAddress);
                if (entry != null)
                {
                    return entry.HostName;
                }
            }
            catch (SocketException)
            {
                return null;
                //unknown host or
                //not every IP has a name
                //log exception (manage it)
            }
            return null;
        }

        public bool Connect(String server, int port)
        {
            try
            {
                //client = new HttpClient();
                //client.BaseAddress = new Uri("http://" + server + ":2000/");
                //client.DefaultRequestHeaders.Accept.Clear();
                //client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("-r -S "));
                // Create a TcpClient. 
                // Note, for this client to work you need to have a TcpServer  
                // connected to the same address as specified by the server, port 
                // combination.
                // Connect to the specified host.

                client = new TcpClient();

                //get IP addresses. 1st address is ip6, 2nd is ip4
                //if there is only one address returned then it is ip4
                IPAddress ip4;

                IPAddress[] IPAddresses = Dns.GetHostAddresses(server);
                
                
                if (IPAddresses.Length == 2)
                {
                    ip4 = IPAddresses[1];
                }
                else ip4 = IPAddresses[0];




                client.Connect(ip4, port);

                just_connected = client.Connected;

                return just_connected;

            }

            catch (SocketException e)
            {
                string error = e.ToString();
                return false;
            }
            
        }

        public bool Connect(IPAddress server, int port)
        {
            try
            {
                client = new TcpClient();
                
                IPEndPoint ipEndPoint = new IPEndPoint(server, port);
                client.Connect(ipEndPoint);

                just_connected = client.Connected;
                return just_connected;

            }

            catch (SocketException e)
            {
                string error = e.ToString();
                return false;
            }

        }

        public bool IsConnected()
        {
            try
            {
                return client.Connected;
            }
            catch (SocketException)
            {
                return false;
            }
            catch (NullReferenceException)
            {
                return false;
            }
        }

        public bool SendReceiveData(String request, ref string result)
        {
            try
            {
                // Translate the passed message into ASCII and store it as a Byte array.
                Byte[] data = System.Text.Encoding.ASCII.GetBytes(request);

                // Get a client stream for reading and writing. 
                //  Stream stream = client.GetStream();
                client.SendTimeout = 1000;
                stream = client.GetStream();

                // Send the message to the connected TcpServer. 
                stream.Write(data, 0, data.Length);

                //Console.WriteLine("Sent: {0}", request);

                // Receive the TcpServer.response. 

                // Buffer to store the response bytes.
                data = new Byte[256];



                stream.ReadTimeout = timeout;
                // Read the first batch of the TcpServer response bytes.
                Int32 bytes = stream.Read(data, 0, data.Length);
                result = System.Text.Encoding.ASCII.GetString(data, 0, bytes);
                //Console.WriteLine("Received: {0}", responseData);
                return true;
            }
            catch (ArgumentNullException)
            {
                return false;
            }
            catch (System.IO.IOException)
            {
                return false;
            }
            catch (TimeoutException)
            {
                return false;
            }

        }
        public bool SendReceiveData(String request, ref string result,bool special_encoding)
        {
            try
            {
                client.SendTimeout = 1000;
                stream = client.GetStream();

                Byte[]  data = new Byte[60];
                Int32 bytes;
                if (just_connected)
                {
                    bytes = stream.Read(data, 0, data.Length); //read the connection string
                    just_connected = false;
                }

                // Translate the passed message into ASCII and store it as a Byte array.
                data = System.Text.Encoding.ASCII.GetBytes(request);
               
                stream.Write(data, 0, data.Length);

                Thread.Sleep(1000);
            
                // Buffer to store the response bytes.
                data = new Byte[60];



                stream.ReadTimeout = timeout;
                //  stream.BeginRead(
                // Read the first batch of the TcpServer response bytes.
                bytes = stream.Read(data, 0, data.Length);
                result = Encoding.ASCII.GetString(data, 0, bytes);


                //Console.WriteLine("Received: {0}", responseData);
                return true;
            }
            catch (ArgumentNullException)
            {
                return false;
            }
            catch (System.IO.IOException)
            {
                return false;
            }
            catch (TimeoutException)
            {
                return false;
            }

        }
        public bool CloseConnection()
        {
            try
            {
                // Close everything.
                stream.Close();
                client.Close();
                return true;
            }
            catch (SocketException)
            {
                return false;
            }
        }

        public string GetHostName(string ipAddress)
        {
            try
            {
                IPHostEntry entry = Dns.GetHostEntry(ipAddress);
                if (entry != null)
                {
                    return entry.HostName;
                }
            }
            catch (SocketException)
            {
                //unknown host or
                //not every IP has a name
                //log exception (manage it)
            }

            return null;
        }
        public TcpClient GetClient
        {
            get { return client; }
        }

    }
}
