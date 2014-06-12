using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.WindowsAzure.Storage.Table;

namespace MSGorilla.Library.Models.AzureModels.Entity
{
    class TopicLine : TableEntity
    {
        public string Content { get; set; }

        public TopicLine()
        {
            ;
        }

        public TopicLine( Message msg, string topicID)
        {
            this.PartitionKey = string.Format("{0}_{1}", topicID, 
                Utils.ToAzureStorageDayBasedString(msg.PostTime.ToUniversalTime()));
            this.RowKey = msg.ID;

            Content = msg.ToJsonString();
        }
    }
}
