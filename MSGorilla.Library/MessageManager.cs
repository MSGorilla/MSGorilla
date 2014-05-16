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
        private CloudTable _homelineTweet;
        private CloudTable _userlineTweet;
        private CloudTable _reply;

        public MessageManager()
        {
            _homelineTweet = AzureFactory.GetTable(AzureFactory.TweetTable.HomelineTweet);
            _userlineTweet = AzureFactory.GetTable(AzureFactory.TweetTable.UserlineTweet);
            _reply = AzureFactory.GetTable(AzureFactory.TweetTable.Reply);
        }

        static string GenerateTimestampConditionQuery(string userid, DateTime before, DateTime after)
        {
            if (before == null)
            {
                before = DateTime.UtcNow;
            }

            if (after == null)
            {
                after = before.AddDays(0 - DefaultTimelineQueryDayRange);
            }

            string query = TableQuery.GenerateFilterCondition(
                "PartitionKey", 
                QueryComparisons.LessThanOrEqual, 
                string.Format("{0}_{1}", userid, Utils.ToAzureStorageDayBasedString(before)));

            query = TableQuery.CombineFilters(
                query,
                TableOperators.And,
                TableQuery.GenerateFilterCondition(
                    "PartitionKey",
                    QueryComparisons.GreaterThanOrEqual,
                    string.Format("{0}_{1}", userid, Utils.ToAzureStorageDayBasedString(after)))
            );

            query = TableQuery.CombineFilters(
                query,
                TableOperators.And,
                TableQuery.GenerateFilterCondition(
                    "RowKey",
                    QueryComparisons.LessThan,
                    Utils.ToAzureStorageSecondBasedString(before))
            );

            query = TableQuery.CombineFilters(
                query,
                TableOperators.And,
                TableQuery.GenerateFilterCondition(
                    "RowKey",
                    QueryComparisons.GreaterThan,
                    Utils.ToAzureStorageSecondBasedString(after))
            );

            return query;
        } 

        public List<Message> UserLine(string userid, DateTime before, DateTime after)
        {
            TableQuery<UserLineEntity> rangeQuery =
                new TableQuery<UserLineEntity>().Where(
                    GenerateTimestampConditionQuery(userid, before, after)
                );

            List<Message> tweets = new List<Message>();
            foreach (UserLineEntity entity in _userlineTweet.ExecuteQuery(rangeQuery))
            {
                tweets.Add(JsonConvert.DeserializeObject<Message>(entity.TweetContent));
            }
            return tweets;
        }

        public List<Message> UserLine(string userid)
        {
            return UserLine(userid, DateTime.UtcNow, DateTime.UtcNow.AddDays(0 - DefaultTimelineQueryDayRange));
        }

        public List<Message> HomeLine(string userid, DateTime before, DateTime after)
        {
            TableQuery<HomeLineEntity> rangeQuery =
                new TableQuery<HomeLineEntity>().Where(
                    GenerateTimestampConditionQuery(userid, before, after)
                );

            List<Message> tweets = new List<Message>();
            foreach (HomeLineEntity entity in _homelineTweet.ExecuteQuery(rangeQuery))
            {
                tweets.Add(JsonConvert.DeserializeObject<Message>(entity.MessageContent));
            }
            return tweets;
        }

        public List<Message> HomeLine(string userid)
        {
            return HomeLine(userid, DateTime.UtcNow, DateTime.UtcNow.AddDays(0 - DefaultTimelineQueryDayRange));
        }

        public MessageDetail GetMessageDetail(string userid, string messageID)
        {
            string pk = Message.ToMessagePK(userid, messageID);
            TableOperation retrieveOperation = TableOperation.Retrieve<UserLineEntity>(pk, messageID);

            MessageDetail tweet = null;
            TableResult retrievedResult = _userlineTweet.Execute(retrieveOperation);
            if (retrievedResult.Result != null)
            {
                UserLineEntity entity = (UserLineEntity)retrievedResult.Result;
                tweet = new MessageDetail(JsonConvert.DeserializeObject<Message>(entity.TweetContent));
                tweet.ReplyCount = entity.ReplyCount;
                tweet.RetweetCount = entity.RetweetCount;
                tweet.Replies = GetAllReplies(entity.RowKey);
            }
            return tweet;
        }

        public List<Reply> GetAllReplies(string tweetID)
        {
            TableQuery<ReplyEntity> query = new TableQuery<ReplyEntity>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, tweetID));

            List<Reply> replies = new List<Reply>();
            // Print the fields for each customer.
            foreach (ReplyEntity entity in _reply.ExecuteQuery(query))
            {
                replies.Add(JsonConvert.DeserializeObject<Reply>(entity.Content));
            }
            return replies;
        }
    }
}
