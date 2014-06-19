using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Microsoft.WindowsAzure.Storage.Table;

namespace MSGorilla.Library.Models.AzureModels.Entity
{
    public class AttachmentEntity : TableEntity
    {
        public string Content { get; set; }
        public AttachmentEntity()
        {
            ;
        }

        public AttachmentEntity(Attachment attachment)
        {
            this.PartitionKey = string.Format("{0}_{1}", attachment.Uploader,
                                                Utils.ToAzureStorageDayBasedString(attachment.UploadTimestamp));
            this.RowKey = string.Format("{0}_{1}",
                Utils.ToAzureStorageSecondBasedString(attachment.UploadTimestamp),
                attachment.FileID);

            attachment.AttachmentId = string.Format("{0};{1}", this.PartitionKey, this.RowKey);
            this.Content = JsonConvert.SerializeObject(attachment);
        }
    }
}
