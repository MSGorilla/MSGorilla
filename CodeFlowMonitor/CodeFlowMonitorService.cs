using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceProcess;
using System.Timers;
using CodeFlowMonitor.CodeFlowService;
using Newtonsoft.Json;
using System.IO;
using MSGorilla.WebAPI.Client;

namespace CodeFlowMonitor
{
    public class CodeFlowMonitorService: ServiceBase
    {
        private static System.Timers.Timer serviceTimer = null;
        private static string _statusFileName = "MonitorStatus.xml";
        private static List<string> _userlist = null;
        private static object _lockobj = new object();

        private static Dictionary<string, Dictionary<string, int>> _MonitoredReviewDict = new Dictionary<string, Dictionary<string, int>>();

        public CodeFlowMonitorService()
        {
            this.ServiceName = "CodeFlowMonitorService";
        }

        protected override void OnStart(string[] args)
        {
            base.OnStart(args);

            //Init monitorReviewDict
            using (StreamReader sr = new StreamReader(_statusFileName))
            {
                _MonitoredReviewDict = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, int>>>(sr.ReadToEnd());
            }  


            // Create a timer to periodically trigger an action
            serviceTimer = new Timer();

            // Hook up the Elapsed event for the timer.
            serviceTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            serviceTimer.AutoReset = true;

            //Set run interval as 10 minutes
            serviceTimer.Interval = 600000;
            serviceTimer.Start();
        }

        protected override void OnStop()
        {
            base.OnStop();
            serviceTimer.Enabled = false;
        }

        private static void _initMonitorUserList()
        {
            if (_userlist == null)
            {
                lock (_lockobj)
                {
                    if (_userlist == null)
                    {
                        _userlist = new List<string>();
                        foreach (string sgAlias in Settings.UserGroupsToMonitor)
                            _userlist.AddRange(MemberShipHelper.GetADSecurityGroupUsers(sgAlias));
                    }
                }
            }
        }

        private static void _recordMonitorStatus()
        {
            lock (_lockobj)
            {
                using (StreamWriter sw = new StreamWriter(_statusFileName))
                {
                    sw.Write(JsonConvert.SerializeObject(_MonitoredReviewDict));

                }                
            }
        }

        public static void OnTimedEvent(object obj, ElapsedEventArgs args)
        {
            _initMonitorUserList();

            bool isNew = false;
            ReviewServiceClient client = new ReviewServiceClient();

            GorillaWebAPI webapi = new GorillaWebAPI("CFMonitor", "User@123");
            foreach (string username in _userlist)
            {
                if (!_MonitoredReviewDict.Keys.Contains(username))
                {
                    _MonitoredReviewDict.Add(username, new Dictionary<string, int>());
                    isNew = true;
                }
                //get all active reviews

                CodeReviewSummary[] reviews = client.GetActiveReviewsForAuthor(username);

                if (reviews != null)
                {
                    //scan each review
                    foreach (var codeReviewSummary in reviews)
                    {
                        CodeReview r = client.GetReview(codeReviewSummary.Key);

                        if (!_MonitoredReviewDict[username].Keys.Contains(r.Key))
                        {
                            _MonitoredReviewDict[username].Add(r.Key, 0);
                            isNew = true;
                        }

                        if (_MonitoredReviewDict[username][r.Key] < r.codePackages.Length)
                        {
                            isNew = true;
                        }

                        if (isNew)
                        {
                            for (int i = _MonitoredReviewDict[username][r.Key]; i < r.codePackages.Length; i++)
                            {
                                CodePackage pkg = r.codePackages[i];
                                StringBuilder reviewers = new StringBuilder();
                                foreach (var reviewer in r.reviewers)
                                    reviewers.Append(string.Format("{0},", reviewer.DisplayName));
                                string message = string.Format("CodeReview {0}Iteration {2} {4}\nAuthor: {1}\nReviewer: {3}",
                                    pkg.Description, pkg.Author, pkg.Revision, reviewers.ToString().TrimEnd(new char[] { ',' }), pkg.IterationComment);
                                Console.WriteLine(message);
                                Console.WriteLine();

                                webapi.PostMessage(message, "none", r.Key);
                            }
                            _MonitoredReviewDict[username][r.Key] = r.codePackages.Length;
                        }

                    }
                }
            }
            _recordMonitorStatus();
        }
    }
}
