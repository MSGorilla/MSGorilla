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
using MSGorilla.Library.Models.ViewModels;
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
        private CloudTable _ownerline;
        private CloudTable _atline;
        private CloudTable _publicSquareLine;
        private CloudTable _topicline;
        private CloudTable _reply;

        private CloudQueue _queue;

        private AccountManager _accManager;
        private AttachmentManager _attManager;
        private SchemaManager _schemaManager;
        private NotifManager _notifManager;
        private TopicManager _topicManager;
        private RichMsgManager _richMsgManager;

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
            _attManager = new AttachmentManager();
            _schemaManager = new SchemaManager();
            _notifManager = new NotifManager();
            _topicManager = new TopicManager();
            _richMsgManager = new RichMsgManager();
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
            if (!Utils.IsValidID(startWith))
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

        public MessagePagination UserLine(string userid, DateTime start, DateTime end, int count = 25, TableContinuationToken continuationToken = null)
        {
            TableQuery<UserLineEntity> rangeQuery =
                new TableQuery<UserLineEntity>().Where(
                    GenerateTimestampConditionQuery(userid, start, end)
                ).Take(count);
            TableQuerySegment<UserLineEntity> queryResult = _userline.ExecuteQuerySegmented(rangeQuery, continuationToken);

            MessagePagination ret = new MessagePagination();
            ret.continuationToken = Utils.Token2String(queryResult.ContinuationToken);
            ret.message = new List<Message>();
            foreach (UserLineEntity entity in queryResult)
            {
                //var msg = JsonConvert.DeserializeObject<Message>(entity.Content);
                ret.message.Add(entity.toMessage());
            }
            return ret;
        }

        public MessagePagination UserLine(string userid, int count = 25, TableContinuationToken continuationToken = null)
        {
            string query = GeneratePKStartWithConditionQuery(userid + "_");

            TableQuery<UserLineEntity> tableQuery = new TableQuery<UserLineEntity>().Where(query).Take(count);
            TableQuerySegment<UserLineEntity> queryResult = _userline.ExecuteQuerySegmented(tableQuery, continuationToken);

            MessagePagination ret = new MessagePagination();
            ret.continuationToken = Utils.Token2String(queryResult.ContinuationToken);
            ret.message = new List<Message>();
            foreach (UserLineEntity entity in queryResult)
            {
                //var msg = JsonConvert.DeserializeObject<Message>(entity.Content);
                ret.message.Add(entity.toMessage());
            }
            return ret;
        }

        public MessagePagination OwnerLine(string userid, DateTime start, DateTime end, int count = 25, TableContinuationToken continuationToken = null)
        {
            TableQuery<OwnerLineEntity> rangeQuery =
                new TableQuery<OwnerLineEntity>().Where(
                    GenerateTimestampConditionQuery(userid, start, end)
                ).Take(count);
            TableQuerySegment<OwnerLineEntity> queryResult = _ownerline.ExecuteQuerySegmented(rangeQuery, continuationToken);

            MessagePagination ret = new MessagePagination();
            ret.continuationToken = Utils.Token2String(queryResult.ContinuationToken);
            ret.message = new List<Message>();
            foreach (OwnerLineEntity entity in queryResult)
            {
                //var msg = JsonConvert.DeserializeObject<Message>(entity.Content);
                ret.message.Add(entity.toMessage());
            }
            return ret;
        }

        public MessagePagination OwnerLine(string userid, int count = 25, TableContinuationToken continuationToken = null)
        {
            string query = GeneratePKStartWithConditionQuery(userid + "_");
            TableQuery<OwnerLineEntity> tableQuery = new TableQuery<OwnerLineEntity>().Where(query).Take(count);
            TableQuerySegment<OwnerLineEntity> queryResult = _ownerline.ExecuteQuerySegmented(tableQuery, continuationToken);

            MessagePagination ret = new MessagePagination();
            ret.continuationToken = Utils.Token2String(queryResult.ContinuationToken);
            ret.message = new List<Message>();
            foreach (OwnerLineEntity entity in queryResult)
            {
                //var msg = JsonConvert.DeserializeObject<Message>(entity.Content);
                ret.message.Add(entity.toMessage());
            }
            return ret;
        }

        public MessagePagination AtLine(string userid, DateTime start, DateTime end, int count = 25, TableContinuationToken continuationToken = null)
        {
            TableQuery<AtLineEntity> rangeQuery =
                new TableQuery<AtLineEntity>().Where(
                    GenerateTimestampConditionQuery(userid, start, end)
                ).Take(count);
            TableQuerySegment<AtLineEntity> queryResult = _atline.ExecuteQuerySegmented(rangeQuery, continuationToken);

            MessagePagination ret = new MessagePagination();
            ret.continuationToken = Utils.Token2String(queryResult.ContinuationToken);
            ret.message = new List<Message>();
            foreach (AtLineEntity entity in queryResult)
            {
                //var msg = JsonConvert.DeserializeObject<Message>(entity.Content);
                ret.message.Add(entity.toMessage());
            }
            return ret;
        }

        public MessagePagination AtLine(string userid, int count = 25, TableContinuationToken continuationToken = null)
        {
            string query = GeneratePKStartWithConditionQuery(userid + "_");
            TableQuery<AtLineEntity> tableQuery = new TableQuery<AtLineEntity>().Where(query).Take(count);
            TableQuerySegment<AtLineEntity> queryResult = _atline.ExecuteQuerySegmented(tableQuery, continuationToken);

            MessagePagination ret = new MessagePagination();
            ret.continuationToken = Utils.Token2String(queryResult.ContinuationToken);
            ret.message = new List<Message>();
            foreach (AtLineEntity entity in queryResult)
            {
                //var msg = JsonConvert.DeserializeObject<Message>(entity.Content);
                ret.message.Add(entity.toMessage());
            }
            return ret;
        }

        public MessagePagination HomeLine(string userid, DateTime start, DateTime end, int count = 25, TableContinuationToken continuationToken = null)
        {
            TableQuery<HomeLineEntity> rangeQuery =
                new TableQuery<HomeLineEntity>().Where(
                    GenerateTimestampConditionQuery(userid, start, end)
                ).Take(count); ;
            TableQuerySegment<HomeLineEntity> queryResult = _homeline.ExecuteQuerySegmented(rangeQuery, continuationToken);

            MessagePagination ret = new MessagePagination();
            ret.continuationToken = Utils.Token2String(queryResult.ContinuationToken);
            ret.message = new List<Message>();
            foreach (HomeLineEntity entity in queryResult)
            {
                //var msg = JsonConvert.DeserializeObject<Message>(entity.Content);
                ret.message.Add(entity.toMessage());
            }
            return ret;
        }

        public MessagePagination HomeLine(string userid, int count = 25, TableContinuationToken continuationToken = null)
        {
            string query = GeneratePKStartWithConditionQuery(userid + "_");

            TableQuery<HomeLineEntity> tableQuery = new TableQuery<HomeLineEntity>().Where(query).Take(count);
            TableQuerySegment<HomeLineEntity> queryResult = _homeline.ExecuteQuerySegmented(tableQuery, continuationToken);

            MessagePagination ret = new MessagePagination();
            ret.continuationToken = Utils.Token2String(queryResult.ContinuationToken);
            ret.message = new List<Message>();
            foreach (HomeLineEntity entity in queryResult)
            {
                //var msg = JsonConvert.DeserializeObject<Message>(entity.Content);
                ret.message.Add(entity.toMessage());
            }
            return ret;
        }

        public MessagePagination TopicLine(string topicID, DateTime start, DateTime end, int count = 25, TableContinuationToken continuationToken = null)
        {
            TableQuery<TopicLine> rangeQuery =
                new TableQuery<TopicLine>().Where(
                    GenerateTimestampConditionQuery(topicID, start, end)
                ).Take(count); ;
            TableQuerySegment<TopicLine> queryResult = _topicline.ExecuteQuerySegmented(rangeQuery, continuationToken);

            MessagePagination ret = new MessagePagination();
            ret.continuationToken = Utils.Token2String(queryResult.ContinuationToken);
            ret.message = new List<Message>();
            foreach (TopicLine entity in queryResult)
            {
                //var msg = JsonConvert.DeserializeObject<Message>(entity.Content);
                ret.message.Add(entity.toMessage());
            }
            return ret;
        }

        public MessagePagination TopicLine(string topicID, int count = 25, TableContinuationToken continuationToken = null)
        {
            string query = GeneratePKStartWithConditionQuery(topicID + "_");

            TableQuery<TopicLine> tableQuery = new TableQuery<TopicLine>().Where(query).Take(count);
            TableQuerySegment<TopicLine> queryResult = _topicline.ExecuteQuerySegmented(tableQuery, continuationToken);

            MessagePagination ret = new MessagePagination();
            ret.continuationToken = Utils.Token2String(queryResult.ContinuationToken);
            ret.message = new List<Message>();
            foreach (TopicLine entity in queryResult)
            {
                //var msg = JsonConvert.DeserializeObject<Message>(entity.Content);
                ret.message.Add(entity.toMessage());
            }
            return ret;
        }

        public List<Message> EventLine(string eventID)
        {
            string query = GeneratePKStartWithConditionQuery(eventID + "_");

            TableQuery<EventLineEntity> rangeQuery = new TableQuery<EventLineEntity>().Where(query);

            List<Message> msgs = new List<Message>();
            foreach (EventLineEntity entity in _eventline.ExecuteQuery(rangeQuery))
            {
                //var msg = JsonConvert.DeserializeObject<Message>(entity.Content);
                msgs.Add(entity.toMessage());
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
                //var msg = JsonConvert.DeserializeObject<Message>(entity.Content);
                ret.message.Add(entity.toMessage());
            }
            return ret;
        }

        public MessagePagination PublicSquareLine(DateTime start, DateTime end, int count = 25, TableContinuationToken continuationToken = null)
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

            TableQuery<PublicSquareLineEntity> rangeQuery = new TableQuery<PublicSquareLineEntity>().Where(query).Take(count); ;
            TableQuerySegment<PublicSquareLineEntity> queryResult = _publicSquareLine.ExecuteQuerySegmented(rangeQuery, continuationToken);

            MessagePagination ret = new MessagePagination();
            ret.continuationToken = Utils.Token2String(queryResult.ContinuationToken);
            ret.message = new List<Message>();
            foreach (PublicSquareLineEntity entity in queryResult)
            {
                //var msg = JsonConvert.DeserializeObject<Message>(entity.Content);
                ret.message.Add(entity.toMessage());
            }
            return ret;
        }

        public MessageDetail GetMessageDetail(string userid, string messageID)
        {
            string pk = Message.ToMessagePK(userid, messageID);
            TableOperation retrieveOperation = TableOperation.Retrieve<UserLineEntity>(pk, messageID);

            MessageDetail msgd = null;
            TableResult retrievedResult = _userline.Execute(retrieveOperation);
            if (retrievedResult.Result != null)
            {
                UserLineEntity entity = (UserLineEntity)retrievedResult.Result;
                //var msg = JsonConvert.DeserializeObject<Message>(entity.Content);
                var msg = entity.toMessage();
                msgd = new MessageDetail(msg);
                msgd.ReplyCount = entity.ReplyCount;
                //tweet.RetweetCount = entity.RetweetCount;
                msgd.Replies = GetAllReplies(entity.RowKey);
            }
            return msgd;
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

        public Message GetMessage(string msgUser, string msgID)
        {
            string pk = Message.ToMessagePK(msgUser, msgID);
            TableOperation retreiveOperation = TableOperation.Retrieve<UserLineEntity>(pk, msgID);
            TableResult retreiveResult = _userline.Execute(retreiveOperation);
            UserLineEntity entity = ((UserLineEntity)retreiveResult.Result);
            if (entity == null)
            {
                return null;
            }
            //var msg = JsonConvert.DeserializeObject<Message>(entity.Content);
            return entity.toMessage();
        }

        public DisplayMessage GetDisplayMessage(string msgUser, string msgID)
        {
            Message msg = GetMessage(msgUser, msgID);
            if (msg == null)
            {
                return null;
            }
            return new DisplayMessage(msg, _accManager, _attManager);
        }

        public Message PostMessage(string userid, 
                                    string eventID, 
                                    string schemaID, 
                                    string[] owner, 
                                    string[] atUser,
                                    string[] topicName, 
                                    string message, 
                                    string richMessage,
                                    string[] attachmentID,
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

            //merge userid from argument as well as message
            HashSet<string> validAtUsers = new HashSet<string>();
            HashSet<string> atUserids = new HashSet<string>();
            atUserids.UnionWith(Utils.GetAtUserid(message));
            if (atUser != null)
            {
                atUserids.UnionWith(atUser);
            }
            foreach (string uid in atUserids)
            {
                var temp = _accManager.FindUser(uid);
                if(temp != null)
                {
                    validAtUsers.Add(temp.Userid);
                }
            }

            //merge topic from argument as well as message
            HashSet<string> topic = new HashSet<string>();
            topic.UnionWith(Utils.GetTopicNames(message));
            if (topicName != null)
            {
                topic.UnionWith(topicName);
            }

            //Insert Rich Message
            string richMessageID = null;
            if (!string.IsNullOrEmpty(richMessage))
            {
                richMessageID = _richMsgManager.PostRichMessage(userid, timestamp, richMessage);
            }            

            // create message
            Message msg = new Message(userid, message, timestamp, eventID, schemaID, owner, validAtUsers.ToArray(), topic.ToArray(), richMessageID, attachmentID);
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
 
            //insert into Atline
            if (message.AtUser != null)
            {
                foreach (string atUserid in message.AtUser)
                {
                    UserProfile user = _accManager.FindUser(atUserid);
                    if (user != null)
                    {
                        insertOperation = TableOperation.InsertOrReplace(new AtLineEntity(user.Userid, message));
                        _atline.Execute(insertOperation);

                        _notifManager.incrementAtlineNotifCount(user.Userid);
                    }
                }
            }            
            
            //insert into Topicline
            if (message.TopicName != null)
            {
                foreach (string topicName in message.TopicName)
                {
                    Topic topic = _topicManager.FindTopicByName(topicName);
                    if (topic == null)
                    {
                        topic = new Topic();
                        topic.Name = topicName;
                        topic.MsgCount = 0;
                        _topicManager.AddTopic(topic);
                        topic = _topicManager.FindTopicByName(topicName);
                    }

                    insertOperation = TableOperation.InsertOrReplace(new TopicLine(message, topic.Id.ToString()));
                    _topicline.Execute(insertOperation);

                    _topicManager.incrementTopicCount(topic.Id);
                    _topicManager.incrementUnreadMsgCountOfFavouriteTopic(topic.Id);
                }
            }
            
            
            //todo:
            //The Userid of owner and AtUser may be ms alais or displayname instead of the
            //real id in MSGorilla System.
            //Should have a way to convert these names to Userid in our system
            if (message.Owner != null)
            {
                foreach (string ownerid in message.Owner)
                {
                    UserProfile user = _accManager.FindUser(ownerid);
                    if (user != null)
                    {
                        insertOperation = TableOperation.InsertOrReplace(new OwnerLineEntity(user.Userid, message));
                        _ownerline.Execute(insertOperation);
                        _notifManager.incrementOwnerlineNotifCount(user.Userid);
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

                _notifManager.incrementHomelineNotifCount(user.Userid);
            }
        }

    }
}
