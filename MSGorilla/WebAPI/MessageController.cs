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
        public MessagePagination UserLine(int count = 25, string token = null)
        {
            return UserLine(whoami(), count, token);
        }
        [HttpGet]
        public MessagePagination UserLine(string userid, int count = 25, string token = null)
        {
            string me = whoami();
            if (string.IsNullOrEmpty(userid))
            {
                userid = me;
            }
            TableContinuationToken tok = Utils.String2Token(token);
            return _messageManager.UserLine(userid, count, tok);
        }

        [HttpGet]
        public List<Message> UserLine(string userid, DateTime end, DateTime start)
        {
            string me = whoami();
            return _messageManager.UserLine(userid, end, start);
        }

        [HttpGet]
        public MessagePagination HomeLine(int count = 25, string token = null)
        {
            return HomeLine(whoami(), count, token);
        }

        [HttpGet]
        public MessagePagination HomeLine(string userid, int count = 25, string token = null)
        {
            string me = whoami();
            if (string.IsNullOrEmpty(userid))
            {
                userid = me;
            }
            TableContinuationToken tok = Utils.String2Token(token);
            return _messageManager.HomeLine(userid, count, tok);
        }

        [HttpGet]
        public List<Message> HomeLine(string userid, DateTime end, DateTime start)
        {
            whoami();
            return _messageManager.HomeLine(userid, start, end);
        }

        [HttpGet]
        public MessagePagination OwnerLine(int count = 25, string token = null)
        {
            return OwnerLine(whoami(), count, token);
        }
        [HttpGet]
        public MessagePagination OwnerLine(string userid, int count = 25, string token = null)
        {
            string me = whoami();
            if (string.IsNullOrEmpty(userid))
            {
                userid = me;
            }
            TableContinuationToken tok = Utils.String2Token(token);
            return _messageManager.OwnerLine(userid, count, tok);
        }
        [HttpGet]
        public List<Message> OwnerLine(string userid, DateTime end, DateTime start)
        {
            whoami();
            return _messageManager.OwnerLine(whoami(), start, end);
        }

        [HttpGet]
        public MessagePagination AtLine(int count = 25, string token = null)
        {
            return AtLine(whoami(), count, token);
        }

        [HttpGet]
        public MessagePagination AtLine(string userid, int count = 25, string token = null)
        {
            string me = whoami();
            if (string.IsNullOrEmpty(userid))
            {
                userid = whoami();
            }

            TableContinuationToken tok = Utils.String2Token(token);
            return _messageManager.AtLine(userid, count, tok);
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
        public MessagePagination PublicSquareLine(int count = 25, string token = null)
        {
            string me = whoami();
            TableContinuationToken tok = Utils.String2Token(token);
            return _messageManager.PublicSquareLine(count, tok);
        }
        [HttpGet]
        public MessagePagination TopicLine(string topicID, int count = 25, string token = null)
        {
            string me = whoami();
            return _messageManager.TopicLine(topicID, count, Utils.String2Token(token));
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
        public Message PostMessage(string message, 
                                    string schemaID = "none", 
                                    string eventID = "none",
                                    string topicID = "none",
                                    [FromUri]string[] owner = null, 
                                    [FromUri]string[] atUser = null)
        {
            return _messageManager.PostMessage(whoami(), eventID, schemaID, topicID, owner, atUser, message, DateTime.UtcNow);
            //return new ActionResult();
        }

        public class MessageModel
        {
            public string Message { get; set; }
            public string SchemaID { get; set; }
            public string EventID { get; set; }
            public string TopicID { get; set; }
            public string[] Owner { get; set; }
            public string[] AtUser { get; set; }
        };

        [HttpPost]
        public Message PostMessage(MessageModel msg){
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
            return _messageManager.PostMessage(whoami(), msg.EventID, msg.SchemaID, msg.TopicID, msg.Owner, msg.AtUser, msg.Message, DateTime.UtcNow);
            //return new ActionResult();
        }
    }
}
