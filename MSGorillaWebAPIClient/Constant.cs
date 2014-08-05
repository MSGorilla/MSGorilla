using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSGorilla.WebAPI.Client
{
    public static class Constant
    {
        public static string UriRoot = "https://msgorilla.cloudapp.net/api";
        public static string UriPostMessage = "/Message/PostMessage";
        public static string UriPostReply = "/Reply/PostReply";
        public static string UriGetMessage = "/Message/GetMessage?userid={0}&messageId={1}";
        public static string UriGetRawMessage = "/Message/GetRawMessage?messageId={1}";
        public static string UriHomeLine = "/Message/HomeLine?userid={0}&group={1}&count={2}&token={3}";
        public static string UriEventLine = "/Message/EventLine?eventId={0}";
        public static string UriUploadAttachment = "/attachment/upload";
        public static string UriNotificationCount = "/Account/GetNotificationCount?userid={0}";
        public static string UriGetMyReply = "/Reply/GetMyReply?count={0}&token={1}";

        public static string UriCreateMetricDataset = "/MetricChart/CreateDataSet?instance={0}&counter={1}&category={2}&group={3}&description={4}";
        public static string UriQueryMetricDataset = "/MetricChart/GetDataSet?instance={0}&counter={1}&category={2}&group={3}";
        public static string UriInserRecord = "/MetricChart/InsertRecord?id={0}&key={1}&value={2}";
        public static string UriUpdateCategoryMessage = "/Category/UpdateCategoryMessage";
    }

    public class MessageModel
    {
        public string Message { get; set; }
        public string SchemaID { get; set; }
        public string EventID { get; set; }
    }
    public class ReplyModel
    {
        public string To { get; set; }
        public string Message { get; set; }
        public string MessageUser { get; set; }
        public string MessageID { get; set; }
    }
}
