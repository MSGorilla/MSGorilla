using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace MSGorilla.Library.Models
{
    public class Tweet
    {
        public const string TweetTypeText = "text";
        public const string TweetTypePicture = "picture";
        public const string TweetTypeRetweet = "retweet";

        public string User { get; set; }
        public string ID { get; set; }
        public string Type { get; set; }    // text, picture, retweet.

        // if type is retweet, then Message should be the jsonString of Origin tweet(no recursive!!!).
        public string Message { get; set; } 
        public string Url { get; set; }
        public DateTime PostTime { get; set; }

        public Tweet(string tweetType, string userid, string message, DateTime timestamp, string url = "")
        {
            User = userid;
            Type = tweetType;
            Message = message;
            Url = url;
            PostTime = timestamp.ToUniversalTime();
            ID = string.Format("{0}_{1}",
                                Utils.ToAzureStorageSecondBasedString(PostTime),
                                Guid.NewGuid().ToString());
        }

        public string ToJsonString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
