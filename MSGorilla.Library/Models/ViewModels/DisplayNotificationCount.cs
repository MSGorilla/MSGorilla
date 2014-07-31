using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSGorilla.Library.Models.ViewModels
{
    public class DisplayNotificationCount
    {
        public string Userid { get; set; }
        public int UnreadHomelineMsgCount { get; set; }
        public int UnreadOwnerlineMsgCount { get; set; }
        public int UnreadAtlineMsgCount { get; set; }
        public int UnreadReplyCount { get; set; }
        public int UnreadImportantMsgCount { get; set; }

        public static implicit operator DisplayNotificationCount(MSGorilla.Library.Models.SqlModels.NotificationCount notif)
        {
            DisplayNotificationCount dnotif = new DisplayNotificationCount();
            dnotif.Userid = notif.Userid;
            dnotif.UnreadAtlineMsgCount = notif.UnreadAtlineMsgCount;
            dnotif.UnreadHomelineMsgCount = notif.UnreadHomelineMsgCount;
            dnotif.UnreadImportantMsgCount = notif.UnreadImportantMsgCount;
            dnotif.UnreadOwnerlineMsgCount = notif.UnreadOwnerlineMsgCount;
            dnotif.UnreadReplyCount = notif.UnreadReplyCount;

            return dnotif;
        }
    }
}
