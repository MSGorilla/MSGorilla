using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.WindowsAzure.Storage.Table;

using MSGorilla.Library.Azure;
using MSGorilla.Library.Exceptions;
using MSGorilla.Library.Models.AzureModels.Entity;

namespace MSGorilla.Library
{
    public class RichMsgManager
    {
        private AWCloudTable _richMsg;
        private static int _blockSize
        {
            get { return 32 * 1024; }
        }
        public RichMsgManager()
        {
            _richMsg = AzureFactory.GetTable(AzureFactory.MSGorillaTable.RichMessage);
        }

        DynamicTableEntity CreateRichMessageEntity(string userid, DateTime timestamp, string richMsg)
        {
            DynamicTableEntity entity = new DynamicTableEntity();
            entity.PartitionKey = string.Format("{0}_{1}", userid,
                Utils.ToAzureStorageDayBasedString(timestamp.ToUniversalTime()));
            entity.RowKey = Guid.NewGuid().ToString();

            entity.Properties["RichMsgID"] = new EntityProperty(string.Format("{0};{1}", entity.PartitionKey, entity.RowKey));

            byte[] stringRawByte = System.Text.UTF8Encoding.UTF8.GetBytes(richMsg);
            if (stringRawByte.Length > 31 * _blockSize)
            {
                throw new RichMessageTooLongException();
            }
            entity.Properties["RichMsgSize"] = new EntityProperty(stringRawByte.Length);
            
            int blockCount = (stringRawByte.Length - 1)/_blockSize + 1;
            for(int i = 0; i < blockCount; i++)
            {
                int size = 0;
                if(i == (blockCount - 1))
                {
                    size = stringRawByte.Length % _blockSize;
                }
                else{
                    size = _blockSize;
                }
                byte[] temp = new byte[size];
                Array.Copy(stringRawByte, _blockSize * i, temp, 0, size);
                entity.Properties["Block" + i] = new EntityProperty(temp);
            }

            return entity;
        }

        public string PostRichMessage(string userid, DateTime timestamp, string richMsg)
        {
            //RichMessageEntity entity = new RichMessageEntity(userid, timestamp, richMsg);

            DynamicTableEntity richMsgEntity = CreateRichMessageEntity(userid, timestamp, richMsg);
            TableOperation insertOperation = TableOperation.InsertOrReplace(richMsgEntity);
            _richMsg.Execute(insertOperation);
            return richMsgEntity.Properties["RichMsgID"].StringValue;
        }

        public string GetRichMessage(string richMsgID)
        {
            if (string.IsNullOrEmpty(richMsgID))
            {
                return null;
            }
            string[] split = richMsgID.Split(';');
            if(split.Length != 2)
            {
                return null;
            }

            TableOperation retrieveOperation = TableOperation.Retrieve<DynamicTableEntity>(split[0], split[1]);

            TableResult retrievedResult = _richMsg.ExecuteRetriveOperation(retrieveOperation);
            if (retrievedResult.Result != null)
            {
                DynamicTableEntity entity = (DynamicTableEntity)retrievedResult.Result;
                if(entity.Properties["RichMsgSize"].Int32Value == null)
                {
                    return "";
                }
                if (entity.Properties["RichMsgSize"].Int32Value <= 0)
                {
                    return "";
                }

                int size = entity.Properties["RichMsgSize"].Int32Value.Value;
                byte[] stringRawByte = new byte[size];

                int blockCount = (size - 1) / _blockSize + 1;
                for (int i = 0; i < blockCount; i++)
                {
                    int blockSize = 0;
                    if (i == (blockCount - 1))
                    {
                        blockSize = size % _blockSize;
                    }
                    else
                    {
                        blockSize = _blockSize;
                    }

                    Array.Copy(entity.Properties["Block" + i].BinaryValue, 0, stringRawByte, i * _blockSize, blockSize);
                }
                return System.Text.Encoding.UTF8.GetString(stringRawByte);
            }
            return null;
        }
    }
}
