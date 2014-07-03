using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MSGorilla.WebAPI.Client;
using System.Timers;

namespace TFSMonitor
{
    class Program
    {
        static void Main(string[] args)
        {
            int minutesGap = 0;

            if (args.Length > 0)
            {
                if (args[0].StartsWith("/minutes:"))
                {
                    string[] pair = args[0].Split(':');
                    int.TryParse(pair[1], out minutesGap);
                }
                else
                {
                    Console.WriteLine("Usage: TFSMonitor.exe [/minutes:<minutesToInclude>]");
                    return;
                }
            }

            TfsHelper.GetWOSSTableBugsLatestUpdated(DateTime.Now.AddMinutes(-1.0f * minutesGap));
            System.Timers.Timer timer = new System.Timers.Timer(1000.0f * 60 * 10); // 10 minutes timer
            // Hook up the Elapsed event for the timer. 
            timer.Elapsed += OnTimedEvent;
            timer.Enabled = true;

            while(Console.ReadKey().KeyChar != 'q')
            {
                Console.WriteLine("Press q to exit");
            }
        }

        private static void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            TfsHelper.GetWOSSTableBugsLatestUpdated(DateTime.Now.AddMinutes(-10.0f));
        }
    }
}
