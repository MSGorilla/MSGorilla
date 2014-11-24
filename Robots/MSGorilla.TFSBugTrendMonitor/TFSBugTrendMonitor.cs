using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using MSGorilla.WebAPI.Client;
using MSGorilla.Library.Models;


namespace MSGorilla.TFSBugTrendMonitor
{
    public class TFSBugTrendMonitor
    {
        GorillaWebAPI client = new GorillaWebAPI("WossTFSMonitor ", "User@123");

        bool ShouldIWord(DateTime currentTime)
        {
            try
            {
                string line = File.ReadAllText("lastUpdateTime.txt");
                DateTime last = DateTime.Parse(line);
                if (currentTime.Date > last.Date)
                {
                    return true;
                }
                return false;
            }
            catch (Exception e)
            {
                Logger.Warning("Fail to get last update time.");
                Logger.Warning(e.Message);
                Logger.Warning(e.StackTrace);
                return true;
            }
            
        }

        void WorkComplete(DateTime currentTime)
        {
            try
            {
                File.WriteAllText("lastUpdateTime.txt", currentTime.ToString());
            }
            catch (Exception e)
            {
                Logger.Warning("Fail to write last update time.");
                Logger.Warning(e.Message);
                Logger.Warning(e.StackTrace);
            }
        }

        public void Work()
        {
            DateTime current = DateTime.Now;
            if (!ShouldIWord(current))
            {
                return;
            }

            Logger.Info("Get latest data.");
            TFSBugProvider bugProvider = TFSBugProvider.GetInstance();
            var wic = bugProvider.GetAllWossBugsUntil(current);

            CounterRecord.ComplexValue cv = TreeBuilder.BuildCVTree(wic);
            CounterRecord record = new CounterRecord(current.Date.ToString("yyyy-MM-dd"));
            record.Value.RelatedValues.Add(cv);

            Logger.Info("Update BugDailyTrend.");
            client.InsertCounterRecord(record, "BugDailyTrend", "woss");
            if (current.DayOfWeek == DayOfWeek.Sunday)
            {
                Logger.Info("Update BugWeeklyTrend.");
                client.InsertCounterRecord(record, "BugWeeklyTrend", "woss");
            }
            if (current.Day == 1)
            {
                Logger.Info("Update BugMonthlyTrend.");
                client.InsertCounterRecord(record, "BugMonthlyTrend", "woss");
            }

            WorkComplete(current);
        }
    }
}
