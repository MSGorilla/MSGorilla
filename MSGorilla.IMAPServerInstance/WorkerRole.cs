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
        IMAPServer.FakeSmtpServer smtpServer;
        public override void Run()
        {
            // This is a sample worker implementation. Replace with your logic.
            Trace.TraceInformation("MSGorilla.IMAPServerInstance entry point called");

            new Thread(new ThreadStart(smtpServer.Start)).Start();

            server.Start();
        }

        public override bool OnStart()
        {
            // Set the maximum number of concurrent connections 
            ServicePointManager.DefaultConnectionLimit = 10000;
            IPAddress addr = IPAddress.Any;
            server = new IMAPServer.IMAPServer(addr, 143);
            smtpServer = new FakeSmtpServer(addr, 25);

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
