using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;

namespace MSGorilla.Library.Models.AzureModels.Entity
{
    public class PublicSquareLineEntity : BaseMessageEntity
    {
        public PublicSquareLineEntity()
        {
            ;
        }

        public PublicSquareLineEntity(Message msg) : base(msg)
        {
            this.PartitionKey = Utils.ToAzureStorageDayBasedString(msg.PostTime.ToUniversalTime());
            this.RowKey = msg.ID;
        }
    }
}
