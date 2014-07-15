using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.ServiceProcess;
using System.Net.Mail;

namespace MSGorilla.StatusReporter
{
    class Program
    {
        static void Main(string[] args)
        {
            //Console.WriteLine(collector.GetUserCount());
            //Console.WriteLine(collector.GetRobotCount());
            //Console.WriteLine(collector.GetTopicCount());
            //Console.WriteLine(string.Join( ",", collector.GetRobotID() ));
            //Console.WriteLine(collector.GetTotalRobotMessageCountByDateUtc(DateTime.UtcNow.AddDays(-1)));

            //Console.WriteLine(Execute("ping localhost", 10));

            //StatusReporter reporter = new StatusReporter();
            //reporter.Report();

            ServiceBase.Run(new ReportService());
        }
    }
}
