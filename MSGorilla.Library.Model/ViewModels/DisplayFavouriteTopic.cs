using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSGorilla.Library.Models.ViewModels
{
    public partial class DisplayFavouriteTopic
    {
        public string userid {get; set;}
        public int topicID {get; set;}
        public int UnreadMsgCount { get; set; }
        public string topicName { get; set; }
        public string topicDescription { get; set; }
        public int topicMsgCount { get; set; }
        public string GroupID { get; set; }
    }
}
