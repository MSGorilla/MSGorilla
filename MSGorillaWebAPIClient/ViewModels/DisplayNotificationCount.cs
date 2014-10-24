using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSGorilla.WebAPI.Models.ViewModels
{
    public partial class DisplayNotificationCount
    {
        public string Userid { get; set; }
        public int UnreadHomelineMsgCount { get; set; }
        public int UnreadOwnerlineMsgCount { get; set; }
        public int UnreadAtlineMsgCount { get; set; }
        public int UnreadReplyCount { get; set; }
        public int UnreadImportantMsgCount { get; set; }
    }
}
