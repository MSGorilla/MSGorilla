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
            try
            {
                Logger.Info("Service started.");
                monitor = new TFSBugTrendMonitor();
                monitor.Work();

                System.Timers.Timer timer = new System.Timers.Timer(1000.0f * 60 * 10); // 1 day later
                // Hook up the Elapsed event for the timer. 
                timer.Elapsed += OnTimedEvent;
                timer.Enabled = true;

                while (Console.ReadKey().KeyChar != 'q')
                {
                    Console.WriteLine("Press q to exit");
                }
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