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

namespace MSGorilla.Library.Azure
{
    public static class AzureFactory
    {
        public enum TweetTable{
            HomelineTweet,
            UserlineTweet,
            Reply,
            ReplyNotification
        }

        public const string QueueName = "messagequeue";

        private static CloudStorageAccount _storageAccount;
        private static Dictionary<TweetTable, string> _dict;
        static AzureFactory()
        {
            string connectionString = CloudConfigurationManager.GetSetting("StorageConnectionString");
            _storageAccount = CloudStorageAccount.Parse(connectionString);
            _dict = new Dictionary<TweetTable, string>();

            _dict.Add(TweetTable.HomelineTweet, "HomelineTweet");
            _dict.Add(TweetTable.UserlineTweet, "UserlineTweet");
            _dict.Add(TweetTable.Reply, "Reply");
            _dict.Add(TweetTable.ReplyNotification, "ReplyNotification");
        }

        public static CloudTable GetTable(TweetTable table)
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
    }
}
