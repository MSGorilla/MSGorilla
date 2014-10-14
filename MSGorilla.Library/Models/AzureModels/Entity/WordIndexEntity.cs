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
        public string MessageUserId { get; set; }
        public string MessageId { get; set; }
        public int WordCount { get; set; }
        public string WordPositions { get; set; }
        //public string MessageSnapshot { get; set; }

        public WordIndexEntity(string word, MessageIdentity msg, List<int> posList)
        {
            Word = word;
            MessageUserId = msg.UserId;
            MessageId = msg.MessageId;
            WordCount = posList.Count();
            WordPositions = Utils.Array2String<int>(posList.ToArray());
            ///MessageSnapshot = GenarateMessageSnapshot(word, posList, msgWords);

            DateTime createdDate = DateTime.UtcNow;
            this.PartitionKey = string.Format("{0}_{1}", Word,
                Utils.ToAzureStorageDayBasedString(createdDate));
            this.RowKey = Utils.ToAzureStorageSecondBasedString(createdDate) + "_" + MessageId;
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

        public override bool Equals(object obj)
        {
            if (obj == this)
            {
                return true;
            }
            if (!(obj is WordIndexEntity))
            {
                return false;
            }

            WordIndexEntity entity = obj as WordIndexEntity;
            return Equals(this.PartitionKey, entity.PartitionKey) &&
                Equals(this.RowKey, entity.RowKey) &&
                Equals(this.Word, entity.Word) &&
                Equals(this.MessageUserId, entity.MessageUserId) &&
                Equals(this.MessageId, entity.MessageId) &&
                Equals(this.WordCount, entity.WordCount) &&
                Equals(this.WordPositions, entity.WordPositions);
        }
    }
}
