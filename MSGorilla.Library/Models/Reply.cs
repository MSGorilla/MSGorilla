using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace MSGorilla.Library.Models
{
    public class Reply
    {
        public string FromUser { get; set; }
        public string ToUser { get; set; }
        public string Message { get; set; }
        public DateTime PostTime { get; set; }
        public string TweetUser { get; set; }
        public string TweetID { get; set; }

        public Reply(string fromUser, string toUser, string message, DateTime timestamp, string tweetUser, string tweetID)
        {
            FromUser = fromUser;
            ToUser = toUser;
            Message = message;
            PostTime = timestamp.ToUniversalTime();
            TweetUser = tweetUser;
            TweetID = tweetID;
        }
        public string toJsonString(){
            return JsonConvert.SerializeObject(this);
        }
    }
}
