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
using Microsoft.WindowsAzure.Storage.Queue;

using MSGorilla.Library;
using MSGorilla.Library.Azure;

namespace MSGorilla.SearchEngineSpider
{
    public class WorkerRole : RoleEntryPoint
    {
        public override void Run()
        {
            // This is a sample worker implementation. Replace with your logic.
            Trace.TraceInformation("MSGorilla.SearchEngineSpider entry point called");

            CloudQueue _queue = AzureFactory.GetQueue(AzureFactory.MSGorillaQueue.SearchEngineSpider);

            while (true)
            {
                try
                {
                    CloudQueueMessage message = null;
                    while ((message = _queue.GetMessage()) == null)
                    {
                        Thread.Sleep(10000);
                    }

                    _queue.UpdateMessage(message,
                        TimeSpan.FromSeconds(60.0),  // Make it visible immediately.
                        MessageUpdateFields.Visibility);








                    _queue.DeleteMessage(message);
                }
                catch (Exception e)
                {
                    Trace.TraceError("Exception in worker role", e.StackTrace);
                }
            }
        }

        public override bool OnStart()
        {
            // Set the maximum number of concurrent connections 
            ServicePointManager.DefaultConnectionLimit = 12;

            // For information on handling configuration changes
            // see the MSDN topic at http://go.microsoft.com/fwlink/?LinkId=166357.

            return base.OnStart();
        }
    }
}
