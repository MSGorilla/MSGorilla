using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Blob;

using System.Configuration;

namespace MSGorilla.Library.Azure
{
    public static class AzureFactory
    {
        public enum MSGorillaTable{
            Homeline,
            Userline,
            PublicSquareLine,
            TopicLine,
            EventLine,
            OwnerLine,
            AtLine,
            Reply,
            ReplyNotification,
            ReplyArchive,
            Attachment
        }

        public const string AttachmentContainer = "attachment";
        public const string QueueName = "messagequeue";

        private static CloudStorageAccount _storageAccount;
        private static Dictionary<MSGorillaTable, string> _dict;
        static AzureFactory()
        {
            string connectionString = CloudConfigurationManager.GetSetting("StorageConnectionString");
            if (string.IsNullOrEmpty(connectionString))
            {
                connectionString = ConfigurationManager.ConnectionStrings["StorageConnectionString"].ConnectionString;
            }
            _storageAccount = CloudStorageAccount.Parse(connectionString);
            _dict = new Dictionary<MSGorillaTable, string>();

            _dict.Add(MSGorillaTable.Homeline, "Homeline");
            _dict.Add(MSGorillaTable.Userline, "Userline");
            _dict.Add(MSGorillaTable.EventLine, "EventlineTweet");
            _dict.Add(MSGorillaTable.PublicSquareLine, "PublicSquareline");
            _dict.Add(MSGorillaTable.TopicLine, "Topicline");
            _dict.Add(MSGorillaTable.OwnerLine, "Ownerline");
            _dict.Add(MSGorillaTable.AtLine, "Atline");
            _dict.Add(MSGorillaTable.Reply, "Reply");
            _dict.Add(MSGorillaTable.ReplyNotification, "ReplyNotification");
            _dict.Add(MSGorillaTable.ReplyArchive, "ReplyArchive");
            _dict.Add(MSGorillaTable.Attachment, "Attachment");
        }

        public static CloudTable GetTable(MSGorillaTable table)
        {
            var client = _storageAccount.CreateCloudTableClient();
            var aztable = client.GetTableReference(_dict[table]);
            aztable.CreateIfNotExists();
            return aztable;
        }

        public static CloudQueue GetQueue()
        {
            var client = _storageAccount.CreateCloudQueueClient();
            var azqueue = client.GetQueueReference(QueueName);
            azqueue.CreateIfNotExists();
            return azqueue;
        }

        public static CloudBlobContainer GetBlobContainer()
        {
            var client = _storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = client.GetContainerReference(AttachmentContainer);
            container.CreateIfNotExists();
            //container.SetPermissions(new BlobContainerPermissions
            //{
            //    PublicAccess = BlobContainerPublicAccessType.Blob
            //});

            return container;
        }
    }
}
