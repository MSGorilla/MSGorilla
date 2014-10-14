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

        public override bool Equals(object obj)
        {
            if (obj == this)
            {
                return true;
            }
            if (!(obj is StatisticsEntity))
            {
                return false;
            }

            StatisticsEntity entity = obj as StatisticsEntity;
            return Equals(this.PartitionKey, entity.PartitionKey) &&
                Equals(this.RowKey, entity.RowKey) &&
                Equals(this.Category, entity.Category) &&
                Equals(this.StatisticalTime, entity.StatisticalTime) &&
                Equals(this.Count, entity.Count);
        }
    }
}
