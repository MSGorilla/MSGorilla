using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using MSGorilla.Library.Azure;
using MSGorilla.Library.Models;
using MSGorilla.Library.Models.SqlModels;
using MSGorilla.Library.Models.AzureModels;
using MSGorilla.Library.Exceptions;
using MSGorilla.Library.Models.AzureModels.Entity;

namespace MSGorilla.Library
{
    public class NotificationManager
    {
        private CloudTable _replyNotification;

        public NotificationManager()
        {
            _replyNotification = AzureFactory.GetTable(AzureFactory.MSGorillaTable.ReplyNotification);
        }

        public List<Reply> GetNewReplies(string userid)
        {
            TableQuery<ReplyEntity> query = new TableQuery<ReplyEntity>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, userid));
            
            List<Reply> replies = new List<Reply>();
            TableBatchOperation batchOperation = new TableBatchOperation();
            int count = 0;

            // Print the fields for each customer.
            foreach (ReplyEntity entity in _replyNotification.ExecuteQuery(query))
            {
                replies.Add(JsonConvert.DeserializeObject<Reply>(entity.Content));

                batchOperation.Add(TableOperation.Delete(entity));
                count++;
                
                if((count % 100) == 0){
                    _replyNotification.ExecuteBatch(batchOperation);
                    batchOperation = new TableBatchOperation();
                    count = 0;
                }
            }

            if (count > 0)
            {
                _replyNotification.ExecuteBatch(batchOperation);
            }

            return replies;
        }
    }
}
