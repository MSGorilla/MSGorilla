using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.WindowsAzure.Storage.Table;

namespace MSGorilla.Library.Models.AzureModels.Entity
{
    public class ReplyEntity : BaseReplyEntity
    {
        public ReplyEntity() { }
        public ReplyEntity(Reply reply)
            : base(reply, reply.MessageID, reply.ID)
        {
            //this.PartitionKey = reply.MessageID;
            //this.RowKey = reply.ReplyID;
       
            //Content = reply.toJsonString();
        }
    }
}
