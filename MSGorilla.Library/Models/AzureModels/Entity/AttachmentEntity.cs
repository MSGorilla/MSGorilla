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
        public string AttachmentID { get; set; }          //{pk};{guid}
        public string FileID { get; set; }
        public string Uploader { get; set; }
        public DateTime UploadTimestamp { get; set; }
        public string Filename { get; set; }
        public string Filetype { get; set; }
        public int Filesize { get; set; }

        public AttachmentEntity()
        {
            ;
        }

        public AttachmentEntity(Attachment attachment)
        {
            this.PartitionKey = Utils.ToAzureStorageDayBasedString(attachment.UploadTimestamp);
            this.RowKey = string.Format("{0}_{1}",
                Utils.ToAzureStorageSecondBasedString(attachment.UploadTimestamp),
                attachment.FileID);

            attachment.AttachmentID = string.Format("{0};{1}", this.PartitionKey, this.RowKey);
            this.AttachmentID = string.Format("{0};{1}", this.PartitionKey, this.RowKey);
            this.FileID = attachment.FileID;
            this.Uploader = attachment.Uploader;
            this.UploadTimestamp = attachment.UploadTimestamp;
            this.Filename = attachment.Filename;
            this.Filetype = attachment.Filetype;
            this.Filesize = attachment.Filesize;
        }

        public Attachment toAttachment()
        {
            Attachment attachment = new Attachment();
            attachment.AttachmentID = this.AttachmentID;
            attachment.FileID = this.FileID;
            attachment.Uploader = this.Uploader;
            attachment.UploadTimestamp = this.UploadTimestamp;
            attachment.Filename = this.Filename;
            attachment.Filetype = this.Filetype;
            attachment.Filesize = this.Filesize;
            return attachment;
        }

        public override bool Equals(object obj)
        {
            if (obj == this)
            {
                return true;
            }
            if (obj == null || !(obj is AttachmentEntity))
            {
                return false;
            }

            AttachmentEntity attachment = obj as AttachmentEntity;
            return Equals(this.AttachmentID, attachment.AttachmentID) &&
                Equals(this.FileID, attachment.FileID) &&
                Equals(this.Uploader, attachment.Uploader) &&
                Equals(this.UploadTimestamp, attachment.UploadTimestamp) &&
                Equals(this.Filename, attachment.Filename) &&
                Equals(this.Filetype, attachment.Filetype) &&
                Equals(this.Filesize, attachment.Filesize);
        }
    }
}
