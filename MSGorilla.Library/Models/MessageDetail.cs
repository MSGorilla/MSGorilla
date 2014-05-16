using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSGorilla.Library.Models
{
    public class MessageDetail
    {
        public string User { get; set; }
        public string ID { get; set; }
        public string EventID { get; set; }    // text, picture, retweet.
        public string SchemaID { get; set; }

        // if type is retweet, then Message should be the jsonString of Origin tweet(no recursive!!!).
        public string Message { get; set; }
        //public string Url { get; set; }
        public DateTime PostTime { get; set; }
        //public int RetweetCount;
        public int ReplyCount;
        public List<Reply> Replies;

        public MessageDetail(Message msg)
        {
            this.User = msg.User;
            this.ID = msg.ID;
            this.EventID = msg.EventID;
            this.SchemaID = msg.SchemaID;
            this.Message = msg.MessageContent;
            this.PostTime = msg.PostTime;
        }
    }
}
