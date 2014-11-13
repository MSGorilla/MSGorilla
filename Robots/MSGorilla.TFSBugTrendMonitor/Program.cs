using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

            GorillaWebAPI client = new GorillaWebAPI("user1", "111111");

            DateTime now = DateTime.Now;
            TFSBugProvider bugProvider = TFSBugProvider.GetInstance();
            for (int i = 60; i >= 0; i--)
            {
                DateTime timestamp = now.AddDays(0 - i);
                var wic = bugProvider.GetAllWossBugsUntil(timestamp);

                CounterRecord.ComplexValue cv = TreeBuilder.BuildCVTree(wic);
                CounterRecord record = new CounterRecord(timestamp.Date.ToString("yyyy-MM-dd"));
                record.Value.RelatedValues.Add(cv);

                client.InsertCounterRecord(record, "bugtrendtest", "msgorilladev");
                Console.WriteLine(record.Key);
            }





            //TreeBuilder.BuildCVTree(wic);
            //for (int i = 0; i < 100; i++)
            //{
            //    Console.WriteLine(
            //        TFSBugProvider.QueryBugCount(
            //        wic, 
            //        TFSBugProvider.State.Active, 
            //        "Stress", 
            //        TFSBugProvider.Category.Table, 
            //        "Sijia Huang"));
            //}

            //foreach (var user in TFSBugProvider.GetCreaters(wic))
            //{
            //    Console.WriteLine(user);
            //}
        }
    }
}


//var openTrendMetric = (from WorkItem wi in workItems
//                       group wi by wi.CreatedBy into gBugs
//                       select new
//                       {
//                           BugOwner = gBugs.Key,
//                           BugCount = gBugs.Count()
//                       });
//try
//{
//    string counter = string.Format("Bug {0} Trend", ops);


//    foreach (var instance in openTrendMetric)
//    {
//        Console.WriteLine(instance);

//    }
//}