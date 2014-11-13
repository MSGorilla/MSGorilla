using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MSGorilla.Library.Models;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace MSGorilla.TFSBugTrendMonitor
{
    public class TreeBuilder
    {
        const string Type = "BugCount";
        static Dictionary<string, CounterRecord.ComplexValue> cache;

        public static List<string> GetAllLevelSortedExistingNode()
        {
            //sort
            List<string> keys = cache.Keys.ToList();
            keys.Sort((left, right) =>
            {
                int llevel = 0, rlevel = 0;
                foreach (var str in left.Split('&'))
                {
                    if (string.IsNullOrEmpty(str))
                    {
                        llevel++;
                    }
                }
                foreach (var str in right.Split('&'))
                {
                    if (string.IsNullOrEmpty(str))
                    {
                        rlevel++;
                    }
                }

                return rlevel - llevel;
            });

            return keys;
        }

        public static string SimplifyKey(string key)
        {
            IEnumerable<string> temp = from k in key.Split('&')
                                     where !string.IsNullOrEmpty(k)
                                     select k;
            return string.Join("&", temp);             
        }
        public static TFSBugProvider.QueryArgument MergeAndQuery(WorkItemCollection workItems, 
            string key, 
            TFSBugProvider.QueryArgument arg)
        {
            TFSBugProvider.QueryArgument temp = TFSBugProvider.QueryArgument.Parse(key);

            if (arg.State != null)
                temp.State = arg.State;
            if(!string.IsNullOrEmpty(arg.Keyword))
                temp.Keyword = arg.Keyword;
            if (arg.Category != null)
                temp.Category = arg.Category;
            if (!string.IsNullOrEmpty(arg.Creater))
                temp.Creater = arg.Creater;

            CounterRecord.ComplexValue cv = new CounterRecord.ComplexValue(
                    SimplifyKey(temp.ToString()),
                    Type,
                    TFSBugProvider.QueryBugCount(workItems, temp));
            cache[temp.ToString()] = cv;

            return temp;
        }

        public static void LinkParent(TFSBugProvider.QueryArgument arg)
        {
            TFSBugProvider.QueryArgument temp = arg.Clone();
            CounterRecord.ComplexValue cv = cache[arg.ToString()];

            temp.State = null;
            if (!Equals(temp, arg) && cache.ContainsKey(temp.ToString()))
            {
                cache[temp.ToString()].RelatedValues.Add(cv);
            }

            temp = arg.Clone();
            temp.Keyword = null;
            if (!Equals(temp, arg) && cache.ContainsKey(temp.ToString()))
            {
                cache[temp.ToString()].RelatedValues.Add(cv);
            }

            temp = arg.Clone();
            temp.Category = null;
            if (!Equals(temp, arg) && cache.ContainsKey(temp.ToString()))
            {
                cache[temp.ToString()].RelatedValues.Add(cv);
            }

            temp = arg.Clone();
            temp.Creater = null;
            if (!Equals(temp, arg) && cache.ContainsKey(temp.ToString()))
            {
                cache[temp.ToString()].RelatedValues.Add(cv);
            }
        }

        public static CounterRecord.ComplexValue BuildCVTree(WorkItemCollection wiCollection)
        {
            cache = new Dictionary<string, CounterRecord.ComplexValue>();
            CounterRecord.ComplexValue root = new CounterRecord.ComplexValue("BugTrend", Type, wiCollection.Count);
            
            //Add bug State Active, Closed, Resolved
            foreach(TFSBugProvider.State state in Enum.GetValues(typeof(TFSBugProvider.State)))
            {
                TFSBugProvider.QueryArgument arg = new TFSBugProvider.QueryArgument(state);
                CounterRecord.ComplexValue cv = new CounterRecord.ComplexValue(
                    state.ToString(),
                    Type,
                    TFSBugProvider.QueryBugCount(wiCollection, arg));
                root.RelatedValues.Add(cv);
                cache[arg.ToString()] = cv;
            }

            //Add bug type Stress, Func like
            List<string> MonitorKeywords = new List<string>() { "Stress", "Function" };
            List<string> existingNode = GetAllLevelSortedExistingNode();
            foreach (var keyword in MonitorKeywords)
            {
                TFSBugProvider.QueryArgument arg = new TFSBugProvider.QueryArgument(null, keyword);
                CounterRecord.ComplexValue cv = new CounterRecord.ComplexValue(
                    keyword,
                    Type,
                    TFSBugProvider.QueryBugCount(wiCollection, arg));
                root.RelatedValues.Add(cv);
                cache[arg.ToString()] = cv;
            }

            foreach (var key in existingNode)
            {
                foreach (var keyword in MonitorKeywords)
                {
                    TFSBugProvider.QueryArgument addtionArg = new TFSBugProvider.QueryArgument(null, keyword);
                    TFSBugProvider.QueryArgument curArg = MergeAndQuery(wiCollection, key, addtionArg);
                    LinkParent(curArg);
                }
            }

            //Add Category, Table or Blob
            existingNode = GetAllLevelSortedExistingNode();
            foreach (TFSBugProvider.Category category in Enum.GetValues(typeof(TFSBugProvider.Category)))
            {
                TFSBugProvider.QueryArgument arg = new TFSBugProvider.QueryArgument(null, null, category);
                CounterRecord.ComplexValue cv = new CounterRecord.ComplexValue(
                    category.ToString(),
                    Type,
                    TFSBugProvider.QueryBugCount(wiCollection, arg));
                root.RelatedValues.Add(cv);
                cache[arg.ToString()] = cv;
            }

            foreach (var key in existingNode)
            {
                foreach (TFSBugProvider.Category category in Enum.GetValues(typeof(TFSBugProvider.Category)))
                {
                    TFSBugProvider.QueryArgument addtionArg = new TFSBugProvider.QueryArgument(null, null, category);
                    TFSBugProvider.QueryArgument curArg = MergeAndQuery(wiCollection, key, addtionArg);
                    LinkParent(curArg);
                }
            }


            //Add Creater, 
            existingNode = GetAllLevelSortedExistingNode();
            foreach (string userid in TFSBugProvider.GetCreaters(wiCollection))
            {
                TFSBugProvider.QueryArgument arg = new TFSBugProvider.QueryArgument(null, null, null, userid);
                CounterRecord.ComplexValue cv = new CounterRecord.ComplexValue(
                    userid,
                    Type,
                    TFSBugProvider.QueryBugCount(wiCollection, arg));
                root.RelatedValues.Add(cv);
                cache[arg.ToString()] = cv;
            }

            foreach (var key in existingNode)
            {
                foreach (string userid in TFSBugProvider.GetCreaters(wiCollection))
                {
                    TFSBugProvider.QueryArgument addtionArg = new TFSBugProvider.QueryArgument(null, null, null, userid);
                    TFSBugProvider.QueryArgument curArg = MergeAndQuery(wiCollection, key, addtionArg);
                    LinkParent(curArg);
                }
            }


            return root;
        }
    }
}
