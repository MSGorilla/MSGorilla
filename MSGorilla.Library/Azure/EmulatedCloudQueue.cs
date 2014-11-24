using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace MSGorilla.Library.Azure
{
    public class EmulatedCloudQueue
    {
        const string queueTableName = "queue";
        private static CloudTable queueTable;

        public string QueueName;

        static EmulatedCloudQueue()
        {
            var client = AzureFactory.AzureStorageAccount.CreateCloudTableClient();
            queueTable = client.GetTableReference(queueTableName);

            try
            {
                queueTable.Create();
            }
            catch (Exception e)
            {
                if (!(e is StorageException && e.InnerException != null && e.InnerException.Message.Equals("The remote server returned an error: (409) Conflict.")))
                {
                    throw e;
                }
            }
        }


        public EmulatedCloudQueue(string queueName)
        {
            QueueName = queueName;
        }

        //Just for single thread
        public string GetMessage()
        {
            TableQuery query = new TableQuery().Where(
                TableQuery.GenerateFilterCondition(
                    "PartitionKey",
                    QueryComparisons.Equal,
                    QueueName)
                ).Take(1);

            foreach (var entity in queueTable.ExecuteQuery(query))
            {
                try
                {
                    queueTable.Execute(TableOperation.Delete(entity));
                    return entity.Properties["Content"].StringValue;
                }
                catch { }
            }

            return null;
        }

        public void AddMessage(string message)
        {
            DynamicTableEntity entity = new DynamicTableEntity(
                QueueName, DateTime.Now.ToString("yyyyMMdd-HHmmssffffff")
                );
            entity.Properties["Content"] = new EntityProperty(message);
            queueTable.Execute(TableOperation.Insert(entity));
        }
    }
}
