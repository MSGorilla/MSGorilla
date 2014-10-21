using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.IO;

using Newtonsoft.Json;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

using MSGorilla.Library;
using MSGorilla.Library.Models.SqlModels;
using MSGorilla.Library.Models;
using MSGorilla.Library.Models.AzureModels.Entity;
using MSGorilla.Library.Models.SqlModels;

namespace MSGorilla.Library.Azure
{
    //public class AWExceptionEntity : TableEntity
    //{
    //    public DateTime Time { get; set; }
    //    public string Table { get; set; }
    //    public string Function { get; set; }
    //    public string Client { get; set; }
    //    public string ExceptionType { get; set; }
    //    public string ExceptionMessage { get; set; }
    //    public string StackTrace { get; set; }

    //    public AWExceptionEntity(Exception e, DateTime timestamp, string table, string function, string client)
    //    {
    //        this.PartitionKey = string.Format("{0:yyyyMMdd}", timestamp);
    //        this.RowKey = string.Format("{0:yyyy-MM-dd hh:mm:ss ffffff}", timestamp);
    //        this.Time = timestamp;
    //        this.Table = table;
    //        this.Function = function;
    //        this.Client = client;
    //        this.ExceptionType = e.GetType().ToString();
    //        this.ExceptionMessage = e.Message;
    //        this.StackTrace = e.StackTrace;
    //    }
    //}
    public static class Logger
    {
        //private static CloudTable exceptionTable;
        //private static CloudTable _richMsg;
        //private static CloudTable _userline;
        //private static CloudTable _homeline;
        //private static CloudTable _publicline;
        //private static AccountManager _accManager;
        //private static TopicManager _topicManager;

        private static System.IO.StreamWriter file;

        static Logger()
        {
            //var client = AzureFactory.AzureStorageAccount.CreateCloudTableClient();
            //exceptionTable = client.GetTableReference("AWTableExceptions");
            //exceptionTable.CreateIfNotExists();

            //_richMsg = AzureFactory.GetTable(AzureFactory.MSGorillaTable.RichMessage).AzureTable;
            //_userline = AzureFactory.GetTable(AzureFactory.MSGorillaTable.Userline).AzureTable;
            //_homeline = AzureFactory.GetTable(AzureFactory.MSGorillaTable.Homeline).AzureTable;
            //_publicline = AzureFactory.GetTable(AzureFactory.MSGorillaTable.PublicSquareLine).AzureTable;
            //_accManager = new AccountManager();
            try
            {
                file = new System.IO.StreamWriter("msgorilla.log.txt");
            }
            catch (Exception e)
            {

            }
        }

        public static void Error(Exception e, 
            DateTime timestamp, 
            string table, 
            string function = "unknown",
            OperationContext opCtx = null,
            string partitionKey = null,
            string rowKey = null)
        {
            AWException exception = new AWException();
            exception.ExceptionMsg = e.Message;
            exception.ExceptionType = e.GetType().ToString();
            exception.StackTrace = e.StackTrace;
            exception.Function = function;
            exception.Table = table;
            exception.Timestamp = timestamp;
            exception.PartitionKey = partitionKey;
            exception.RowKey = rowKey;

            if (opCtx != null && opCtx.LastResult != null)
            {
                exception.HttpStatusCode = opCtx.LastResult.HttpStatusCode;
                exception.ServiceRequestID = opCtx.LastResult.ServiceRequestID;

                using (TextWriter writer = new StringWriter())
                {
                    using (XmlWriter xmlWriter = XmlWriter.Create(writer))
                    {
                        opCtx.LastResult.WriteXml(xmlWriter);
                    }
                    exception.LastResultXml = writer.ToString();
                }
            }

            //Create a new thread to insert log and publish event message
            Thread thread = new Thread(new ParameterizedThreadStart(LogError));
            thread.Start(exception);

            //Console.WriteLine(JsonConvert.SerializeObject(exception));
        }


        //WossTableLogger
        private static void LogError(Object exception)
        {
            AWException excep = exception as AWException;
            if (file != null)
            {
                file.WriteLine(JsonConvert.SerializeObject(excep));
                file.Flush();
            }

            using (var ctx = new MSGorillaEntities())
            {
                ctx.AWExceptions.Add(excep);
                ctx.SaveChanges();
            }
        }

        //private const string _loggerMSGorillaUserid = "WossTableLogger";
        //private const string _groupID = "msgorilladev";

        //private static void PostExceptionEvent(AWException e)
        //{
        //    string message = string.Format(
        //        "#Woss Table Exception# @t-yig\r\n{0:yyyy-MM-dd HH:mm:ss}\r\n{1} occour when call {2} on table {3}",
        //        e.Timestamp,
        //        e.ExceptionType,
        //        e.Function,
        //        e.Table);
        //    string richMessage = string.Format(
        //        "{0}\r\n===============Stack Trace===============\r\n{1}",
        //        e.ExceptionMsg,
        //        e.StackTrace);
        //    string richMsgID = PostRichMessage(_loggerMSGorillaUserid, e.Timestamp.DateTime, richMessage);

        //    Message msg = new Message(
        //        _loggerMSGorillaUserid, 
        //        _groupID, 
        //        message, 
        //        e.Timestamp.DateTime,
        //        null, null,
        //        new string[] { "t-yig" }, new string[] { "t-yig" },
        //        new string[] { "Woss Table Exception" }, 
        //        richMsgID, null, 0);

        //    //insert into Userline
        //    TableOperation insertOperation = TableOperation.InsertOrReplace(new UserLineEntity(msg));
        //    _userline.Execute(insertOperation);

        //    UserProfile user = _accManager.FindUser(_loggerMSGorillaUserid);
        //    user.MessageCount++;
        //    _accManager.UpdateUser(user);

        //    //insert into poster's homeline
        //    insertOperation = TableOperation.InsertOrReplace(new HomeLineEntity(msg.User, msg));
        //    _homeline.Execute(insertOperation);

        //    //insert into publicline
        //    insertOperation = TableOperation.InsertOrReplace(new PublicSquareLineEntity(msg));
        //    _publicline.Execute(insertOperation);

        //    //Notify t-yig
        //    insertOperation = TableOperation.InsertOrReplace(new AtLineEntity("t-yig", msg));
        //    UserProfile tyig = _accManager.FindUser("t-yig");

        //    //insert into topicline
        //    //Topic topic = _topicManager.FindTopicByName("Woss Table Exception");
            

        //    //
        //}

        //#region Rich Message
        //static DynamicTableEntity CreateRichMessageEntity(string userid, DateTime timestamp, string richMsg)
        //{
        //    DynamicTableEntity entity = new DynamicTableEntity();
        //    entity.PartitionKey = string.Format("{0}_{1}", userid,
        //        Utils.ToAzureStorageDayBasedString(timestamp.ToUniversalTime()));
        //    entity.RowKey = Guid.NewGuid().ToString();

        //    entity.Properties["RichMsgID"] = new EntityProperty(string.Format("{0};{1}", entity.PartitionKey, entity.RowKey));

        //    byte[] stringRawByte = System.Text.UTF8Encoding.UTF8.GetBytes(richMsg);
        //    if (stringRawByte.Length > 31 * RichMsgManager.BlockSize)
        //    {
        //        richMsg = richMsg.Substring(0, 31 * RichMsgManager.BlockSize - 10) + "......";
        //    }
        //    entity.Properties["RichMsgSize"] = new EntityProperty(stringRawByte.Length);

        //    int blockCount = (stringRawByte.Length - 1) / RichMsgManager.BlockSize + 1;
        //    for (int i = 0; i < blockCount; i++)
        //    {
        //        int size = 0;
        //        if (i == (blockCount - 1))
        //        {
        //            size = stringRawByte.Length % RichMsgManager.BlockSize;
        //        }
        //        else
        //        {
        //            size = RichMsgManager.BlockSize;
        //        }
        //        byte[] temp = new byte[size];
        //        Array.Copy(stringRawByte, RichMsgManager.BlockSize * i, temp, 0, size);
        //        entity.Properties["Block" + i] = new EntityProperty(temp);
        //    }

        //    return entity;
        //}

        //static string PostRichMessage(string userid, DateTime timestamp, string richMsg)
        //{
        //    DynamicTableEntity richMsgEntity = CreateRichMessageEntity(userid, timestamp, richMsg);
        //    TableOperation insertOperation = TableOperation.InsertOrReplace(richMsgEntity);
        //    _richMsg.Execute(insertOperation);
        //    return richMsgEntity.Properties["RichMsgID"].StringValue;
        //}
        //#endregion
    }
}
