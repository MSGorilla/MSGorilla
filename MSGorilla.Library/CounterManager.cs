using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.WindowsAzure.Storage.Table;

using MSGorilla.Library.Exceptions;
using MSGorilla.Library.Models;
using MSGorilla.Library.Azure;
using MSGorilla.Library.Models.AzureModels.Entity;

namespace MSGorilla.Library
{
    public class CounterManager
    {
        private AWCloudTable _counterSet;
        private AWCloudTable _counterRecord;

        static string ToRecordPK(string groupID, string name)
        {
            return string.Format("{0}_{1}", groupID, name);
        }

        static string ToRecordRK(int id)
        {
            return string.Format("{0:0000000000}", id);
        }
        public CounterManager()
        {
            _counterSet = AzureFactory.GetTable(AzureFactory.MSGorillaTable.CounterSet);
            _counterRecord = AzureFactory.GetTable(AzureFactory.MSGorillaTable.CounterRecord);
        }

        public CounterSet CreateCounterSet(string groupID, string name, string description = null)
        {
            if (GetCounterSet(groupID, name) != null)
            {
                throw new CounterSetAlreadyExistException();
            }
            CounterSet cs = new CounterSet();
            cs.Group = groupID;
            cs.Name = name;
            cs.Description = description;
            cs.RecordCount = 0;
            cs.LastUpdateTime = DateTime.UtcNow;
            CounterSetEntity entity = cs;
            TableOperation insertOperation = TableOperation.Insert(entity);
            _counterSet.Execute(insertOperation);
            return entity;
        }

        public CounterSetEntity GetCounterSet(string groupID, string name)
        {
            TableOperation retrieveOperation = TableOperation.Retrieve<CounterSetEntity>(groupID, name);
            TableResult result = _counterSet.ExecuteRetriveOperation(retrieveOperation);
            if (result.Result == null)
            {
                return null;
            }
            CounterSetEntity entity = (CounterSetEntity)result.Result;
            return entity;
        }

        public List<CounterSet> GetCounterSetByGroup(string groupID)
        {
            TableQuery<CounterSetEntity> query = new TableQuery<CounterSetEntity>().Where(
                TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, groupID)
                );

            List<CounterSet> counterSets = new List<CounterSet>();
            foreach (var entity in _counterSet.ExecuteQuery(query))
            {
                counterSets.Add(entity);
            }
            return counterSets;
        }

        public CounterRecord GetSingleCounterRecord(string groupID, string name, int number = 0)
        {
            TableOperation retrieveOperation = TableOperation.Retrieve(
                ToRecordPK(groupID, name), 
                ToRecordRK(number));
            
            return CounterRecordHelper.ParseEntity(
                _counterRecord.ExecuteRetriveOperation(retrieveOperation).Result as DynamicTableEntity
                );
        }

        public List<string> GetTopCounters(CounterSet counterSet)
        {
            CounterRecord record = GetSingleCounterRecord(counterSet.Group, counterSet.Name, counterSet.RecordCount - 1);
            List<string> counters = new List<string>();
            if (record != null && 
                record.Value != null && 
                record.Value.RelatedValues != null && 
                record.Value.RelatedValues.Count > 0)
            {
                foreach (var v in record.Value.RelatedValues)
                {
                    counters.Add(v.Name);
                }
            }

            return counters;
        }

        public List<CounterRecord> GetCounterRecords(string name, string groupID, int start = 0, int end = -1)
        {
            string query = TableQuery.CombineFilters(
                TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, ToRecordPK(groupID, name)),
                TableOperators.And,
                TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.GreaterThanOrEqual, ToRecordRK(start))
                );

            if (end > 0)
            {
                query = TableQuery.CombineFilters(
                    query,
                    TableOperators.And,
                    TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.LessThanOrEqual, ToRecordRK(end))
                );
            }            

            TableQuery<DynamicTableEntity> q = new TableQuery<DynamicTableEntity>().Where(query);
            List<CounterRecord> records = new List<CounterRecord>();
            foreach (var entity in _counterRecord.ExecuteQuery(q))
            {
                records.Add(CounterRecordHelper.ParseEntity(entity));
            }
            return records;
        }

        public void InsertCounterRecord(CounterRecord record, string name, string groupID)
        {
            int id;
            DynamicTableEntity entity = CounterRecordHelper.ToTableEntity(record, "pk", "rk");

            int loopCount = 0;
            while (true)
            {
                try
                {
                    CounterSetEntity counterSet = GetCounterSet(groupID, name);
                    if (counterSet == null)
                    {
                        throw new CounterSetNotFoundException();
                    }

                    id = counterSet.RecordCount;
                    counterSet.RecordCount++;

                    TableOperation updateOperation = TableOperation.InsertOrReplace(counterSet);
                    _counterSet.Execute(updateOperation);

                    break;
                }
                catch (Exception e)
                {
                    loopCount++;
                    if (loopCount >= 100)
                    {
                        throw e;
                    }
                }
            }
            
            entity.PartitionKey = ToRecordPK(groupID, name);
            entity.RowKey = ToRecordRK(id);
            TableOperation insertOperation = TableOperation.Insert(entity);
            _counterRecord.Execute(insertOperation);
        }
    }
}
