using System;
using System.Collections.Generic;
using System.Text;

namespace RabbitMQManager
{
    public class ManagerAuthorizationGroupActivities
    {
        public Int32 PermissionActivityCode { get; set; }
        public string Description { get; set; }
    }
    public class ManagerVersion
    {
        public int Mandatory { get; set; }
        public string VersionNum { get; set; }
        public string ApkLink { get; set; }
        public int NewMessageTime { get; set; }
    }

    public class PasswordManager
    {
        public string RequestID { get; set; }
        public string pTime { get; set; }
        public Nullable<long> StatusChangeTime { get; set; }
        public string AgentId { get; set; }
        public string AgentName { get; set; }
        public string EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public string ActivityCode { get; set; }
        public string ActivityDescription { get; set; }
        public string Cust_Key { get; set; }
        public string CustName { get; set; }
        public Nullable<int> DocType { get; set; }
        public Nullable<long> DocNum { get; set; }
        public string DocName { get; set; }
        public string Comment { get; set; }
        public string ManagerEmployeeId { get; set; }
        public string ManagerName { get; set; }
        public string RequestStatus { get; set; }
        public string ManagerStatusTime { get; set; }
        public string ManagerComment { get; set; }
        public Nullable<int> ManagerDeviceType { get; set; }
        public Nullable<int> TransmissionState { get; set; }
        public string MTN_InsertDate { get; set; }
        public Nullable<int> _id { get; set; }
        public string user_id { get; set; }
        public string CommId { get; set; }
        public string SubjectDescription { get; set; }
        public Nullable<int> ManagerPlatform { get; set; }
        public Nullable<int> IsTest { get; set; }
    }


    public class ManagerQueue { 
        public string QueueName { get; set; }
        public string PushAddr { get; set; }
        public string DeviceID { get; set; }
    }

    public class ManagerQueueName
    {
        public string ManagerID { get; set; }
        public string ManagerEmployeeId { get; set; }
        public string QueueName { get; set; }
    }

    public class Agent { 
        public string EmployeeId { get; set; }
        public string ManagerEmployeeId { get; set; }
    }

    public class ManagerDevice {
        public string ManagerDeviceID { get; set; }
        public string ManagerQueueName { get; set; }
    }
}
