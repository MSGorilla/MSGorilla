using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.WindowsAzure.Storage.Table;

using MSGorilla.Library.Models.SqlModels;

using MSGorilla.Library.Exceptions;
using MSGorilla.Library.Azure;
using Newtonsoft.Json;


namespace MSGorilla.Library
{
    public class MetricRecord
    {
        public DateTime Timestamp { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
        public MetricRecord() { }
        public MetricRecord(string key, string value, DateTime timestamp)
        {
            this.Key = key;
            this.Value = value;
            this.Timestamp = timestamp;
        }
    }
    public class MetricEntity
    {
        public class RecordCountOverflowException : Exception
        {
            public RecordCountOverflowException():
                base("Each entity may contain at most " + MaxMetricRecord + " records.")
            { }
        }


        public const int MaxMetricRecord = 100;

        public int Count
        {
            get
            {
                return _entity["count"].Int32Value.Value;
            }
            private set
            {
                _entity["count"] = new EntityProperty(value);
            }
        }

        private DynamicTableEntity _entity;
        public MetricEntity(string partitionKey, string rowKey)
        {
            _entity = new DynamicTableEntity();
            _entity["count"] = new EntityProperty(0);
            _entity.PartitionKey = partitionKey;
            _entity.RowKey = rowKey;
        }

        public MetricEntity(DynamicTableEntity entity)
        {
            _entity = entity;
        }

        public void Put(string key, string value, DateTime timestamp)
        {
            if (this.Count >= MaxMetricRecord)
            {
                throw new RecordCountOverflowException();
            }

            MetricRecord record = new MetricRecord(key, value, timestamp);
            _entity[string.Format("record{0:000}", this.Count)] = new EntityProperty(JsonConvert.SerializeObject(record));

            this.Count++;
        }

        public MetricRecord Get(int index)
        {
            if (index > Count || index < 0)
            {
                return null;
            }
            string str = _entity[string.Format("record{0:000}", index)].StringValue;

            return JsonConvert.DeserializeObject<MetricRecord>(str);
        }

        public List<MetricRecord> ToList()
        {
            List<MetricRecord> list = new List<MetricRecord>();
            for (int i = 0; i < this.Count; i++)
            {
                list.Add(Get(i));
            }
            return list;
        }

        public ITableEntity ToITableEntity()
        {
            return _entity;
        }
    }

    public class MetricManager
    {
        private CloudTable _metricData;

        public MetricManager()
        {
            _metricData = AzureFactory.GetTable(AzureFactory.MSGorillaTable.MetricDataSet);
        }

        public MetricDataSet GetDateSet(int id)
        {
            using(var _gorillaCtx = new MSGorillaEntities())
            {
                MetricDataSet dataset = _gorillaCtx.MetricDataSets.Find(id);
                return dataset;
            }
        }

        public MetricDataSet AddDataSet(string creater, string name, string groupID, string description = null)
        {
            using (var _gorillaCtx = new MSGorillaEntities())
            {
                Group group = _gorillaCtx.Groups.Find(groupID);
                if (group == null)
                {
                    throw new GroupNotExistException();
                }

                UserProfile user = _gorillaCtx.UserProfiles.Find(creater);
                if (user == null)
                {
                    throw new UserNotFoundException(creater);
                }

                MetricDataSet data = new MetricDataSet();
                data.Name = name;
                data.GroupID = group.GroupID;
                data.Description = description;
                data.Creater = creater;
                data.RecordCount = 0;

                _gorillaCtx.MetricDataSets.Add(data);
                _gorillaCtx.SaveChanges();

                return data;
            }
        }

        public void RemoveDataSet(int id)
        {
            using (var _gorillaCtx = new MSGorillaEntities())
            {
                MetricDataSet data = _gorillaCtx.MetricDataSets.Find(id);
                if (data == null)
                {
                    throw new MetricDataSetNotFoundException();
                }

                _gorillaCtx.MetricDataSets.Remove(data);
                _gorillaCtx.SaveChanges();
            }
        }

        private const string RowKeyFormat = "000000000";
        public void AppendDataRecord(int id, string key, string value)
        {
            using (var _gorillaCtx = new MSGorillaEntities())
            {
                MetricDataSet data = _gorillaCtx.MetricDataSets.Find(id);
                if (data == null)
                {
                    throw new MetricDataSetNotFoundException();
                }

                MetricEntity mentity = null;
                if ((data.RecordCount % MetricEntity.MaxMetricRecord) == 0)
                {
                    //create a new entity
                    mentity = new MetricEntity(id.ToString(), data.RecordCount.ToString(RowKeyFormat));
                }
                else
                {
                    //retrive the last entity
                    TableResult result = _metricData.Execute(
                        TableOperation.Retrieve<DynamicTableEntity>(
                            id.ToString(), 
                            ((data.RecordCount / MetricEntity.MaxMetricRecord) * MetricEntity.MaxMetricRecord).ToString(RowKeyFormat)
                        )
                    );

                    mentity = new MetricEntity((DynamicTableEntity)result.Result);
                }

                //insert new data record
                mentity.Put(key, value, DateTime.UtcNow);
                TableOperation operation = TableOperation.InsertOrReplace(mentity.ToITableEntity());
                _metricData.Execute(operation);

                data.RecordCount++;
                _gorillaCtx.SaveChanges();
            }
        }

        public List<MetricRecord> RetriveDataRecord(int id, int startIndex, int endIndex)
        {
            List<MetricRecord> result = new List<MetricRecord>();

            startIndex = startIndex < 0 ? 0 : startIndex;
            if (endIndex < startIndex)
            {
                return result;
            }

            string query = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, id.ToString());
            query = TableQuery.CombineFilters(
                query,
                TableOperators.And,
                TableQuery.GenerateFilterCondition(
                    "RowKey", 
                    QueryComparisons.GreaterThanOrEqual,
                    ((startIndex / MetricEntity.MaxMetricRecord) * MetricEntity.MaxMetricRecord).ToString(RowKeyFormat))
                );

            query = TableQuery.CombineFilters(
                query,
                TableOperators.And,
                TableQuery.GenerateFilterCondition(
                    "RowKey", 
                    QueryComparisons.LessThanOrEqual,
                    ((endIndex / MetricEntity.MaxMetricRecord) * MetricEntity.MaxMetricRecord).ToString(RowKeyFormat))
                );

            TableQuery rangeQuery = new TableQuery().Where(query);
            foreach (DynamicTableEntity entity in _metricData.ExecuteQuery(rangeQuery))
            {
                MetricEntity mentity = new MetricEntity(entity);
                int offset = int.Parse(entity.RowKey);
                int index;
                for (int i = 0; i < mentity.Count; i++)
                {
                    index = offset + i;
                    if (index >= startIndex && index <= endIndex)
                    {
                        result.Add(mentity.Get(i));
                    }
                }
            }

            return result;
        }

        public List<MetricRecord> RetriveLastestDataRecord(MetricDataSet dataset, int count = 100)
        {
            int startIndex = dataset.RecordCount - count;
            int endIndex = dataset.RecordCount - 1;
            if (startIndex < 0)
            {
                startIndex = 0;
            }

            return RetriveDataRecord(dataset.Id, startIndex, endIndex);
        }
    }
}
