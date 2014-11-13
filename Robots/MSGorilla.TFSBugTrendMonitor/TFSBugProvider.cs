using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace MSGorilla.TFSBugTrendMonitor
{
    public class TFSBugProvider
    {
        public enum State { Active, Resolved, Closed}
        public enum Category { Table, Blob}

        WorkItemStore workItemStore; 

        static TFSBugProvider _obj;
        private TFSBugProvider()
        {
            TfsTeamProjectCollection _tpc = new TfsTeamProjectCollection(
                new Uri("https://microsoft.visualstudio.com/defaultcollection"), 
                new UICredentialsProvider()
                );
            _tpc.EnsureAuthenticated();
            workItemStore = _tpc.GetService<WorkItemStore>();
        }

        public static TFSBugProvider GetInstance()
        {
            if (_obj == null)
            {
                _obj = new TFSBugProvider();
            }

            return _obj;
        }

        public WorkItemCollection GetAllWossBugsUntil(DateTime timestamp)
        {
            var workItems = workItemStore.Query(String.Format(@"
                   Select [State]
                   From WorkItems
                   Where [Work Item Type] = 'Bug'
                   AND [Area Path] Under 'WSSC\WSSC-Windows Server and System Center\FSC-File Server and Clustering\WOSS'
                   AND [{0} Date] <= '{1}'", "Created", timestamp.Date));
            return workItems;
        }

        public class QueryArgument
        {
            public State? State;
            public string Keyword;
            public Category? Category;
            public string Creater;

            public QueryArgument() { }
            public QueryArgument(State? state = null, 
                string keyword = null, 
                Category? category = null,
                string creater = null)
            {
                this.State = state;
                this.Keyword = keyword;
                this.Category = category;
                this.Creater = creater;
            }

            public QueryArgument(QueryArgument right)
            {
                this.State = right.State;
                this.Keyword = right.Keyword;
                this.Category = right.Category;
                this.Creater = right.Creater;
            }

            public QueryArgument Clone()
            {
                return new QueryArgument(this.State, this.Keyword, this.Category, this.Creater);
            }

            public override bool Equals(object obj)
            {
                if(obj == null || obj.GetType() != typeof(QueryArgument)){
                    return false;
                }
                if(obj == this){
                    return true;
                }

                QueryArgument temp = obj as QueryArgument;
                return this.State == temp.State &&
                    this.Keyword == temp.Keyword &&
                    this.Category == temp.Category &&
                    this.Creater == temp.Creater;
            }
            public override string ToString()
            {
                return string.Format("{0}&{1}&{2}&{3}",
                    State.ToString(),
                    Keyword,
                    Category.ToString(),
                    Creater);
            }

            public static QueryArgument Parse(string str)
            {
                QueryArgument ret = new QueryArgument();
                if (string.IsNullOrEmpty(str) || str.Split('&').Length != 4)
                {
                    return ret;
                }

                string[] split = str.Split('&');
                if (!string.IsNullOrEmpty(split[0]))
                {
                    ret.State = (TFSBugProvider.State)Enum.Parse(typeof(TFSBugProvider.State), split[0]);
                }

                ret.Keyword = split[1];
                if (string.IsNullOrEmpty(ret.Keyword))
                {
                    ret.Keyword = null;
                }
                if (!string.IsNullOrEmpty(split[2]))
                {
                    ret.Category = (TFSBugProvider.Category)Enum.Parse(typeof(TFSBugProvider.Category), split[2]);
                }

                ret.Creater = split[3];
                if (string.IsNullOrEmpty(ret.Creater))
                {
                    ret.Creater = null;
                }

                return ret;
            }
        }

        public static int QueryBugCount(
            WorkItemCollection workItems,
            QueryArgument arg)
        {
            int count = 0;

            foreach (WorkItem wi in workItems)
            {
                if (arg.State != null && wi.State != arg.State.ToString())
                {
                    continue;
                }
                if (!string.IsNullOrEmpty(arg.Keyword) &&
                    !wi.Fields["Keywords"].Value.ToString().ToLower().Contains(arg.Keyword.ToLower()))
                {
                    continue;
                }
                if (arg.Category != null)
                {
                    // Blob bug
                    if (wi.AreaPath.EndsWith("WBlob"))
                    {
                        if (arg.Category != Category.Blob)
                        {
                            continue;
                        }
                    }
                    // Table bug
                    else if (arg.Category != Category.Table)
                    {
                        continue;
                    }
                }
                if (!string.IsNullOrEmpty(arg.Creater) && wi.CreatedBy != arg.Creater)
                {
                    continue;
                }
                count++;
            }
            return count;
        }

        public static HashSet<string> GetCreaters(WorkItemCollection wiCollection)
        {
            HashSet<string> set = new HashSet<string>();
            foreach (WorkItem wi in wiCollection)
            {
                set.Add(wi.CreatedBy);
            }
            return set;
        }
    }
}
