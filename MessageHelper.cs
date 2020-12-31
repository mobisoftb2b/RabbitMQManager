using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using Newtonsoft.Json;
using NLog;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace RabbitMQManager
{
    public class MessageHelper
    {
        ManagersDB dbManager = new ManagersDB();
        private static Logger logger = LogManager.GetCurrentClassLogger();
        int historyDays = 7;
        public MessageHelper() {
            historyDays = int.Parse(ConfigurationManager.AppSettings["historyDays"]);
        }
        public void HandleMessageFakeAgent(string message, out string jsonMessageToSend, out string queueName) {
            jsonMessageToSend = "";
            queueName = null;
            try
            {
                dynamic jsonMessage = JsonConvert.DeserializeObject(message);
                if (jsonMessage.command != null) {
                    if ((String)jsonMessage.command == "askApprove")
                    {
                        jsonMessageToSend = message;
                        dynamic consumerToken = (dynamic)jsonMessage.consumerToken;
                        queueName = ((dynamic)consumerToken[0]).queueName;
                        string managerId = ((string)jsonMessage.ManagerEmployeeId).Replace("'", "");
                        string RequestID = (string)jsonMessage.RequestID;
                        jsonMessageToSend = ArrangeMessage_FromFakeAgent("answer from agent", managerId, RequestID);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
        }

        public void HandleMessage(string message, out MessageData messagedata) {
            messagedata = new MessageData();
            messagedata.jsonMessageToSend = "";
            messagedata.managerQueues = new List<ManagerQueue>();
            messagedata.queueName = null;
            string queueName = null;
            string deviceUniqueID = "";
            messagedata.deviceUniqueID = "";
            messagedata.sendNotification = false;
            messagedata.isForwardMessage = false;
            messagedata.jsonMessageToReturn = "";
            messagedata.keepShort = false;
            messagedata.clientType = 0; //1 - manager, 2 - agent

            try
            {
                dynamic jsonMessage = JsonConvert.DeserializeObject(message);

                if (jsonMessage.command != null)
                {
                    object data;
                    //string managerId;
                    messagedata.testNumber = "0";
                    string messageId = "";
                    int status = 0;
                    messagedata.command = (String)jsonMessage.command;
                    logger.Info(messagedata.command);
                    switch (messagedata.command)
                    {
                        case "loginUser":
                            messagedata.clientType = 1;
                            if (jsonMessage.QueueName != null)
                                messagedata.queueName = (string)jsonMessage.QueueName;
                            messagedata.deviceUniqueID = ((dynamic)jsonMessage.deviceInfo).DeviceUniqueID;
                            messagedata.managerEmployeeId = (string)jsonMessage.ManagerEmplId;
                            if (jsonMessage.testNumber != null)
                                messagedata.testNumber = (string)jsonMessage.testNumber;
                            if (jsonMessage.MessageId != null)
                                messageId = (string)jsonMessage.MessageId;
                            ManagerQueue_LoginUser(messagedata.managerEmployeeId, (string)jsonMessage.Password, messagedata.deviceUniqueID, out status);
                            data = status == 0 ? GetManagerAuthorizationGroupActivities(messagedata.managerEmployeeId) : null;
                            messagedata.jsonMessageToSend = ArrangeMessage_LoginUser(status, data, messagedata.deviceUniqueID, messagedata.testNumber, messageId);
                            logger.Info(messagedata.jsonMessageToSend);
                            messagedata.keepShort = true;
                            break;
                        case "getManagerActivities":
                            GetDeviceIDFromTabletMessage((dynamic)jsonMessage.consumerToken, out deviceUniqueID, out queueName);
                            messagedata.deviceUniqueID = deviceUniqueID;
                            messagedata.queueName = queueName;
                            messagedata.employeeId = (String)jsonMessage.EmployeeId;
                            messagedata.clientType = 2;
                            logger.Info(queueName);
                            logger.Info(deviceUniqueID);
                            data = GetManagerAuthorizationGroupActivities(messagedata.employeeId);
                            messagedata.jsonMessageToSend = ArrangeMessage_Data("GetManagerAuthorizationGroupActivities", data, messagedata.deviceUniqueID);
                            logger.Info(messagedata.jsonMessageToSend);
                            break;
                        case "getAllManagersConnected":
                            GetDeviceIDFromTabletMessage((dynamic)jsonMessage.consumerToken, out deviceUniqueID, out queueName);
                            messagedata.deviceUniqueID = deviceUniqueID;
                            messagedata.queueName = queueName;
                            messagedata.employeeId = (String)jsonMessage.EmployeeId;
                            messagedata.subject = (String)jsonMessage.Subject;
                            messagedata.clientType = 2;
                            logger.Info(queueName);
                            logger.Info(deviceUniqueID);
                            data = GetAllManagersConnected();
                            if (jsonMessage.testNumber != null)
                                messagedata.testNumber = (string)jsonMessage.testNumber;
                            messagedata.jsonMessageToSend = ArrangeMessage_Data("GetAllManagersConnected", data, messagedata.deviceUniqueID, messagedata.testNumber);
                            logger.Info(messagedata.jsonMessageToSend);
                            break;
                        case "login":
                            messagedata.clientType = 1;
                            if (jsonMessage.QueueName != null)
                                messagedata.queueName = (string)jsonMessage.QueueName;
                            messagedata.deviceUniqueID = ((dynamic)jsonMessage.deviceInfo).DeviceUniqueID;
                            messagedata.managerEmployeeId = (string)jsonMessage.ManagerEmplId;
                            if (jsonMessage.testNumber != null)
                                messagedata.testNumber = (string)jsonMessage.testNumber;
                            if (jsonMessage.MessageId != null)
                                messageId = (string)jsonMessage.MessageId;
                            Login((string)jsonMessage.ManagerID, messagedata.managerEmployeeId, messagedata.queueName, (object)jsonMessage.deviceInfo, (string)jsonMessage.pushToken);
                            messagedata.jsonMessageToSend = ArrangeMessageLogin(messagedata.deviceUniqueID, (string)jsonMessage.ManagerEmplId, messagedata.testNumber, messageId);
                            logger.Info(messagedata.jsonMessageToSend);
                            messagedata.keepShort = true;
                            break;
                        case "keepalive":
                            messagedata.clientType = 1;
                            if (jsonMessage.QueueName != null)
                                messagedata.queueName = (string)jsonMessage.QueueName;
                            if (jsonMessage.MessageId != null)
                                messageId = (string)jsonMessage.MessageId;
                            messagedata.deviceUniqueID = ((dynamic)jsonMessage.deviceInfo).DeviceUniqueID;
                            KeepAlive((object)jsonMessage.deviceInfo);
                            messagedata.jsonMessageToSend = ArrangeCommonMessage(messagedata.deviceUniqueID, "keepalive", messageId);
                            logger.Info(messagedata.jsonMessageToSend);
                            messagedata.keepShort = true;
                            break;
                        case "logout":
                            messagedata.clientType = 1;
                            if (jsonMessage.QueueName != null)
                                messagedata.queueName = (string)jsonMessage.QueueName;
                            if (jsonMessage.MessageId != null)
                                messageId = (string)jsonMessage.MessageId;
                            deviceUniqueID = ((dynamic)jsonMessage.deviceInfo).DeviceUniqueID;
                            messagedata.managerEmployeeId = (string)jsonMessage.ManagerEmplId;
                            Logout((string)jsonMessage.ManagerID, messagedata.managerEmployeeId, messagedata.queueName, (object)jsonMessage.deviceInfo);
                            messagedata.jsonMessageToSend = ArrangeCommonMessage(deviceUniqueID, "logout", messageId);
                            logger.Info(messagedata.jsonMessageToSend);
                            messagedata.keepShort = true;
                            break;
                        case "history":
                            messagedata.clientType = 1;
                            if (jsonMessage.QueueName != null)
                                messagedata.queueName = (string)jsonMessage.QueueName;
                            if (jsonMessage.MessageId != null)
                                messageId = (string)jsonMessage.MessageId;
                            deviceUniqueID = jsonMessage.deviceUniqueID;
                            messagedata.managerEmployeeId = (string)jsonMessage.ManagerEmplId;
                            data = GetManagerHistory(messagedata.managerEmployeeId, false);
                            if (jsonMessage.testNumber != null)
                                messagedata.testNumber = (string)jsonMessage.testNumber;
                            messagedata.jsonMessageToSend = ArrangeMessage_Data("history finish", data, deviceUniqueID, messagedata.testNumber, messageId);
                            logger.Info(messagedata.jsonMessageToSend);
                            messagedata.keepShort = true;
                            break;
                        case "refresh":
                            messagedata.clientType = 1;
                            if (jsonMessage.QueueName != null)
                                messagedata.queueName = (string)jsonMessage.QueueName;
                            if (jsonMessage.MessageId != null)
                                messageId = (string)jsonMessage.MessageId;
                            deviceUniqueID = jsonMessage.deviceUniqueID;
                            messagedata.managerEmployeeId = (string)jsonMessage.ManagerEmplId;
                            data = GetManagerHistory(messagedata.managerEmployeeId, true);
                            messagedata.jsonMessageToSend = ArrangeMessage_Data("refresh finish", data, deviceUniqueID, "0", messageId);
                            logger.Info(messagedata.jsonMessageToSend);
                            messagedata.keepShort = true;
                            break;
                        case "updateAck":
                            messagedata.jsonMessageToSend = message;
                            GetDeviceIDFromTabletMessage((dynamic)jsonMessage.consumerToken, out deviceUniqueID, out queueName);
                            messagedata.deviceUniqueID = deviceUniqueID;
                            messagedata.queueName = queueName;
                            messagedata.clientType = 2;
                            messagedata.employeeId = (String)jsonMessage.EmployeeId;
                            messagedata.subject = (String)jsonMessage.Subject;
                            messagedata.managerEmployeeId = ((string)jsonMessage.ManagerEmployeeId).Replace("'", "");
                            messagedata.managerName = (String)jsonMessage.ManagerName;
                            messagedata.managerQueues = GetManagerQueues(messagedata.managerEmployeeId);
                            if (jsonMessage.testNumber != null)
                                messagedata.testNumber = (string)jsonMessage.testNumber;
                            messagedata.jsonMessageToReturn = ArrangeMessage_toTablet("updateAckReceived", (dynamic)jsonMessage, messagedata.deviceUniqueID, messagedata.managerEmployeeId, messagedata.managerQueues, messagedata.testNumber);
                            messagedata.isForwardMessage = true;
                            break;
                        case "askApprove":
                            messagedata.jsonMessageToSend = message;
                            GetDeviceIDFromTabletMessage((dynamic)jsonMessage.consumerToken, out deviceUniqueID, out queueName);
                            messagedata.deviceUniqueID = deviceUniqueID;
                            messagedata.queueName = queueName;
                            messagedata.clientType = 2;
                            messagedata.employeeId = (String)jsonMessage.EmployeeId;
                            messagedata.subject = (String)jsonMessage.Subject;
                            messagedata.managerEmployeeId = ((string)jsonMessage.ManagerEmployeeId).Replace("'", "");
                            messagedata.managerName = (String)jsonMessage.ManagerName;
                            messagedata.agentId = (String)jsonMessage.AgentId;
                            messagedata.agentName = (String)jsonMessage.AgentName;
                            messagedata.activityCode = (String)jsonMessage.ActivityCode;
                            messagedata.activityDescription = (String)jsonMessage.ActivityDescription;
                            messagedata.managerQueues = GetManagerQueues(messagedata.managerEmployeeId);
                            if (jsonMessage.testNumber != null)
                                messagedata.testNumber = (string)jsonMessage.testNumber;
                            messagedata.jsonMessageToReturn = ArrangeMessage_toTablet("askApproveReceived", (dynamic)jsonMessage, messagedata.deviceUniqueID, messagedata.managerEmployeeId, messagedata.managerQueues, messagedata.testNumber);
                            messagedata.sendNotification = true;
                            messagedata.isForwardMessage = true;
                            break;
                        case "cancel":
                            messagedata.jsonMessageToSend = message;
                            GetDeviceIDFromTabletMessage((dynamic)jsonMessage.consumerToken, out deviceUniqueID, out queueName);
                            messagedata.deviceUniqueID = deviceUniqueID;
                            messagedata.queueName = queueName;
                            messagedata.clientType = 2;
                            messagedata.employeeId = (String)jsonMessage.EmployeeId;
                            messagedata.subject = (String)jsonMessage.Subject;
                            messagedata.managerEmployeeId = ((string)jsonMessage.ManagerEmployeeId).Replace("'", "");
                            messagedata.managerName = (String)jsonMessage.ManagerName;
                            messagedata.agentId = (String)jsonMessage.AgentId;
                            messagedata.agentName = (String)jsonMessage.AgentName;
                            messagedata.activityCode = (String)jsonMessage.ActivityCode;
                            messagedata.activityDescription = (String)jsonMessage.ActivityDescription;
                            messagedata.managerQueues = GetManagerQueues(messagedata.managerEmployeeId);
                            if (jsonMessage.testNumber != null)
                                messagedata.testNumber = (string)jsonMessage.testNumber;
                            messagedata.jsonMessageToReturn = ArrangeMessage_toTablet("cancelReceived", (dynamic)jsonMessage, messagedata.deviceUniqueID, messagedata.managerEmployeeId, messagedata.managerQueues, messagedata.testNumber);
                            messagedata.isForwardMessage = true;
                            break;
                    }
                }

            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
        }

        void GetDeviceIDFromTabletMessage(dynamic consumerToken, out string deviceUniqueID, out string queueName) {
            deviceUniqueID = null;
            queueName = null;
            try
            {
                deviceUniqueID = ((dynamic)consumerToken[0]).AndroidId;
                queueName = ((dynamic)consumerToken[0]).queueName;
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
        }
        public void AddLog(Guid? AgentLog_ID, string Request, string Response, string queueName, string DeviceID, int ClientType, string Command, string AgentId, string AgentName, string EmployeeId, string ActivityCode, string ActivityDescription, string ManagerEmployeeId, string ManagerName, string Subject, Nullable<int> SentToManagersCount, out Guid? ManagersQueueLog_ID)
        {
            dbManager.ManagersQueue_Log_Add(AgentLog_ID, Request, Response, queueName, DeviceID, ClientType, Command, AgentId, AgentName, EmployeeId, ActivityCode, ActivityDescription, ManagerEmployeeId, ManagerName, Subject, SentToManagersCount, out ManagersQueueLog_ID);
        }

        /*
        public void AddLogDevices(Guid ManagerQueueLog_ID, List<ManagerQueue> devices) {
            dbManager.ManagersQueue_Log_Devices_Add(ManagerQueueLog_ID, devices);
        }
        */
        public void AddLogFirebase(Guid ManagersQueueLog_ID, string ManagerEmployeeId, string DeviceID, string PushAddr, string LogMessage, bool IsError)
        {
            dbManager.ManagersQueue_FirebaseLog_Add(ManagersQueueLog_ID, ManagerEmployeeId, DeviceID, PushAddr, LogMessage, IsError);
        }

        public List<ManagerQueue> GetManagerQueues(string ManagerID) {
            return dbManager.GetManagerQueue(ManagerID);
        }

        private void Login(String ManagerID, string ManagerEmployeeId, string QueueName, dynamic deviceInfo, string pushToken)
        {
            var DeviceName = (string)deviceInfo.DeviceName + "-" + (string)deviceInfo.DeviceBrand;
            dbManager.ManagersQueue_Login(ManagerID, ManagerEmployeeId, QueueName, DeviceName, (string)deviceInfo.DeviceUniqueID, pushToken);
        }

        private void KeepAlive(dynamic deviceInfo)
        {
            dbManager.ManagersQueue_Update((string)deviceInfo.DeviceUniqueID);
        }

        private void Logout(String ManagerID, string ManagerEmployeeId, string QueueName, dynamic deviceInfo)
        {
            var DeviceName = (string)deviceInfo.DeviceName + "-" + (string)deviceInfo.DeviceBrand;
            dbManager.ManagersQueue_Logout(ManagerID, ManagerEmployeeId, QueueName, DeviceName, (string)deviceInfo.DeviceUniqueID);
        }
        private List<ManagerQueueName> GetAllManagersConnected(){
            try
            {
                List<ManagerQueueName> data = dbManager.GetAllManagersConnected();
                return data;
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
            return null;
        }


        private List<ManagerAuthorizationGroupActivities> GetManagerAuthorizationGroupActivities(String EmployeeId)
        {
            try
            {
                List<ManagerAuthorizationGroupActivities> data = dbManager.GetManagerAuthorizationGroupActivities(EmployeeId);
                return data;
                //String dataString = JsonConvert.SerializeObject(data);
                //return dataString;
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
            return null;
        }


        private String ArrangeMessage_LoginUser(int status, object data, string deviceUniqueID, string testNumber = "0", string messageId = "")
        {
            LoginUserResponse lr = new LoginUserResponse() { status = status, command = "loginUser", testNumber = testNumber, data = data, deviceUniqueID = deviceUniqueID, requestMessageId = messageId };

            string result = JsonConvert.SerializeObject(lr);

            return result;
        }

        private String ArrangeMessage_Data(string messageStr, object data, string deviceUniqueID, string testNumber = "0", string messageId = "")
        {

            MessageToSend sendM = new MessageToSend() { message = messageStr, deviceUniqueID = deviceUniqueID, data = data, testNumber = testNumber, requestMessageId=messageId };

            string result = JsonConvert.SerializeObject(sendM);

            return result;
        }

        private String ArrangeMessage_toTablet(string messageStr, dynamic data, string deviceUniqueID, string managerId, List<ManagerQueue> managerQueues,  string testNumber = "0")
        {
            ApproveAnswer answer = new ApproveAnswer()
            {
                RequestID = data.RequestID,
                AgentId = data.AgentId,
                EmployeeId = data.EmployeeId,
                ActivityCode = data.ActivityCode,
                ManagerEmployeeId = data.ManagerEmployeeId,
                RequestStatus = data.RequestStatus
            };
            int managersCount = 0;
            string mesStr = "";
            if (managerQueues != null)
            {
                managersCount = managerQueues.Count;
                foreach (ManagerQueue queue in managerQueues)
                {
                    mesStr += queue.QueueName + ", ";
                }
            }
            messageStr += $" sent to managerId {managerId} {managersCount} manager devices, queues: " + mesStr;

            MessageToSend sendM = new MessageToSend() { message = messageStr, deviceUniqueID = deviceUniqueID, data = answer, testNumber = testNumber };

            string result = JsonConvert.SerializeObject(sendM);

            return result;
        }

        private String ArrangeMessage_FromFakeAgent(string messageStr, string ManagerEmployeeId, string RequestID)
        {
            ApproveAnswer answer = new ApproveAnswer()
            {
                RequestID = RequestID,
                ManagerEmployeeId = ManagerEmployeeId,
                RequestStatus = "11"
            };

            MessageToSend sendM = new MessageToSend() { message = messageStr, deviceUniqueID = null, data = answer };

            string result = JsonConvert.SerializeObject(sendM);

            return result;
        }

        private String ArrangeMessageLogin(string deviceUniqueID, string ManagerEmployeeId,  string testNumber ="0", string messageId="")
        {
            List<ManagerVersion> mv = dbManager.GetManagerVersion(ManagerEmployeeId);
            dynamic Config = JsonConvert.DeserializeObject(@"{""ValueSize"":3}");
            LoginResponse lr = new LoginResponse() { command = "login", testNumber = testNumber, deviceUniqueID = deviceUniqueID, Config = Config, version = mv[0].VersionNum, apk = mv[0].ApkLink, updateMandatory = mv[0].Mandatory, newMessageTime= mv[0].NewMessageTime, requestMessageId = messageId };

            string result = JsonConvert.SerializeObject(lr);

            return result;
        }

        public List<string> GetAllManagers(int numManagers)
        {
            return dbManager.GetAllManagers(numManagers);
        }

        public List<Agent> GetAllAgents()
        {
            return dbManager.GetAllAgents();
        }

        private String ArrangeCommonMessage(string deviceUniqueID, string command, string messageId = "")
        {
            LogoutResponse lr = new LogoutResponse() { command = command, deviceUniqueID = deviceUniqueID, requestMessageId=messageId};

            string result = JsonConvert.SerializeObject(lr);

            return result;
        }


        private List<PasswordManager> GetManagerHistory(string EmployeeId, bool refresh) {
            List<PasswordManager> result;
            if (refresh)
                result = dbManager.GetManagerHistory(EmployeeId, 1);
            else
                result = dbManager.GetManagerHistory(EmployeeId, historyDays);
            return result;
        }

        private void ManagerQueue_LoginUser(string ManagerEmployeeId, string Password, string deviceID, out int Status)
        {
            string error = "";
            Status = 1;
            dbManager.ManagerQueue_LoginUser(ManagerEmployeeId, Password, out Status, out error);
            logger.Info($@" Userlogin ManagerEmployeeId={ManagerEmployeeId} from  deviceid {deviceID} result status={Status} {error}");
        }
        public void CallFCMNode(Guid? ManagersQueueLog_ID, string ManagerEmployeeId, string pushAddress, string message, string deviceID) {
            int exitCode;
            logger.Info($@" Sending to pushAddress {pushAddress} from {deviceID} message {message}");
            string FCMApp = ConfigurationManager.AppSettings["FCMApp"];
            string channelId = ConfigurationManager.AppSettings["FCMchannelId"];

            var proc = new System.Diagnostics.Process();
            proc.StartInfo.CreateNoWindow = true;
            proc.StartInfo.RedirectStandardInput = true;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.RedirectStandardError = true;
            proc.StartInfo.FileName = "node.exe";
            //proc.StartInfo.Arguments = "-i " + FCMApp + " " + channelId + " " + pushAddress + " " + @"""" + message + @"""";
            proc.StartInfo.Arguments = FCMApp + " " + channelId + " " + pushAddress + " " + @"""" + message + @"""";
            proc.Start();
            proc.WaitForExit();

            // *** Read the streams ***
            // Warning: This approach can lead to deadlocks, see Edit #2
            string output = proc.StandardOutput.ReadToEnd();
            string error = proc.StandardError.ReadToEnd();

            exitCode = proc.ExitCode;

            logger.Info("Sent message", message);
            if (!String.IsNullOrEmpty(output))
            {
                bool IsError = output.Contains("errorInfo");
                logger.Error(output);
                AddLogFirebase(ManagersQueueLog_ID.Value, ManagerEmployeeId, deviceID, pushAddress, output, IsError);
            }
            if (!String.IsNullOrEmpty(error))
            {
                AddLogFirebase(ManagersQueueLog_ID.Value, ManagerEmployeeId, deviceID, pushAddress, error, true);
            }
            //Console.WriteLine("output>>" + (String.IsNullOrEmpty(output) ? "(none)" : output));
            //Console.WriteLine("error>>" + (String.IsNullOrEmpty(error) ? "(none)" : error));
            //Console.WriteLine("ExitCode: " + exitCode.ToString(), "ExecuteCommand");
            proc.Close();
        }
    }

}
