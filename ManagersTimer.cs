using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using System.Timers;

namespace RabbitMQManager
{
    public class ManagersTimer
    {
        private Timer timer;
        ManagersDB dbManager;
        int managerTimeoutMin;

        public ManagersTimer() {
            int timerElapsed = Int32.Parse(ConfigurationManager.AppSettings["timerElapsed"]);
            managerTimeoutMin = Int32.Parse(ConfigurationManager.AppSettings["managerTimeoutMin"]);
            timer = new Timer(timerElapsed);
            timer.Elapsed += new ElapsedEventHandler(this.OnTimer);

            dbManager = new ManagersDB();
        }

        public void Start() {
            timer.Start();
        }

        public void OnTimer(object sender, ElapsedEventArgs args)
        {
            dbManager.ManagerQueue_LogOffInactiveManagers(managerTimeoutMin);
        }

        public void Stop() {
            timer.Stop();
        }
    }
}
