using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MSGorilla.Library.Models.SqlModels;
using MSGorilla.Library.DAL;

namespace MSGorilla.Library.Models.ViewModels
{
    public class DisplayFavouriteTopic
    {
        public string userid {get; set;}
        public int topicID {get; set;}
        public int UnreadMsgCount { get; set; }
        public string topicName { get; set; }
        public string topicDescription { get; set; }
        public int topicMsgCount { get; set; }

        public DisplayFavouriteTopic(FavouriteTopic ftopic, MSGorillaContext _gorillaCtx)
        {
            Topic topic;
            if (ftopic == null || ((topic = _gorillaCtx.Topics.Find(ftopic.TopicID)) == null))
            {
                topicID = UnreadMsgCount = topicMsgCount = 0;
                userid = topicName = topicDescription = null;
            }
            else
            {
                userid = ftopic.Userid;
                topicID = ftopic.TopicID;
                UnreadMsgCount = ftopic.UnreadMsgCount;
                topicName = topic.Name;
                topicDescription = topic.Description;
                topicMsgCount = topic.MsgCount;
            }
        }
    }
}
