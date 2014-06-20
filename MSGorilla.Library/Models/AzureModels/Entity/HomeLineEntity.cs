using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;

namespace MSGorilla.Library.Models.AzureModels.Entity
{
    public class HomeLineEntity : TableEntity
    {
        public string Content { get; set; }

        public HomeLineEntity()
        {
            ;
        }

        public HomeLineEntity(string user,
                                    Message msg)
        {
            this.PartitionKey = string.Format("{0}_{1}", user, 
                Utils.ToAzureStorageDayBasedString(msg.PostTime.ToUniversalTime()));
            this.RowKey = msg.ID;

            Content = msg.ToJsonString();
        }
    }
}
