using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MSGorilla.WebAPI.Client;
using System.Timers;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            TfsHelper.GetWOSSTableBugsLatestUpdated(DateTime.Now.AddDays(-1.0f));

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
