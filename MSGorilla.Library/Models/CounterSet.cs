using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSGorilla.Library.Models
{
    public class CounterSet
    {
        public string Group;
        public string Name;
        public string Description;
        public DateTime LastUpdateTime;
        public int RecordCount;

        public static implicit operator CounterSet(MSGorilla.Library.Models.AzureModels.Entity.CounterSetEntity entity)
        {
            CounterSet cs = new CounterSet();
            cs.Group = entity.PartitionKey;
            cs.Name = entity.RowKey;
            cs.Description = entity.Description;
            cs.LastUpdateTime = entity.LastUpdateTimestamp;
            cs.RecordCount = entity.RecordCount;

            return cs;
        }
    }
}
