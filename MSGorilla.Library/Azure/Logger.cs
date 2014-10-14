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
    public class AWExceptionEntity : TableEntity
    {
        public DateTime Time { get; set; }
        public string Table { get; set; }
        public string Function { get; set; }
        public string Client { get; set; }
        public string ExceptionType { get; set; }
        public string ExceptionMessage { get; set; }
        public string StackTrace { get; set; }

        public AWExceptionEntity(Exception e, DateTime timestamp, string table, string function, string client)
        {
            this.PartitionKey = string.Format("{0:yyyyMMdd}", timestamp);
            this.RowKey = string.Format("{0:yyyy-MM-dd hh:mm:ss ffffff}", timestamp);
            this.Time = timestamp;
            this.Table = table;
            this.Function = function;
            this.Client = client;
            this.ExceptionType = e.GetType().ToString();
            this.ExceptionMessage = e.Message;
            this.StackTrace = e.StackTrace;
        }
    }
    public static class Logger
    {
        private static CloudTable exceptionTable;
        static Logger()
        {
            var client = AzureFactory.AzureStorageAccount.CreateCloudTableClient();
            exceptionTable = client.GetTableReference("awtableexceptions");
            exceptionTable.CreateIfNotExists();
        }

        public static void Error(Exception e, 
            DateTime timestamp, 
            string table, 
            string function = "unknown",
            string client = null)
        {
            AWExceptionEntity exception = new AWExceptionEntity(e, timestamp, table, function, client);
            TableOperation insertOperation = TableOperation.InsertOrMerge(exception);
            exceptionTable.Execute(insertOperation);

            Console.WriteLine(JsonConvert.SerializeObject(exception));
        }
    }
}
