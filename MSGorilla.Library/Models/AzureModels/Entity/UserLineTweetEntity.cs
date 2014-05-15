using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;

namespace MSGorilla.Library.Models.AzureModels
{
    public class UserLineTweetEntity : TableEntity
    {
        public string TweetContent { get; set; }

        public int RetweetCount { get; set; }

        public int ReplyCount { get; set; }

        public UserLineTweetEntity(Tweet tweet, int retweetCount = 0, int replyCount = 0)
        {
            this.PartitionKey = tweet.User;    //Partition key
            this.RowKey = tweet.ID;
            TweetContent = tweet.ToJsonString();
            RetweetCount = retweetCount;
            ReplyCount = replyCount;
        }

        public UserLineTweetEntity(string userid, string tweetID)
        {
            this.PartitionKey = userid;
            this.RowKey = tweetID;
        }

        public UserLineTweetEntity() { }
    }
}
