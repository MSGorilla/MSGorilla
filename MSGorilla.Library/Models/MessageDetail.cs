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
        public string Type { get; set; }    // text, picture, retweet.

        // if type is retweet, then Message should be the jsonString of Origin tweet(no recursive!!!).
        public string Message { get; set; }
        public string Url { get; set; }
        public DateTime PostTime { get; set; }
        public int RetweetCount;
        public int ReplyCount;
        public List<Reply> Replies;

        public MessageDetail(Message tweet)
        {
            this.User = tweet.User;
            this.ID = tweet.ID;
            this.Type = tweet.Type;
            this.Message = tweet.MessageContent;
            this.Url = tweet.Url;
            this.PostTime = tweet.PostTime;
        }
    }
}
