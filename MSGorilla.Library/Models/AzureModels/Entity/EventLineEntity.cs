using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;

namespace MSGorilla.Library.Models.AzureModels.Entity
{
    public class EventLineEntity : TableEntity
    {
        public string Content { get; set; }
        public string RichMessage { get; set; }
        
        public EventLineEntity()
        {
            ;
        }

        public EventLineEntity(Message msg)
        {
            this.PartitionKey = string.Format("{0}_{1}", msg.EventID, 
                Utils.ToAzureStorageDayBasedString(msg.PostTime.ToUniversalTime()));
            this.RowKey = msg.ID;

            Content = msg.ToJsonString();
            RichMessage = msg.RichMessage;

        }
    }
}
