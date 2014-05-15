using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;

namespace MSGorilla.Library.Models.AzureModels.Entity
{
    public class HomeLineTweetEntity : TableEntity
    {
        public string TweetContent { get; set; }

        public HomeLineTweetEntity()
        {
            ;
        }

        public HomeLineTweetEntity(string user,
                                    Tweet tweet)
        {
            this.PartitionKey = string.Format("{0}_{1}", user, 
                Utils.ToAzureStorageDayBasedString(tweet.PostTime.ToUniversalTime()));
            this.RowKey = string.Format(
                    "{0}_{1}",
                    Utils.ToAzureStorageSecondBasedString(tweet.PostTime.ToUniversalTime()),
                    Guid.NewGuid().ToString()
                );

            TweetContent = tweet.ToJsonString();
        }
    }
}
