using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.WindowsAzure.Storage.Table;

namespace MSGorilla.Library.Models.AzureModels.Entity
{
    //Deprecated
    public class RichMessageEntity : TableEntity
    {
        public string RichMsgID { get; set; }
        public string RichMessage { get; set; }


        public RichMessageEntity() { }

        public RichMessageEntity(string userid, DateTime timestamp, string richMsg)
        {
            this.PartitionKey = string.Format("{0}_{1}", userid,
                Utils.ToAzureStorageDayBasedString(timestamp.ToUniversalTime()));
            this.RowKey = Guid.NewGuid().ToString();
            this.RichMsgID = string.Format("{0};{1}", this.PartitionKey, this.RowKey);
            this.RichMessage = richMsg;
        }
    }
}
