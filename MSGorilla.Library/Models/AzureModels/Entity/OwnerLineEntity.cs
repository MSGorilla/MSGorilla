using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;

namespace MSGorilla.Library.Models.AzureModels.Entity
{
    class OwnerLineEntity : TableEntity
    {
        public string Content { get; set; }

        public OwnerLineEntity()
        {
            ;
        }

        public OwnerLineEntity(string ownerid, Message msg)
        {
            this.PartitionKey = string.Format("{0}_{1}", ownerid, 
                Utils.ToAzureStorageDayBasedString(msg.PostTime.ToUniversalTime()));
            this.RowKey = msg.ID;

            Content = msg.ToJsonString();
        }
    }
}
