using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;

using MSGorilla.Library.Azure;
using MSGorilla.Library.Models;
using MSGorilla.Library.Models.SqlModels;
using MSGorilla.Library.Models.AzureModels;
using MSGorilla.Library.Exceptions;
using MSGorilla.Library.Models.AzureModels.Entity;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace MSGorilla.Library
{
    public class MessageManager
    {
        private const int DefaultTimelineQueryDayRange = 3;
        private CloudTable _homeline;
        private CloudTable _userline;
        private CloudTable _eventline;
        private CloudTable _publicSquareLine;
        private CloudTable _reply;

        private CloudQueue _queue;

        private AccountManager _accManager;
        private SchemaManager _schemaManager;

        public MessageManager()
        {
            _homeline = AzureFactory.GetTable(AzureFactory.MSGorillaTable.Homeline);
            _userline = AzureFactory.GetTable(AzureFactory.MSGorillaTable.Userline);
            _eventline = AzureFactory.GetTable(AzureFactory.MSGorillaTable.EventLine);
            _publicSquareLine = AzureFactory.GetTable(AzureFactory.MSGorillaTable.PublicSquareLine);
            _reply = AzureFactory.GetTable(AzureFactory.MSGorillaTable.Reply);

            _queue = AzureFactory.GetQueue();

            _accManager = new AccountManager();
            _schemaManager = new SchemaManager();
        }

        static string GenerateTimestampConditionQuery(string userid, DateTime start, DateTime end)
        {
            if (end == null)
            {
                end = DateTime.UtcNow;
            }

            if (start == null)
            {
                start = end.AddDays(0 - DefaultTimelineQueryDayRange);
            }

            string query = TableQuery.GenerateFilterCondition(
                "PartitionKey", 
                QueryComparisons.LessThan, 
                string.Format("{0}_{1}", userid, Utils.NextKeyString(Utils.ToAzureStorageDayBasedString(end))));

            query = TableQuery.CombineFilters(
                query,
                TableOperators.And,
                TableQuery.GenerateFilterCondition(
                    "PartitionKey",
                    QueryComparisons.GreaterThanOrEqual,
                    string.Format("{0}_{1}", userid, Utils.ToAzureStorageDayBasedString(start)))
            );

            query = TableQuery.CombineFilters(
                query,
                TableOperators.And,
                TableQuery.GenerateFilterCondition(
                    "RowKey",
                    QueryComparisons.LessThan,
                    Utils.NextKeyString(Utils.ToAzureStorageSecondBasedString(end)))
            );

            query = TableQuery.CombineFilters(
                query,
                TableOperators.And,
                TableQuery.GenerateFilterCondition(
                    "RowKey",
                    QueryComparisons.GreaterThan,
                    Utils.ToAzureStorageSecondBasedString(start))
            );

            return query;
        }

        public List<Message> UserLine(string userid, DateTime start, DateTime end)
        {
            TableQuery<UserLineEntity> rangeQuery =
                new TableQuery<UserLineEntity>().Where(
                    GenerateTimestampConditionQuery(userid, start, end)
                );

            List<Message> msgs = new List<Message>();
            foreach (UserLineEntity entity in _userline.ExecuteQuery(rangeQuery))
            {
                msgs.Add(JsonConvert.DeserializeObject<Message>(entity.Content));
            }
            msgs.Reverse();
            return msgs;
        }

        //public List<Message> UserLine(string userid)
        //{
        //    return UserLine(userid, DateTime.UtcNow, DateTime.UtcNow.AddDays(0 - DefaultTimelineQueryDayRange));
        //}

        public List<Message> HomeLine(string userid, DateTime start, DateTime end)
        {
            TableQuery<HomeLineEntity> rangeQuery =
                new TableQuery<HomeLineEntity>().Where(
                    GenerateTimestampConditionQuery(userid, start, end)
                );

            List<Message> msgs = new List<Message>();
            foreach (HomeLineEntity entity in _homeline.ExecuteQuery(rangeQuery))
            {
                msgs.Add(JsonConvert.DeserializeObject<Message>(entity.Content));
            }
            msgs.Reverse();
            return msgs;
        }

        //public List<Message> HomeLine(string userid)
        //{
        //    return HomeLine(userid, DateTime.UtcNow, DateTime.UtcNow.AddDays(0 - DefaultTimelineQueryDayRange));
        //}

        public List<Message> EventLine(string eventID)
        {
            string query = TableQuery.GenerateFilterCondition(
                "PartitionKey",
                QueryComparisons.LessThan,
                Utils.NextKeyString(eventID));

            query = TableQuery.CombineFilters(
                query,
                TableOperators.And,
                TableQuery.GenerateFilterCondition(
                    "PartitionKey",
                    QueryComparisons.GreaterThanOrEqual,
                    eventID
                )
            );

            TableQuery<EventLineEntity> rangeQuery = new TableQuery<EventLineEntity>().Where(query);

            List<Message> msgs = new List<Message>();
            foreach (EventLineEntity entity in _eventline.ExecuteQuery(rangeQuery))
            {
                msgs.Add(JsonConvert.DeserializeObject<Message>(entity.Content));
            }
            msgs.Reverse();
            return msgs;
        }

        public List<Message> PublicSquareLine(DateTime start, DateTime end)
        {
            if (end == null)
            {
                end = DateTime.UtcNow;
            }
            if (start == null)
            {
                start = end.AddDays(0 - 1);
            }

            string query = TableQuery.GenerateFilterCondition(
                "PartitionKey",
                QueryComparisons.LessThan,
                Utils.NextKeyString(Utils.ToAzureStorageDayBasedString(end)));

            query = TableQuery.CombineFilters(
                query,
                TableOperators.And,
                TableQuery.GenerateFilterCondition(
                    "PartitionKey",
                    QueryComparisons.GreaterThanOrEqual,
                    Utils.ToAzureStorageDayBasedString(start))
            );

            query = TableQuery.CombineFilters(
                query,
                TableOperators.And,
                TableQuery.GenerateFilterCondition(
                    "RowKey",
                    QueryComparisons.LessThan,
                    Utils.NextKeyString(Utils.ToAzureStorageSecondBasedString(end)))
            );

            query = TableQuery.CombineFilters(
                query,
                TableOperators.And,
                TableQuery.GenerateFilterCondition(
                    "RowKey",
                    QueryComparisons.GreaterThanOrEqual,
                    Utils.ToAzureStorageSecondBasedString(start))
            );

            TableQuery<PublicSquareLineEntity> rangeQuery = new TableQuery<PublicSquareLineEntity>().Where(query);

            List<Message> msgs = new List<Message>();
            foreach (PublicSquareLineEntity entity in  _publicSquareLine.ExecuteQuery(rangeQuery))
            {
                msgs.Add(JsonConvert.DeserializeObject<Message>(entity.Content));
            }
            msgs.Reverse();
            return msgs;
        }


        public MessageDetail GetMessageDetail(string userid, string messageID)
        {
            string pk = Message.ToMessagePK(userid, messageID);
            TableOperation retrieveOperation = TableOperation.Retrieve<UserLineEntity>(pk, messageID);

            MessageDetail msg = null;
            TableResult retrievedResult = _userline.Execute(retrieveOperation);
            if (retrievedResult.Result != null)
            {
                UserLineEntity entity = (UserLineEntity)retrievedResult.Result;
                msg = new MessageDetail(JsonConvert.DeserializeObject<Message>(entity.Content));
                msg.ReplyCount = entity.ReplyCount;
                //tweet.RetweetCount = entity.RetweetCount;
                msg.Replies = GetAllReplies(entity.RowKey);
            }
            return msg;
        }

        public List<Reply> GetAllReplies(string msgID)
        {
            TableQuery<ReplyEntity> query = new TableQuery<ReplyEntity>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, msgID));

            List<Reply> replies = new List<Reply>();
            // Print the fields for each customer.
            foreach (ReplyEntity entity in _reply.ExecuteQuery(query))
            {
                replies.Add(JsonConvert.DeserializeObject<Reply>(entity.Content));
            }
            return replies;
        }


        public void PostMessage(string userid, string eventID, string schemaID, string message, DateTime timestamp)
        {
            if (message.Length > 2048)
            {
                throw new MessageTooLongException();
            }
            if (_accManager.FindUser(userid) == null)
            {
                throw new UserNotFoundException(userid);
            }

            if (!_schemaManager.Contain(schemaID))
            {
                throw new SchemaNotFoundException();
            }

            if (!Utils.IsValidID(eventID))
            {
                throw new InvalidIDException("Event");
            }

            Message msg = new Message(userid, message, timestamp, eventID, schemaID);
            //insert into Userline
            TableOperation insertOperation = TableOperation.Insert(new UserLineEntity(msg));
            _userline.Execute(insertOperation);

            //insert into owner's homeline
            insertOperation = TableOperation.Insert(new HomeLineEntity(msg.User, msg));
            _homeline.Execute(insertOperation);

            //insert into Eventline
            insertOperation = TableOperation.Insert(new EventLineEntity(msg));
            _eventline.Execute(insertOperation);

            //insert into PublicSquareLine
            insertOperation = TableOperation.Insert(new PublicSquareLineEntity(msg));
            _publicSquareLine.Execute(insertOperation);

            //insert into QueueMessage
            //QueueMessage queueMessage = new QueueMessage(QueueMessage.TypeMessage, msg.ToJsonString());
            _queue.AddMessage(msg.toAzureCloudQueueMessage());
        }

        public void SpreadMessage(Message message)
        {
            List<UserProfile> followers = _accManager.Followers(message.User);
            //followers.Add(_accManager.FindUser(message.User));
            //speed tweet to followers

            //todo: BatchInsert
            foreach (UserProfile user in followers)
            {
                HomeLineEntity entity = new HomeLineEntity(user.Userid, message);
                TableOperation insertOperation = TableOperation.Insert(entity);
                _homeline.Execute(insertOperation);
            }
        }
    }
}
