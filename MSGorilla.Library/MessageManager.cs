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
        private CloudQueue _spiderqueue;
        private CloudQueue _mailMessageQueue;

        private AccountManager _accManager;
        private AttachmentManager _attManager;
        private SchemaManager _schemaManager;
        private NotifManager _notifManager;
        private TopicManager _topicManager;
        private RichMsgManager _richMsgManager;
        private GroupManager _groupManager;

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

            _queue = AzureFactory.GetQueue(AzureFactory.MSGorillaQueue.Dispatcher);
            _spiderqueue = AzureFactory.GetQueue(AzureFactory.MSGorillaQueue.SearchEngineSpider);
            _mailMessageQueue = AzureFactory.GetQueue(AzureFactory.MSGorillaQueue.MailMessage);

            _accManager = new AccountManager();
            _attManager = new AttachmentManager();
            _schemaManager = new SchemaManager();
            _notifManager = new NotifManager();
            _topicManager = new TopicManager();
            _richMsgManager = new RichMsgManager();
            _groupManager = new GroupManager();
        }

        static string GenerateTimestampConditionQuery(string userid, DateTime start, DateTime end, string groupID = null)
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

            string rowkeyStartWith = string.IsNullOrEmpty(groupID) ? "" : groupID + "_";
            query = TableQuery.CombineFilters(
                query,
                TableOperators.And,
                TableQuery.GenerateFilterCondition(
                    "RowKey",
                    QueryComparisons.LessThan,
                    Utils.NextKeyString(rowkeyStartWith + Utils.ToAzureStorageSecondBasedString(start)))
            );

            query = TableQuery.CombineFilters(
                query,
                TableOperators.And,
                TableQuery.GenerateFilterCondition(
                    "RowKey",
                    QueryComparisons.GreaterThan,
                    rowkeyStartWith + Utils.ToAzureStorageSecondBasedString(end))
            );

            return query;
        }

        static string GenerateStartWithConditionQuery(string startWith, string propertyName = "PartitionKey")
        {
            if (!Utils.IsValidID(startWith))
            {
                throw new InvalidIDException();
            }

            string query = TableQuery.GenerateFilterCondition(
                propertyName,
                QueryComparisons.LessThan,
                Utils.NextKeyString(startWith));

            query = TableQuery.CombineFilters(
                query,
                TableOperators.And,
                TableQuery.GenerateFilterCondition(
                    propertyName,
                    QueryComparisons.GreaterThanOrEqual,
                    startWith
                )
            );
            return query;
        }

        public MessagePagination UserLine(string userid, string groupID, DateTime start, DateTime end, int count = 25, TableContinuationToken continuationToken = null)
        {
            TableQuery<BaseMessageEntity> rangeQuery =
                new TableQuery<BaseMessageEntity>().Where(
                    GenerateTimestampConditionQuery(userid, start, end, groupID)
                ).Take(count);
            TableQuerySegment<BaseMessageEntity> queryResult = _userline.ExecuteQuerySegmented(rangeQuery, continuationToken);

            MessagePagination ret = new MessagePagination();
            ret.continuationToken = Utils.Token2String(queryResult.ContinuationToken);
            ret.message = new List<Message>();
            foreach (BaseMessageEntity entity in queryResult)
            {
                //var msg = JsonConvert.DeserializeObject<Message>(entity.Content);
                ret.message.Add(entity.ToMessage());
            }
            return ret;
        }

        public MessagePagination UserLine(string userid, string groupID, int count = 25, TableContinuationToken continuationToken = null)
        {
            string query = GenerateStartWithConditionQuery(userid + "_");
            query = TableQuery.CombineFilters(
                query,
                TableOperators.And,
                GenerateStartWithConditionQuery(groupID + "_", "RowKey")
                );

            TableQuery<BaseMessageEntity> tableQuery = new TableQuery<BaseMessageEntity>().Where(query).Take(count);
            TableQuerySegment<BaseMessageEntity> queryResult = _userline.ExecuteQuerySegmented(tableQuery, continuationToken);

            MessagePagination ret = new MessagePagination();
            ret.continuationToken = Utils.Token2String(queryResult.ContinuationToken);
            ret.message = new List<Message>();
            foreach (BaseMessageEntity entity in queryResult)
            {
                //var msg = JsonConvert.DeserializeObject<Message>(entity.Content);
                ret.message.Add(entity.ToMessage());
            }
            return ret;
        }

        public MessagePagination OwnerLine(string userid, DateTime start, DateTime end, int count = 25, TableContinuationToken continuationToken = null)
        {
            TableQuery<BaseMessageEntity> rangeQuery =
                new TableQuery<BaseMessageEntity>().Where(
                    GenerateTimestampConditionQuery(userid, start, end)
                ).Take(count);
            TableQuerySegment<BaseMessageEntity> queryResult = _ownerline.ExecuteQuerySegmented(rangeQuery, continuationToken);

            MessagePagination ret = new MessagePagination();
            ret.continuationToken = Utils.Token2String(queryResult.ContinuationToken);
            ret.message = new List<Message>();
            foreach (BaseMessageEntity entity in queryResult)
            {
                //var msg = JsonConvert.DeserializeObject<Message>(entity.Content);
                ret.message.Add(entity.ToMessage());
            }
            return ret;
        }

        public MessagePagination OwnerLine(string userid, int count = 25, TableContinuationToken continuationToken = null)
        {
            string query = GenerateStartWithConditionQuery(userid + "_");
            TableQuery<BaseMessageEntity> tableQuery = new TableQuery<BaseMessageEntity>().Where(query).Take(count);
            TableQuerySegment<BaseMessageEntity> queryResult = _ownerline.ExecuteQuerySegmented(tableQuery, continuationToken);

            MessagePagination ret = new MessagePagination();
            ret.continuationToken = Utils.Token2String(queryResult.ContinuationToken);
            ret.message = new List<Message>();
            foreach (BaseMessageEntity entity in queryResult)
            {
                //var msg = JsonConvert.DeserializeObject<Message>(entity.Content);
                ret.message.Add(entity.ToMessage());
            }
            return ret;
        }

        public MessagePagination AtLine(string userid, DateTime start, DateTime end, int count = 25, TableContinuationToken continuationToken = null)
        {
            TableQuery<BaseMessageEntity> rangeQuery =
                new TableQuery<BaseMessageEntity>().Where(
                    GenerateTimestampConditionQuery(userid, start, end)
                ).Take(count);
            TableQuerySegment<BaseMessageEntity> queryResult = _atline.ExecuteQuerySegmented(rangeQuery, continuationToken);

            MessagePagination ret = new MessagePagination();
            ret.continuationToken = Utils.Token2String(queryResult.ContinuationToken);
            ret.message = new List<Message>();
            foreach (BaseMessageEntity entity in queryResult)
            {
                //var msg = JsonConvert.DeserializeObject<Message>(entity.Content);
                ret.message.Add(entity.ToMessage());
            }
            return ret;
        }

        public MessagePagination AtLine(string userid, int count = 25, TableContinuationToken continuationToken = null)
        {
            string query = GenerateStartWithConditionQuery(userid + "_");
            TableQuery<BaseMessageEntity> tableQuery = new TableQuery<BaseMessageEntity>().Where(query).Take(count);
            TableQuerySegment<BaseMessageEntity> queryResult = _atline.ExecuteQuerySegmented(tableQuery, continuationToken);

            MessagePagination ret = new MessagePagination();
            ret.continuationToken = Utils.Token2String(queryResult.ContinuationToken);
            ret.message = new List<Message>();
            foreach (BaseMessageEntity entity in queryResult)
            {
                //var msg = JsonConvert.DeserializeObject<Message>(entity.Content);
                ret.message.Add(entity.ToMessage());
            }
            return ret;
        }

        public MessagePagination HomeLine(string userid, string groupID, DateTime start, DateTime end, int count = 25, TableContinuationToken continuationToken = null)
        {
            TableQuery<BaseMessageEntity> rangeQuery =
                new TableQuery<BaseMessageEntity>().Where(
                    GenerateTimestampConditionQuery(userid, start, end, groupID)
                ).Take(count); ;
            TableQuerySegment<BaseMessageEntity> queryResult = _homeline.ExecuteQuerySegmented(rangeQuery, continuationToken);

            MessagePagination ret = new MessagePagination();
            ret.continuationToken = Utils.Token2String(queryResult.ContinuationToken);
            ret.message = new List<Message>();
            foreach (BaseMessageEntity entity in queryResult)
            {
                //var msg = JsonConvert.DeserializeObject<Message>(entity.Content);
                ret.message.Add(entity.ToMessage());
            }
            return ret;
        }

        public MessagePagination HomeLine(string userid, string groupID, int count = 25, TableContinuationToken continuationToken = null)
        {
            string query = GenerateStartWithConditionQuery(userid + "_");
            query = TableQuery.CombineFilters(
                query,
                TableOperators.And,
                GenerateStartWithConditionQuery(groupID + "_", "RowKey")
                );

            TableQuery<BaseMessageEntity> tableQuery = new TableQuery<BaseMessageEntity>().Where(query).Take(count);
            TableQuerySegment<BaseMessageEntity> queryResult = _homeline.ExecuteQuerySegmented(tableQuery, continuationToken);

            MessagePagination ret = new MessagePagination();
            ret.continuationToken = Utils.Token2String(queryResult.ContinuationToken);
            ret.message = new List<Message>();
            foreach (BaseMessageEntity entity in queryResult)
            {
                //var msg = JsonConvert.DeserializeObject<Message>(entity.Content);
                ret.message.Add(entity.ToMessage());
            }
            return ret;
        }

        public MessagePagination TopicLine(string topicID, DateTime start, DateTime end, int count = 25, TableContinuationToken continuationToken = null)
        {
            TableQuery<BaseMessageEntity> rangeQuery =
                new TableQuery<BaseMessageEntity>().Where(
                    GenerateTimestampConditionQuery(topicID, start, end)
                ).Take(count); ;
            TableQuerySegment<BaseMessageEntity> queryResult = _topicline.ExecuteQuerySegmented(rangeQuery, continuationToken);

            MessagePagination ret = new MessagePagination();
            ret.continuationToken = Utils.Token2String(queryResult.ContinuationToken);
            ret.message = new List<Message>();
            foreach (BaseMessageEntity entity in queryResult)
            {
                //var msg = JsonConvert.DeserializeObject<Message>(entity.Content);
                ret.message.Add(entity.ToMessage());
            }
            return ret;
        }

        public MessagePagination TopicLine(string topicID, int count = 25, TableContinuationToken continuationToken = null)
        {
            string query = GenerateStartWithConditionQuery(topicID + "_");

            TableQuery<BaseMessageEntity> tableQuery = new TableQuery<BaseMessageEntity>().Where(query).Take(count);
            TableQuerySegment<BaseMessageEntity> queryResult = _topicline.ExecuteQuerySegmented(tableQuery, continuationToken);

            MessagePagination ret = new MessagePagination();
            ret.continuationToken = Utils.Token2String(queryResult.ContinuationToken);
            ret.message = new List<Message>();
            foreach (BaseMessageEntity entity in queryResult)
            {
                //var msg = JsonConvert.DeserializeObject<Message>(entity.Content);
                ret.message.Add(entity.ToMessage());
            }
            return ret;
        }

        public List<Message> EventLine(string eventID)
        {
            string query = GenerateStartWithConditionQuery(System.Web.HttpUtility.UrlEncode(eventID) + "_");

            TableQuery<BaseMessageEntity> rangeQuery = new TableQuery<BaseMessageEntity>().Where(query);

            List<Message> msgs = new List<Message>();
            foreach (BaseMessageEntity entity in _eventline.ExecuteQuery(rangeQuery))
            {
                //var msg = JsonConvert.DeserializeObject<Message>(entity.Content);
                msgs.Add(entity.ToMessage());
            }
            return msgs;
        }

        public MessagePagination PublicSquareLine(string groupID, int count = 25, TableContinuationToken continuationToken = null)
        {
            TableQuery<BaseMessageEntity> tableQuery =
                new TableQuery<BaseMessageEntity>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.GreaterThan, groupID + "_")).Take(count);

            TableQuerySegment<BaseMessageEntity> queryResult = _publicSquareLine.ExecuteQuerySegmented(tableQuery, continuationToken);

            MessagePagination ret = new MessagePagination();
            ret.continuationToken = Utils.Token2String(queryResult.ContinuationToken);
            ret.message = new List<Message>();
            foreach (BaseMessageEntity entity in queryResult)
            {
                //var msg = JsonConvert.DeserializeObject<Message>(entity.Content);
                ret.message.Add(entity.ToMessage());
            }
            return ret;
        }

        public MessagePagination PublicSquareLine(string groupID, DateTime start, DateTime end, int count = 25, TableContinuationToken continuationToken = null)
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
                groupID + "_" + Utils.NextKeyString(Utils.ToAzureStorageDayBasedString(start)));

            query = TableQuery.CombineFilters(
                query,
                TableOperators.And,
                TableQuery.GenerateFilterCondition(
                    "PartitionKey",
                    QueryComparisons.GreaterThanOrEqual,
                    groupID + "_" + Utils.ToAzureStorageDayBasedString(end))
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

            TableQuery<BaseMessageEntity> rangeQuery = new TableQuery<BaseMessageEntity>().Where(query).Take(count); ;
            TableQuerySegment<BaseMessageEntity> queryResult = _publicSquareLine.ExecuteQuerySegmented(rangeQuery, continuationToken);

            MessagePagination ret = new MessagePagination();
            ret.continuationToken = Utils.Token2String(queryResult.ContinuationToken);
            ret.message = new List<Message>();
            foreach (BaseMessageEntity entity in queryResult)
            {
                //var msg = JsonConvert.DeserializeObject<Message>(entity.Content);
                ret.message.Add(entity.ToMessage());
            }
            return ret;
        }


        public static TableOperation RetrieveUserlineMsgByID<TElement>(string messageID) where TElement : ITableEntity
        {
            try
            {
                string[] parts = messageID.Split('_');
                double timespan = Double.Parse(parts[0]);
                DateTime timestamp = DateTime.MaxValue.AddMilliseconds(0 - timespan);

                string group = parts[1];
                string userid = parts[2];

                string pk = string.Format("{0}_{1}", userid, Utils.ToAzureStorageDayBasedString(timestamp, false));
                string rk = string.Format("{0}_{1}", group, messageID);

                return TableOperation.Retrieve<TElement>(pk, rk);
            }
            catch
            {
                throw new InvalidMessageIDException();
            }
        }

        public Message GetRawMessage(string messageID)
        {
            TableOperation retrieveOperation = RetrieveUserlineMsgByID<BaseMessageEntity>(messageID);

            TableResult retrievedResult = _userline.Execute(retrieveOperation);
            if (retrievedResult.Result != null)
            {
                BaseMessageEntity entity = (BaseMessageEntity)retrievedResult.Result;
                return entity.ToMessage();
            }
            return null;
        }

        public List<Reply> GetAllReplies(string msgID)
        {
            TableQuery<BaseReplyEntity> query = new TableQuery<BaseReplyEntity>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, msgID));

            List<Reply> replies = new List<Reply>();
            // Print the fields for each customer.
            foreach (BaseReplyEntity entity in _reply.ExecuteQuery(query))
            {
                replies.Add(entity.ToReply());
            }
            return replies;
        }

        public Message GetMessage(string msgID)
        {
            TableOperation retreiveOperation = RetrieveUserlineMsgByID<UserLineEntity>(msgID);
            TableResult retreiveResult = _userline.Execute(retreiveOperation);
            UserLineEntity entity = ((UserLineEntity)retreiveResult.Result);
            if (entity == null)
            {
                return null;
            }
            //var msg = JsonConvert.DeserializeObject<Message>(entity.Content);
            return entity.ToMessage();
        }

        public Message PostMessage(string userid,
                                    string groupID,
                                    string eventID, 
                                    string schemaID, 
                                    string[] owner, 
                                    string[] atUser,
                                    string[] topicName, 
                                    string message, 
                                    string richMessage,
                                    string[] attachmentID,
                                    int importance,
                                    DateTime timestamp)
        {
            if ("none".Equals(message))
            {
                message = "";
            }
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
                try
                {
                    Membership member = _groupManager.CheckMembership(groupID, userid);
                    validAtUsers.Add(member.MemberID);
                }
                catch { }
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
            Message msg = new Message(userid, groupID, message, timestamp, eventID, schemaID, owner, validAtUsers.ToArray(), topic.ToArray(), richMessageID, attachmentID, importance);
            //insert into Userline
            TableOperation insertOperation = TableOperation.InsertOrReplace(new UserLineEntity(msg));
            _userline.Execute(insertOperation);

            //insert into poster's homeline
            insertOperation = TableOperation.InsertOrReplace(new HomeLineEntity(msg.User, msg));
            _homeline.Execute(insertOperation);

            //insert into QueueMessage
            //QueueMessage queueMessage = new QueueMessage(QueueMessage.TypeMessage, msg.ToJsonString());
            _queue.AddMessage(msg.toAzureCloudQueueMessage());
            _spiderqueue.AddMessage(msg.toAzureCloudQueueMessage());
            _mailMessageQueue.AddMessage(msg.toAzureCloudQueueMessage());

            user.MessageCount++;
            _accManager.UpdateUser(user);

            return msg;
        }

        public void SpreadMessage(Message message)
        {
            TableOperation insertOperation;

            //insert into Eventline
            if (!string.IsNullOrEmpty(message.EventID) && !"none".Equals(message.EventID))
            {
                insertOperation = TableOperation.InsertOrReplace(new EventLineEntity(message));
                _eventline.Execute(insertOperation);
            }

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
                    Topic topic = _topicManager.FindTopicByName(topicName, new string[]{message.Group});
                    if (topic == null)
                    {
                        topic = new Topic();
                        topic.Name = topicName;
                        topic.MsgCount = 0;
                        topic.GroupID = message.Group;
                        _topicManager.AddTopic(topic);
                        topic = _topicManager.FindTopicByName(topicName, new string[] { message.Group });
                    }

                    insertOperation = TableOperation.InsertOrReplace(new TopicLine(message, topic.Id.ToString()));
                    _topicline.Execute(insertOperation);

                    _topicManager.incrementTopicCount(topic.Id);
                    _topicManager.incrementUnreadMsgCountOfFavouriteTopic(topic.Id);
                }
            }

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


            //insert into homeline
            HashSet<string> groupMember = new HashSet<string>();
            foreach (var member in _groupManager.GetAllGroupMember(message.Group))
            {
                groupMember.Add(member.MemberID.ToLower());
            }


            List<UserProfile> followers = _accManager.Followers(message.User);
            //followers.Add(_accManager.FindUser(message.User));
            //speed tweet to followers
            foreach (UserProfile user in followers)
            {
                if (!groupMember.Contains(user.Userid.ToLower()))
                {
                    continue;
                }
                HomeLineEntity entity = new HomeLineEntity(user.Userid, message);
                insertOperation = TableOperation.InsertOrReplace(entity);
                _homeline.Execute(insertOperation);

                _notifManager.incrementHomelineNotifCount(user.Userid);

                if (message.Importance == 0)
                {
                    _notifManager.incrementImportantMsgCount(user.Userid);
                }
            }
        }
    }
}
