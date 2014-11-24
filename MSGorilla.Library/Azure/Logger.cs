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

namespace MSGorilla.Library.Azure
{
    public static class Logger
    {

        private static System.IO.StreamWriter file;

        static Logger()
        {
            try
            {
                file = new System.IO.StreamWriter("msgorilla.log.txt");
            }
            catch
            {

            }
        }

        public static string RequestResult2XMLString(RequestResult reqResult)
        {
            using (TextWriter writer = new StringWriter())
            {
                using (XmlWriter xmlWriter = XmlWriter.Create(writer))
                {
                    reqResult.WriteXml(xmlWriter);
                }
                return writer.ToString();
            }
        }

        public static void Error(Exception e, 
            DateTimeOffset requestStartTime,
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
            exception.RequestStartTime = requestStartTime;
            exception.Timestamp = timestamp;
            exception.PartitionKey = partitionKey;
            exception.RowKey = rowKey;
            

            //ignore the table error in table: MetricDataSet
            if (table != null && table.Contains("MetricDataSet"))
            {
                return;
            }

            if (opCtx != null && opCtx.LastResult != null)
            {
                exception.HttpStatusCode = opCtx.LastResult.HttpStatusCode;
                exception.ServiceRequestID = opCtx.LastResult.ServiceRequestID;
                exception.LastResultXml = RequestResult2XMLString(opCtx.LastResult);
            }

            if (opCtx.RequestResults != null && opCtx.RequestResults.Count > 0)
            {
                StringBuilder sb = new StringBuilder();
                foreach (var reqResult in opCtx.RequestResults)
                {
                    sb.Append(RequestResult2XMLString(reqResult));
                    sb.Append("\r\n==============================\r\n");
                }

                exception.RequestResults = sb.ToString();
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
    }
}
