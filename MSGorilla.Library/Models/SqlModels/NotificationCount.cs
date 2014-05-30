using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel.DataAnnotations;

namespace MSGorilla.Library.Models.SqlModels
{
    [Serializable]
    class NotificationCount
    {
        [Key]
        public string Userid { get; set; }
        public int UnreadHomelineMsgCount { get; set; }
        public int UnreadOwnerlineMsgCount { get; set; }
        public int UnreadUserlineMsgCount { get; set; }
    }
}
