using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;

namespace MSGorilla.Library.Models.AzureModels.Entity
{
    class OwnerLineEntity : BaseMessageEntity
    {
        public OwnerLineEntity()
        {
            ;
        }

        public OwnerLineEntity(string ownerid, Message msg) : base(msg)
        {
            this.PartitionKey = string.Format("{0}_{1}", ownerid, 
                Utils.ToAzureStorageDayBasedString(msg.PostTime.ToUniversalTime()));
            this.RowKey = msg.ID;
        }
    }
}
