using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeFlowMonitor
{
    public class CFTrendStatus
    {
        public int Week { get; set; }
        public DateTime Date { get; set; }
        public Dictionary<DateTime, int> ReviewCountHistory { get; set; }
        public Dictionary<string, int> ReviewCountPeople { get; set; }

        public void AddReviewCount(string username)
        {
            if (Date.Date.Equals(DateTime.Now.Date))
                ReviewCountHistory[Date]++;
            else
            {
                if (ReviewCountHistory == null)
                    ReviewCountHistory = new Dictionary<DateTime, int>();

                Date = DateTime.Now.Date;
                ReviewCountHistory.Add(Date, 1);

                if (ReviewCountHistory.Count > 7)
                    ReviewCountHistory.Remove(ReviewCountHistory.OrderBy(t => t.Key).First().Key);
            }

            if (ReviewCountPeople == null)
                ReviewCountPeople = new Dictionary<string, int>();

            int weekNow = DateTime.Now.DayOfYear / 7;
            if (Week == weekNow)
            {
                if (!ReviewCountPeople.Keys.Contains(username))
                    ReviewCountPeople.Add(username, 1);
                else
                    ReviewCountPeople[username]++;
            }
            else
            {
                ReviewCountPeople.Clear();
                Week = weekNow;

                if (!ReviewCountPeople.Keys.Contains(username))
                    ReviewCountPeople.Add(username, 1);
                else
                    ReviewCountPeople[username]++;
            }
        }
    }
}
