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
        public static string UriPostMessage = "/Message/PostMessage?message={0}&schemaId={1}&eventID={2}";
        public static string UriPostReply = "/Message/PostReply?to={0}&message={1}&messageUser={2}&messageID={3}";
        public static string UriGetMessage = "/Message/GetMessage?userid={0}&messageId={1}";
        public static string UriHomeLine = "/Message/HomeLine?start={0}&end={1}";
        public static string UriEventLine = "/Message/EventLine?eventId={0}";

    }
}
