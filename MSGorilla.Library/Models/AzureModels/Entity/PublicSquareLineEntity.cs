using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;

namespace MSGorilla.Library.Models.AzureModels.Entity
{
    public class PublicSquareLineEntity : TableEntity
    {
        public string Content { get; set; }

        public PublicSquareLineEntity()
        {
            ;
        }

        public PublicSquareLineEntity(Message msg)
        {
            this.PartitionKey = Utils.ToAzureStorageDayBasedString(msg.PostTime.ToUniversalTime());
            this.RowKey = msg.ID;

            Content = msg.ToJsonString();
        }
    }
}
