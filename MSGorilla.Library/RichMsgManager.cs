using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.WindowsAzure.Storage.Table;

using MSGorilla.Library.Azure;
using MSGorilla.Library.Models.AzureModels.Entity;
namespace MSGorilla.Library
{
    public class RichMsgManager
    {
        private CloudTable _richMsg;
        public RichMsgManager()
        {
            _richMsg = AzureFactory.GetTable(AzureFactory.MSGorillaTable.RichMessage);
        }

        public RichMessageEntity PostRichMessage(string userid, DateTime timestamp, string richMsg)
        {
            RichMessageEntity entity = new RichMessageEntity(userid, timestamp, richMsg);
            TableOperation insertOperation = TableOperation.InsertOrReplace(entity);
            _richMsg.Execute(insertOperation);
            return entity;
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

            TableOperation retrieveOperation = TableOperation.Retrieve<RichMessageEntity>(split[0], split[1]);

            TableResult retrievedResult = _richMsg.Execute(retrieveOperation);
            if (retrievedResult.Result != null)
            {
                RichMessageEntity entity = (RichMessageEntity)retrievedResult.Result;
                return entity.RichMessage;
            }
            return null;
        }
    }
}
