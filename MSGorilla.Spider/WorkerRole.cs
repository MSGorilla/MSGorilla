using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;

using MSGorilla.Library;
using MSGorilla.Library.Azure;
using MSGorilla.Library.Models;
using MSGorilla.Library.Models.AzureModels;
using MSGorilla.Library.Models.SqlModels;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace MSGorilla.Spider
{
    public class WorkerRole : RoleEntryPoint
    {
        public override void Run()
        {
            // This is a sample worker implementation. Replace with your logic.
            MSGorilla.Library.Logger.Info("MSGorilla.Spider entry point called");

            EmulatedCloudQueue _queue = AzureFactory.GetQueue(AzureFactory.MSGorillaQueue.SearchEngineSpider);
            SearchManager _manager = new SearchManager();

            while (true)
            {
                try
                {
                    string message = null;
                    while ((message = _queue.GetMessage()) == null)
                    {
                        Thread.Sleep(10000);
                    }

                    //_queue.UpdateMessage(message,
                    //    TimeSpan.FromSeconds(60.0),  // Make it visible immediately.
                    //    MessageUpdateFields.Visibility);

                    Message msg = JsonConvert.DeserializeObject<Message>(message);
                    //string content = (string)mess.Content;
                    //Message tweet = JsonConvert.DeserializeObject<Message>(content);

                    _manager.SpideMessage(msg);
                    //_queue.DeleteMessage(message);
                }
                catch (Exception e)
                {
                    MSGorilla.Library.Logger.Error("Exception in worker role");
                    MSGorilla.Library.Logger.Error(e.StackTrace);
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
