using System;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using MSGorilla.WebAPI.Client;

namespace MSGorilla.HelloWorldMonitor
{
    public class HelloWorldMonitor
    {

        static void Main(string[] args)
        {
            var appSettings = ConfigurationManager.AppSettings;

            int interval = 1800; // post a message every 30 minutes
            string intervalSecStr = appSettings["IntervalSec"];
            if (intervalSecStr != null)
            {
                interval = Convert.ToInt32(appSettings["IntervalSec"]) * 1000;
            }

            var groupId = appSettings["PostGroupId"];
            var apiEndpointUri = appSettings["ApiEndpointUri"];
            var messageFormatString = appSettings["MessageFormatString"];
            var username = appSettings["Username"];
            var password = appSettings["Password"];

            var client = new GorillaWebAPI(username, password, apiEndpointUri);

            Console.WriteLine("*** HelloWorldMonitor is running ***");


            while (true)
            {
                bool postSuccessful = false;
                for (int retry = 0; retry < 3; retry++) // retry 3 times
                {
                    DateTime now = DateTime.Now;
                    string nowText = String.Format(messageFormatString, now);
                    Console.WriteLine(DateTime.Now + "> Posting a message: " + nowText);
                    try
                    {
                        client.PostMessage(nowText, groupId, "none", "none", null, new string[] { "HelloWorldMonitor" });
                        postSuccessful = true;
                    }
                    catch (Exception e)
                    {
                        Console.Error.WriteLine(DateTime.Now + "> Failed to post the message: " + e.Message);

                    }
                    if (postSuccessful)
                    {
                        Console.WriteLine(DateTime.Now + "> Message posted.");
                        break;
                    }
                    Thread.Sleep(1000 * 60 * 3); // sleep for 3 minutes to wait for the network to recover
                }
                if (!postSuccessful)
                {
                    Console.Error.WriteLine(DateTime.Now + "> Failed to post the message for this hour after retrying 3 times.");
                }
                Console.WriteLine(DateTime.Now + "> Sleep for an hour.");
                Thread.Sleep(interval);
            }
        }
    }
}
