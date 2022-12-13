using BlackService.OpcUaClient;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BlackService
{
    public class ProjectInfoSubscriber : BackgroundService
    {
        Socket _client;
        EndPoint _remoteEndPoint;
        byte[] _data;
        int _recv;
        Boolean _isReceivingStarted = false;
        string serverIP = "127.0.0.1";
        string topicName = "Project_Info";
        ConnectionStatus connectionStatus;
        string Command = "Subscribe";

        public ProjectInfoSubscriber()
        {
            // Setup Address
            ConnectionSetup connectionSetup = new ConnectionSetup();
            serverIP = connectionSetup.IpAddress;
            var port = connectionSetup.SubscriberPort;

            connectionStatus = new ConnectionStatus();

            IPAddress serverIPAddress = IPAddress.Parse(serverIP);
            int serverPort = Convert.ToInt32(port);

            _client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            _remoteEndPoint = new IPEndPoint(serverIPAddress, serverPort);

            

            Console.WriteLine(connectionStatus.UAClient);
            Console.WriteLine(connectionStatus.UAClient.Connect());
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {           

            string message = Command + "|" + topicName;
            _client.SendTo(Encoding.ASCII.GetBytes(message), _remoteEndPoint);
            _data = new byte[1024];

            while (!stoppingToken.IsCancellationRequested)
            {
                EndPoint publisherEndPoint = _client.LocalEndPoint;
                _recv = _client.ReceiveFrom(_data, ref publisherEndPoint);
                if (_recv != 0)
                {
                    string msg = Encoding.ASCII.GetString(_data, 0, _recv);// + "," + publisherEndPoint.ToString();
                    string[] words = msg.Split('|');
                    foreach (var word in words)
                    {
                        Console.WriteLine(word);
                        if (string.Equals(word, "apple"))
                        {
                            //connectionStatus.UAClient.Connect();
                            Console.WriteLine("apple");
                            Thread.Sleep(2000);
                        }
                    }
                    //Console.WriteLine(msg);
                    Console.WriteLine();
                }

                await Task.Delay(1000, stoppingToken);
            }

        }

       
    }
}
