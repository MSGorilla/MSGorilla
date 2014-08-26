using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Diagnostics;
using System.Windows;
using System.Windows.Navigation;

using Outlook = Microsoft.Office.Interop.Outlook;

using MSGorilla.WebAPI.Client;

namespace MSGorilla.OutlookAddin
{
    public class Utils
    {
        public static string GetCurrentUserID()
        {
            return Globals.ThisAddIn.CurrentUserID;
        }

        public static GorillaWebAPI GetGorillaClient()
        {
            GorillaWebAPI client = new GorillaWebAPI("ShareAccount_" + GetCurrentUserID(), "********");
            return client;
        }

        public static Hyperlink CreateLink(string name, string uri, RequestNavigateEventHandler navigateHandler = null)
        {
            Hyperlink link = new Hyperlink();
            link.IsEnabled = true;
            link.Inlines.Add(name);
            link.NavigateUri = new Uri(uri);
            if (navigateHandler == null)
            {
                link.RequestNavigate += (sender, args) => Process.Start(args.Uri.ToString());
            }
            else
            {
                link.RequestNavigate += navigateHandler;
            }
            return link;
        }

        public static string ThumbnailConverter(string url)
        {
            return "";
        }

        public static string ProcessRichMessage(string richMessage)
        {
            richMessage = richMessage.Substring(1, richMessage.Length - 2);
            richMessage = richMessage.Replace("\\\\", "\\");
            richMessage = richMessage.Replace("\\\"", "\"");
            richMessage = richMessage.Replace("\\r", "");
            richMessage = richMessage.Replace("\\n", "");
            richMessage = richMessage.Replace("\\'", "'");

            return richMessage;
        }
    }
}
