using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.WindowsAzure.Storage.Table;
using MSGorilla.SearchEngine;

namespace MSGorilla.Library.Models.AzureModels.Entity
{
    class SearchHistoryEntity : TableEntity
    {
        public DateTime LastSearchDateUTC { get; set; }

        public SearchHistoryEntity() { }

        public SearchHistoryEntity(string resultId, DateTime searchDateUTC)
        {
            LastSearchDateUTC = searchDateUTC;

            this.PartitionKey = resultId;
            this.RowKey = resultId;
        }
    }
}
