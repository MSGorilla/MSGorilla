using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.WindowsAzure.Storage.Table;

namespace MSGorilla.Library.Models.AzureModels
{
    public class ReplyEntity : TableEntity
    {
        public string Content { get; set; }

        public ReplyEntity() { }
        public ReplyEntity(Reply reply)
        {
            this.PartitionKey = reply.TweetID;
            this.RowKey = string.Format("{0}_{1}", 
                Utils.ToAzureStorageSecondBasedString(reply.PostTime.ToUniversalTime()),
                Guid.NewGuid().ToString());

            Content = reply.toJsonString();
        }
    }
}
