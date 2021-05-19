using NLog;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Configuration;
using Newtonsoft.Json;
using System.Runtime.InteropServices.ComTypes;
using System.Threading;
using System.Net.Security;

namespace RabbitMQManager
{
    public class RabbitMQManagersApprove
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        MessageHelper mhelper = new MessageHelper();
        ClientLog clientLog = new ClientLog();


        ConnectionFactory factory;
        IConnection connection;
        IModel channel;


        String exchangeName = ConfigurationManager.AppSettings["exchangeName"];//"managerApp1";
        String queueNameGlobal = ConfigurationManager.AppSettings["queueNameGlobal"]; //"managerAppQ1";
        int shortTimeoutSec = int.Parse(ConfigurationManager.AppSettings["shortTimeoutSec"]);

        private readonly object balanceLock = new object();

        public RabbitMQManagersApprove() {
            
        }

        private bool InitConnection(string loginfo) {

            try
            {
                lock (balanceLock)
                {
                    logger.Info("trying to establish connection " + loginfo);
                    factory = new ConnectionFactory()
                    {
                        HostName = ConfigurationManager.AppSettings["hostname"],
                        UserName = ConfigurationManager.AppSettings["usernameRMQ"], //test
                        Password = ConfigurationManager.AppSettings["passwordRMQ"], //test
                        Port = int.Parse(ConfigurationManager.AppSettings["portRMQ"]),//7080
                        VirtualHost = "/"

                    };
                    if (ConfigurationManager.AppSettings["useSSL"] == "true")
                    {
                        // Note: This should NEVER be "localhost"
                        factory.Ssl.ServerName = ConfigurationManager.AppSettings["hostname"];
                        factory.Ssl.Enabled = true;
                        factory.Ssl.AcceptablePolicyErrors = SslPolicyErrors.RemoteCertificateNameMismatch |
                                                    SslPolicyErrors.RemoteCertificateChainErrors;
                    }
                    //factory.AutomaticRecoveryEnabled = true; if I set it true, I cannot create more than 60 simultaneous connections
                    /*
                     * factory.AuthMechanisms = new AuthMechanismFactory[] { new ExternalMechanismFactory() };
                    // Note: This should NEVER be "localhost"
                       factory.Ssl.ServerName = ConfigurationManager.AppSettings["rabbitmqServerName"];
                       factory.Ssl.ServerName = "[certificate cn]";
                    // Path to my .p12 file.
                        factory.Ssl.CertPath = ConfigurationManager.AppSettings["certificateFilePath"];
                    // Passphrase for the certificate file - set through OpenSSL
                        factory.Ssl.CertPassphrase = ConfigurationManager.AppSettings["certificatePassphrase"];
                        factory.Ssl.Enabled = true;
                    // Make sure TLS 1.2 is supported & enabled by your operating system
                        factory.Ssl.Version = SslProtocols.Tls12;
                     */
                    logger.Info("before CreateConnection " + loginfo);
                    connection = factory.CreateConnection();
                    if (connection == null)
                    {
                        logger.Error("connection not created" + loginfo);
                        return false;
                    }
                    logger.Info("before CreateModel " + loginfo);
                    channel = connection.CreateModel();
                    if (channel == null)
                    {
                        logger.Error("channel not created" + loginfo);
                        return false;
                    }


                    IDictionary<String, Object> args = null;
                    if (ConfigurationManager.AppSettings["ttl"] != "none")
                    {

                        args = new Dictionary<String, Object>();
                        args.Add("x-message-ttl", Int32.Parse(ConfigurationManager.AppSettings["ttl"]));
                    }
                    bool durable = true;
                    channel.ExchangeDeclare(exchangeName, ExchangeType.Direct, durable: true);
                    channel.QueueDeclare(queue: queueNameGlobal, durable: durable, exclusive: false, autoDelete: false, arguments: args);

                    channel.QueueBind(queueNameGlobal, exchangeName, queueNameGlobal);
                    logger.Info("connection succeed" + loginfo);
                }
                return true;
            }
            catch (Exception ex) {
                logger.Error(ex, loginfo);
                return false;
            }
        }

        public bool Init(string loginfo) {
            bool result = false;
            try
            {
                result = InitConnection(loginfo);
            }
            catch (RabbitMQ.Client.Exceptions.BrokerUnreachableException ex) {
                logger.Error(ex, loginfo);
                Thread.Sleep(5000);

                //retry
                result = InitConnection(loginfo);
            }
            catch (Exception ex) {
                logger.Error(ex, loginfo);
                return false;
            }
            return result;
        }

        public void BindQueue(string queueName) {
            try
            {
                IDictionary<String, Object> args = null;
                if (ConfigurationManager.AppSettings["ttl"] != "none")
                {

                    args = new Dictionary<String, Object>();
                    args.Add("x-message-ttl", Int32.Parse(ConfigurationManager.AppSettings["ttl"]));
                }
                channel.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: args);
                channel.QueueBind(queueName, exchangeName, queueName);
            }
            catch (Exception ex) {
                logger.Error(ex);
            }
        }

        public void Close() {
            try
            {
                if (channel != null)
                {
                    if (channel.IsOpen)
                        channel.Close();
                    if (connection.IsOpen)
                        connection.Close();
                }
            }
            catch (Exception ex) {
                logger.Error(ex);
            }
        }
        ~RabbitMQManagersApprove()  // finalizer
        {
            Close();
        }
        public void ReceiveMessage() {
            try
            {

                logger.Info(" [*] Receiver Waiting for messages.");

                var consumer = new EventingBasicConsumer(channel);
                var queueName = queueNameGlobal;//channel.QueueDeclare().QueueName;
                logger.Info("queue name " + queueName);
                consumer.Received += ReceiveSubscriber;
                //consumer.Received += (model, ea) =>
                //{
                //    var body = ea.Body.ToArray();
                //    var message = Encoding.UTF8.GetString(body);
                //    Console.WriteLine(" [x] Received {0}", message);
                //};
                channel.BasicConsume(queue: queueName, autoAck: true, consumer: consumer);
            }
            catch (Exception ex) {
                logger.Error(ex);
            }
        }
        public void ReceiveMessageSubscribe(string queueName, System.EventHandler<RabbitMQ.Client.Events.BasicDeliverEventArgs> subscriber)
        {
            try
            {

                //Console.WriteLine(" [*] Receiver Waiting for messages.");
                var consumer = new EventingBasicConsumer(channel);
                //Console.WriteLine("queue name " + queueName);
                consumer.Received += subscriber;
                channel.BasicConsume(queue: queueName, autoAck: true, consumer: consumer);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
        }

        public void SendMessageFromFakeAgent(string message)
        {
            try
            {
                string jsonMessageToSend;
                string queueName;
                mhelper.HandleMessageFakeAgent(message, out jsonMessageToSend, out queueName);
                if (queueName != null)
                {
                    SendMessage(jsonMessageToSend, queueName);
                }

            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }

        }

        private void ReceiveSubscriber(object model, BasicDeliverEventArgs ea) {
            try
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                logger.Info(" [x] Received {0}", message);
                //string jsonMessageToSend, jsonMessageToReturn;
                //List<ManagerQueue> managerQueues = null;
                //string deviceUniqueID;
                //string queueName;
                //bool sendNotification;
                //bool isForwardMessage;
                //bool keepShort;
                //int clientType = 0;
                MessageData messagedata = null;
                mhelper.HandleMessage(ref message, out messagedata);
                if (messagedata.isForwardMessage)
                {   //askApprove or cancel
                    //first add parent (agent record log)
                    Guid ManagersQueueLog_ID = messagedata.agentMessageId;
                    if (messagedata.queueName != null && !String.IsNullOrEmpty(messagedata.jsonMessageToReturn))
                    {
                        SendMessage(messagedata.jsonMessageToReturn, messagedata.queueName, 0, messagedata.keepShort);
                        mhelper.AddLog(ManagersQueueLog_ID, null, message, messagedata.jsonMessageToReturn, messagedata.queueName, messagedata.deviceUniqueID, messagedata.clientType, messagedata.command, messagedata.agentId, messagedata.agentName, messagedata.employeeId, messagedata.activityCode, messagedata.activityDescription, messagedata.managerEmployeeId, messagedata.managerName, messagedata.subject, messagedata.managerQueues.Count, null, null);
                    }
                    if (messagedata.managerQueues != null && messagedata.managerQueues.Count > 0)
                    {
                        //if (ManagersQueueLog_ID != null)
                        //    mhelper.AddLogDevices(ManagersQueueLog_ID.Value, messagedata.managerQueues);
                        foreach (var managerQueue in messagedata.managerQueues)
                        {
                            SendMessage(messagedata.jsonMessageToSend, managerQueue.QueueName);
                            Guid ManagersQueueLog_ID2 = Guid.NewGuid();
                            mhelper.AddLog(ManagersQueueLog_ID2, ManagersQueueLog_ID, message, messagedata.jsonMessageToSend, managerQueue.QueueName, managerQueue.DeviceID, 1, messagedata.command, messagedata.agentId, messagedata.agentName, messagedata.employeeId, messagedata.activityCode, messagedata.activityDescription, messagedata.managerEmployeeId, messagedata.managerName, messagedata.subject, null, null, null);
                            if (messagedata.sendNotification)
                            {
                                string notificationMessageManagerApprove = ConfigurationManager.AppSettings["notificationMessageManagerApprove"];
                                mhelper.CallFCMNode(ManagersQueueLog_ID2, messagedata.managerEmployeeId, managerQueue.PushAddr, notificationMessageManagerApprove, managerQueue.DeviceID);
                                if (!String.IsNullOrEmpty(managerQueue.PushQueueName))
                                    SendMessage(messagedata.jsonMessagePush, managerQueue.PushQueueName);
                            }
                        }
                    }
                    
                }
                else //all other cases - just return a message to queueName
                {
                    if (messagedata.queueName != null)
                    {
                        SendMessage(messagedata.jsonMessageToSend, messagedata.queueName, 0, messagedata.keepShort);
                        Guid ManagersQueueLog_ID3 = Guid.NewGuid();
                        if (messagedata.command == "sendLog")
                        {
                            mhelper.AddLog(ManagersQueueLog_ID3, null, "sendLog", messagedata.jsonMessageToSend, messagedata.queueName, messagedata.deviceUniqueID, messagedata.clientType, messagedata.command, messagedata.agentId, messagedata.agentName, messagedata.employeeId, messagedata.activityCode, messagedata.activityDescription, messagedata.managerEmployeeId, messagedata.managerName, messagedata.subject, null, null, null);
                            clientLog.SaveFile(messagedata.fileContent, messagedata.fileName);
                        }
                        else
                            if (messagedata.command == "received")
                        {
                            Guid? agentLogID = Guid.Parse(messagedata.AgentMessageId); 
                            mhelper.AddLog(ManagersQueueLog_ID3, agentLogID, message, messagedata.jsonMessageToSend, messagedata.queueName, messagedata.deviceUniqueID, messagedata.clientType, messagedata.command, messagedata.agentId, messagedata.agentName, messagedata.employeeId, messagedata.activityCode, messagedata.activityDescription, messagedata.managerEmployeeId, messagedata.managerName, messagedata.subject, null, messagedata.managerEmployeeId, messagedata.requestStatus);
                        }
                        else
                            mhelper.AddLog(ManagersQueueLog_ID3, null, message, messagedata.jsonMessageToSend, messagedata.queueName, messagedata.deviceUniqueID, messagedata.clientType, messagedata.command, messagedata.agentId, messagedata.agentName, messagedata.employeeId, messagedata.activityCode, messagedata.activityDescription, messagedata.managerEmployeeId, messagedata.managerName, messagedata.subject, null, null, messagedata.requestStatus);
                    }
                }
                
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }

        }
       
        public void SendMessage(String messageToSend, string queueName, int testNumber=0, bool keepShort=false) {
            try
            {
                string message = messageToSend;
                var body = Encoding.UTF8.GetBytes(message);
                
                if (!String.IsNullOrEmpty(queueName))
                {
                    IBasicProperties props = null;
                    if (keepShort)
                    {
                        props = channel.CreateBasicProperties();
                        props.ContentType = "text/plain";
                        props.DeliveryMode = 2;
                        props.Expiration = (shortTimeoutSec * 1000).ToString();//only 10 seconds live
                    }
                    channel.BasicPublish(exchange: exchangeName,
                                         routingKey: queueName,
                                         basicProperties: props,
                                         body: body);
                    var loggerMessage = $" [x] Sent {message} to exchange {exchangeName}, queue {queueName}, testNumber {testNumber}";
                    logger.Info(loggerMessage);
                    System.Diagnostics.Debug.WriteLine(loggerMessage);
                }
                else
                { logger.Error("queueName is empty"); }
            }
            catch (Exception ex) {
                logger.Error(ex);
            }
        }

        public int TestNotification(string ManagerEmployeeId, string message) {
            List<ManagerQueue> managerQueues = mhelper.GetManagerQueues(ManagerEmployeeId);
            foreach (ManagerQueue queue in managerQueues) {
                mhelper.CallFCMNode(Guid.NewGuid(), ManagerEmployeeId, queue.PushAddr, message, queue.DeviceID);
            }
            return managerQueues.Count;
        }

        public void TestFCMMessage(string pushAddress, string message) {
            mhelper.CallFCMNode(Guid.NewGuid(), "123", pushAddress, message, "");
        }

        public List<string> GetAllManagers(int numManagers)
        {
            return mhelper.GetAllManagers(numManagers);
        }

        public List<Agent> GetAllAgents()
        {
            return mhelper.GetAllAgents();
        }

    }
}
