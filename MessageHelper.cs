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

        public void HandleMessage(ref String message, out MessageData messagedata) {
            messagedata = new MessageData();
            messagedata.jsonMessageToSend = "";
            messagedata.managerQueues = new List<ManagerQueue>();
            messagedata.queueName = null;
            string deviceUniqueID = "";
            messagedata.deviceUniqueID = "";
            messagedata.sendNotification = false;
            messagedata.isForwardMessage = false;
            messagedata.jsonMessageToReturn = "";
            messagedata.keepShort = false;
            messagedata.clientType = 0; //1 - manager, 2 - agent
            string notificationMessageManagerApprove = ConfigurationManager.AppSettings["notificationMessageManagerApprove"];
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
                    string messagePush = "";
                    string ManagerName = "";
                    messagedata.command = (String)jsonMessage.command;
                    logger.Info(messagedata.command);
                    string agentRequestId = "";
                    Nullable<Guid> agentMessageId;
                    String requestStatus = null;
                    String managerComment = null;
                    String managerID = null;
                    String managerEmplId = null;
                    String queueName = null;
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
                            ManagerQueue_LoginUser(messagedata.managerEmployeeId, (string)jsonMessage.Password, messagedata.deviceUniqueID, out status, out ManagerName);
                            data = status == 0 ? GetManagerAuthorizationGroupActivities(messagedata.managerEmployeeId) : null;
                            messagedata.jsonMessageToSend = ArrangeMessage_LoginUser("loginUser", status, data, messagedata.deviceUniqueID, messagedata.testNumber, messageId, ManagerName);
                            logger.Info(messagedata.jsonMessageToSend);
                            messagedata.keepShort = true;
                            break;
                        case "loginUserNew":
                            messagedata.clientType = 1;
                            messagedata.keepShort = true;
                            if (jsonMessage.QueueName != null)
                                messagedata.queueName = (string)jsonMessage.QueueName;
                            messagedata.deviceUniqueID = ((dynamic)jsonMessage.deviceInfo).DeviceUniqueID;
                            messagedata.managerEmployeeId = (string)jsonMessage.ManagerEmplId;
                            if (jsonMessage.testNumber != null)
                                messagedata.testNumber = (string)jsonMessage.testNumber;
                            if (jsonMessage.MessageId != null)
                                messageId = (string)jsonMessage.MessageId;
                            ManagerQueue_LoginUser(messagedata.managerEmployeeId, (string)jsonMessage.Password, messagedata.deviceUniqueID, out status, out ManagerName);
                            data = null;
                            messagedata.jsonMessageToSend = ArrangeMessage_LoginUser("loginUserNew", status, data, messagedata.deviceUniqueID, messagedata.testNumber, messageId, ManagerName);
                            logger.Info(messagedata.jsonMessageToSend);
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
                            if (jsonMessage.PushQueueName)
                                messagedata.pushQueueName = (string)jsonMessage.PushQueueName;
                            messagedata.deviceUniqueID = ((dynamic)jsonMessage.deviceInfo).DeviceUniqueID;
                            messagedata.managerEmployeeId = (string)jsonMessage.ManagerEmplId;
                            if (jsonMessage.testNumber != null)
                                messagedata.testNumber = (string)jsonMessage.testNumber;
                            if (jsonMessage.MessageId != null)
                                messageId = (string)jsonMessage.MessageId;
                            Login((string)jsonMessage.ManagerID, messagedata.managerEmployeeId, messagedata.queueName, messagedata.pushQueueName, (object)jsonMessage.deviceInfo, (string)jsonMessage.pushToken);
                            messagedata.jsonMessageToSend = ArrangeMessageLogin("login", messagedata.deviceUniqueID, (string)jsonMessage.ManagerEmplId, messagedata.testNumber, messageId);
                            logger.Info(messagedata.jsonMessageToSend);
                            messagedata.keepShort = true;
                            break;
                        case "loginNew":
                            messagedata.clientType = 1;
                            messagedata.keepShort = true;
                            if (jsonMessage.QueueName != null)
                                messagedata.queueName = (string)jsonMessage.QueueName;
                            if (jsonMessage.QueuePushName != null) 
                                messagedata.pushQueueName = (string)jsonMessage.QueuePushName;
                            messagedata.deviceUniqueID = ((dynamic)jsonMessage.deviceInfo).DeviceUniqueID;
                            messagedata.managerEmployeeId = (string)jsonMessage.ManagerEmplId;
                            if (jsonMessage.testNumber != null)
                                messagedata.testNumber = (string)jsonMessage.testNumber;
                            if (jsonMessage.MessageId != null)
                                messageId = (string)jsonMessage.MessageId;
                            Login((string)jsonMessage.ManagerID, messagedata.managerEmployeeId, messagedata.queueName, messagedata.pushQueueName, (object)jsonMessage.deviceInfo, (string)jsonMessage.pushToken);
                            data = GetManagerAuthorizationGroupActivities(messagedata.managerEmployeeId);
                            messagedata.jsonMessageToSend = ArrangeMessageLogin("loginNew", messagedata.deviceUniqueID, (string)jsonMessage.ManagerEmplId, messagedata.testNumber, messageId, data);
                            logger.Info(messagedata.jsonMessageToSend);
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
                        case "sendLog":
                            messagedata.clientType = 1;
                            if (jsonMessage.QueueName != null)
                                messagedata.queueName = (string)jsonMessage.QueueName;
                            if (jsonMessage.MessageId != null)
                                messageId = (string)jsonMessage.MessageId;
                            if (jsonMessage.FileName != null)
                                messagedata.fileName = (string)jsonMessage.FileName;
                            if (jsonMessage.FileData != null)
                                messagedata.fileContent = (string)jsonMessage.FileData;
                            messagedata.deviceUniqueID = ((dynamic)jsonMessage.deviceInfo).DeviceUniqueID;
                            messagedata.managerEmployeeId = (string)jsonMessage.ManagerEmplId;
                            messagedata.fileName = messagedata.managerEmployeeId + "_" + messagedata.fileName;
                            messagedata.jsonMessageToSend = ArrangeMessageSendLog(messagedata.deviceUniqueID, messagedata.fileName, messageId);
                            logger.Info(messagedata.jsonMessageToSend);
                            messagedata.keepShort = true;
                            break;
                        case "received":
                            messagedata.clientType = 1;
                            if (jsonMessage.QueueName != null)
                                messagedata.queueName = (string)jsonMessage.QueueName;
                            if (jsonMessage.MessageId != null)
                                messageId = (string)jsonMessage.MessageId;
                            if (jsonMessage.RequestStatus != null)
                                messagedata.requestStatus = (string)jsonMessage.RequestStatus;
                            if (jsonMessage.AgentMessageId != null)
                                messagedata.AgentMessageId = (string)jsonMessage.AgentMessageId;
                            messagedata.managerEmployeeId = (string)jsonMessage.ManagerEmplId;
                            messagedata.deviceUniqueID = ((dynamic)jsonMessage.deviceInfo).DeviceUniqueID;
                            UpdateReceivedMessage(messagedata.AgentMessageId, messagedata.requestStatus, messagedata.managerEmployeeId);
                            messagedata.jsonMessageToSend = ArrangeCommonMessage(deviceUniqueID, "received", messageId);
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
                        case "managerLog":
                            agentRequestId = (String)jsonMessage.RequestID;
                            agentMessageId = (jsonMessage.AgentMessageId == null ? (Nullable<Guid>)null : Guid.Parse((String)jsonMessage.AgentMessageId));
                            requestStatus = (String)jsonMessage.RequestStatus;
                            managerComment = (String)jsonMessage.ManagerComment;
                            managerID = (String)jsonMessage.ManagerID;
                            managerEmplId = (String)jsonMessage.ManagerEmplId;
                            queueName = (string)jsonMessage.QueueName;
                            AddLog(Guid.NewGuid(), agentMessageId, message, "", "", "", 1, "managerLog", "", "", "", "", "", managerEmplId, "", "", 0, "", "");
                            AddRequestLog(agentRequestId, agentMessageId, requestStatus, managerComment, managerID, managerEmplId, queueName);
                            messagedata = null;
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
                            messagedata.agentMessageId = Guid.NewGuid();
                            message = message.Insert(message.LastIndexOf('}') - 1, $@",""AgentMessageId"":""{messagedata.agentMessageId}""");
                            break;
                        case "askApprove":
                            GetDeviceIDFromTabletMessage((dynamic)jsonMessage.consumerToken, out deviceUniqueID, out queueName);
                            messagedata.deviceUniqueID = deviceUniqueID;
                            messagedata.queueName = queueName;
                            messagedata.clientType = 2;
                            messagedata.requestID = (String)jsonMessage.RequestID;
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
                            messagedata.agentMessageId = Guid.NewGuid();
                            message = message.Insert(message.LastIndexOf('}'), $@",""AgentMessageId"":""{messagedata.agentMessageId}""");
                            messagePush = message.Insert(message.LastIndexOf('}'), $@",""PushMessage"":""{notificationMessageManagerApprove}""");
                            if (jsonMessage.test != null)
                                messagedata.test = (string)jsonMessage.test;
                            //logger.Info(message);
                            messagedata.jsonMessageToSend = message;
                            messagedata.jsonMessagePush = messagePush;
                            messagedata.requestType = RequestType.AskApprove;
                            AddRequestLog(messagedata.requestID, messagedata.agentMessageId, "AskApproveReceivedFromAgent", "", "", messagedata.managerEmployeeId, "");
                            break;
                        case "cancel":
                            GetDeviceIDFromTabletMessage((dynamic)jsonMessage.consumerToken, out deviceUniqueID, out queueName);
                            messagedata.deviceUniqueID = deviceUniqueID;
                            messagedata.queueName = queueName;
                            messagedata.clientType = 2;
                            messagedata.requestID = (String)jsonMessage.RequestID;
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
                            messagedata.agentMessageId = Guid.NewGuid();
                            message = message.Insert(message.LastIndexOf('}'), $@",""AgentMessageId"":""{messagedata.agentMessageId}""");
                            messagedata.jsonMessageToSend = message;
                            messagedata.requestType = RequestType.Cancel;
                            AddRequestLog(messagedata.requestID, messagedata.agentMessageId, "CancelReceivedFromAgent", "", "", messagedata.managerEmployeeId, "");
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
        public void AddRequestLog(String RequestID, Nullable<Guid> AgentLog_ID, String requestStatus, String managerComment, String managerID, String managerEmplId, String queueName) {
            dbManager.ManagersQueue_RequestLog_Add(RequestID, AgentLog_ID, requestStatus, managerComment, managerID, managerEmplId, queueName);
        }
        public void AddPushLog(Guid ManagersQueueLog_ID, string PushQueueName, string Request, string ManagerEmployeeId, string AgentId, string AgentName)
        {
            dbManager.ManagersQueue_Log_Push_Add(ManagersQueueLog_ID, PushQueueName, Request, ManagerEmployeeId, AgentId, AgentName);
        }

        public void AddLog(Guid ManagersQueueLog_ID, Guid? AgentLog_ID, string Request, string Response, string queueName, string DeviceID, int ClientType, string Command, string AgentId, string AgentName, string EmployeeId, string ActivityCode, string ActivityDescription, string ManagerEmployeeId, string ManagerName, string Subject, Nullable<int> SentToManagersCount, string ReceivedManagerEmployeeId, string ReceivedStatus)
        {
            dbManager.ManagersQueue_Log_Add(ManagersQueueLog_ID, AgentLog_ID, Request, Response, queueName, DeviceID, ClientType, Command, AgentId, AgentName, EmployeeId, ActivityCode, ActivityDescription, ManagerEmployeeId, ManagerName, Subject, SentToManagersCount, ReceivedManagerEmployeeId, ReceivedStatus);
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

        private void Login(String ManagerID, string ManagerEmployeeId, string QueueName, string PushQueueName,  dynamic deviceInfo, string pushToken)
        {
            var DeviceName = (string)deviceInfo.DeviceName + "-" + (string)deviceInfo.DeviceBrand;
            dbManager.ManagersQueue_Login(ManagerID, ManagerEmployeeId, QueueName, PushQueueName, DeviceName, (string)deviceInfo.DeviceUniqueID, pushToken);
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


        private void UpdateReceivedMessage(String OriginalAgentMessageId, String RequestStatus, String ManagerEmployeeId)
        {
            try
            {
                Guid OriginalAgentLogID = Guid.Parse(OriginalAgentMessageId);
                dbManager.ManagerQueueLog_UpdateReceivedMessage(OriginalAgentLogID, RequestStatus, ManagerEmployeeId);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
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


        private String ArrangeMessage_LoginUser(string command, int status, object data, string deviceUniqueID, string testNumber = "0", string messageId = "", string ManagerName="")
        {
            LoginUserResponse lr = new LoginUserResponse() { status = status, command = command, name= ManagerName,  testNumber = testNumber, data = data, deviceUniqueID = deviceUniqueID, requestMessageId = messageId };

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

        private String ArrangeMessageLogin(string command, string deviceUniqueID, string ManagerEmployeeId,  string testNumber ="0", string messageId="", object data = null)
        {
            List<ManagerVersion> mv = dbManager.GetManagerVersion(ManagerEmployeeId);
            dynamic Config = JsonConvert.DeserializeObject(@"{""ValueSize"":3}");
            LoginResponse lr = new LoginResponse() { command = command, testNumber = testNumber, deviceUniqueID = deviceUniqueID, Config = Config, version = mv[0].VersionNum, apk = mv[0].ApkLink, updateMandatory = mv[0].Mandatory, newMessageTime= mv[0].NewMessageTime, requestMessageId = messageId, data = data };

            string result = JsonConvert.SerializeObject(lr);

            return result;
        }

        private String ArrangeMessageSendLog(string deviceUniqueID, string fileName, string messageId = "")
        {
            SendLogResponse sr = new SendLogResponse() { command = "sendLog", deviceUniqueID = deviceUniqueID, requestMessageId=messageId, fileName=fileName };
            string result = JsonConvert.SerializeObject(sr);
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

        private void ManagerQueue_LoginUser(string ManagerEmployeeId, string Password, string deviceID, out int Status, out string ManagerName)
        {
            string error = "";
            Status = 1;
            ManagerName = "";
            try
            {
                dbManager.ManagerQueue_LoginUser(ManagerEmployeeId, Password, out Status, out error, out ManagerName);
                logger.Info($@" Userlogin ManagerEmployeeId={ManagerEmployeeId} from  deviceid {deviceID} result status={Status} Name={ManagerName} {error}");
            }
            catch (Exception ex) {
                logger.Error(ex);
                Status = -1;
            }
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
