using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

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
    public class PostManager
    {
        private CloudQueue _queue;
        private CloudTable _homeline;
        private CloudTable _userline;
        private CloudTable _eventline;
        private CloudTable _publicSquareline;
        private CloudTable _reply;
        private CloudTable _replyNotification;

        private AccountManager _accManager;
        private SchemaManager _schemaManager;

        public PostManager(){
            _queue = AzureFactory.GetQueue();
            _homeline = AzureFactory.GetTable(AzureFactory.MSGorillaTable.Homeline);
            _userline = AzureFactory.GetTable(AzureFactory.MSGorillaTable.Userline);
            _eventline = AzureFactory.GetTable(AzureFactory.MSGorillaTable.EventLine);
            _publicSquareline = AzureFactory.GetTable(AzureFactory.MSGorillaTable.PublicSquareLine);
            _reply = AzureFactory.GetTable(AzureFactory.MSGorillaTable.Reply);
            _replyNotification = AzureFactory.GetTable(AzureFactory.MSGorillaTable.ReplyNotification);

            _accManager = new AccountManager();
            _schemaManager = new SchemaManager();
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

            //insert into Eventline
            insertOperation = TableOperation.Insert(new EventLineEntity(msg));
            _eventline.Execute(insertOperation);

            //insert into PublicSquareLine
            insertOperation = TableOperation.Insert(new PublicSquareLineEntity(msg));
            _publicSquareline.Execute(insertOperation);

            //insert into QueueMessage
            //QueueMessage queueMessage = new QueueMessage(QueueMessage.TypeMessage, msg.ToJsonString());
            _queue.AddMessage(msg.toAzureCloudQueueMessage());
        }

        //public void PostRetweet(string userid, string originTweetUser, string originTweetID, DateTime timestamp)
        //{
        //    if (_accManager.FindUser(userid) == null)
        //    {
        //        throw new UserNotFoundException(userid);
        //    }

        //    TableOperation retreiveOperation = TableOperation.Retrieve<UserLineEntity>(originTweetUser, originTweetID);
        //    TableResult retreiveResult = _userline.Execute(retreiveOperation);
        //    UserLineEntity originTweet = ((UserLineEntity)retreiveResult.Result);

        //    if (originTweet == null)
        //    {
        //        throw new TweetNotFoundException();
        //    }

        //    JObject oTweet = JObject.Parse(originTweet.Content);
        //    if (Message.TweetTypeRetweet.Equals(oTweet["Type"]))
        //    {
        //        throw new RetweetARetweetException();
        //    }

        //    Message tweet = new Message(Message.TweetTypeRetweet, userid, originTweet.Content, timestamp);
        //    //insert into Userline
        //    TableOperation insertOperation = TableOperation.Insert(new UserLineEntity(tweet));
        //    _userline.Execute(insertOperation);

        //    //insert into QueueMessage
        //    QueueMessage queueMessage = new QueueMessage(QueueMessage.TypeTweet, tweet.ToJsonString());
        //    _queue.AddMessage(queueMessage.toAzureCloudQueueMessage());

        //    //update retweet count
        //    originTweet.RetweetCount++;
        //    TableOperation updateOperation = TableOperation.Replace(originTweet);
        //    _userline.Execute(updateOperation);
        //}

        public void SpreadMessage(Message message)
        {
            List<UserProfile> followers = _accManager.Followers(message.User);
            followers.Add(_accManager.FindUser(message.User));
            //speed tweet to followers

            //todo: BatchInsert
            foreach (UserProfile user in followers)
            {
                HomeLineEntity entity = new HomeLineEntity(user.Userid, message);
                TableOperation insertOperation = TableOperation.Insert(entity);
                _homeline.Execute(insertOperation);
            }
        }


        public void PostReply(  string fromUser, 
                                string toUser, 
                                string content, 
                                DateTime timestamp,
                                string originMessageUser, 
                                string originMessageID)
        {
            if (_accManager.FindUser(fromUser) == null)
            {
                throw new UserNotFoundException(fromUser);
            }
            if (_accManager.FindUser(toUser) == null)
            {
                throw new UserNotFoundException(toUser);
            }
            if (_accManager.FindUser(originMessageUser) == null)
            {
                throw new UserNotFoundException(originMessageUser);
            }

            string pk = Message.ToMessagePK(originMessageUser, originMessageID);
            TableOperation retreiveOperation = TableOperation.Retrieve<UserLineEntity>(pk, originMessageID);
            TableResult retreiveResult = _userline.Execute(retreiveOperation);
            UserLineEntity originMsg = ((UserLineEntity)retreiveResult.Result);

            if (originMsg == null)
            {
                throw new MessageNotFoundException();
            }

            Reply reply = new Reply(fromUser, toUser, content, timestamp, originMessageUser, originMessageID);
            //insert reply
            ReplyEntity replyEntity = new ReplyEntity(reply);
            TableOperation insertOperation = TableOperation.Insert(replyEntity);
            _reply.Execute(insertOperation);

            //update reply count
            originMsg.ReplyCount++;
            TableOperation updateOperation = TableOperation.Replace(originMsg);
            _userline.Execute(updateOperation);

            //notif user as well as the message publisher
            ReplyNotificationEntifity notifUserEntity = new ReplyNotificationEntifity(reply);
            TableBatchOperation batchInsert = new TableBatchOperation();
            batchInsert.Insert(notifUserEntity);
            if (!reply.ToUser.Equals(reply.MessageUser))
            {
                ReplyNotificationEntifity notifMsgPublisherEntity = new ReplyNotificationEntifity(reply.MessageUser, reply);
                batchInsert.Insert(notifMsgPublisherEntity);
            }            
            _replyNotification.ExecuteBatch(batchInsert);
        }
    }
}
