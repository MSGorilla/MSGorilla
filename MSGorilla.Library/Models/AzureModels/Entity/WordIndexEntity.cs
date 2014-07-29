using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MSGorilla.SearchEngine;
using MSGorilla.SearchEngine.Common;

namespace MSGorilla.Library.Models.AzureModels.Entity
{
    public class WordIndexEntity : TableEntity
    {
        public string Word { get; set; }
        public DateTime CreatedTime { get; set; }
        public string MessageUserId { get; set; }
        public string MessageId { get; set; }
        public int WordCount { get; set; }
        public string WordPositions { get; set; }
        //public string MessageSnapshot { get; set; }

        public WordIndexEntity(string word, string userId, string msgId, List<int> posList)
        {
            Word = word;
            CreatedTime = DateTime.UtcNow;
            MessageUserId = userId;
            MessageId = msgId;
            WordCount = posList.Count();
            WordPositions = Utils.Array2String<int>(posList.ToArray());
            ///MessageSnapshot = GenarateMessageSnapshot(word, posList, msgWords);

            this.PartitionKey = string.Format("{0}_{1}", Word,
                Utils.ToAzureStorageDayBasedString(CreatedTime));
            this.RowKey = msgId;
        }

        public MessageIdentity GetMessage()
        {
            return new MessageIdentity(MessageUserId, MessageId);
        }

        public List<int> GetPostionsList()
        {
            return new List<int>(Utils.String2IntArray(WordPositions));
        }

        private string GenarateMessageSnapshot(string word, List<int> posList, string[] msgWords)
        {
            return "";
        }

        public WordIndexEntity() { }
    }
}
