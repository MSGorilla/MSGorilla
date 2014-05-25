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
            this.PartitionKey = reply.MessageID;
            this.RowKey = reply.ReplyID;

            Content = reply.toJsonString();
        }
    }
}
