using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Collections;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace EyeXTcpServer
{

    public delegate void ClientMessageHandler(object sender, JObject json);

    class JsonTcpServer
    {
        private TcpListener tcpListener;
        private Thread listenThread;
        private int serverPort;
        private List<TcpClient> connectedClients = new List<TcpClient>();

        public event ClientMessageHandler ClientMessageReceieved;

        public JsonTcpServer(int port)
        {
            this.serverPort = port;
            this.tcpListener = new TcpListener(IPAddress.Any, serverPort);
            this.listenThread = new Thread(new ThreadStart(ListenForClients));
            this.listenThread.Start();
        }

        public void sendToAllClients(JObject jobject)
        {
            lock (connectedClients)
            {
                foreach (TcpClient connectedClient in connectedClients)
                {
                    // TODO Check accessing of a disposed object here
                    try
                    {
                        NetworkStream ns = connectedClient.GetStream();
                        string strResponse = JsonConvert.SerializeObject(jobject, Formatting.None) + "\r\n";
                        //string strResponse = jobject.ToString() + "\r\n";
                        byte[] b = Encoding.ASCII.GetBytes(strResponse);
                        ns.Write(b, 0, b.Length);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }
            }
        }

        private void ListenForClients()
        {
            this.tcpListener.Start();
            Console.WriteLine("TCP Server Started");

            while (true)
            {
                //blocks until a client has connected to the server
                TcpClient client = this.tcpListener.AcceptTcpClient();
                Console.WriteLine("TCP Server: Client Connected");
                //create a thread to handle communication 
                //with connected client
                Thread clientThread = new Thread(new ParameterizedThreadStart(HandleClientComm));
                clientThread.Start(client);
            }
        }

        private void HandleClientComm(object client)
        {
            TcpClient tcpClient = (TcpClient)client;
            NetworkStream clientStream = tcpClient.GetStream();
            lock (connectedClients)
            {
                this.connectedClients.Add(tcpClient);
            }

            byte[] bytes = new byte[4096];
            int bytesRead;
            string message = null;

            while (true)
            {
                bytesRead = 0;

                try
                {
                    //blocks until a client sends a message
                    bytesRead = clientStream.Read(bytes, 0, 4096);
                    message += Encoding.ASCII.GetString(bytes, 0, bytesRead);

                    if (message.IndexOf("\n") > -1)
                    {
                        //break;
                        //handle Command
                        Console.WriteLine("Command: " + message);
                        try
                        {

                            
                            JObject json = JObject.Parse(message);
                            
                            // Pass to handler
                            if (ClientMessageReceieved != null)
                            {
                                ClientMessageReceieved(this, json);
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("JSON Parse Error Occured");
                            Console.WriteLine(e.Message);
                        }
                        message = null;

                    }

                }
                catch (Exception e)
                {
                    //a socket error has occured
                    Console.WriteLine("Socket Error Occured");
                    Console.WriteLine(e.Message);
                    break;
                }

                if (bytesRead == 0)
                {
                    //the client has disconnected from the server
                    break;
                }

                //message has successfully been received
                // ASCIIEncoding encoder = new ASCIIEncoding();
                //System.Diagnostics.Debug.WriteLine(encoder.GetString(data, 0, bytesRead));
            }

            this.connectedClients.Remove(tcpClient);
            tcpClient.Close();
        }
    }

}
