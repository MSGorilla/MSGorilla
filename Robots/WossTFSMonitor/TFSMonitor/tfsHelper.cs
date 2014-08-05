using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using Microsoft.EngineeringServices.DataContracts.Model;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using System.Net;
using System.Windows.Forms;
using MSGorilla.WebAPI.Client;

namespace TFSMonitor
{
    public class TfsHelper
    {
        static TfsTeamProjectCollection tpc = null;

        private static string DisplayName2Alias(string dispName)
        {
            if (string.IsNullOrEmpty(dispName))
            {
                return "";
            }
            if (Constants.disp2aliasMap.ContainsKey(dispName.ToLower()))
            {
                return Constants.disp2aliasMap[dispName.ToLower()];
            }
            return dispName;
        }

        public static void GetWOSSTableBugsLatestUpdated(DateTime lastCheckTime)
        {
            
            string bugLinkTemplate = "https://microsoft.visualstudio.com/DefaultCollection/WSSC/_workitems";
            string bugLinkDataTemplate = "#_a=edit&id={0}&triage=true";
            string[] monitoredFields = new string[] {
                        "Title", 
                        "Work Item Type",
                        "Issue Type",
                        "Severity",
                        "State",
                        "Reason",
                        "Resolved By",
                        "Iteration Path",
                        "Assigned To",
                        "Activated By",
                        "Priority",
                        "Found In",
                        "Task Type",
                        "Triage",
                        "ID",
                        "SD Changelist",
                        "Repro Steps"
                        };

            string fieldChangeTemplate =
                "<div class=\"tfs-collapsible-content\" style=\"display: block;\">" +
                    "<div class=\"container\">" +
                        "<table  class=MsoNormalTable border=1 cellspacing=0 cellpadding=0 width=\"100%\" style='width:1000px;mso-cellspacing:0cm;border:solid green 1.5pt;mso-yfti-tbllook:1184;mso-padding-alt:0cm 0cm 0cm 0cm'>" +
                            "<tbody>" +
                                "<tr style=\"background-color: rgb(134, 202, 247); mso-yfti-irow: 0; mso-yfti-firstrow: yes; color: black; font-family: \"sans-serif\"; font-size: 9pt\"><td>Field</td><td>New value</td><td>Old Value</td></tr>" +
                                "fieldChangeRows" +
                            "</tbody>" +
                        "</table>" +
                    "</div>" +
                "</div>";
            if (tpc == null)
            {
                tpc = new TfsTeamProjectCollection(new Uri("https://microsoft.visualstudio.com/defaultcollection"), new UICredentialsProvider());
                tpc.EnsureAuthenticated();
            }
            if (tpc != null)
            {
                try
                {
                    WorkItemStore workItemStore = tpc.GetService<WorkItemStore>(); var workItems = workItemStore.Query(String.Format(@"
                   Select [Title]
                   From WorkItems
                   Where [Work Item Type] = 'Bug'
                   AND [Area Path] Under 'WSSC\WSSC-Windows Server and System Center\FSC-File Server and Clustering\WOSS'
                   AND [Issue Type] = 'Code Defect'
                   AND [Changed Date] > '{0}'", lastCheckTime.Date.AddDays(-1.0f)));

                    // Update existing work item
                    if (workItems.Count > 0)
                    {
                        foreach (WorkItem wi in workItems)
                        {
                            if (wi.ChangedDate.CompareTo(lastCheckTime) < 0)
                            {
                                continue;
                            }

                            string message = "";

                            int id = wi.Id;
                            string owner = DisplayName2Alias(wi.CreatedBy);
                            string title = wi.Title;
                            string issueType = (string)wi.Fields["Issue Type"].Value;
                            string assignTo = DisplayName2Alias((string)wi.Fields["Assigned To"].Value);
                            string build = (string)wi.Fields["Found In"].Value;
                            string changeBy = DisplayName2Alias(wi.ChangedBy);
                            string closeBy = DisplayName2Alias((string)wi.Fields["Closed By"].Value);
                            string resolveBy = DisplayName2Alias((string)wi.Fields["Resolved By"].Value);
                                                                                    
                            if (wi.Revision == 1)// new created
                            {
                                message = string.Format("#WOSS TFS Created# #WOSS TFS {0}# Created by @{1}\n", id, owner);
                                message += "Bug Title: " + title + "\n";
                                message += "Bug Assigned to @" + assignTo + "\n";
                                if (!string.IsNullOrEmpty(build)) message += "Bug found in build #WOSS Build " + build + "#\n";
                            }
                            else if (wi.ChangedBy == "Payload Tracking (microsoft)") // resolved by SD check-in
                            {
                                message = string.Format("#WOSS TFS Checkin# #WOSS TFS {0}# has check-in #WOSS Change {1}#  by @{2}\n", id, wi.Fields["SD Changelist"].Value, wi.Fields["SD User"].Value);
                            }
                            else if (wi.State != (string)(wi.Fields["State"].OriginalValue)) // State Changes
                            {
                                if (wi.State == "Active")
                                {
                                    message = string.Format("#WOSS TFS Activated# #WOSS TFS {0}# Activated by @{1}\n", id, wi.Fields["Activated By"].Value);
                                }
                                else if (wi.State == "Closed")
                                {
                                    message = string.Format("#WOSS TFS Closed# #WOSS TFS {0}# Closed by @{1}\n", id, closeBy);
                                }
                                else
                                {
                                    message = string.Format("#WOSS TFS Resolved# #WOSS TFS {0}# Resolved by @{1}\n", id, resolveBy);
                                }
                            }
                            else // Other changes
                            {
                                message = string.Format("#WOSS TFS Changed# #WOSS TFS {0}# changed by @{1} ({2})\n", wi.Id, changeBy, wi.ChangedBy);
                            }

                            StringBuilder richMessage = new StringBuilder();
                            var revLatest = wi.Revisions[wi.Revision - 1];
                            foreach (var fieldName in monitoredFields)
                            {
                                if (revLatest.Fields[fieldName].Value != null && revLatest.Fields[fieldName].Value != revLatest.Fields[fieldName].OriginalValue)
                                {
                                    if (revLatest.Fields[fieldName].Value.GetType() == typeof(System.Int32))
                                    {
                                        var cV = (int)revLatest.Fields[fieldName].Value;
                                        var oV = (int)(revLatest.Fields[fieldName].OriginalValue == null ? 0 : revLatest.Fields[fieldName].OriginalValue);
                                        if (cV == oV)
                                        {
                                            continue;
                                        }
                                    }

                                    string oriValue = (string)revLatest.Fields[fieldName].OriginalValue;
                                    if (string.IsNullOrEmpty(oriValue)) oriValue = "(null)";
                                    richMessage.Append(string.Format("<tr><td>{0}</td><td>{1}</td><td>{2}</td></tr>", revLatest.Fields[fieldName].Name, revLatest.Fields[fieldName].Value, oriValue));
                                }
                            }

                            //message += "Bug link: " + bugLinkTemplate + Uri.EscapeDataString(string.Format(bugLinkDataTemplate, id));
                            message += "Bug link: " + bugLinkTemplate + string.Format(bugLinkDataTemplate, id);

                            Console.WriteLine(message);
                            // twitter message
                            try
                            {
                                GorillaWebAPI webAPI = new GorillaWebAPI("WossTFSMonitor", "User@123");                                
                                webAPI.PostMessage(message, null, "none", id.ToString(), new string[] { "WOSS TFS" }, new string[] { owner }, new string[] { assignTo, resolveBy, closeBy, changeBy }, fieldChangeTemplate.Replace("fieldChangeRows", richMessage.ToString()));
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine("Exception happen when twittering: " + e);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Exception throw: " + e);
                    return;
                }

            }
        }

    }
}
