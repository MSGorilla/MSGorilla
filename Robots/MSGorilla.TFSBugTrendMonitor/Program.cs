using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Net;
using System.Net.Security;

using MSGorilla.Library.Models;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client;

using MSGorilla.WebAPI.Client;


namespace MSGorilla.TFSBugTrendMonitor
{
    class Program
    {
        static TFSBugTrendMonitor monitor;
        public static string DisplayName2Alias(string name)
        {
            return name;
        }

        static void Main(string[] args)
        {
            Constant.UriRoot = "http://localhost:14061/api";
            ServicePointManager.ServerCertificateValidationCallback =
                new RemoteCertificateValidationCallback(
                    delegate
                    { return true; }
                );

            try
            {
                monitor = new TFSBugTrendMonitor();

                monitor.Work();

                System.Timers.Timer timer = new System.Timers.Timer(1000.0f * 60 * 10); // 1 day later
                // Hook up the Elapsed event for the timer. 
                timer.Elapsed += OnTimedEvent;
                timer.Enabled = true;
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
                Logger.Error(e.StackTrace);
            }

            
        }

        private static void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            try
            {
                monitor.Work();
            }
            catch (Exception exception)
            {
                Logger.Error(exception.Message);
                Logger.Error(exception.StackTrace);
            }
        }
    }
}