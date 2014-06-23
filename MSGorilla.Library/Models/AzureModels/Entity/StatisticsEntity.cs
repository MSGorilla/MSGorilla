using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;

namespace MSGorilla.Library.Models.AzureModels.Entity
{
    public class StatisticsEntity : TableEntity
    {
        public string Category { get; set; }
        public DateTime StatisticalTime { get; set; }
        public double Count { get; set; }

        public StatisticsEntity(string category, double count, DateTime countdate)
        {
            this.PartitionKey = string.Format("{0}_{1}", category,
                Utils.ToAzureStorageDayBasedString(countdate.ToUniversalTime()));
            this.RowKey = Guid.NewGuid().ToString();

            Category = category;
            StatisticalTime = DateTime.UtcNow;
            Count = count;
        }
    }
}
