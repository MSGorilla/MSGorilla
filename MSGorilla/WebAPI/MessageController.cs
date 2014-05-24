using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

using MSGorilla.Library;
using MSGorilla.Filters;
using MSGorilla.Library.Models;
using MSGorilla.Library.Exceptions;
using MSGorilla.Library.Models.SqlModels;

using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage.Table;

namespace MSGorilla.WebApi
{
    public class MessageController : BaseController
    {
        MessageManager _messageManager = new MessageManager();

        [HttpGet]
        public List<Message> UserLine()
        {
            return _messageManager.UserLine(whoami(), DateTime.UtcNow.AddDays(-3), DateTime.UtcNow);
        }
        [HttpGet]
        public List<Message> UserLine(DateTime end, DateTime start)
        {
            return _messageManager.UserLine(whoami(), end, start);
        }

        public class Token
        {
            public string NextPartitionKey { get; set; }
            public string NextRowKey { get; set; }
            public string NextTableName { get; set; }
            //public int TargetLocation;
        }
        [HttpGet]
        public MessagePagination UserLine(string userid, int count, [FromUri]Token token = null)
        {
            TableContinuationToken tok = null;
            if (token != null)
            {
                tok = new TableContinuationToken();
                tok.NextPartitionKey = token.NextPartitionKey;
                tok.NextRowKey = token.NextRowKey;
                tok.NextTableName = token.NextTableName;
                //tok.TargetLocation = token.TargetLocation;
            }
            
            return _messageManager.HomeLine(userid, count, tok);
        }

        [HttpGet]
        public List<Message> UserLine(string userid)
        {
            if(string.IsNullOrEmpty(userid))
                userid = whoami();
            return _messageManager.UserLine(userid, DateTime.UtcNow.AddDays(-3), DateTime.UtcNow);
        }
        [HttpGet]
        public List<Message> UserLine(string userid, DateTime end, DateTime start)
        {
            string me = whoami();
            return _messageManager.UserLine(userid, end, start);
        }

        [HttpGet]
        public List<Message> HomeLine()
        {
            return _messageManager.HomeLine(whoami(), DateTime.UtcNow.AddDays(-3), DateTime.UtcNow);
        }
        [HttpGet]
        public List<Message> HomeLine(DateTime end, DateTime start)
        {
            return _messageManager.HomeLine(whoami(), start, end);
        }

        [HttpGet]
        public List<Message> EventLine()
        {
            return _messageManager.EventLine("none");
        }
        [HttpGet]
        public List<Message> EventLine(string eventID)
        {
            return _messageManager.EventLine(eventID);
        }
        [HttpGet]
        public List<Message> PublicSquareLine(DateTime start, DateTime end)
        {
            return _messageManager.PublicSquareLine(start, end);
        }
        [HttpGet]
        public List<Message> PublicSquareLine()
        {
            DateTime end = DateTime.UtcNow;
            DateTime start = end.AddDays(-1);
            return _messageManager.PublicSquareLine(start, end);
        }
        [HttpGet]
        public MessageDetail GetMessage(string userid, string messageID)
        {
            return _messageManager.GetMessageDetail(userid, messageID);
        }

        [HttpGet]
        public List<Reply> GetMessageReply(string msgID)
        {
            return _messageManager.GetAllReplies(msgID);
        }

        [HttpGet, HttpPost]
        public ActionResult PostMessage(string message, string schemaID = "none", string eventID = "none")
        {
            _messageManager.PostMessage(whoami(), eventID, schemaID, message, DateTime.UtcNow);
            return new ActionResult();
        }

        public class MessageModel
        {
            public string Message { get; set; }
            public string SchemaID { get; set; }
            public string EventID { get; set; }
        };

        [HttpPost]
        public ActionResult PostMessage(MessageModel msg){
            if (string.IsNullOrEmpty(msg.Message))
            {
                throw new MessageNullException();
            }
            if (string.IsNullOrEmpty(msg.SchemaID))
            {
                msg.SchemaID = "none";
            }
            if (string.IsNullOrEmpty(msg.EventID))
            {
                msg.EventID = "none";
            }
            _messageManager.PostMessage(whoami(), msg.EventID, msg.SchemaID, msg.Message, DateTime.UtcNow);
            return new ActionResult();
        }
    }
}
