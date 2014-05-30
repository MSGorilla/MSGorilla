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
    public class MessagePagination
    {
        public List<Message> message {get; set;}
        public string continuationToken { get; set; }
    }

    public class MessageManager
    {
        private const int DefaultTimelineQueryDayRange = 3;
        private CloudTable _homeline;
        private CloudTable _userline;
        private CloudTable _eventline;
        private CloudTable _ownerline;
        private CloudTable _atline;
        private CloudTable _publicSquareLine;
        private CloudTable _topicline;
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
            _topicline = AzureFactory.GetTable(AzureFactory.MSGorillaTable.TopicLine);
            _ownerline = AzureFactory.GetTable(AzureFactory.MSGorillaTable.OwnerLine);
            _atline = AzureFactory.GetTable(Azure.AzureFactory.MSGorillaTable.AtLine);
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
                string.Format("{0}_{1}", userid, Utils.NextKeyString(Utils.ToAzureStorageDayBasedString(start))));

            query = TableQuery.CombineFilters(
                query,
                TableOperators.And,
                TableQuery.GenerateFilterCondition(
                    "PartitionKey",
                    QueryComparisons.GreaterThanOrEqual,
                    string.Format("{0}_{1}", userid, Utils.ToAzureStorageDayBasedString(end)))
            );

            query = TableQuery.CombineFilters(
                query,
                TableOperators.And,
                TableQuery.GenerateFilterCondition(
                    "RowKey",
                    QueryComparisons.LessThan,
                    Utils.NextKeyString(Utils.ToAzureStorageSecondBasedString(start)))
            );

            query = TableQuery.CombineFilters(
                query,
                TableOperators.And,
                TableQuery.GenerateFilterCondition(
                    "RowKey",
                    QueryComparisons.GreaterThan,
                    Utils.ToAzureStorageSecondBasedString(end))
            );

            return query;
        }

        static string GeneratePKStartWithConditionQuery(string startWith)
        {
            if (Utils.IsValidID(startWith))
            {
                throw new InvalidIDException();
            }

            string query = TableQuery.GenerateFilterCondition(
                "PartitionKey",
                QueryComparisons.LessThan,
                Utils.NextKeyString(startWith));

            query = TableQuery.CombineFilters(
                query,
                TableOperators.And,
                TableQuery.GenerateFilterCondition(
                    "PartitionKey",
                    QueryComparisons.GreaterThanOrEqual,
                    startWith
                )
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
            return msgs;
        }

        public MessagePagination UserLine(string userid, int count = 25, TableContinuationToken continuationToken = null)
        {
            string query = GeneratePKStartWithConditionQuery(userid);

            TableQuery<UserLineEntity> tableQuery = new TableQuery<UserLineEntity>().Where(query).Take(count);
            TableQuerySegment<UserLineEntity> queryResult = _userline.ExecuteQuerySegmented(tableQuery, continuationToken);

            MessagePagination ret = new MessagePagination();
            ret.continuationToken = Utils.Token2String(queryResult.ContinuationToken);
            ret.message = new List<Message>();
            foreach (UserLineEntity entity in queryResult)
            {
                ret.message.Add(JsonConvert.DeserializeObject<Message>(entity.Content));
            }
            return ret;
        }

        public List<Message> OwnerLine(string userid, DateTime start, DateTime end)
        {
            TableQuery<UserLineEntity> rangeQuery =
                new TableQuery<UserLineEntity>().Where(
                    GenerateTimestampConditionQuery(userid, start, end)
                );

            List<Message> msgs = new List<Message>();
            foreach (UserLineEntity entity in _ownerline.ExecuteQuery(rangeQuery))
            {
                msgs.Add(JsonConvert.DeserializeObject<Message>(entity.Content));
            }
            return msgs;
        }

        public MessagePagination OwnerLine(string userid, int count = 25, TableContinuationToken continuationToken = null)
        {
            string query = GeneratePKStartWithConditionQuery(userid);
            TableQuery<OwnerLineEntity> tableQuery = new TableQuery<OwnerLineEntity>().Where(query).Take(count);
            TableQuerySegment<OwnerLineEntity> queryResult = _ownerline.ExecuteQuerySegmented(tableQuery, continuationToken);

            MessagePagination ret = new MessagePagination();
            ret.continuationToken = Utils.Token2String(queryResult.ContinuationToken);
            ret.message = new List<Message>();
            foreach (OwnerLineEntity entity in queryResult)
            {
                ret.message.Add(JsonConvert.DeserializeObject<Message>(entity.Content));
            }
            return ret;
        }

        public MessagePagination AtLine(string userid, int count = 25, TableContinuationToken continuationToken = null)
        {
            string query = GeneratePKStartWithConditionQuery(userid);
            TableQuery<AtLineEntity> tableQuery = new TableQuery<AtLineEntity>().Where(query).Take(count);
            TableQuerySegment<AtLineEntity> queryResult = _atline.ExecuteQuerySegmented(tableQuery, continuationToken);

            MessagePagination ret = new MessagePagination();
            ret.continuationToken = Utils.Token2String(queryResult.ContinuationToken);
            ret.message = new List<Message>();
            foreach (AtLineEntity entity in queryResult)
            {
                ret.message.Add(JsonConvert.DeserializeObject<Message>(entity.Content));
            }

            return ret;
        }

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
            return msgs;
        }

        public MessagePagination HomeLine(string userid, int count = 25, TableContinuationToken continuationToken = null)
        {
            string query = GeneratePKStartWithConditionQuery(userid);

            TableQuery<HomeLineEntity> tableQuery = new TableQuery<HomeLineEntity>().Where(query).Take(count);
            TableQuerySegment<HomeLineEntity> queryResult = _homeline.ExecuteQuerySegmented(tableQuery, continuationToken);

            MessagePagination ret = new MessagePagination();
            ret.continuationToken = Utils.Token2String(queryResult.ContinuationToken);
            ret.message = new List<Message>();
            foreach (HomeLineEntity entity in queryResult)
            {
                ret.message.Add(JsonConvert.DeserializeObject<Message>(entity.Content));
            }
            return ret;
        }

        //public List<Message> HomeLine(string userid)
        //{
        //    return HomeLine(userid, DateTime.UtcNow, DateTime.UtcNow.AddDays(0 - DefaultTimelineQueryDayRange));
        //}

        public MessagePagination TopicLine(string topicID, int count = 25, TableContinuationToken continuationToken = null)
        {
            string query = GeneratePKStartWithConditionQuery(topicID);

            TableQuery<TopicLine> tableQuery = new TableQuery<TopicLine>().Where(query).Take(count);
            TableQuerySegment<TopicLine> queryResult = _homeline.ExecuteQuerySegmented(tableQuery, continuationToken);

            MessagePagination ret = new MessagePagination();
            ret.continuationToken = Utils.Token2String(queryResult.ContinuationToken);
            ret.message = new List<Message>();
            foreach (TopicLine entity in queryResult)
            {
                ret.message.Add(JsonConvert.DeserializeObject<Message>(entity.Content));
            }
            return ret;
        }

        public List<Message> EventLine(string eventID)
        {
            string query = GeneratePKStartWithConditionQuery(eventID);

            TableQuery<EventLineEntity> rangeQuery = new TableQuery<EventLineEntity>().Where(query);

            List<Message> msgs = new List<Message>();
            foreach (EventLineEntity entity in _eventline.ExecuteQuery(rangeQuery))
            {
                msgs.Add(JsonConvert.DeserializeObject<Message>(entity.Content));
            }
            return msgs;
        }

        public MessagePagination PublicSquareLine(int count = 25, TableContinuationToken continuationToken = null)
        {
            TableQuery<PublicSquareLineEntity> tableQuery =
                new TableQuery<PublicSquareLineEntity>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.GreaterThan, "")).Take(count);

            TableQuerySegment<PublicSquareLineEntity> queryResult = _publicSquareLine.ExecuteQuerySegmented(tableQuery, continuationToken);

            MessagePagination ret = new MessagePagination();
            ret.continuationToken = Utils.Token2String(queryResult.ContinuationToken);
            ret.message = new List<Message>();
            foreach (PublicSquareLineEntity entity in queryResult)
            {
                ret.message.Add(JsonConvert.DeserializeObject<Message>(entity.Content));
            }
            return ret;
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
                Utils.NextKeyString(Utils.ToAzureStorageDayBasedString(start)));

            query = TableQuery.CombineFilters(
                query,
                TableOperators.And,
                TableQuery.GenerateFilterCondition(
                    "PartitionKey",
                    QueryComparisons.GreaterThanOrEqual,
                    Utils.ToAzureStorageDayBasedString(end))
            );

            query = TableQuery.CombineFilters(
                query,
                TableOperators.And,
                TableQuery.GenerateFilterCondition(
                    "RowKey",
                    QueryComparisons.LessThan,
                    Utils.NextKeyString(Utils.ToAzureStorageSecondBasedString(start)))
            );

            query = TableQuery.CombineFilters(
                query,
                TableOperators.And,
                TableQuery.GenerateFilterCondition(
                    "RowKey",
                    QueryComparisons.GreaterThanOrEqual,
                    Utils.ToAzureStorageSecondBasedString(end))
            );

            TableQuery<PublicSquareLineEntity> rangeQuery = new TableQuery<PublicSquareLineEntity>().Where(query);

            List<Message> msgs = new List<Message>();
            foreach (PublicSquareLineEntity entity in  _publicSquareLine.ExecuteQuery(rangeQuery))
            {
                msgs.Add(JsonConvert.DeserializeObject<Message>(entity.Content));
            }
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

        public Message PostMessage(string userid, 
                                    string eventID, 
                                    string schemaID, 
                                    string topicID,
                                    string[] owner, 
                                    string[] atUser, 
                                    string message, 
                                    DateTime timestamp)
        {
            if (message.Length > 2048)
            {
                throw new MessageTooLongException();
            }
            UserProfile user = _accManager.FindUser(userid);
            if (user == null)
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

            Message msg = new Message(userid, message, timestamp, eventID, schemaID, topicID, owner, atUser);
            //insert into Userline
            TableOperation insertOperation = TableOperation.InsertOrReplace(new UserLineEntity(msg));
            _userline.Execute(insertOperation);

            //insert into poster's homeline
            insertOperation = TableOperation.InsertOrReplace(new HomeLineEntity(msg.User, msg));
            _homeline.Execute(insertOperation);

            //insert into QueueMessage
            //QueueMessage queueMessage = new QueueMessage(QueueMessage.TypeMessage, msg.ToJsonString());
            _queue.AddMessage(msg.toAzureCloudQueueMessage());

            user.MessageCount++;
            _accManager.UpdateUser(user);

            return msg;
        }

        public void SpreadMessage(Message message)
        {
            //insert into Eventline
            TableOperation insertOperation = TableOperation.InsertOrReplace(new EventLineEntity(message));
            _eventline.Execute(insertOperation);

            //insert into PublicSquareLine
            insertOperation = TableOperation.InsertOrReplace(new PublicSquareLineEntity(message));
            _publicSquareLine.Execute(insertOperation);

            if (Utils.IsValidID(message.TopicID))
            {
                //insert into TopicLine
                insertOperation = TableOperation.InsertOrReplace(new TopicLine(message));
                _topicline.Execute(insertOperation);
            }
            

            //todo:
            //The Userid of owner and AtUser may be ms alais or displayname instead of the
            //real id in MSGorilla System.
            //Should have a way to convert these names to Userid in our system
            if (message.Owner != null)
            {
                foreach (string ownerid in message.Owner)
                {
                    if (Utils.IsValidID(ownerid))
                    {
                        insertOperation = TableOperation.InsertOrReplace(new OwnerLineEntity(ownerid, message));
                        _ownerline.Execute(insertOperation);
                    }
                }
            }

            if (message.AtUser != null)
            {
                foreach (string atUserid in message.AtUser)
                {
                    if (Utils.IsValidID(atUserid))
                    {
                        insertOperation = TableOperation.InsertOrReplace(new AtLineEntity(atUserid, message));
                        _atline.Execute(insertOperation);
                    }
                }
            }

            List<UserProfile> followers = _accManager.Followers(message.User);
            //followers.Add(_accManager.FindUser(message.User));
            //speed tweet to followers
            foreach (UserProfile user in followers)
            {
                HomeLineEntity entity = new HomeLineEntity(user.Userid, message);
                insertOperation = TableOperation.InsertOrReplace(entity);
                _homeline.Execute(insertOperation);
            }
        }
    }
}
