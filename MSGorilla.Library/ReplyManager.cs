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

        public ReplyManager()
        {
            _reply = AzureFactory.GetTable(AzureFactory.MSGorillaTable.Reply);
            _replyNotification = AzureFactory.GetTable(AzureFactory.MSGorillaTable.ReplyNotification);
            _replyArchive = AzureFactory.GetTable(AzureFactory.MSGorillaTable.ReplyArchive);
            _userline = AzureFactory.GetTable(AzureFactory.MSGorillaTable.Userline);
            _accManager = new AccountManager();
            _notifManager = new NotifManager();
        }

        public ReplyPagiantion GetReply(string userid, int count = 25, TableContinuationToken token = null)
        {
            TableQuery<ReplyNotificationEntifity> query = new TableQuery<ReplyNotificationEntifity>()
                .Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, userid))
                .Take(count);

            TableQuerySegment<ReplyNotificationEntifity> queryResult = _replyNotification.ExecuteQuerySegmented(query, token);

            ReplyPagiantion ret = new ReplyPagiantion();
            ret.continuationToken = Utils.Token2String(queryResult.ContinuationToken);
            ret.reply = new List<Reply>();
            foreach (ReplyNotificationEntifity entity in queryResult)
            {
                ret.reply.Add(JsonConvert.DeserializeObject<Reply>(entity.Content));
            }
            return ret;
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
            TableQuery<ReplyNotificationEntifity> query = new TableQuery<ReplyNotificationEntifity>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, userid));
            List<Reply> replies = new List<Reply>();

            foreach (ReplyNotificationEntifity entity in _replyArchive.ExecuteQuery(query))
            {
                replies.Add(JsonConvert.DeserializeObject<Reply>(entity.Content));
            }
            replies.Reverse();
            return replies;
        }


        public Reply PostReply(string fromUser,
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

            //notif user
            ReplyNotificationEntifity notifEntity = new ReplyNotificationEntifity(reply);
            TableOperation insert = TableOperation.Insert(notifEntity);
            _replyNotification.Execute(insert);
            _replyArchive.Execute(insert);

            _notifManager.incrementReplyNotifCount(toUser);

            return reply;
        }
    }
}
