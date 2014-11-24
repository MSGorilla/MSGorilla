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
using MSGorilla.Library.Models.ViewModels;
using MSGorilla.Library.Exceptions;
using MSGorilla.Library.Models.AzureModels.Entity;

namespace MSGorilla.Library
{
    public class ReplyManager
    {
        private CloudTable _reply;
        private CloudTable _replyNotification;
        private CloudTable _replyArchive;
        private CloudTable _userline;

        private AccountManager _accManager;
        private NotifManager _notifManager;
        private RichMsgManager _richMsgManager;

        public ReplyManager()
        {
            _reply = AzureFactory.GetTable(AzureFactory.MSGorillaTable.Reply);
            _replyNotification = AzureFactory.GetTable(AzureFactory.MSGorillaTable.ReplyNotification);
            _replyArchive = AzureFactory.GetTable(AzureFactory.MSGorillaTable.ReplyArchive);
            _userline = AzureFactory.GetTable(AzureFactory.MSGorillaTable.Userline);
            _accManager = new AccountManager();
            _notifManager = new NotifManager();
            _richMsgManager = new RichMsgManager();
        }

        public DisplayReplyPagination GetReply(string userid, int count = 25, TableContinuationToken token = null)
        {
            TableQuery<BaseReplyEntity> query = new TableQuery<BaseReplyEntity>()
                .Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, userid))
                .Take(count);

            TableQuerySegment<BaseReplyEntity> queryResult = _replyNotification.ExecuteQuerySegmented(query, token);

            ReplyPagination ret = new ReplyPagination();
            ret.continuationToken = Utils.Token2String(queryResult.ContinuationToken);
            ret.message = new List<Reply>();
            foreach (BaseReplyEntity entity in queryResult)
            {
                ret.message.Add(entity.ToReply());
            }
            return new DisplayReplyPagination(ret);
        }

        //public List<Reply> GetReplyNotif(string userid)
        //{
        //    TableQuery<ReplyNotificationEntifity> query = new TableQuery<ReplyNotificationEntifity>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, userid));
            
        //    List<Reply> replies = new List<Reply>();
        //    TableBatchOperation batchOperation = new TableBatchOperation();
        //    int count = 0;

        //    // Print the fields for each customer.
        //    foreach (ReplyNotificationEntifity entity in _replyNotification.ExecuteQuery(query))
        //    {
        //        replies.Add(JsonConvert.DeserializeObject<Reply>(entity.Content));

        //        batchOperation.Add(TableOperation.Delete(entity));
        //        count++;
                
        //        if((count % 100) == 0){
        //            _replyNotification.ExecuteBatch(batchOperation);
        //            batchOperation = new TableBatchOperation();
        //            count = 0;
        //        }
        //    }

        //    if (count > 0)
        //    {
        //        _replyNotification.ExecuteBatch(batchOperation);
        //    }

        //    return replies;
        //}

        public List<Reply> GetAllReply(string userid)
        {
            TableQuery<BaseReplyEntity> query = new TableQuery<BaseReplyEntity>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, userid));
            List<Reply> replies = new List<Reply>();

            foreach (BaseReplyEntity entity in _replyArchive.ExecuteQuery(query))
            {
                replies.Add(entity.ToReply());
            }
            //replies.Reverse();
            return replies;
        }


        public Reply PostReply(string user,
                                string[] atUser,
                                string msgContent,
                                string richMessage,
                                string[] attachmentID,
                                DateTime timestamp,
                                //string originMessageUser,
                                string originMessageID)
        {
            if (_accManager.FindUser(user) == null)
            {
                throw new UserNotFoundException(user);
            }

            //merge toUser list and @somebody in the message content
            List<String> validToUsers = new List<String>();
            HashSet<string> ToUserIDs = new HashSet<string>();
            ToUserIDs.UnionWith(Utils.GetAtUserid(msgContent));
            if (atUser != null)
            {
                ToUserIDs.UnionWith(atUser);
            }
            foreach (string uid in ToUserIDs)
            {
                var temp = _accManager.FindUser(uid);
                if (temp != null)
                {
                    validToUsers.Add(temp.Userid);
                }
            }
            atUser = validToUsers.ToArray();

            TableOperation retreiveOperation = MessageManager.RetrieveUserlineMsgByID<UserLineEntity>(originMessageID);
            TableResult retreiveResult = _userline.Execute(retreiveOperation);
            UserLineEntity originMsg = ((UserLineEntity)retreiveResult.Result);

            if (originMsg == null)
            {
                throw new MessageNotFoundException();
            }

            //Insert Rich Message
            string richMessageID = null;
            if (!string.IsNullOrEmpty(richMessage))
            {
                richMessageID = _richMsgManager.PostRichMessage(user, timestamp, richMessage);
            }

            //Reply reply = new Reply(user, atUser, msgContent, timestamp, originMessageUser, originMessageID);
            Reply reply = new Reply(user, originMsg.Group, msgContent, timestamp, null, originMessageID, atUser, richMessageID, attachmentID);

            //insert reply
            ReplyEntity replyEntity = new ReplyEntity(reply);
            TableOperation insertOperation = TableOperation.Insert(replyEntity);
            _reply.Execute(insertOperation);

            //update reply count
            originMsg.ReplyCount++;
            TableOperation updateOperation = TableOperation.Replace(originMsg);
            _userline.Execute(updateOperation);

            foreach(string userid in atUser){
                if (_accManager.FindUser(userid) == null)
                {
                    continue;
                }
                //notif user
                ReplyNotificationEntifity notifEntity = new ReplyNotificationEntifity(userid, reply);
                insertOperation = TableOperation.Insert(notifEntity);
                _replyNotification.Execute(insertOperation);
                _replyArchive.Execute(insertOperation);

                _notifManager.incrementReplyNotifCount(userid);
            }            

            return reply;
        }
    }
}
