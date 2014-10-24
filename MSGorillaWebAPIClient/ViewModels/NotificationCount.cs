using System;
using System.Collections.Generic;

namespace MSGorilla.WebAPI.Models.ViewModels
{
    public partial class NotificationCount
    {
        public string Userid { get; set; }
        public int UnreadHomelineMsgCount { get; set; }
        public int UnreadOwnerlineMsgCount { get; set; }
        public int UnreadAtlineMsgCount { get; set; }
        public int UnreadReplyCount { get; set; }
        public int UnreadImportantMsgCount { get; set; }

    }
}
