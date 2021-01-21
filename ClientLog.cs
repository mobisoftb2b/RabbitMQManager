using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Text;
using NLog;


namespace RabbitMQManager
{
    class ClientLog
    {
        string clientLogFolder = ConfigurationManager.AppSettings["clientLogFolder"];
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public void SaveFile(string fileContent, string fileName) {
            string path = clientLogFolder + fileName;

            try
            {
                // Create the file, or overwrite if the file exists.
                using (FileStream fs = File.Create(path))
                {
                    byte[] info = new UTF8Encoding(true).GetBytes(fileContent);
                    // Add some information to the file.
                    fs.Write(info, 0, info.Length);
                }
            }

            catch (Exception ex)
            {
                logger.Error(ex);
            }
        }
    }
}
