using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace BlackService.PubSubServer
{
    public class WorkerThreadParameters
    {
        Socket _server;

        public Socket Server
        {
            get { return _server; }
            set { _server = value; }
        }
        string _message;

        public string Message
        {
            get { return _message; }
            set { _message = value; }
        }
        List<EndPoint> _subscriberListForThisTopic;

        public List<EndPoint> SubscriberListForThisTopic
        {
            get { return _subscriberListForThisTopic; }
            set { _subscriberListForThisTopic = value; }
        }
    }
}
