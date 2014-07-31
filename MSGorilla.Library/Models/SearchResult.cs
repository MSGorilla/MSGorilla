using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSGorilla.Library.Models
{
    public class SearchResult
    {
        public string ResultId { get; set; }
        public DateTime SearchDate { get; set; }
        public double TakenTime { get; set; }
        public int ResultsCount { get; set; }

        public SearchResult(string resultId, DateTime searchDateUTC, double takenTime, int resultsCount)
        {
            ResultId = resultId;
            SearchDate = searchDateUTC;
            TakenTime = takenTime;
            ResultsCount = resultsCount;
        }
    }
}
