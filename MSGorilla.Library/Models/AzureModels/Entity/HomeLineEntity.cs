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
        public string MessageContent { get; set; }

        public HomeLineEntity()
        {
            ;
        }

        public HomeLineEntity(string user,
                                    Message tweet)
        {
            this.PartitionKey = string.Format("{0}_{1}", user, 
                Utils.ToAzureStorageDayBasedString(tweet.PostTime.ToUniversalTime()));
            this.RowKey = string.Format(
                    "{0}_{1}",
                    Utils.ToAzureStorageSecondBasedString(tweet.PostTime.ToUniversalTime()),
                    Guid.NewGuid().ToString()
                );

            MessageContent = tweet.ToJsonString();
        }
    }
}
