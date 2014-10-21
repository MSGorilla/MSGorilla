using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace MSGorilla.Library.Azure
{
    public class AWDataMismatchException : Exception
    {
        public static string Serialize(ITableEntity entity)
        {
            if (entity == null)
            {
                return "null";
            }
            if (!(entity is DynamicTableEntity))
            {
                return JsonConvert.SerializeObject(entity);
            }

            DynamicTableEntity dentity = entity as DynamicTableEntity;
            StringBuilder sb = new StringBuilder("{");
            sb.Append(string.Format("\"PartitionKey\": \"{0}\",", entity.PartitionKey));
            sb.Append(string.Format("\"RowKey\": \"{0}\",", entity.RowKey));
            sb.Append(string.Format("\"Timestamp\": \"{0:u}\",", entity.Timestamp));

            foreach (var pair in dentity.Properties)
            {
                sb.Append('"');
                sb.Append(pair.Key);
                sb.Append("\":");
                sb.Append(JsonConvert.SerializeObject(pair.Value.PropertyAsObject));
                sb.Append(",");
            }
            sb.Remove(sb.Length - 1, 1);
            sb.Append("}");

            return sb.ToString();
        }
        static string Serialize(List<ITableEntity> entities)
        {
            if (entities == null)
            {
                return "null";
            }
            StringBuilder sb = new StringBuilder("[");
            foreach (ITableEntity entity in entities)
            {
                sb.Append(Serialize(entity));
                sb.Append(", ");
            }
            sb.Append("]");
            return sb.ToString();
        }
        static string ErrorMessage(ITableEntity azureEntity, ITableEntity wossEntity)
        {
            StringBuilder sb = new StringBuilder("Entity mismatch between azure and woss table:\r\n");
            if (azureEntity == null)
            {
                sb.Append("Azure entity is null\r\n");
            }
            else
            {
                sb.Append("Azure entity is " + azureEntity.GetType() + ":\r\n");
                sb.Append(Serialize(azureEntity));
                sb.Append("\r\n");
            }

            if (wossEntity == null)
            {
                sb.Append("Woss entity is null\r\n");
            }
            else
            {
                sb.Append("Azure entity is " + wossEntity.GetType() + ":\r\n");
                sb.Append(Serialize(azureEntity));
            }
            return sb.ToString();
        }

        static string ErrorMessage(List<ITableEntity> azureEntities, List<ITableEntity> wossEntities)
        {
            StringBuilder sb = new StringBuilder("Entity query result mismatch between azure and woss table:\r\n");
            if (azureEntities == null || azureEntities.Count == 0)
            {
                sb.Append("Count of Azure entity is 0\r\n");
            }
            else
            {
                sb.Append("Azure entity is " + azureEntities.GetType() + ":\r\n");
                sb.Append(Serialize(azureEntities));
                sb.Append("\r\n");
            }

            if (wossEntities == null)
            {
                sb.Append("Woss entity is null\r\n");
            }
            else
            {
                sb.Append("Woss entity is " + wossEntities.GetType() + ":\r\n");
                sb.Append(Serialize(wossEntities));
            }
            return sb.ToString();
        }
        public AWDataMismatchException(ITableEntity azureEntity, ITableEntity wossEntity)
            : base(ErrorMessage(azureEntity, wossEntity))
        {
        }

        public AWDataMismatchException(List<ITableEntity> azureEntities, List<ITableEntity> wossEntities)
            : base(ErrorMessage(azureEntities, wossEntities))
        {
        }
    }
}
