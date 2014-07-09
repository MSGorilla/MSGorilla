using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace MSGorilla.Library.Models.SqlModels
{
    [Serializable, DataContract]
    public class NotificationCount
    {
        [Key, DataMember]
        public string Userid { get; set; }
        [DataMember]
        public int UnreadImportantMsgCount { get; set; }
        [DataMember]
        public int UnreadHomelineMsgCount { get; set; }
        [DataMember]
        public int UnreadOwnerlineMsgCount { get; set; }
        [DataMember]
        public int UnreadAtlineMsgCount { get; set; }
        [DataMember]
        public int UnreadReplyCount { get; set; }
    }
}
