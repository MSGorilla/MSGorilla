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
        public string ResultId { get; set; }
        public DateTime LastSearchDateUTC { get; set; }
        public double TakenTime { get; set; }
        public int ResultsCount { get; set; }

        public SearchHistoryEntity() { }

        public SearchHistoryEntity(string resultId, DateTime searchDateUTC, double takenTime, int resultsCount)
        {
            ResultId = resultId;
            LastSearchDateUTC = searchDateUTC;
            TakenTime = takenTime;
            ResultsCount = resultsCount;

            this.PartitionKey = resultId;
            this.RowKey = resultId;
        }
    }
}
