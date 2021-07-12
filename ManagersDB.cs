using System;
using System.Collections.Generic;
using System.Data;
using System.Configuration;
using System.Data.SqlClient;
using Newtonsoft.Json;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;

namespace RabbitMQManager
{
    public class ManagersDB
    {
        string connectionString = ConfigurationManager.ConnectionStrings["ManagersConnectionString"].ConnectionString;

        private static Logger logger = LogManager.GetCurrentClassLogger();

        public int ManagersQueue_Login(string ManagerID, string ManagerEmployeeId, string QueueName, string PushQueueName, string DeviceName, string DeviceID, string PushAddr)
        {
            try
            {
                int result = -1;
                using (var conn = new SqlConnection(connectionString))
                using (var command = new SqlCommand("ManagersQueue_Login", conn)
                {
                    CommandType = CommandType.StoredProcedure

                })
                {
                    command.Parameters.Add(new SqlParameter("@ManagerID", ManagerID));
                    command.Parameters.Add(new SqlParameter("@ManagerEmployeeId", ManagerEmployeeId));
                    command.Parameters.Add(new SqlParameter("@QueueName", QueueName));
                    command.Parameters.Add(new SqlParameter("@PushQueueName", PushQueueName));
                    command.Parameters.Add(new SqlParameter("@DeviceName", DeviceName));
                    command.Parameters.Add(new SqlParameter("@DeviceID", DeviceID));
                    command.Parameters.Add(new SqlParameter("@PushAddr", PushAddr));
                    conn.Open();
                    result = command.ExecuteNonQuery();
                    conn.Close();
                }
                return result;
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
            return -1;
        }

        public int ManagersQueue_Update(string DeviceID)
        {
            try
            {
                int result = -1;
                using (var conn = new SqlConnection(connectionString))
                using (var command = new SqlCommand("ManagersQueue_Update", conn)
                {
                    CommandType = CommandType.StoredProcedure

                })
                {
                    command.Parameters.Add(new SqlParameter("@DeviceID", DeviceID));
                    conn.Open();
                    result = command.ExecuteNonQuery();
                    conn.Close();
                }
                return result;
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
            return -1;
        }

        public int ManagerQueue_LogOffInactiveManagers(int idleTimeMin)
        {
            try
            {
                int result = -1;
                using (var conn = new SqlConnection(connectionString))
                using (var command = new SqlCommand("ManagerQueue_LogOffInactiveManagers", conn)
                {
                    CommandType = CommandType.StoredProcedure

                })
                {
                    command.Parameters.Add(new SqlParameter("@IdleTime", idleTimeMin));
                    conn.Open();
                    result = command.ExecuteNonQuery();
                    conn.Close();
                }
                return result;
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
            return -1;
        }

        public int ManagersQueue_Logout(string ManagerID, string ManagerEmployeeId, string QueueName, string DeviceName, string DeviceID)
        {
            try
            {
                int result = -1;
                using (var conn = new SqlConnection(connectionString))
                using (var command = new SqlCommand("ManagersQueue_Logout", conn)
                {
                    CommandType = CommandType.StoredProcedure

                })
                {
                    command.Parameters.Add(new SqlParameter("@ManagerID", ManagerID));
                    command.Parameters.Add(new SqlParameter("@ManagerEmployeeId", ManagerEmployeeId));
                    command.Parameters.Add(new SqlParameter("@QueueName", QueueName));
                    command.Parameters.Add(new SqlParameter("@DeviceName", DeviceName));
                    command.Parameters.Add(new SqlParameter("@DeviceID", DeviceID));
                    conn.Open();
                    result = command.ExecuteNonQuery();
                    conn.Close();
                }
                return result;
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
            return -1;
        }

        /*
        public int ManagersQueue_Log_Devices_Add(Guid ManagerQueueLog_ID, List<ManagerQueue> managerQueues) {
            List<ManagerDevice> managerDevices = new List<ManagerDevice>();
            foreach (ManagerQueue queue in managerQueues) {
                ManagerDevice device = new ManagerDevice() { ManagerDeviceID = queue.DeviceID, ManagerQueueName = queue.QueueName };
                managerDevices.Add(device);
            }
            string jsonDevices = JsonConvert.SerializeObject(managerDevices);
            try
            {
                int result = -1;
                using (var conn = new SqlConnection(connectionString))
                using (var command = new SqlCommand("ManagersQueue_Log_Devices_Add", conn)
                {
                    CommandType = CommandType.StoredProcedure

                })
                {
                    command.Parameters.Add(new SqlParameter("@ManagerQueueLog_ID", ManagerQueueLog_ID));
                    command.Parameters.Add(new SqlParameter("@JsonDevices", jsonDevices));

                    conn.Open();
                    result = command.ExecuteNonQuery();
                    conn.Close();
                }
                return result;
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
            return -1;
        }
        */

        public int ManagerQueueLog_UpdateReceivedMessage(Guid OriginalAgentLogID, String RequestStatus, String ManagerEmployeeId) {
            try
            {
                int result = -1;
                using (var conn = new SqlConnection(connectionString))
                using (var command = new SqlCommand("ManagersQueue_Log_UpdateReceived", conn)
                {
                    CommandType = CommandType.StoredProcedure

                })
                {
                    command.Parameters.Add(new SqlParameter("@OriginalAgentLogID", OriginalAgentLogID));
                    command.Parameters.Add(new SqlParameter("@ReceivedStatus", RequestStatus));
                    command.Parameters.Add(new SqlParameter("@ReceivedManagerEmployeeId", ManagerEmployeeId));

                    conn.Open();
                    result = command.ExecuteNonQuery();
                    conn.Close();
                }
                return result;
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
            return -1;
        }

        public int ManagersQueue_Log_Push_Add(Guid ManagersQueueLog_ID, string PushQueueName, string Request, string ManagerEmployeeId, string AgentId, string AgentName) {
            try
            {
                int result = -1;
                using (var conn = new SqlConnection(connectionString))
                using (var command = new SqlCommand("ManagersQueue_Log_Push_Add", conn)
                {
                    CommandType = CommandType.StoredProcedure

                })
                {
                    command.Parameters.Add(new SqlParameter("@ManagerQueueLog_ID", ManagersQueueLog_ID));
                    command.Parameters.Add(new SqlParameter("@PushQueueName", PushQueueName));
                    command.Parameters.Add(new SqlParameter("@Request", Request));
                    command.Parameters.Add(new SqlParameter("@ManagerEmployeeId", @ManagerEmployeeId));
                    command.Parameters.Add(new SqlParameter("@AgentId", AgentId));
                    command.Parameters.Add(new SqlParameter("@AgentName", AgentName));

                    conn.Open();
                    result = command.ExecuteNonQuery();
                    conn.Close();
                }
                return result;
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
            return -1;
        }

        public int ManagersQueue_RequestLog_Add(String RequestID, Nullable<Guid> AgentLog_ID, String requestStatus, String managerComment, String managerID, String managerEmplId, String queueName)
        {
            try
            {
                int result = -1;
                using (var conn = new SqlConnection(connectionString))
                using (var command = new SqlCommand("ManagersQueue_RequestLog_Add", conn)
                {
                    CommandType = CommandType.StoredProcedure

                })
                {
                    command.Parameters.Add(new SqlParameter("@RequestID", RequestID));
                    command.Parameters.Add(new SqlParameter("@AgentLog_ID", AgentLog_ID == null? (object)DBNull.Value : (Guid)AgentLog_ID));
                    command.Parameters.Add(new SqlParameter("@RequestStatus", requestStatus));
                    command.Parameters.Add(new SqlParameter("@ManagerComment", managerComment));
                    command.Parameters.Add(new SqlParameter("@ManagerID", managerID));
                    command.Parameters.Add(new SqlParameter("@ManagerEmplId", managerEmplId));
                    command.Parameters.Add(new SqlParameter("@QueueName", queueName));

                    conn.Open();
                    result = command.ExecuteNonQuery();
                    conn.Close();
                }
                return result;
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
            return -1;
        }

        public int ManagersQueue_Log_Add(Guid ManagersQueueLog_ID, Guid? AgentLog_ID, string Request, string Response, string QueueName, string DeviceID, int ClientType, string Command, string AgentId, string AgentName, string EmployeeId, string ActivityCode, string ActivityDescription, string ManagerEmployeeId, string ManagerName, string Subject, Nullable<int> SentToManagersCount, string ReceivedManagerEmployeeId, string ReceivedStatus)
        {
            try
            {
                int result = -1;
                using (var conn = new SqlConnection(connectionString))
                using (var command = new SqlCommand("ManagersQueue_Log_Add", conn)
                {
                    CommandType = CommandType.StoredProcedure

                })
                {
                    command.Parameters.Add(new SqlParameter("@ManagersQueueLog_ID", ManagersQueueLog_ID));
                    command.Parameters.Add(new SqlParameter("@AgentLog_ID", AgentLog_ID));
                    command.Parameters.Add(new SqlParameter("@Request", Request));
                    command.Parameters.Add(new SqlParameter("@Response", Response));
                    command.Parameters.Add(new SqlParameter("@QueueName", QueueName));
                    command.Parameters.Add(new SqlParameter("@DeviceID", DeviceID));
                    command.Parameters.Add(new SqlParameter("@ClientType", ClientType));
                    command.Parameters.Add(new SqlParameter("@Command", Command));
                    command.Parameters.Add(new SqlParameter("@AgentId", AgentId));
                    command.Parameters.Add(new SqlParameter("@AgentName", AgentName));
                    command.Parameters.Add(new SqlParameter("@EmployeeId", EmployeeId));
                    command.Parameters.Add(new SqlParameter("@ActivityCode", ActivityCode));
                    command.Parameters.Add(new SqlParameter("@ActivityDescription", ActivityDescription));
                    command.Parameters.Add(new SqlParameter("@ManagerEmployeeId", ManagerEmployeeId));
                    command.Parameters.Add(new SqlParameter("@ManagerName", ManagerName));
                    command.Parameters.Add(new SqlParameter("@Subject", Subject));
                    command.Parameters.Add(new SqlParameter("@SentToManagersCount", SentToManagersCount));
                    command.Parameters.Add(new SqlParameter("@ReceivedManagerEmployeeId", ReceivedManagerEmployeeId));
                    command.Parameters.Add(new SqlParameter("@ReceivedStatus", ReceivedStatus));

                    conn.Open();
                    result = command.ExecuteNonQuery();
                    conn.Close();
                }
                return result;
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
            return -1;
        }

        public int ManagersQueue_FirebaseLog_Add(Guid ManagersQueueLog_ID, string ManagerEmployeeId, string DeviceID, string PushAddr, string LogMessage, bool IsError)
        {
            try
            {
                int result = -1;
                using (var conn = new SqlConnection(connectionString))
                using (var command = new SqlCommand("ManagersQueue_FirebaseLog_Add", conn)
                {
                    CommandType = CommandType.StoredProcedure

                })
                {
                    command.Parameters.Add(new SqlParameter("@ManagersQueueLog_ID", ManagersQueueLog_ID));
                    command.Parameters.Add(new SqlParameter("@ManagerEmployeeId", ManagerEmployeeId));
                    command.Parameters.Add(new SqlParameter("@DeviceID", DeviceID));
                    command.Parameters.Add(new SqlParameter("@PushAddr", PushAddr));
                    command.Parameters.Add(new SqlParameter("@LogMessage", LogMessage));
                    command.Parameters.Add(new SqlParameter("@IsError", IsError));
                    conn.Open();
                    result = command.ExecuteNonQuery();
                    conn.Close();
                }
                return result;
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
            return -1;
        }

        public List<PasswordManager> GetManagerHistory(string EmployeeId, int HistoryDays)
        {
            try
            {
                var PasswordManagerList = new List<PasswordManager>();
                using (var conn = new SqlConnection(connectionString))
                using (var command = new SqlCommand("GetManagerHistory", conn)
                {
                    CommandType = CommandType.StoredProcedure

                })
                {
                    command.Parameters.Add(new SqlParameter("@EmployeeId", EmployeeId));
                    command.Parameters.Add(new SqlParameter("@HistoryDays", HistoryDays));
                    conn.Open();
                    var reader = command.ExecuteReader();
                    // iterate through results, printing each to console
                    while (reader.Read())
                    {
                        var PasswordManagerItem = new PasswordManager()
                        {
                            RequestID = reader.SafeGetString("RequestID"),
                            pTime = reader.SafeGetString("pTime"),
                            StatusChangeTime = reader.SafeGetLong("StatusChangeTime"),
                            AgentId = reader.SafeGetString("AgentId"),
                            AgentName = reader.SafeGetString("AgentName"),
                            EmployeeId = reader.SafeGetString("EmployeeId"),
                            EmployeeName = reader.SafeGetString("EmployeeName"),
                            ActivityCode = reader.SafeGetString("ActivityCode"),
                            ActivityDescription = reader.SafeGetString("ActivityDescription"),
                            Cust_Key = reader.SafeGetString("Cust_Key"),
                            CustName = reader.SafeGetString("CustName"),
                            DocType = reader.SafeGetInt("DocType"),
                            DocNum = reader.SafeGetLong("DocNum"),
                            DocName = reader.SafeGetString("DocName"),
                            Comment = reader.SafeGetString("Comment"),
                            ManagerEmployeeId = reader.SafeGetString("ManagerEmployeeId"),
                            ManagerName = reader.SafeGetString("ManagerName"),
                            RequestStatus = reader.SafeGetString("RequestStatus"),
                            ManagerStatusTime = reader.SafeGetString("ManagerStatusTime"),
                            ManagerComment = reader.SafeGetString("ManagerComment"),
                            ManagerDeviceType = reader.SafeGetInt("ManagerDeviceType"),
                            TransmissionState = reader.SafeGetInt("TransmissionState"),
                            MTN_InsertDate = reader.SafeGetString("MTN_InsertDate"),
                            _id = reader.SafeGetInt("_id"),
                            user_id = reader.SafeGetString("user_id"),
                            CommId = reader.SafeGetString("CommId"),
                            SubjectDescription = reader.SafeGetString("SubjectDescription"),
                            ManagerPlatform = reader.SafeGetInt("ManagerPlatform"),
                            IsTest = reader.SafeGetInt("IsTest")
                        };
                        PasswordManagerList.Add(PasswordManagerItem);
                        //Console.WriteLine((String)reader["ItemCode"] + ' ' + (String)reader["Image"] + ' ' + ((DateTime)reader["Date"]).ToLongDateString());
                    }
                    conn.Close();
                }
                return PasswordManagerList;
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
            return null;
        }
        public List<ManagerAuthorizationGroupActivities> GetManagerAuthorizationGroupActivities(String EmployeeId)
        {
            try
            {
                var getManagerAuthorizationGroupList = new List<ManagerAuthorizationGroupActivities>();
                using (var conn = new SqlConnection(connectionString))
                using (var command = new SqlCommand("GetManagerAuthorizationGroupActivities", conn)
                {
                    CommandType = CommandType.StoredProcedure

                })
                {
                    command.Parameters.Add(new SqlParameter("@EmployeeId", EmployeeId));
                    conn.Open();
                    var reader = command.ExecuteReader();
                    // iterate through results, printing each to console
                    while (reader.Read())
                    {
                        var getManagerAuthorizationGroup = new ManagerAuthorizationGroupActivities() { PermissionActivityCode = (Int32)reader["PermissionActivityCode"], Description = (String)reader["Description"] };
                        getManagerAuthorizationGroupList.Add(getManagerAuthorizationGroup);
                        //Console.WriteLine((String)reader["ItemCode"] + ' ' + (String)reader["Image"] + ' ' + ((DateTime)reader["Date"]).ToLongDateString());
                    }
                    conn.Close();
                }

                return getManagerAuthorizationGroupList;
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                return null;
            }
        }

        public List<ManagerQueueName> GetAllManagersConnected()
        {
            try
            {
                var ManagerQueueNameList = new List<ManagerQueueName>();
                using (var conn = new SqlConnection(connectionString))
                using (var command = new SqlCommand("ManagerQueue_GetAllManagersConnected", conn)
                {
                    CommandType = CommandType.StoredProcedure

                })
                {
                    conn.Open();
                    var reader = command.ExecuteReader();
                    // iterate through results, printing each to console
                    while (reader.Read())
                    {
                        var getManagerQueueName = new ManagerQueueName() { ManagerID = (String)reader["ManagerID"], ManagerEmployeeId = (String)reader["ManagerEmployeeId"], QueueName = (String)reader["QueueName"] };
                        ManagerQueueNameList.Add(getManagerQueueName);
                        //Console.WriteLine((String)reader["ItemCode"] + ' ' + (String)reader["Image"] + ' ' + ((DateTime)reader["Date"]).ToLongDateString());
                    }
                    conn.Close();
                }

                return ManagerQueueNameList;
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                return null;
            }
        }

        public List<ManagerQueue> GetManagerQueue(string ManagerEmployeeId)
        {
            try
            {
                var ManagerQueueList = new List<ManagerQueue>();
                using (var conn = new SqlConnection(connectionString))
                using (var command = new SqlCommand("ManagersQueue_GetQueue", conn)
                {
                    CommandType = CommandType.StoredProcedure

                })
                {
                    command.Parameters.Add(new SqlParameter("@ManagerEmployeeId", ManagerEmployeeId));
                    conn.Open();
                    var reader = command.ExecuteReader();
                    // iterate through results, printing each to console
                    while (reader.Read())
                    {
                        var ManagerQueueItem = new ManagerQueue();
                        ManagerQueueItem.QueueName = (String)reader["QueueName"];
                        ManagerQueueItem.PushQueueName = reader["PushQueueName"] == DBNull.Value ? String.Empty : (String)reader["PushQueueName"];
                        ManagerQueueItem.PushAddr = (String)reader["PushAddr"];
                        ManagerQueueItem.DeviceID = (String)reader["DeviceID"];

                        ManagerQueueList.Add(ManagerQueueItem);
                        //Console.WriteLine((String)reader["ItemCode"] + ' ' + (String)reader["Image"] + ' ' + ((DateTime)reader["Date"]).ToLongDateString());
                    }
                    conn.Close();
                }

                return ManagerQueueList;
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                return null;
            }
        }



        public List<ManagerVersion> GetManagerVersion(string ManagerEmployeeId)
        {
            try
            {
                var ManagerVersionList = new List<ManagerVersion>();
                using (var conn = new SqlConnection(connectionString))
                using (var command = new SqlCommand("GetManagerVersion", conn)
                {
                    CommandType = CommandType.StoredProcedure

                })
                {
                    command.Parameters.Add(new SqlParameter("@ManagerEmployeeId", ManagerEmployeeId));
                    conn.Open();
                    var reader = command.ExecuteReader();
                    // iterate through results, printing each to console
                    while (reader.Read())
                    {
                        var ManagerVersionItem = new ManagerVersion() { Mandatory = (Int32)reader["Mandatory"], VersionNum = (String)reader["VersionNum"], ApkLink = (String)reader["ApkLink"], NewMessageTime = (Int32)reader["NewMessageTime"] };
                        ManagerVersionList.Add(ManagerVersionItem);
                        //Console.WriteLine((String)reader["ItemCode"] + ' ' + (String)reader["Image"] + ' ' + ((DateTime)reader["Date"]).ToLongDateString());
                    }
                    conn.Close();
                }

                return ManagerVersionList;
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                return null;
            }
        }

        public List<string> GetAllManagers(int numManagers)
        {
            try
            {
                var ManagerList = new List<string>();
                using (var conn = new SqlConnection(connectionString))
                using (var command = new SqlCommand("GetAllManagers", conn)
                {
                    CommandType = CommandType.StoredProcedure

                })
                {
                    command.Parameters.Add(new SqlParameter("@numManagers", numManagers));
                    conn.Open();
                    var reader = command.ExecuteReader();
                    // iterate through results, printing each to console
                    while (reader.Read())
                    {
                        var managerId = (string)reader["ManagerEmployeeId"];
                        ManagerList.Add(managerId);
                    }
                    conn.Close();
                }

                return ManagerList;
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                return null;
            }
        }

        public List<Agent> GetAllAgents()
        {
            try
            {
                var AgentList = new List<Agent>();
                using (var conn = new SqlConnection(connectionString))
                using (var command = new SqlCommand("GetAllAgents", conn)
                {
                    CommandType = CommandType.StoredProcedure

                })
                {
                    conn.Open();
                    var reader = command.ExecuteReader();
                    // iterate through results, printing each to console
                    while (reader.Read())
                    {
                        var agent  = new Agent() { EmployeeId = (string)reader["EmployeeId"], ManagerEmployeeId = (string)reader["ManagerEmployeeId"] };
                        AgentList.Add(agent);
                    }
                    conn.Close();
                }

                return AgentList;
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                return null;
            }
        }

        public int ManagerQueue_LoginUser(string ManagerEmployeeId, string Password, out int status,  out string error, out string ManagerName)
        {
            //int result = -1;
            status = 0;
            error = "";
            ManagerName = "";
            try
            {
                var ManagerVersionList = new List<ManagerVersion>();
                using (var conn = new SqlConnection(connectionString))
                using (var command = new SqlCommand("ManagerQueue_LoginUser", conn)
                {
                    CommandType = CommandType.StoredProcedure

                })
                {
                    command.Parameters.Add(new SqlParameter("@EmployeeId", ManagerEmployeeId));
                    command.Parameters.Add(new SqlParameter("@Password", Password));
                    conn.Open();
                    var reader = command.ExecuteReader();
                    // iterate through results, printing each to console

                    while (reader.Read())
                    {
                        status = (Int32)reader["Status"];
                        ManagerName = (String)reader["Name"];
                        error = (String)reader["Error"];
                    }
                    conn.Close();
                }

                return 1;
            }
            catch (Exception ex)
            {
                status = 1;
                error = ex.Message;
                logger.Error(ex);
                return -1;
            }
        }


    }
}
