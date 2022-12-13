using BlackService.OpcUaClient;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Opc.Ua.Configuration;
using Opc.Ua;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace BlackService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService<PubSubServerService>();
                    services.AddHostedService<ProjectInfoSubscriber>();
                });

    }     
}
