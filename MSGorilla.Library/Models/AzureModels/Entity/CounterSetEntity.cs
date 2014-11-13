using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;

namespace MSGorilla.Library.Models.AzureModels.Entity
{
    public class CounterSetEntity : TableEntity
    {
        public string Description { get; set; }
        public DateTime LastUpdateTimestamp { get; set; }
        public int RecordCount { get; set; }
        public CounterSetEntity() { }
        public CounterSetEntity(string groupID, string description = null)
        {
            this.PartitionKey = groupID;
            this.Description = description;
        }

        public static implicit operator CounterSetEntity(MSGorilla.Library.Models.CounterSet counterset)
        {
            CounterSetEntity entity = new CounterSetEntity();
            entity.PartitionKey = counterset.Group;
            entity.RowKey = counterset.Name;
            entity.Description = counterset.Name;
            entity.LastUpdateTimestamp = counterset.LastUpdateTime;
            entity.RecordCount = counterset.RecordCount;
            return entity;
        }
    }
}
