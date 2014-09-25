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
            Attachment,
            RichMessage,
            MetricDataSet,
            CategoryMessage,
            Statistics,
            WordsIndex,
            SearchResults,
            SearchHistory
        }

        public enum MSGorillaQueue
        {
            Dispatcher,
            SearchEngineSpider,
            MailMessage
        }

        public enum MSGorillaBlobContainer
        {
            Attachment
        }

        private static CloudStorageAccount _storageAccount;
        private static Dictionary<MSGorillaTable, string> _tableDict;
        private static Dictionary<MSGorillaQueue, string> _queueDict;
        private static Dictionary<MSGorillaBlobContainer, string> _containerDict;

        static AzureFactory()
        {
            // init storage account
            string connectionString = CloudConfigurationManager.GetSetting("StorageConnectionString");
            if (string.IsNullOrEmpty(connectionString))
            {
                connectionString = ConfigurationManager.ConnectionStrings["StorageConnectionString"].ConnectionString;
            }
            _storageAccount = CloudStorageAccount.Parse(connectionString);

            // init table dict
            _tableDict = new Dictionary<MSGorillaTable, string>();
            _tableDict.Add(MSGorillaTable.Homeline, "Homeline");
            _tableDict.Add(MSGorillaTable.Userline, "Userline");
            _tableDict.Add(MSGorillaTable.EventLine, "EventlineTweet");
            _tableDict.Add(MSGorillaTable.PublicSquareLine, "PublicSquareline");
            _tableDict.Add(MSGorillaTable.TopicLine, "Topicline");
            _tableDict.Add(MSGorillaTable.OwnerLine, "Ownerline");
            _tableDict.Add(MSGorillaTable.AtLine, "Atline");
            _tableDict.Add(MSGorillaTable.Reply, "Reply");
            _tableDict.Add(MSGorillaTable.ReplyNotification, "ReplyNotification");
            _tableDict.Add(MSGorillaTable.ReplyArchive, "ReplyArchive");
            _tableDict.Add(MSGorillaTable.Attachment, "Attachment");
            _tableDict.Add(MSGorillaTable.RichMessage, "RichMessage");
            _tableDict.Add(MSGorillaTable.MetricDataSet, "MetricDataSet");
            _tableDict.Add(MSGorillaTable.CategoryMessage, "CategoryMessage");
            _tableDict.Add(MSGorillaTable.Statistics, "Statistics");
            _tableDict.Add(MSGorillaTable.WordsIndex, "WordsIndex");
            _tableDict.Add(MSGorillaTable.SearchResults, "SearchResults");
            _tableDict.Add(MSGorillaTable.SearchHistory, "SearchHistory");

            // init queue dict
            _queueDict = new Dictionary<MSGorillaQueue, string>();
            _queueDict.Add(MSGorillaQueue.Dispatcher, "messagequeue");
            _queueDict.Add(MSGorillaQueue.SearchEngineSpider, "spider");
            _queueDict.Add(MSGorillaQueue.MailMessage, "mailmessage");

            // init blob container dict
            _containerDict = new Dictionary<MSGorillaBlobContainer, string>();
            _containerDict.Add(MSGorillaBlobContainer.Attachment, "attachment");
        }

        public static CloudTable GetTable(MSGorillaTable table)
        {
            var client = _storageAccount.CreateCloudTableClient();
            var aztable = client.GetTableReference(_tableDict[table]);
            aztable.CreateIfNotExists();
            return aztable;
        }

        public static CloudQueue GetQueue(MSGorillaQueue queue)
        {
            var client = _storageAccount.CreateCloudQueueClient();
            var azqueue = client.GetQueueReference(_queueDict[queue]);
            azqueue.CreateIfNotExists();
            return azqueue;
        }

        public static CloudBlobContainer GetBlobContainer(MSGorillaBlobContainer container)
        {
            var client = _storageAccount.CreateCloudBlobClient();
            var azcontainer = client.GetContainerReference(_containerDict[container]);
            azcontainer.CreateIfNotExists();
            return azcontainer;
        }
    }
}
