using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;

namespace MSGorilla.Library.Models.AzureModels.Entity
{
    public class UserLineEntity : BaseMessageEntity
    {
        public int ReplyCount { get; set; }

        public UserLineEntity(Message msg, int replyCount = 0) : base(msg)
        {
            this.PartitionKey = string.Format("{0}_{1}", msg.User,
                Utils.ToAzureStorageDayBasedString(msg.PostTime.ToUniversalTime()));    //Partition key
            this.RowKey = msg.ID;
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
