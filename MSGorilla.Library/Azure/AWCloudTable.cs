using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

using Newtonsoft.Json;

namespace MSGorilla.Library.Azure
{
    public class AWCloudTable
    {
        //Timestamp when finish the feature, only check entities after the timestamp
        static DateTime CheckTimestamp = DateTime.Parse("2014-10-17T08:00:00Z");

        //static DateTime PartitionTimestamp = DateTime.Now;
        public CloudTable AzureTable { get; private set; }
        public CloudTable WossTable { get; private set; }

        //Uncompleted
        public static bool Equal(DynamicTableEntity a, DynamicTableEntity b)
        {
            if (a == null && b == null)
            {
                return true;
            }
            if ((a == null && b != null) || (a != null && b == null))
            {
                return false;
            }

            if (!Equals(a.PartitionKey, b.PartitionKey) || !Equals(a.RowKey, b.RowKey))
            {
                return false;
            }

            if (a.Properties.Count != b.Properties.Count)
            {
                return false;
            }

            foreach (var key in a.Properties.Keys)
            {
                var value = a.Properties[key];
                if (!b.Properties.ContainsKey(key) || !value.Equals(b.Properties[key]))
                {
                    return false;
                }
            }

            return true;
        }

        public static bool Equal(ITableEntity a, ITableEntity b)
        {
            if (a == null || b == null)
            {
                if (a == null && b == null)
                {
                    return true;
                }
                return false;
            }

            if (a is DynamicTableEntity && b is DynamicTableEntity)
            {
                return Equal(a as DynamicTableEntity, b as DynamicTableEntity);
            }

            return a.Equals(b);
        }

        static bool Equal(List<ITableEntity> a, List<ITableEntity> b)
        {
            if (a == null || b == null)
            {
                if (a == null && b == null)
                {
                    return true;
                }
                return false;
            }

            if (a.Count != b.Count)
            {
                return false;
            }

            for (int i = 0; i < a.Count; i++)
            {
                if (!Equal(a[i], b[i]))
                {
                    return false;
                }
            }
            return true;
        }

        static void Check(ITableEntity azureEntity, ITableEntity wossEntity)
        {
            if (azureEntity == null)
            {
                return;
            }
            if (azureEntity.Timestamp < CheckTimestamp)
            {
                return;
            }
            if (!Equal(azureEntity, wossEntity))
            {
                throw new AWDataMismatchException(azureEntity, wossEntity);
            }
        }

        static void Check<TElement>(
            List<TElement> azureEntities, 
            List<TElement> wossEntities
            ) where TElement : ITableEntity, new()
        {
            List<ITableEntity> azure = null;
            List<ITableEntity> woss = null;
            if (azureEntities != null)
            {
                azure = new List<ITableEntity>();
                foreach (var entity in azureEntities)
                {
                    if (entity.Timestamp >= CheckTimestamp)
                    {
                        azure.Add(entity);
                    }
                }
            }
            if (wossEntities != null)
            {
                woss = new List<ITableEntity>();
                foreach (var entity in wossEntities)
                {
                    if (entity.Timestamp >= CheckTimestamp)
                    {
                        azure.Add(entity);
                    }
                }
            }
            if (!Equal(azure, woss))
            {
                throw new AWDataMismatchException(azure, woss);
            }
        }

        public AWCloudTable() { }

        public AWCloudTable(CloudTable azureTable, CloudTable wossTable = null)
        {
            this.AzureTable = azureTable;
            this.WossTable = wossTable;
        }

        //Main for Insert, InsertOrReplact, Update operation
        //Only check the same operation works fine on woss table
        public TableResult Execute(TableOperation operation, 
            TableRequestOptions requestOptions = null, 
            OperationContext operationContext = null)
        {
            TableResult ret = null;
            try
            {
                ret = this.AzureTable.Execute(operation, requestOptions, operationContext);
            }
            catch (Exception e)
            {
                Logger.Error(e, DateTime.Now, this.AzureTable.StorageUri.ToString(), "Execute", operationContext);
            }


            if (this.WossTable != null)
            {
                OperationContext opContext = new OperationContext();
                try
                {
                    TableResult result = this.WossTable.Execute(operation, requestOptions, opContext);
                }
                catch (Exception e)
                {
                    //opContext.LastResult.ServiceRequestID;
                    //opContext
                    Logger.Error(e, DateTime.Now, this.WossTable.StorageUri.ToString(), "Execute", opContext);
                }
            }
            
            return ret;
        }

        //Retrive single object
        //check whether the result from azure table and woss table are the same or not
        public TableResult ExecuteRetriveOperation(TableOperation operation, 
            TableRequestOptions requestOptions = null,
            OperationContext operationContext = null)
        {
            TableResult ret = this.AzureTable.Execute(operation, requestOptions, operationContext);

            if (ret.Result != null && this.WossTable != null)
            {
                OperationContext opContext = new OperationContext();
                try
                {
                    TableResult wossret = this.WossTable.Execute(operation, requestOptions, opContext);
                    Check(ret.Result as ITableEntity, wossret.Result as ITableEntity);
                }
                catch (Exception e)
                {
                    Logger.Error(e, DateTime.Now, this.WossTable.StorageUri.ToString(), "ExecuteRetriveOperation", opContext);
                }
            }

            return ret;
        }

        //check whether the result from azure table and woss table are the same or not
        public TableQuerySegment<TElement> ExecuteQuerySegmented<TElement>(
            TableQuery<TElement> query, 
            TableContinuationToken token, 
            TableRequestOptions requestOptions = null, 
            OperationContext operationContext = null
            ) where TElement : ITableEntity, new()
        {
            var ret = this.AzureTable.ExecuteQuerySegmented<TElement>(query, token, requestOptions, operationContext);

            List<TElement> azureEntities = ret.ToList<TElement>();
            if (azureEntities.Count > 0 && this.WossTable != null)
            {
                OperationContext opContext = new OperationContext();
                try
                {
                    var wossret = this.WossTable.ExecuteQuerySegmented<TElement>(query, token, requestOptions, opContext);
                    List<TElement> wossEntities = wossret.ToList<TElement>();
                    Check(azureEntities, wossEntities);
                }
                catch (Exception e)
                {
                    Logger.Error(e, DateTime.Now, this.WossTable.StorageUri.ToString(), "ExecuteQuerySegmented<TElement>", opContext);
                }
            }

            return ret;
        }

        //check whether the result from azure table and woss table are the same or not
        public IEnumerable<TElement> ExecuteQuery<TElement>(
            TableQuery<TElement> query, 
            TableRequestOptions requestOptions = null, 
            OperationContext operationContext = null) 
            where TElement : ITableEntity, new()
        {
            var ret = this.AzureTable.ExecuteQuery<TElement>(query, requestOptions, operationContext);

            List<TElement> azureEntities = ret.ToList<TElement>();
            if (azureEntities.Count > 0 && this.WossTable != null)
            {
                OperationContext opContext = new OperationContext();
                try
                {
                    var wossret = this.WossTable.ExecuteQuery<TElement>(query, requestOptions, opContext);
                    List<TElement> wossEntities = wossret.ToList<TElement>();
                    Check(azureEntities, wossEntities);
                }
                catch (Exception e)
                {
                    Logger.Error(e, DateTime.Now, this.WossTable.StorageUri.ToString(), "ExecuteQuery<TElement>", opContext);
                }
            }

            return ret;
        }
    }
}
