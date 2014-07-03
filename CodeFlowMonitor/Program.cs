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
            //DiscoveryServiceReference.DiscoveryServiceClient client = new DiscoveryServiceReference.DiscoveryServiceClient();
            //var d = client.Discover();
            //CodeFlowMonitorService.OnTimedEvent(null, null);
            ServiceBase.Run(new CodeFlowMonitorService());
        }
    }
}
