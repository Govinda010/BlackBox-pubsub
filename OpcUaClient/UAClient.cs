/* ========================================================================
 * Copyright (c) 2005-2020 The OPC Foundation, Inc. All rights reserved.
 *
 * OPC Foundation MIT License 1.00
 * 
 * Permission is hereby granted, free of charge, to any person
 * obtaining a copy of this software and associated documentation
 * files (the "Software"), to deal in the Software without
 * restriction, including without limitation the rights to use,
 * copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the
 * Software is furnished to do so, subject to the following
 * conditions:
 * 
 * The above copyright notice and this permission notice shall be
 * included in all copies or substantial portions of the Software.
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
 * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
 * OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 * NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
 * HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
 * WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
 * OTHER DEALINGS IN THE SOFTWARE.
 *
 * The complete license agreement can be found here:
 * http://opcfoundation.org/License/MIT/1.00/
 * ======================================================================*/

using System;
using System.Collections;
using System.IO;
using System.Threading.Tasks;
using Opc.Ua;
using Opc.Ua.Client;

namespace BlackService.OpcUaClient
{
    /// <summary>
    /// OPC UA Client with examples of basic functionality.
    /// </summary>
    public class UAClient 
    {
        #region Private Fields
        private ApplicationConfiguration m_configuration;
        private string m_serverUrl = "opc.tcp://192.168.214.1:4840";// "opc.tcp://192.168.30.108:4840";
        private Session m_session;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the UAClient class.
        /// </summary>
        public UAClient(ApplicationConfiguration configuration)
        {
            m_configuration = configuration;
            m_configuration.CertificateValidator.CertificateValidation += CertificateValidation;
        }
        #endregion


        #region Public Properties
        /// <summary>
        /// Gets the client session.
        /// </summary>
        public Session Session => m_session;

        /// <summary>
        /// Gets or sets the server URL.
        /// </summary>
        public string ServerUrl
        {
            get
            {
                return m_serverUrl;
            }
            set
            {
                m_serverUrl = value;
            }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Creates a session with the UA server
        /// </summary>
        public bool Connect()
        {
            bool returnValue = false;
            bool tryAgain = true;
            while (tryAgain)
            {
                try
                {
                    if (m_session != null && m_session.Connected == true)
                    {
                        Console.WriteLine("Session already connected!");
                    }
                    else
                    {
                        Console.WriteLine("Connecting...");

                        // Get the endpoint by connecting to server's discovery endpoint.
                        // Try to find the first endopint without security.
                        EndpointDescription endpointDescription = CoreClientUtils.SelectEndpoint(m_serverUrl, true);

                        EndpointConfiguration endpointConfiguration = EndpointConfiguration.Create(m_configuration);
                        ConfiguredEndpoint endpoint = new ConfiguredEndpoint(null, endpointDescription, endpointConfiguration);

                        // Create the session
                        Session session = Session.Create(
                            m_configuration,
                            endpoint,
                            false,
                            false,
                            m_configuration.ApplicationName,
                            30 * 60 * 1000,
                            new UserIdentity("OpcUaClient", "Sunrise010##"),
                            null).Result;

                        // Assign the created session
                        if (session != null && session.Connected)
                        {
                            m_session = session;
                        }
                        tryAgain = false;

                    }

                    returnValue = true;
                }
                catch (Exception ex)
                {
                    // Log Error
                    //string logMessage = String.Format("Create Session Error : {0}.", ex.Message);
                    //Console.WriteLine(logMessage);
                    returnValue = false;
                }
            }
            return returnValue;
        }

        /// <summary>
        /// Disconnects the session.
        /// </summary>
        public void Disconnect()
        {
            try
            {
                if (m_session != null)
                {
                    Console.WriteLine("Disconnecting...");

                    m_session.Close();
                    m_session.Dispose();
                    m_session = null;

                    // Log Session Disconnected event
                    //string logMessage = String.Format("Session Disconnected.");
                    //Console.WriteLine(logMessage);
                }
                else
                {
                    Console.WriteLine("Session not created!");
                }
            }
            catch (Exception ex)
            {
                // Log Error
                //string logMessage = String.Format("Disconnect Error : {0}.", ex.Message);
                //Console.WriteLine(logMessage);
            }
        }

        // <summary>
        // Create Subscription and MonitoredItems for DataChanges
        // </summary>
        public void SubscribeToDataChanges()
        {
            if (m_session == null || m_session.Connected == false)
            {
                Console.WriteLine("Session not connected!");
                return;
            }

            try
            {
                // Create a subscription for receiving data change notifications

                // Define Subscription parameters
                Subscription subscription = new Subscription(m_session.DefaultSubscription);

                subscription.DisplayName = "Console ReferenceClient Subscription";
                subscription.PublishingEnabled = true;
                subscription.PublishingInterval = 1;

                m_session.AddSubscription(subscription);

                // Create the subscription on Server side
                subscription.Create();
                Console.WriteLine("New Subscription created with SubscriptionId = {0}.", subscription.Id);

                // Create MonitoredItems for data changes

                MonitoredItem intMonitoredItem = new MonitoredItem(subscription.DefaultItem);
                // Watch tool change node
                intMonitoredItem.StartNodeId = new NodeId("ns=2;s=/NC/_N_NC_GD4_ACX/_VENTILE_SIGNAL");
                intMonitoredItem.AttributeId = Attributes.Value;
                intMonitoredItem.DisplayName = "Int32 Variable";
                intMonitoredItem.SamplingInterval = 10;
                intMonitoredItem.Notification += OnMonitoredItemNotification;

                subscription.AddItem(intMonitoredItem);
                subscription.ApplyChanges();




            }
            catch (Exception ex)
            {
                //Console.WriteLine("Subscribe error: {0}", ex.Message);
                Connect();
            }

        }
        #endregion


        #region Private Methods

        /// <summary>
        /// Handle DataChange notifications from Server
        /// </summary>
        private void OnMonitoredItemNotification(MonitoredItem monitoredItem, MonitoredItemNotificationEventArgs e)
        {
            try
            {
                ReadVentileSignal();
            }
            catch (Exception ex)
            {
                Console.WriteLine("OnMonitoredItemNotification error: {0}", ex.Message);
                Connect();
            }
        }



        /// <summary>
        /// Handles the certificate validation event.
        /// This event is triggered every time an untrusted certificate is received from the server.
        /// </summary>
        private void CertificateValidation(CertificateValidator sender, CertificateValidationEventArgs e)
        {
            bool certificateAccepted = true;

            // ****
            // Implement a custom logic to decide if the certificate should be accepted or not and set certificateAccepted flag accordingly.
            // The certificate can be retrieved from the e.Certificate field
            // ***

            if (certificateAccepted)
            {
                Console.WriteLine("Untrusted Certificate accepted. SubjectName = {0}", e.Certificate.SubjectName);
            }

            e.Accept = certificateAccepted;
        }

        private void ReadVentileSignal()
        {
            bool vetileSignal;
            if (m_session == null || m_session.Connected == false)
            {
                Console.WriteLine("Session not connected!");
            }

            try
            {

                // build a list of nodes to be read
                ReadValueIdCollection nodesToRead = new ReadValueIdCollection()
                    {
                        //Tool Name in spindle
                        new ReadValueId() { NodeId = "ns=2;s=/NC/_N_NC_GD4_ACX/_VENTILE_SIGNAL",AttributeId = Attributes.Value},
                    };
                // Read the node attributes
                DataValueCollection resultsValues = null;
                DiagnosticInfoCollection diagnosticInfos = null;


                // Call Read Service
                m_session.Read(
                    null,
                    0,
                    TimestampsToReturn.Both,
                    nodesToRead,
                    out resultsValues,
                    out diagnosticInfos);

                // Validate the results
                ClientBase.ValidateResponse(resultsValues, nodesToRead);
                vetileSignal = Convert.ToBoolean(resultsValues[0].Value);
                if (vetileSignal)
                {
                    ReadGUDNode();
                }
                else
                {
                    ResetDB();
                }





            }
            catch (Exception ex)
            {
                // Log Error
                //string logMessage = String.Format("Read Nodes Error : {0}.", ex.Message);
                //Console.WriteLine(logMessage);
                Connect();
            }
        }

        private void WriteNodes(DataValueCollection resultsValues)
        {
            int count = 0;
            if (m_session == null || m_session.Connected == false)
            {
                Console.WriteLine("Session not connected!");
                return;
            }

            try
            {
                // Write the configured nodes
                WriteValueCollection nodesToWrite = new WriteValueCollection();

                // Display the results.
                foreach (DataValue result in resultsValues)
                {
                    string nodeId = String.Format("ns=2;s=/Plc/DB104.DBB{0}", count.ToString());
                    WriteValue byteWriteVal = new WriteValue();
                    byteWriteVal.NodeId = new NodeId(nodeId);
                    byteWriteVal.AttributeId = Attributes.Value;
                    byteWriteVal.Value.Value = Convert.ToByte(result.Value);
                    nodesToWrite.Add(byteWriteVal);
                    count++;
                }
                WriteValue ventileReset = new WriteValue();
                ventileReset.NodeId = new NodeId("ns=2;s=/Plc/DB104.DBX13.2");
                ventileReset.AttributeId = Attributes.Value;
                ventileReset.Value.Value = Convert.ToByte(0);
                nodesToWrite.Add(ventileReset);

                WriteValue ventileOkSet = new WriteValue();
                ventileOkSet.NodeId = new NodeId("ns=2;s=/NC/_N_NC_GD4_ACX/_VENTILE_OK");
                ventileOkSet.AttributeId = Attributes.Value;
                ventileOkSet.Value.Value = Convert.ToByte(1);
                nodesToWrite.Add(ventileReset);

                WriteValue ventileSet = new WriteValue();
                ventileSet.NodeId = new NodeId("ns=2;s=/Plc/DB104.DBX13.1");
                ventileSet.AttributeId = Attributes.Value;
                ventileSet.Value.Value = Convert.ToByte(1);
                nodesToWrite.Add(ventileSet);

                // Write the node attributes
                StatusCodeCollection results = null;
                DiagnosticInfoCollection diagnosticInfos;
                Console.WriteLine("Writing nodes...");

                RequestHeader requestHeader = new RequestHeader();
                requestHeader.AuditEntryId = "OpcUaClient";
                requestHeader.GetHashCode();

                // Call Write Service
                m_session.Write(null,
                                nodesToWrite,
                                out results,
                                out diagnosticInfos);

                // Validate the response
                ClientBase.ValidateResponse(results, nodesToWrite);

                // Display the results.
                Console.WriteLine("Write Results :");

                foreach (StatusCode writeResult in results)
                {
                    Console.WriteLine("     {0}", writeResult);
                }
            }
            catch (Exception ex)
            {
                // Log Error
                string logMessage = String.Format("Write Nodes Error : {0}.", ex.Message);
                Console.WriteLine(logMessage);
                Connect();
            }
        }

        private void ResetDB()
        {
            if (m_session == null || m_session.Connected == false)
            {
                Console.WriteLine("Session not connected!");
                return;
            }

            try
            {
                // Write the configured nodes
                WriteValueCollection nodesToWrite = new WriteValueCollection();

                // Display the results.
                for (int count = 0; count < 13; count++)
                {
                    string nodeId = String.Format("ns=2;s=/Plc/DB104.DBB{0}", count.ToString());
                    WriteValue byteWriteVal = new WriteValue();
                    byteWriteVal.NodeId = new NodeId(nodeId);
                    byteWriteVal.AttributeId = Attributes.Value;
                    byteWriteVal.Value.Value = Convert.ToByte(0);
                    nodesToWrite.Add(byteWriteVal);
                }
                WriteValue ventileReset = new WriteValue();
                ventileReset.NodeId = new NodeId("ns=2;s=/Plc/DB104.DBX13.2");
                ventileReset.AttributeId = Attributes.Value;
                ventileReset.Value.Value = Convert.ToByte(1);
                nodesToWrite.Add(ventileReset);

                WriteValue ventileOkSet = new WriteValue();
                ventileOkSet.NodeId = new NodeId("ns=2;s=/NC/_N_NC_GD4_ACX/_VENTILE_OK");
                ventileOkSet.AttributeId = Attributes.Value;
                ventileOkSet.Value.Value = Convert.ToByte(0);
                nodesToWrite.Add(ventileReset);

                WriteValue ventileSet = new WriteValue();
                ventileSet.NodeId = new NodeId("ns=2;s=/Plc/DB104.DBX13.1");
                ventileSet.AttributeId = Attributes.Value;
                ventileSet.Value.Value = Convert.ToByte(0);
                nodesToWrite.Add(ventileSet);

                // Write the node attributes
                StatusCodeCollection results = null;
                DiagnosticInfoCollection diagnosticInfos;
                Console.WriteLine("Writing nodes...");

                RequestHeader requestHeader = new RequestHeader();
                requestHeader.AuditEntryId = "OpcUaClient";
                requestHeader.GetHashCode();

                // Call Write Service
                m_session.Write(null,
                                nodesToWrite,
                                out results,
                                out diagnosticInfos);

                // Validate the response
                ClientBase.ValidateResponse(results, nodesToWrite);

                // Display the results.
                Console.WriteLine("Write Results :");

                foreach (StatusCode writeResult in results)
                {
                    Console.WriteLine("     {0}", writeResult);
                }
            }
            catch (Exception ex)
            {
                // Log Error
                //string logMessage = String.Format("Write Nodes Error : {0}.", ex.Message);
                //Console.WriteLine(logMessage);
                Connect();
            }
        }
        private void ReadGUDNode()
        {
            if (m_session == null || m_session.Connected == false)
            {
                Console.WriteLine("Session not connected!");
            }
            try
            {
                ReadValueIdCollection nodesToRead = new ReadValueIdCollection()
                {
                    // Read the tool number of place in magazine
                        new ReadValueId() { NodeId = "ns=2;s=/NC/_N_NC_GD4_ACX/_VENTILE[1]", AttributeId = Attributes.Value },
                        new ReadValueId() { NodeId = "ns=2;s=/NC/_N_NC_GD4_ACX/_VENTILE[2]", AttributeId = Attributes.Value },
                        new ReadValueId() { NodeId = "ns=2;s=/NC/_N_NC_GD4_ACX/_VENTILE[3]", AttributeId = Attributes.Value },
                        new ReadValueId() { NodeId = "ns=2;s=/NC/_N_NC_GD4_ACX/_VENTILE[4]", AttributeId = Attributes.Value },
                        new ReadValueId() { NodeId = "ns=2;s=/NC/_N_NC_GD4_ACX/_VENTILE[5]", AttributeId = Attributes.Value },
                        new ReadValueId() { NodeId = "ns=2;s=/NC/_N_NC_GD4_ACX/_VENTILE[6]", AttributeId = Attributes.Value },
                        new ReadValueId() { NodeId = "ns=2;s=/NC/_N_NC_GD4_ACX/_VENTILE[7]", AttributeId = Attributes.Value },
                        new ReadValueId() { NodeId = "ns=2;s=/NC/_N_NC_GD4_ACX/_VENTILE[8]", AttributeId = Attributes.Value },
                        new ReadValueId() { NodeId = "ns=2;s=/NC/_N_NC_GD4_ACX/_VENTILE[9]", AttributeId = Attributes.Value },
                        new ReadValueId() { NodeId = "ns=2;s=/NC/_N_NC_GD4_ACX/_VENTILE[10]", AttributeId = Attributes.Value },
                        new ReadValueId() { NodeId = "ns=2;s=/NC/_N_NC_GD4_ACX/_VENTILE[11]", AttributeId = Attributes.Value },
                        new ReadValueId() { NodeId = "ns=2;s=/NC/_N_NC_GD4_ACX/_VENTILE[12]", AttributeId = Attributes.Value },
                        new ReadValueId() { NodeId = "ns=2;s=/NC/_N_NC_GD4_ACX/_VENTILE[13]", AttributeId = Attributes.Value }

                };
                // Read the node attributes
                DataValueCollection resultsValues = null;
                DiagnosticInfoCollection diagnosticInfos = null;


                // Call Read Service
                m_session.Read(
                    null,
                    0,
                    TimestampsToReturn.Both,
                    nodesToRead,
                    out resultsValues,
                    out diagnosticInfos);

                // Validate the results
                ClientBase.ValidateResponse(resultsValues, nodesToRead);
                WriteNodes(resultsValues);
                // Display the results.
                foreach (DataValue result in resultsValues)
                {
                    Console.WriteLine("Read Value = {0} , StatusCode = {1}", result.Value, result.StatusCode);
                }


            }
            catch (Exception ex)
            {
                // Log Error
                //string logMessage = String.Format("Read Nodes Error : {0}.", ex.Message);
                //Console.WriteLine(logMessage);
                Connect();
            }
        }
        #endregion  
    }
}

