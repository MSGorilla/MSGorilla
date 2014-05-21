using CodeFlowMonitor.CodeFlowService;
using MSGorilla.WebAPI.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CodeFlowMonitor
{
    class Program
    {
        static void Main(string[] args)
        {
            CodeFlowMonitorService service = new CodeFlowMonitorService();
            CodeFlowMonitorService.OnTimedEvent(null, null);

            //ServiceBase.Run(new CodeFlowMonitorService());
            /*
            GorillaWebAPI webapi = new GorillaWebAPI("CFMonitor", "User@123");
            
            //Post Message
            webapi.PostMessage("first one", "none", "1");
            webapi.PostMessage("second one", "none", "1");
            //Get Event Line
            webapi.EventLine("1");
            
            foreach(var a in webapi.EventLine("1"))
            {
                Console.WriteLine(a.ToJsonString());
            }
            foreach (var a in webapi.HomeLine(DateTime.UtcNow.AddDays(-1), DateTime.UtcNow))
            {
                Console.WriteLine(a.ToJsonString());
            }*/
            
        }
    }
}
