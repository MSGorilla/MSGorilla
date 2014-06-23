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
        NotifManager _notifManager = new NotifManager();
        TopicManager _topicManager = new TopicManager();
        RichMsgManager _richMsgManager = new RichMsgManager();

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
        public DisplayMessagePagination HomeLine(int count = 25, string token = null, string filter = "")
        {
            DateTime start, end;
            end = DateTime.UtcNow;

            switch (filter)
            {
                case "latest24hours":
                    start = end.AddDays(-1);
                    break;
                case "latest7days":
                    start = end.AddDays(-7);
                    break;
                case "latest1month":
                    start = end.AddMonths(-1);
                    break;
                case "":
                case "all":
                default:
                    return HomeLine(whoami(), count, token);
                    break;
            }

            return HomeLine(whoami(), start, end, count, token);
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

            if (me.Equals(userid) && token == null)
            {
                _notifManager.clearHomelineNotifCount(me);
            }
            return new DisplayMessagePagination(_messageManager.HomeLine(userid, count, tok));
        }

        [HttpGet]
        public DisplayMessagePagination HomeLine(string userid, DateTime start, DateTime end, int count = 25, string token = null)
        {
            string me = whoami();
            if (string.IsNullOrEmpty(userid))
            {
                userid = me;
            }
            TableContinuationToken tok = Utils.String2Token(token);

            if (me.Equals(userid))
            {
                _notifManager.clearHomelineNotifCount(me);
            }
            return new DisplayMessagePagination(_messageManager.HomeLine(userid, start, end, count, tok));
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

            if (me.Equals(userid) && token == null)
            {
                _notifManager.clearOwnerlineNotifCount(me);
            }
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

            if (me.Equals(userid) && token == null)
            {
                _notifManager.clearAtlineNotifCount(me);
            }

            return new DisplayMessagePagination(_messageManager.AtLine(userid, count, tok));
        }

        [HttpGet]
        public List<DisplayMessage> EventLine()
        {
            whoami();
            return EventLine("none");
        }

        [HttpGet]
        public List<DisplayMessage> EventLine(string eventID)
        {
            whoami();
            var msglist = _messageManager.EventLine(eventID);
            var msg = new List<DisplayMessage>();
            AccountManager accManager = new AccountManager();
            AttachmentManager attManage = new AttachmentManager();
            foreach (var m in msglist)
            {
                msg.Add(new DisplayMessage(m, accManager, attManage));
            }

            return msg;
        }

        [HttpGet]
        public List<Message> PublicSquareLine(DateTime start, DateTime end)
        {
            whoami();
            return _messageManager.PublicSquareLine(start, end);
        }

        [HttpGet]
        public DisplayMessagePagination PublicSquareLine(int count = 25, string token = null)
        {
            whoami();
            TableContinuationToken tok = Utils.String2Token(token);
            return new DisplayMessagePagination(_messageManager.PublicSquareLine(count, tok));
        }

        [HttpGet]
        public DisplayMessagePagination TopicLine(string topic, int count = 25, string token = null)
        {
            string me = whoami();
            var t = _topicManager.FindTopicByName(topic);
            if (t == null)
            {
                return null;
            }

            TableContinuationToken tok = Utils.String2Token(token);
            if (tok == null)
            {
                _topicManager.clearUnreadMsgCountOfFavouriteTopic(me, t.Id);
            }
            return new DisplayMessagePagination(_messageManager.TopicLine(t.Id.ToString(), count, tok));
        }

        [HttpGet]
        public MessageDetail GetMessage(string userid, string messageID)
        {
            whoami();
            return _messageManager.GetMessageDetail(userid, messageID);
        }

        [HttpGet]
        public DisplayMessage GetDisplayMessage(string msgUser, string msgID)
        {
            whoami();
            return _messageManager.GetDisplayMessage(msgUser, msgID);
        }

        [HttpGet]
        public List<DisplayReply> GetMessageReply(string msgID)
        {
            whoami();
            var replylist = _messageManager.GetAllReplies(msgID);
            var reply = new List<DisplayReply>();
            AccountManager accManager = new AccountManager();
            foreach (var r in replylist)
            {
                reply.Add(new DisplayReply(r, accManager));
            }

            return reply;
        }

        [HttpGet]
        public string GetRichMessage(string richMsgID)
        {
            return _richMsgManager.GetRichMessage(richMsgID);
        }

        [HttpGet, HttpPost]
        public DisplayMessage PostMessage(string message,
                                    string schemaID = "none",
                                    string eventID = "none",
                                    [FromUri]string[] owner = null,
                                    [FromUri]string[] atUser = null,
                                    [FromUri]string[] topicName = null,
                                    string richMessage = null,
                                    [FromUri]string[] attachmentID = null)
        {
            return new DisplayMessage(_messageManager.PostMessage(whoami(), eventID, schemaID, owner, atUser, topicName, message, richMessage, attachmentID, DateTime.UtcNow), new AccountManager(), new AttachmentManager());
            //return new ActionResult();
        }

        public class MessageModel
        {
            public string Message { get; set; }
            public string SchemaID { get; set; }
            public string EventID { get; set; }
            public string[] TopicName { get; set; }
            public string[] Owner { get; set; }
            public string[] AtUser { get; set; }
            public string RichMessage { get; set; }
            public string[] AttachmentID { get; set; }
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
            return PostMessage(msg.Message, msg.SchemaID, msg.EventID, msg.Owner, msg.AtUser, msg.TopicName, msg.RichMessage, msg.AttachmentID);
            //return new ActionResult();
        }
    }

}
