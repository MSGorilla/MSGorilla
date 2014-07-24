using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;

using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Table;

using MSGorilla.Library.Azure;
using MSGorilla.Library.Exceptions;
using MSGorilla.Library.Models;
using MSGorilla.Library.Models.SqlModels;
using MSGorilla.Library.Models.AzureModels;
using MSGorilla.Library.Models.ViewModels;
using MSGorilla.Library.Models.AzureModels.Entity;
using Microsoft.WindowsAzure;

namespace MSGorilla.Library
{
    public class AttachmentManager
    {
        private CloudTable _attachment;
        private CloudBlobContainer _blobcontainer;
        private static string _policyName = "MySASPolicy";

        private string _sasToken;
        private DateTime _tokenGeneratedTimestamp;

        private string SASToken
        {
            get
            {
                if (string.IsNullOrEmpty(_sasToken))
                {
                    _tokenGeneratedTimestamp = DateTime.UtcNow;
                    _sasToken = _blobcontainer.GetSharedAccessSignature(new SharedAccessBlobPolicy(), _policyName);
                }
                else if (DateTime.UtcNow > _tokenGeneratedTimestamp.AddMinutes(30))
                {
                    _tokenGeneratedTimestamp = DateTime.UtcNow;
                    _sasToken = _blobcontainer.GetSharedAccessSignature(new SharedAccessBlobPolicy(), _policyName);
                }
                return _sasToken;
            }
        }

        public AttachmentManager()
        {
            _attachment = AzureFactory.GetTable(AzureFactory.MSGorillaTable.Attachment);
            _blobcontainer = AzureFactory.GetBlobContainer(AzureFactory.MSGorillaBlobContainer.Attachment);

            BlobContainerPermissions blobPermissions = new BlobContainerPermissions();

            blobPermissions.SharedAccessPolicies.Add(_policyName, new SharedAccessBlobPolicy()
            {
                // To ensure SAS is valid immediately, don’t set start time.
                // This way, you can avoid failures caused by small clock differences.
                SharedAccessExpiryTime = DateTime.UtcNow.AddHours(1),
                Permissions = SharedAccessBlobPermissions.Read
            });

            // The public access setting explicitly specifies that 
            // the container is private, so that it can't be accessed anonymously.
            blobPermissions.PublicAccess = BlobContainerPublicAccessType.Off;

            // Set the permission policy on the container.
            _blobcontainer.SetPermissions(blobPermissions);

            // Get the shared access signature to share with users.
        }

        

        public Attachment Upload(string filename, 
                            string filetype, 
                            int filesize, 
                            Stream filestream, 
                            string uploader)
        {
            Attachment attachment = new Attachment();
            attachment.UploadTimestamp = DateTime.UtcNow;
            attachment.Filename = filename;
            attachment.Filetype = filetype;
            attachment.Filesize = filesize;
            attachment.Uploader = uploader;
            attachment.FileID = Guid.NewGuid().ToString() + Path.GetExtension(filename);
            //attachment.AttachmentId = GenerateAttachmentID(attachment.Uploader, attachment.FileID, attachment.UploadTimestamp);

            AttachmentEntity attachmentEntity = new AttachmentEntity(attachment);
            TableOperation insertOperation = TableOperation.InsertOrReplace(attachmentEntity);
            _attachment.Execute(insertOperation);

            CloudBlockBlob blockBlob = _blobcontainer.GetBlockBlobReference(attachment.FileID);
            blockBlob.UploadFromStream(filestream);

            blockBlob.Properties.ContentType = filetype;
            return attachment;
        }

        public Attachment Upload(string filename,
            string filetype,
            byte[] filebytes,
            string uploader)
        {
            if(filebytes == null)
            {
                return null;
            }
            MemoryStream stream = new MemoryStream(filebytes);
            Attachment attachment = Upload(filename, filetype, filebytes.Length, stream, uploader);
            return attachment;
        }

        public Attachment GetAttachmentInfo(string attachmetID)
        {
            if (string.IsNullOrEmpty(attachmetID))
            {
                return null;
            }

            string[] split = attachmetID.Split(';');
            if (split.Length != 2)
            {
                return null;
            }

            string pk = split[0];
            string rk = split[1];
            TableOperation retrieveOperation = TableOperation.Retrieve<AttachmentEntity>(pk, rk);
            TableResult retrievedResult = _attachment.Execute(retrieveOperation);

            if (retrievedResult.Result == null)
            {
                return null;
            }
            AttachmentEntity entity = (AttachmentEntity)retrievedResult.Result;
            return entity.toAttachment();
        }

        public Stream GetAttachment(Attachment attachment)
        {
            if (attachment == null)
            {
                throw new AttachmentNotFoundException();
            }
            CloudBlockBlob blockBlob = _blobcontainer.GetBlockBlobReference(attachment.FileID);
            if (!blockBlob.Exists())
            {
                throw new AttachmentNotFoundException();
            }

            return blockBlob.OpenRead();
        }

        public string GetDownloadLink(Attachment attachment)
        {
            if (attachment == null)
            {
                throw new AttachmentNotFoundException();
            }
            CloudBlockBlob blockBlob = _blobcontainer.GetBlockBlobReference(attachment.FileID);

            return blockBlob.Uri.ToString() + this.SASToken;
        }

        //public static string GenerateAttachmentID(string uploader, string guid, DateTime timestamp)
        //{
        //    string pk = uploader + "_" + Utils.ToAzureStorageDayBasedString(timestamp);
        //    string rk = Utils.ToAzureStorageSecondBasedString(timestamp) + "_" + guid;
        //    return pk + ";" + rk;
        //}
    }
}
