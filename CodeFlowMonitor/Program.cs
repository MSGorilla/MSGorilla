using MSGorilla.WebAPI.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeFlowMonitor
{
    class Program
    {
        static void Main(string[] args)
        {
            GorillaWebAPI webapi = new GorillaWebAPI("CFMonitor", "User@123");
            webapi.
            webapi.HomeLine(DateTime.UtcNow.AddDays(-1), DateTime.UtcNow);
        }
    }
}
