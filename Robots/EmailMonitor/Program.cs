using System;
using System.Linq;
using System.Net;
using System.Collections.Generic;
using System.Configuration;
using Microsoft.Exchange.WebServices.Data;
using MSGorilla.WebAPI.Client;
using MSGorilla.EmailMonitor;
using mshtml;
using System.Text.RegularExpressions;
using System.IO;
using System.Threading;


namespace HelloWorld
{
    class Program
    {
        public static void MonitorEmail()
        {
            EmailMonitor monitor = new EmailMonitor(
                    ConfigurationManager.AppSettings["Address"],
                    ConfigurationManager.AppSettings["Password"],
                    ConfigurationManager.AppSettings["MSGorillaAccount"],
                    ConfigurationManager.AppSettings["MSGorillaAccountPassword"],
                    ConfigurationManager.AppSettings["MonitorEmailGroupAddress"]
                );

            while (true)
            {
                try
                {
                    int count = monitor.ScanAndReport();

                    Thread.Sleep(60000);
                }
                catch (Exception e)
                {
                    Logger.Error(e.StackTrace);
                    Thread.Sleep(30000);
                }
            }
        }

        public static void MonitorReply()
        {
            MSGorillaEmailReplier replier = new MSGorillaEmailReplier(
                ConfigurationManager.AppSettings["Address"],
                ConfigurationManager.AppSettings["Password"],
                ConfigurationManager.AppSettings["MSGorillaAccount"],
                ConfigurationManager.AppSettings["MSGorillaAccountPassword"]);

            while (true)
            {
                replier.CheckReply();
                Thread.Sleep(60000);
            }
        }
        static void Main(string[] args)
        {
            //MSGorilla.WebAPI.Client.Constant.UriRoot = "https://127.0.0.1:44301/api";
            //ServicePointManager.ServerCertificateValidationCallback = ((sender, certificate, chain, sslPolicyErrors) => true);

            Logger.Info("User email account:    " + ConfigurationManager.AppSettings["Address"]);
            Logger.Info("User MSGorilla account:    " + ConfigurationManager.AppSettings["MSGorillaAccount"]);
            Logger.Info("Monitor email group account:    " + ConfigurationManager.AppSettings["MonitorEmailGroupAddress"]);

            Thread emailMonitorThread = new Thread(MonitorEmail);
            Thread replyMonitorThread = new Thread(MonitorReply);

            emailMonitorThread.Start();
            replyMonitorThread.Start();
        }
    }
}