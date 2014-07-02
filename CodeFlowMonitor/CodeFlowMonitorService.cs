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
    public class CodeFlowMonitorService : ServiceBase
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

                                    #region Generate review string
                                    StringBuilder reviewers = new StringBuilder("<table><tr style=\"border: 1px solid; border-top: none; border-left: none;border-right: none;\"><td width=\"40%\"><b>Reviewer</b></td><td width=\"30%\"><b>Type</b></td><td width=\"30%\"><b>Status</b></td></tr>");
                                    StringBuilder reviewerstr = new StringBuilder();
                                    foreach (var reviewer in r.reviewers)
                                    {
                                        reviewers.Append(string.Format("<tr><td><b>{3}</b>({0})</td><td>{1}</td><td>{2}</td></tr>",
                                            reviewer.Name, reviewer.Required ? "<b>Required</b>" : "Optional",
                                            reviewer.Status == ReviewerStatus.SignedOff ? "<font color=\"green\">SignedOff</font>" : reviewer.Status.ToString(),
                                            reviewer.DisplayName));
                                        reviewerstr.Append(string.Format(" @{0},", reviewer.Name.Substring(reviewer.Name.IndexOf('\\') + 1)));
                                    }
                                    reviewers.Append("</table>");
                                    #endregion

                                    #region Generate changed file
                                    StringBuilder changedfiles = new StringBuilder("<table><tr style=\"border: 1px solid; border-top: none; border-left: none;border-right: none;\"><td width=\"50px\"><b>Change</b></td><td width=\"30%\"><b>Type</b></td><td><b>FilePath</b></td></tr>");
                                    foreach (var file in pkg.FileChanges)
                                    {
                                        changedfiles.Append(string.Format("<tr><td><b>{0}</b></td><td>{1}</td></tr>", file.ChangeType, file.DepotFilePath));
                                    }
                                    changedfiles.Append("</table>");

                                    string description = pkg.Description;
                                    string newDescription = description;
                                    string pat = @"(\d{4,})";
                                    #endregion

                                    #region replace tfs number with topic
                                    // Instantiate the regular expression object.
                                    Regex reg = new Regex(pat, RegexOptions.IgnoreCase);

                                    // Match the regular expression pattern against a text string.
                                    Match m = reg.Match(description);
                                    List<string> replacedValueList = new List<string>();
                                    while (m.Success)
                                    {
                                        //Console.WriteLine("Match" + (++matchCount));
                                        if (!replacedValueList.Contains(m.Value))
                                        {
                                            newDescription = newDescription.Replace(m.Value, string.Format(" #WOSS TFS {0}# ", m.Value));
                                            replacedValueList.Add(m.Value);
                                        }
                                        m = m.NextMatch();
                                    }
                                    #endregion

                                    string title = string.Format("Code Review: ({0}){1}", pkg.Author.Substring(pkg.Author.IndexOf('\\') + 1), r.Name);

                                    #region Iteration Title
                                    string iteration = "";
                                    if (i == 0)
                                        iteration = "New review submitted";
                                    else if (string.IsNullOrEmpty(r.CompletionMessage))
                                        iteration = "Author started new iteration";
                                    else
                                        iteration = "Review completed";
                                    #endregion

                                    string link = "http://codeflow/Client/CodeFlow2010.application?server=http://codeflow/Services/DiscoveryService.svc&review=" + codeReviewSummary.Key;

                                    string richMessage = string.Format("<h3>{0}</h3><br/><h4><b>{1}</b></h4><br/><b>Open in: [<a href=\"{2}\">CodeFlow</a>]</b><br/><b>Author: {3}</b><br/>{4}<br/><b>Description:</b><br/>{5}<br/><h4><b>Affected Files</b></h4>{6}",
                                        title, iteration, link, pkg.Author, reviewers.ToString(),
                                        pkg.Description.Trim(), changedfiles.ToString());

                                    string message = string.Format("#WOSS Codeflow# {0}\nIteration {2} {4}\nAuthor: {1}\nReviewer: {3}\n#WOSS Change {5}#\nReview: {6}",
                                        newDescription.Trim(), pkg.Author.Substring(pkg.Author.IndexOf('\\') + 1), pkg.Revision, reviewerstr.ToString().TrimEnd(new char[] { ',' }),
                                        pkg.IterationComment, pkg.SourceInfo.SourceName, link);
                                    Logger.WriteInfo(message);

                                    webapi.PostMessage(message, "none", r.Key, new string[] { "WOSS Codeflow" }, new string[] { pkg.Author.Substring(pkg.Author.IndexOf('\\') + 1) },
                                        new string[] { }, richMessage);
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
            catch (Exception e)
            {
                Logger.WriteInfo("Exception happened, try get the codeflow info in next run");
                Logger.WriteInfo(e.Message);
            }
            _recordMonitorStatus();
        }
    }
}
