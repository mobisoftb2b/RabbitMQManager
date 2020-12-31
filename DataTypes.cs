using System;
using System.Collections.Generic;
using System.Text;

namespace RabbitMQManager
{
    public class MessageData {
        public string jsonMessageToSend { get; set; }
        public string queueName { get; set; }
        public List<ManagerQueue> managerQueues { get; set; }
        public string deviceUniqueID { get; set; }
        public bool sendNotification { get; set; }
        public string jsonMessageToReturn { get; set; }
        public bool isForwardMessage { get; set; }
        public bool keepShort { get; set; }
        public int clientType { get; set; }
        public string command { get; set; }
        public string agentId { get; set; }
        public string agentName { get; set; }
        public string employeeId { get; set; }
        public string activityCode { get; set; }
        public string activityDescription { get; set; }
        public string managerEmployeeId { get; set; }
        public string managerName { get; set; }
        public string subject { get; set; }
        public string testNumber { get; set; }
    }
    class LoginUserResponse { 
        public int status { get; set; }
        public string command { get; set; }
        public string requestMessageId { get; set; }
        public string testNumber { get; set; }
        public string deviceUniqueID { get; set; }
        public object data { get; set; }
    }
    class LoginResponse
    {
        public string command { get; set; }
        public string requestMessageId { get; set; }
        public string testNumber { get; set; }
        public string deviceUniqueID { get; set; }
        public dynamic Config { get; set; }
        public string version { get; set; }
        public string apk { get; set; }
        public int updateMandatory { get; set; }
        public int newMessageTime { get; set; }
    };

    class LogoutResponse
    {
        public string command { get; set; }
        public string requestMessageId { get; set; }
        public string deviceUniqueID { get; set; }
    };

    
    class SingleMessage
    {
        public object header { get; set; }
        public object[] lines { get; set; }
    }
    class MessageToSend
    {
        public string message { get; set; }

        public string requestMessageId { get; set; }

        public string testNumber { get; set; }
        public string deviceUniqueID { get; set; }

        public object data { get; set; }
    }
    class ApproveAnswer {
        public string RequestID { get; set; }
        public string AgentId { get; set; }

        public string EmployeeId { get; set; }

        public string ActivityCode { get; set; }

        public string ManagerEmployeeId { get; set; }

        public string RequestStatus { get; set; }
    }

}
