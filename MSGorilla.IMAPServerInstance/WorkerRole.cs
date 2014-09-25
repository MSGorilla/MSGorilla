using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;

using MSGorilla.IMAPServer;

namespace MSGorilla.IMAPServerInstance
{
    public class WorkerRole : RoleEntryPoint
    {
        IMAPServer.IMAPServer server;
        public override void Run()
        {
            // This is a sample worker implementation. Replace with your logic.
            Trace.TraceInformation("MSGorilla.IMAPServerInstance entry point called");

            server.Start();
        }

        public override bool OnStart()
        {
            // Set the maximum number of concurrent connections 
            ServicePointManager.DefaultConnectionLimit = 10000;
            IPAddress addr = Dns.Resolve(Dns.GetHostName()).AddressList[0];
            server = new IMAPServer.IMAPServer(addr, 143);


            // For information on handling configuration changes
            // see the MSDN topic at http://go.microsoft.com/fwlink/?LinkId=166357.

            return base.OnStart();
        }

        public override void OnStop()
        {
            server.Stop();
            base.OnStop();
        }
    }
}
