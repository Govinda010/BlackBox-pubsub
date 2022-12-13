using Opc.Ua;
using Opc.Ua.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace BlackService.OpcUaClient
{
    public class ConnectionStatus
    {
        public ConnectionStatus()
        {
            try
            {
                string AppName = "Arrtsm";//Issusedto

                ApplicationConfiguration config = new ApplicationConfiguration()
                {
                    ApplicationName = AppName,
                    ApplicationUri = Utils.Format(@"urn:{0}:" + AppName, System.Net.Dns.GetHostName()),
                    ApplicationType = ApplicationType.Client,
                    SecurityConfiguration = new SecurityConfiguration
                    {
                        ApplicationCertificate = new CertificateIdentifier
                        {
                            StoreType = @"Directory",
                            StorePath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + @"\Cert\TrustedIssuer",
                            SubjectName = "CN=" + AppName + ", DC=" + System.Net.Dns.GetHostName()
                        },
                        TrustedIssuerCertificates = new CertificateTrustList
                        {
                            StoreType = @"Directory",
                            StorePath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + @"\Cert\TrustedIssuer"
                        },
                        TrustedPeerCertificates = new CertificateTrustList
                        {
                            StoreType = @"Directory",
                            StorePath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + @"\Cert\TrustedIssuer"
                        },
                        RejectedCertificateStore = new CertificateTrustList
                        {
                            StoreType = @"Directory",
                            StorePath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + @"\Cert\RejectedCertificates"
                        },
                        AutoAcceptUntrustedCertificates = true,
                        AddAppCertToTrustedStore = true,
                        RejectSHA1SignedCertificates = false//important
                    },
                    TransportConfigurations = new TransportConfigurationCollection(),
                    TransportQuotas = new TransportQuotas { OperationTimeout = 15000 },
                    ClientConfiguration = new ClientConfiguration { DefaultSessionTimeout = 60000 },
                    TraceConfiguration = new TraceConfiguration
                    {
                        DeleteOnLoad = true
                    },
                    DisableHiResClock = false

                };
                config.Validate(ApplicationType.Client).GetAwaiter().GetResult();

                if (config.SecurityConfiguration.AutoAcceptUntrustedCertificates)
                {
                    config.CertificateValidator.CertificateValidation += (s, ee) =>
                    { ee.Accept = (ee.Error.StatusCode == StatusCodes.BadCertificateUntrusted); };
                }
                var application = new ApplicationInstance
                {
                    ApplicationName = AppName,
                    ApplicationType = ApplicationType.Client,
                    ApplicationConfiguration = config

                };
                //set 0 Trace mask=>stop show log in output window.
                Utils.SetTraceMask(0);//
                application.CheckApplicationInstanceCertificate(true, 2048).GetAwaiter().GetResult();//create certificate




                // create the UA Client object and connect to configured server.
                UAClient uaClient = new UAClient(application.ApplicationConfiguration);
                uAClient = uaClient;


            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }



        private UAClient uAClient;

        public UAClient UAClient
        {
            get { return uAClient; }
            private set { uAClient = value; }
        }


    }
}
