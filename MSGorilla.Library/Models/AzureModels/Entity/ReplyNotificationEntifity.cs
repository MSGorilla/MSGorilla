﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;

namespace MSGorilla.Library.Models.AzureModels.Entity
{
    public class ReplyNotificationEntifity : BaseReplyEntity
    {
        public string Content { get; set; }

        public ReplyNotificationEntifity(string userid, Reply reply) : base(reply, userid, reply.ReplyID)
        {
            //this.PartitionKey = userid;
            //this.RowKey = reply.ReplyID;

            //Content = reply.toJsonString();
        }

        public ReplyNotificationEntifity()
        {

        }
    }
}
