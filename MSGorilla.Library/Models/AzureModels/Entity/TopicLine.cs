﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.WindowsAzure.Storage.Table;

namespace MSGorilla.Library.Models.AzureModels.Entity
{
    class TopicLine : BaseMessageEntity
    {
        //public TopicLine()
        //{
        //    ;
        //}

        public TopicLine( Message msg, string topicID) : base(msg)
        {
            this.PartitionKey = string.Format("{0}_{1}", topicID, 
                Utils.ToAzureStorageDayBasedString(msg.PostTime.ToUniversalTime()));
            this.RowKey = msg.ID;
        }
    }
}
