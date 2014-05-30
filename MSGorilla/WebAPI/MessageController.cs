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
    public class DisplayMessagePagination
    {
        public string continuationToken { get; set; }
        public List<DisplayMessage> message { get; set; }

        public DisplayMessagePagination(MessagePagination msg){
            continuationToken = msg.continuationToken;
            var msglist = msg.message;

            message = new List<DisplayMessage>();
            foreach (var m in msglist)
            {
                message.Add(new DisplayMessage(m));
            }
        }
    }

    public class DisplayMessage : Message
    {
        public string DisplayName { get; private set; }

        public string PortraitUrl { get; private set; }

        public string Description { get; private set; }

        public DisplayMessage(string userid, string message, DateTime timestamp, string eventID, string schemaID, string[] owner, string[] atUser)
            : base(userid, message, timestamp, eventID, schemaID, owner, atUser)
        {
            AccountManager accmng = new AccountManager();
            var userinfo = accmng.FindUser(User);
            if (userinfo == null)
            {
                DisplayName = userid;
                PortraitUrl = "";
                Description = "";
            }
            else
            {
                DisplayName = userinfo.DisplayName;
                PortraitUrl = userinfo.PortraitUrl;
                Description = userinfo.Description;
            }
        }

        public DisplayMessage(Message msg)
            : this(msg.User, msg.MessageContent, msg.PostTime, msg.EventID, msg.SchemaID, msg.Owner, msg.AtUser)
        {
        }
    }

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
        public DisplayMessagePagination PublicSquareLine(int count = 25, string token = null)
        {
            string me = whoami();
            TableContinuationToken tok = Utils.String2Token(token);
            return new DisplayMessagePagination(_messageManager.PublicSquareLine(count, tok));
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
        public DisplayMessage PostMessage(string message,
                                    string schemaID = "none",
                                    string eventID = "none",
                                    [FromUri]string[] owner = null,
                                    [FromUri]string[] atUser = null)
        {
            return new DisplayMessage(_messageManager.PostMessage(whoami(), eventID, schemaID, owner, atUser, message, DateTime.UtcNow));
            //return new ActionResult();
        }

        public class MessageModel
        {
            public string Message { get; set; }
            public string SchemaID { get; set; }
            public string EventID { get; set; }
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
            return new DisplayMessage(_messageManager.PostMessage(whoami(), msg.EventID, msg.SchemaID, msg.Owner, msg.AtUser, msg.Message, DateTime.UtcNow));
            //return new ActionResult();
        }
    }

}
