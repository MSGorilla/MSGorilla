using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;

namespace MSGorilla.Library.Models.AzureModels.Entity
{
    public class EventLineEntity : BaseMessageEntity
    {        
        //public EventLineEntity()
        //{
        //    ;
        //}

        public EventLineEntity(Message msg) : base(msg)
        {
            this.PartitionKey = string.Format("{0}_{1}", this.EventID, 
                Utils.ToAzureStorageDayBasedString(msg.PostTime.ToUniversalTime()));
            this.RowKey = msg.ID;
        }
    }
}
