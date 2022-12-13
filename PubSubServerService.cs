using BlackService.PubSubServer;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BlackService
{
    public class PubSubServerService : BackgroundService
    {
        private readonly ILogger<PubSubServerService> _logger;

        public PubSubServerService(ILogger<PubSubServerService> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Run(() =>
            {
                HostPublishSubscribeServices();
            });
           
        }

        private static void HostPublishSubscribeServices()
        {
            SubscriberService subscriberService = new SubscriberService();
            subscriberService.StartSubscriberService();

            PublisherService publisherService = new PublisherService();
            publisherService.StartPublisherService();
        }
    }
}
