using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

using MSGorilla.WebAPI.Client;
using MSGorilla.WebAPI.Models.ViewModels;

namespace MSGorilla.WossTableMonitor
{
    public partial class WossMonitorService : ServiceBase
    {
        const string Counter = "Exception";
        const string Category = "WossTableException";
        const string GroupID = "msgorilladev";
        const int ReportPeriod = 60 * 60 * 1000;    //Every one hour

        private GorillaWebAPI client;
        private DisplayMetricDataSet execDataset;
        private DisplayMetricDataSet retriveDataset;
        private DisplayMetricDataSet queryDataset;

        private static System.Timers.Timer serviceTimer = null;
        public WossMonitorService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            // Create a timer to periodically trigger an action
            serviceTimer = new Timer();

            // Hook up the Elapsed event for the timer.
            serviceTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            serviceTimer.AutoReset = true;

            serviceTimer.Interval = 10000;
            serviceTimer.Start();

            client = new GorillaWebAPI("WossTablemonitor", "User@123");
            execDataset = client.QueryMetricDataSet(
                ExceptionProvider.FunctionName.Execute.ToString(),
                Counter,
                Category,
                GroupID);
            retriveDataset = client.QueryMetricDataSet(
                ExceptionProvider.FunctionName.ExecuteRetriveOperation.ToString(),
                Counter,
                Category,
                GroupID);
            queryDataset = client.QueryMetricDataSet(
                ExceptionProvider.FunctionName.ExecuteQuerySegmented.ToString(),
                Counter,
                Category,
                GroupID);

            Logger.Info("Service start.");
            base.OnStart(args);
        }

        public void OnTimedEvent(object obj, ElapsedEventArgs args)
        {
            serviceTimer.Interval = ReportPeriod;

            try
            {
                string key = DateTimeOffset.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");
                int count = ExceptionProvider.GetExceptionCount(ExceptionProvider.FunctionName.Execute);
                client.InsertMetricRecord(execDataset, key, count);
                Logger.Info(string.Format("{0} exceptions recorded in Function: {1}", count, ExceptionProvider.FunctionName.Execute));

                count = ExceptionProvider.GetExceptionCount(ExceptionProvider.FunctionName.ExecuteRetriveOperation);
                client.InsertMetricRecord(retriveDataset, key, count);
                Logger.Info(string.Format("{0} exceptions recorded in Function: {1}", count, ExceptionProvider.FunctionName.ExecuteRetriveOperation));

                count = ExceptionProvider.GetExceptionCount(ExceptionProvider.FunctionName.ExecuteQuerySegmented);
                client.InsertMetricRecord(queryDataset, key, count);
                Logger.Info(string.Format("{0} exceptions recorded in Function: {1}", count, ExceptionProvider.FunctionName.ExecuteQuerySegmented));
            }
            catch (Exception e)
            {
                Logger.Error("Fail to report Woss table exception count.");
                Logger.Error(e.Message);
                Logger.Error(e.StackTrace);
            }
        }

        protected override void OnStop()
        {
            serviceTimer.Enabled = false;
            base.OnStop();
            Logger.Info("Service stopped");
        }
    }
}
