using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace BlackService
{
    public class ConnectionSetup
    {
        private string ipAddress;

        public string IpAddress
        {
            get{ return ipAddress; }
            private set { ipAddress = value; }
            
        }

        private string publisherPort;

        public string PublisherPort
        {
            get 
            {
               
                return publisherPort;
            }
            private set { publisherPort = value; }
            
           
        }

        private string subscriberPort;

        public string SubscriberPort
        {
            get
            {
                return subscriberPort; 
            }
            private set { subscriberPort = value; }
        }

        public ConnectionSetup()
        {
            ipAddress = new ConfigurationBuilder().AddJsonFile("appsettings.json")
                   .Build().GetSection("IpAddress").Value;

            publisherPort = new ConfigurationBuilder().AddJsonFile("appsettings.json")
                    .Build().GetSection("PublisherPort").Value;
            subscriberPort = new ConfigurationBuilder().AddJsonFile("appsettings.json")
                  .Build().GetSection("SubscriberPort").Value;
        }


    }
}
