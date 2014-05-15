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
    public class TweetManager
    {
        private const int DefaultTimelineQueryDayRange = 7;
        private CloudTable _homelineTweet;
        private CloudTable _userlineTweet;
        private CloudTable _reply;

        public TweetManager()
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

            string query = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, userid);
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

        public List<Tweet> UserLine(string userid, DateTime before, DateTime after)
        {
            TableQuery<UserLineTweetEntity> rangeQuery =
                new TableQuery<UserLineTweetEntity>().Where(
                    GenerateTimestampConditionQuery(userid, before, after)
                );

            List<Tweet> tweets = new List<Tweet>();
            foreach (UserLineTweetEntity entity in _userlineTweet.ExecuteQuery(rangeQuery))
            {
                tweets.Add(JsonConvert.DeserializeObject<Tweet>(entity.TweetContent));
            }
            return tweets;
        }

        public List<Tweet> UserLine(string userid)
        {
            return UserLine(userid, DateTime.UtcNow, DateTime.UtcNow.AddDays(0 - DefaultTimelineQueryDayRange));
        }

        public List<Tweet> HomeLine(string userid, DateTime before, DateTime after)
        {
            TableQuery<HomeLineTweetEntity> rangeQuery =
                new TableQuery<HomeLineTweetEntity>().Where(
                    GenerateTimestampConditionQuery(userid, before, after)
                );

            List<Tweet> tweets = new List<Tweet>();
            foreach (HomeLineTweetEntity entity in _homelineTweet.ExecuteQuery(rangeQuery))
            {
                tweets.Add(JsonConvert.DeserializeObject<Tweet>(entity.TweetContent));
            }
            return tweets;
        }

        public List<Tweet> HomeLine(string userid)
        {
            return HomeLine(userid, DateTime.UtcNow, DateTime.UtcNow.AddDays(0 - DefaultTimelineQueryDayRange));
        }

        public TweetDetail GetTweetDetail(string userid, string tweetID)
        {
            TableOperation retrieveOperation = TableOperation.Retrieve<UserLineTweetEntity>(userid, tweetID);

            TweetDetail tweet = null;
            TableResult retrievedResult = _userlineTweet.Execute(retrieveOperation);
            if (retrievedResult.Result != null)
            {
                UserLineTweetEntity entity = (UserLineTweetEntity)retrievedResult.Result;
                tweet = new TweetDetail(JsonConvert.DeserializeObject<Tweet>(entity.TweetContent));
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
