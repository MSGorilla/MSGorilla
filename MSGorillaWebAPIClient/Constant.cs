using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSGorilla.WebAPI.Client
{
    public static class Constant
    {
        public static string UriRoot = "http://msgorilla.cloudapp.net/api";
        public static string UriPostMessage = "/Message/PostMessage";
        public static string UriPostReply = "/Message/PostReply";
        public static string UriGetMessage = "/Message/GetMessage?userid={0}&messageId={1}";
        public static string UriHomeLine = "/Message/HomeLine?start={0}&end={1}";
        public static string UriEventLine = "/Message/EventLine?eventId={0}";

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
