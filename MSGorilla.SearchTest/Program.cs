using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;

using MSGorilla.Library;
using MSGorilla.Library.Azure;
using MSGorilla.Library.Models;
using MSGorilla.Library.Models.AzureModels;
using MSGorilla.Library.Models.SqlModels;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace MSGorilla.SearchTest
{
    class Program
    {
        static void Main(string[] args)
        {
            CloudQueue _queue = AzureFactory.GetQueue(AzureFactory.MSGorillaQueue.SearchEngineSpider);
            SearchManager _manager = new SearchManager();

            _manager.SearchMessage("woss user1");

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

                    Message msg = JsonConvert.DeserializeObject<Message>(message.AsString);

                    _manager.SpideMessage(msg);
                    _queue.DeleteMessage(message);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Exception in worker role", e.StackTrace);
                }
            }
        }
    }
}
