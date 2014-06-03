using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

using MSGorilla.Library;
using MSGorilla.Filters;
using MSGorilla.Library.Models;
using MSGorilla.Library.Models.ViewModels;
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
        public DisplayMessagePagination UserLine(int count = 25, string token = null)
        {
            return UserLine(whoami(), count, token);
        }
        [HttpGet]
        public DisplayMessagePagination UserLine(string userid, int count = 25, string token = null)
        {
            string me = whoami();
            if (string.IsNullOrEmpty(userid))
            {
                userid = me;
            }
            TableContinuationToken tok = Utils.String2Token(token);
            return new DisplayMessagePagination(_messageManager.UserLine(userid, count, tok));
        }

        [HttpGet]
        public List<Message> UserLine(string userid, DateTime end, DateTime start)
        {
            string me = whoami();
            return _messageManager.UserLine(userid, end, start);
        }

        [HttpGet]
        public DisplayMessagePagination HomeLine(int count = 25, string token = null)
        {
            return HomeLine(whoami(), count, token);
        }

        [HttpGet]
        public DisplayMessagePagination HomeLine(string userid, int count = 25, string token = null)
        {
            string me = whoami();
            if (string.IsNullOrEmpty(userid))
            {
                userid = me;
            }
            TableContinuationToken tok = Utils.String2Token(token);
            return new DisplayMessagePagination(_messageManager.HomeLine(userid, count, tok));
        }

        [HttpGet]
        public List<Message> HomeLine(string userid, DateTime end, DateTime start)
        {
            whoami();
            return _messageManager.HomeLine(userid, start, end);
        }

        [HttpGet]
        public DisplayMessagePagination OwnerLine(int count = 25, string token = null)
        {
            return OwnerLine(whoami(), count, token);
        }
        [HttpGet]
        public DisplayMessagePagination OwnerLine(string userid, int count = 25, string token = null)
        {
            string me = whoami();
            if (string.IsNullOrEmpty(userid))
            {
                userid = me;
            }
            TableContinuationToken tok = Utils.String2Token(token);
            return new DisplayMessagePagination(_messageManager.OwnerLine(userid, count, tok));
        }
        [HttpGet]
        public List<Message> OwnerLine(string userid, DateTime end, DateTime start)
        {
            whoami();
            return _messageManager.OwnerLine(whoami(), start, end);
        }

        [HttpGet]
        public DisplayMessagePagination AtLine(int count = 25, string token = null)
        {
            return AtLine(whoami(), count, token);
        }

        [HttpGet]
        public DisplayMessagePagination AtLine(string userid, int count = 25, string token = null)
        {
            string me = whoami();
            if (string.IsNullOrEmpty(userid))
            {
                userid = whoami();
            }

            TableContinuationToken tok = Utils.String2Token(token);
            return new DisplayMessagePagination(_messageManager.AtLine(userid, count, tok));
        }

        [HttpGet]
        public List<DisplayMessage> EventLine()
        {
            return EventLine("none");
        }

        [HttpGet]
        public List<DisplayMessage> EventLine(string eventID)
        {
            var msglist = _messageManager.EventLine(eventID);
            var msg = new List<DisplayMessage>();
            AccountManager accManager = new AccountManager();
            foreach (var m in msglist)
            {
                msg.Add(new DisplayMessage(m, accManager));
            }

            return msg;
        }

        [HttpGet]
        public List<Message> PublicSquareLine(DateTime start, DateTime end)
        {
            return _messageManager.PublicSquareLine(start, end);
        }

        [HttpGet]
        public DisplayMessagePagination PublicSquareLine(int count = 25, string token = null)
        {
            string me = whoami();
            TableContinuationToken tok = Utils.String2Token(token);
            return new DisplayMessagePagination(_messageManager.PublicSquareLine(count, tok));
        }

        [HttpGet]
        public DisplayMessagePagination TopicLine(string topicID, int count = 25, string token = null)
        {
            string me = whoami();
            return new DisplayMessagePagination(_messageManager.TopicLine(topicID, count, Utils.String2Token(token)));
        }

        [HttpGet]
        public MessageDetail GetMessage(string userid, string messageID)
        {
            return _messageManager.GetMessageDetail(userid, messageID);
        }

        [HttpGet]
        public List<DisplayReply> GetMessageReply(string msgID)
        {
            var replylist = _messageManager.GetAllReplies(msgID);
            var reply = new List<DisplayReply>();
            AccountManager accManager = new AccountManager();
            foreach (var r in replylist)
            {
                reply.Add(new DisplayReply(r, accManager));
            }

            return reply;
        }

        [HttpGet, HttpPost]
        public DisplayMessage PostMessage(string message,
                                    string schemaID = "none", 
                                    string eventID = "none", 
                                    string topicID = "none",
                                    [FromUri]string[] owner = null, 
                                    [FromUri]string[] atUser = null)
        {
            return new DisplayMessage(_messageManager.PostMessage(whoami(), eventID, schemaID, topicID, owner, atUser, message, DateTime.UtcNow), new AccountManager());
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
        public DisplayMessage PostMessage(MessageModel msg)
        {
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
            return PostMessage(msg.Message, msg.SchemaID, msg.EventID, msg.TopicID, msg.Owner, msg.AtUser);
            //return new ActionResult();
        }
    }

}
