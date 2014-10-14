using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.WindowsAzure.Storage.Table;
using MSGorilla.SearchEngine;

namespace MSGorilla.Library.Models.AzureModels.Entity
{
    class SearchResultEntity : TableEntity
    {
        public string MessageUserId { get; set; }
        public string MessageId { get; set; }

        public SearchResultEntity() { }

        public SearchResultEntity(string resultId, int sortRank, MessageIdentity msg)
        {
            MessageUserId = msg.UserId;
            MessageId = msg.MessageId;

            this.PartitionKey = resultId;
            this.RowKey = (int.MaxValue - sortRank).ToString("D10") + "_" + MessageId;
        }

        public MessageIdentity GetMessage()
        {
            return new MessageIdentity(MessageUserId, MessageId);
        }

        public override bool Equals(object obj)
        {
            if (obj == this)
            {
                return true;
            }
            if (!(obj is SearchResultEntity))
            {
                return false;
            }

            SearchResultEntity entity = obj as SearchResultEntity;
            return Equals(this.PartitionKey, entity.PartitionKey) &&
                Equals(this.RowKey, entity.RowKey) &&
                Equals(this.MessageId, entity.MessageId) &&
                Equals(this.MessageUserId, entity.MessageUserId);
        }
    }
}
