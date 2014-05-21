using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace CodeFlowMonitor
{
    public sealed class Settings
    {
        public static List<string> ProjectsToMonitor
        {
            get
            {
                string s = Environment.ExpandEnvironmentVariables(ConfigurationManager.AppSettings["ProjectsToMonitor"].ToString());
                return s.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries).ToList();
            }
        }

        public static List<string> UserGroupsToMonitor
        {
            get
            {
                string s = Environment.ExpandEnvironmentVariables(ConfigurationManager.AppSettings["UserGroupsToMonitor"].ToString());
                return s.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            }
        }
    }
    
}
