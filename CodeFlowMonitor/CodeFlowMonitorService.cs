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
using System.Text.RegularExpressions;

namespace CodeFlowMonitor
{
    public class CodeFlowMonitorService: ServiceBase
    {
        private static System.Timers.Timer serviceTimer = null;
        private static string _statusFileName = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), "MonitorStatus.xml");
        private static List<string> _userlist = null;
        private static object _lockobj = new object();

        private static Dictionary<string, Dictionary<string, int>> _MonitoredReviewDict = new Dictionary<string, Dictionary<string, int>>();

        public CodeFlowMonitorService()
        {
            this.ServiceName = "CodeFlowMonitorService";
        }

        protected override void OnStart(string[] args)
        {
            Logger.WriteInfo("---------------------------------------------");
            Logger.WriteInfo("Start service...");
            base.OnStart(args);

            //Init monitorReviewDict
            Logger.WriteInfo("Init monitorReviewDict");
            if (File.Exists(_statusFileName))
            {
                using (StreamReader sr = new StreamReader(_statusFileName))
                {
                    _MonitoredReviewDict = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, int>>>(sr.ReadToEnd());
                }
            }

            // Create a timer to periodically trigger an action
            serviceTimer = new Timer();

            // Hook up the Elapsed event for the timer.
            serviceTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            serviceTimer.AutoReset = true;

            serviceTimer.Interval = 10000;
            serviceTimer.Start();

            Logger.WriteInfo("Service started");
        }

        protected override void OnStop()
        {
            serviceTimer.Enabled = false;
            base.OnStop();
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
                Logger.WriteInfo("RecordMonitorStatus");
                using (StreamWriter sw = new StreamWriter(_statusFileName))
                {
                    sw.Write(JsonConvert.SerializeObject(_MonitoredReviewDict));

                }                
            }
        }

        public static void OnTimedEvent(object obj, ElapsedEventArgs args)
        {
            //Set run interval as 10 minutes
            serviceTimer.Interval = 600000;

            Logger.WriteInfo("OnTime event, start query active reviews");

            try
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
                                        reviewers.Append(string.Format(" @{0},", reviewer.Name.Substring(reviewer.Name.IndexOf('\\') + 1)));

                                    string description = pkg.Description;
                                    string newDescription = description;
                                    string pat = @"(\d{4,})";

                                    // Instantiate the regular expression object.
                                    Regex reg = new Regex(pat, RegexOptions.IgnoreCase);

                                    // Match the regular expression pattern against a text string.
                                    Match m = reg.Match(description);
                                    while (m.Success)
                                    {
                                        //Console.WriteLine("Match" + (++matchCount));
                                        newDescription = newDescription.Replace(m.Value, string.Format(" #WOSS TFS {0}# ", m.Value));
                                        m = m.NextMatch();
                                    }

                                    string message = string.Format("#WOSS Codeflow# {0}\nIteration {2} {4}\nAuthor: {1}\nReviewer: {3}\n#WOSS Change {5}#\nReview: http://codeflow/Client/CodeFlow2010.application?server=http://codeflow/Services/DiscoveryService.svc&review={6}",
                                        newDescription.Trim(), pkg.Author.Substring(pkg.Author.IndexOf('\\') + 1), pkg.Revision, reviewers.ToString().TrimEnd(new char[] { ',' }), 
                                        pkg.IterationComment, pkg.SourceInfo.SourceName, codeReviewSummary.Key);
                                    Logger.WriteInfo(message);

                                    webapi.PostMessage(message, "none", r.Key, new string[]{"WOSS Codeflow"}, new string[]{pkg.Author.Substring(pkg.Author.IndexOf('\\') + 1)});
                                }
                                _MonitoredReviewDict[username][r.Key] = r.codePackages.Length;
                            }
                            else
                            {
                                Logger.WriteInfo("no new reviews for user " + username);
                            }

                        }
                    }
                }
            }
            catch(Exception e)
            {
                Logger.WriteInfo("Exception happened, try get the codeflow info in next run");
                Logger.WriteInfo(e.Message);
            }
            _recordMonitorStatus();
        }
    }
}
