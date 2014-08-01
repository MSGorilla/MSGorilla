using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MSGorilla.Library.Models.SqlModels;

namespace MSGorilla.Library.Models.ViewModels
{
    public class DisplayTopic
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int MsgCount { get; set; }
        public string GroupID { get; set; }
        public bool IsLiked { get; private set; }
        public DisplayTopic(Topic topic, bool isliked)
        {
            Id = topic.Id;
            Name = topic.Name;
            Description = topic.Description;
            MsgCount = topic.MsgCount;
            IsLiked = isliked;
            GroupID = topic.GroupID;
        }
    }
}
