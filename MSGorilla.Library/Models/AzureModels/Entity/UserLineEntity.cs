using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;

namespace MSGorilla.Library.Models.AzureModels
{
    public class UserLineEntity : TableEntity
    {
        public string Content { get; set; }

        //public int RetweetCount { get; set; }

        public int ReplyCount { get; set; }

        public UserLineEntity(Message msg, int replyCount = 0)
        {
            this.PartitionKey = string.Format("{0}_{1}", msg.User,
                Utils.ToAzureStorageDayBasedString(msg.PostTime.ToUniversalTime()));    //Partition key
            this.RowKey = msg.ID;
            Content = msg.ToJsonString();
            //RetweetCount = retweetCount;
            ReplyCount = replyCount;
        }

        //public UserLineEntity(string userid, string tweetID)
        //{
        //    this.PartitionKey = userid;
        //    this.RowKey = tweetID;
        //}

        public UserLineEntity() { }
    }
}
