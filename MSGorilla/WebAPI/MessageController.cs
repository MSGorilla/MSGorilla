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

        /// <summary>
        /// Return the messages in the current user's userline in a list
        /// </summary>
        /// <param name="userid">user id</param>
        /// <param name="count">count of message in the list</param>
        /// <param name="token">continuous token</param>
        /// <param name="filter">filter, can be "latest24hours", "latest7days", "latest1month" or "all"</param>
        /// <returns></returns>
        [HttpGet]
        public DisplayMessagePagination UserLine(string filter, string userid, int count = 25, string token = null)
        {
            DateTime start, end;
            if (GetFilterDateTime(filter, out start, out end))
            {
                return UserLine(userid, start, end, count, token);
            }
            else
            {
                return UserLine(userid, count, token);
            }
        }

        /// <summary>
        /// Return the messages in a user's userline in a list
        /// </summary>
        /// <param name="userid">user id</param>
        /// <param name="count">count of messages in the list</param>
        /// <param name="token">continuous token</param>
        /// <returns></returns>
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

        /// <summary>
        /// Deprecated. Return the messages in a user's userline in a list
        /// </summary>
        /// <param name="userid">user id</param>
        /// <param name="end">end timestamp</param>
        /// <param name="start">start timestamp</param>
        /// <param name="count">count of messages in the list</param>
        /// <param name="token">continuous token</param>
        /// <returns></returns>
        [HttpGet]
        public DisplayMessagePagination UserLine(string userid, DateTime start, DateTime end, int count = 25, string token = null)
        {
            string me = whoami();
            if (string.IsNullOrEmpty(userid))
            {
                userid = me;
            }
            TableContinuationToken tok = Utils.String2Token(token);
            return new DisplayMessagePagination(_messageManager.UserLine(userid, start, end, count, tok));
        }

        /// <summary>
        /// Return the messages in the current user's homeline in a list
        /// </summary>
        /// <param name="count">count of messages in the list</param>
        /// <param name="token">continuous token</param>
        /// <param name="filter">filter, can be "latest24hours", "latest7days", "latest1month" or "all"</param>
        /// <returns></returns>
        [HttpGet]
        public DisplayMessagePagination HomeLine(string filter, string userid, int count = 25, string token = null)
        {
            DateTime start, end;
            if (GetFilterDateTime(filter, out start, out end))
            {
                return HomeLine(userid, start, end, count, token);
            }
            else
            {
                return HomeLine(userid, count, token);
            }
        }

        /// <summary>
        /// Return the messages in a user's userline in a list
        /// </summary>
        /// <param name="userid">user id</param>
        /// <param name="count">count of messages in the list</param>
        /// <param name="token">continuous token</param>
        /// <returns></returns>
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

        /// <summary>
        /// Deprecated. Return the messages in a user's homeline in a list
        /// </summary>
        /// <param name="userid">user id</param>
        /// <param name="start">start timestamp</param>
        /// <param name="end">end timestamp</param>
        /// <param name="count">count of messages in the list</param>
        /// <param name="token">continuous token</param>
        /// <returns></returns>
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

        /// <summary>
        /// Return the messages in the current user's ownerline in a list
        /// </summary>
        /// <param name="userid">user id</param>
        /// <param name="count">count of messages int the list</param>
        /// <param name="token">continuous token</param>
        /// <param name="filter">filter, can be "latest24hours", "latest7days", "latest1month" or "all"</param>
        /// <returns></returns>
        [HttpGet]
        public DisplayMessagePagination OwnerLine(string filter, string userid, int count = 25, string token = null)
        {
            DateTime start, end;
            if (GetFilterDateTime(filter, out start, out end))
            {
                return OwnerLine(userid, start, end, count, token);
            }
            else
            {
                return OwnerLine(userid, count, token);
            }
        }

        /// <summary>
        /// Return the messages in a user's ownerline in a list
        /// </summary>
        /// <param name="userid">user id</param>
        /// <param name="count">count of messages int the list</param>
        /// <param name="token">continuous token</param>
        /// <returns></returns>
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

        /// <summary>
        /// Deprecated. Return the messages in a user's owner in a list
        /// </summary>
        /// <param name="userid">user id</param>
        /// <param name="start">start time</param>
        /// <param name="end">end time</param>
        /// <param name="count">count of messages int the list</param>
        /// <param name="token">continuous token</param>
        /// <returns></returns>
        [HttpGet]
        public DisplayMessagePagination OwnerLine(string userid, DateTime start, DateTime end, int count = 25, string token = null)
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
            return new DisplayMessagePagination(_messageManager.OwnerLine(userid, start, end, count, tok));
        }

        /// <summary>
        /// Return the messages in the current user's atline in a list
        /// </summary>
        /// <param name="userid">user id</param>
        /// <param name="count">count of messages int the list</param>
        /// <param name="token">continuous token</param>
        /// <param name="filter">filter, can be "latest24hours", "latest7days", "latest1month" or "all"</param>
        /// <returns></returns>
        [HttpGet]
        public DisplayMessagePagination AtLine(string filter, string userid, int count = 25, string token = null)
        {
            DateTime start, end;
            if (GetFilterDateTime(filter, out start, out end))
            {
                return AtLine(userid, start, end, count, token);
            }
            else
            {
                return AtLine(userid, count, token);
            }
        }

        /// <summary>
        /// Return the messages in a user's atline in a list
        /// </summary>
        /// <param name="userid">user id</param>
        /// <param name="count">count of messages int the list</param>
        /// <param name="token">continuous token</param>
        /// <returns></returns>
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

        /// <summary>
        /// Return the messages in a user's atline in a list
        /// </summary>
        /// <param name="userid">user id</param>
        /// <param name="start">start time</param>
        /// <param name="end">end time</param>
        /// <param name="count">count of messages int the list</param>
        /// <param name="token">continuous token</param>
        /// <returns></returns>
        [HttpGet]
        public DisplayMessagePagination AtLine(string userid, DateTime start, DateTime end, int count = 25, string token = null)
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

            return new DisplayMessagePagination(_messageManager.AtLine(userid, start, end, count, tok));
        }

        /// <summary>
        /// Return all messages do not have a eventid in a list.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public List<DisplayMessage> EventLine()
        {
            whoami();
            return EventLine("none");
        }

        /// <summary>
        /// Return all messages have the same eventid in a list
        /// </summary>
        /// <param name="eventID">event id</param>
        /// <returns></returns>
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

        /// <summary>
        /// Return a messages list order by post time desc
        /// </summary>
        /// <param name="count">count of messages in the list</param>
        /// <param name="token">continuous token</param>
        /// <param name="filter">filter, can be "latest24hours", "latest7days", "latest1month" or "all"</param>
        /// <returns></returns>
        [HttpGet]
        public DisplayMessagePagination PublicSquareLine(string filter, int count = 25, string token = null)
        {
            DateTime start, end;
            if (GetFilterDateTime(filter, out start, out end))
            {
                return PublicSquareLine(start, end, count, token);
            }
            else
            {
                return PublicSquareLine(count, token);
            }
        }

        /// <summary>
        /// Deprecated. Return all messages posted in a certain time
        /// </summary>
        /// <param name="start">start time</param>
        /// <param name="end">end time</param>
        /// <param name="count">count of messages in the list</param>
        /// <param name="token">continuous token</param>
        /// <returns></returns>
        [HttpGet]
        public DisplayMessagePagination PublicSquareLine(DateTime start, DateTime end, int count = 25, string token = null)
        {
            whoami();
            TableContinuationToken tok = Utils.String2Token(token);
            return new DisplayMessagePagination(_messageManager.PublicSquareLine(start, end, count, tok));
        }

        /// <summary>
        /// Return a messages list order by post time desc
        /// </summary>
        /// <param name="count">count of messages in the list</param>
        /// <param name="token">continuous token</param>
        /// <returns></returns>
        [HttpGet]
        public DisplayMessagePagination PublicSquareLine(int count = 25, string token = null)
        {
            whoami();
            TableContinuationToken tok = Utils.String2Token(token);
            return new DisplayMessagePagination(_messageManager.PublicSquareLine(count, tok));
        }

        /// <summary>
        /// Return a list of messages having the same topic
        /// </summary>
        /// <param name="topic">topic name</param>
        /// <param name="count">count of messages in the list</param>
        /// <param name="token">continuous token</param>
        /// <param name="filter">filter, can be "latest24hours", "latest7days", "latest1month" or "all"</param>
        /// <returns></returns>
        [HttpGet]
        public DisplayMessagePagination TopicLine(string filter, string topic, int count = 25, string token = null)
        {
            DateTime start, end;
            if (GetFilterDateTime(filter, out start, out end))
            {
                return TopicLine(topic, start, end, count, token);
            }
            else
            {
                return TopicLine(topic, count, token);
            }
        }

        /// <summary>
        /// Return a list of messages having the same topic
        /// </summary>
        /// <param name="topic">topic name</param>
        /// <param name="count">count of messages in the list</param>
        /// <param name="token">continuous token</param>
        /// <returns></returns>
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

        /// <summary>
        /// Return a list of messages having the same topic
        /// </summary>
        /// <param name="topic">topic name</param>
        /// <param name="start">start timestamp</param>
        /// <param name="end">end timestamp</param>
        /// <param name="count">count of messages in the list</param>
        /// <param name="token">continuous token</param>
        /// <returns></returns>
        [HttpGet]
        public DisplayMessagePagination TopicLine(string topic, DateTime start, DateTime end, int count = 25, string token = null)
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
            return new DisplayMessagePagination(_messageManager.TopicLine(t.Id.ToString(), start, end, count, tok));
        }

        /// <summary>
        /// Deprecated. Return the detail of a Message
        /// </summary>
        /// <param name="userid">user id of whom posted the message</param>
        /// <param name="messageID">message id</param>
        /// <returns></returns>
        [HttpGet]
        public MessageDetail GetMessage(string userid, string messageID)
        {
            whoami();
            return _messageManager.GetMessageDetail(userid, messageID);
        }

        /// <summary>
        /// Return the details of a message
        /// </summary>
        /// <param name="msgUser">user id of whom posted the message</param>
        /// <param name="msgID">message id</param>
        /// <returns></returns>
        [HttpGet]
        public DisplayMessage GetDisplayMessage(string msgUser, string msgID)
        {
            whoami();
            return _messageManager.GetDisplayMessage(msgUser, msgID);
        }

        /// <summary>
        /// Return all replies within the message in a list
        /// </summary>
        /// <param name="msgID">message id</param>
        /// <returns></returns>
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

        /// <summary>
        /// Return the rich messsage.
        /// </summary>
        /// <param name="richMsgID">rich message id</param>
        /// <returns></returns>
        [HttpGet]
        public string GetRichMessage(string richMsgID)
        {
            return _richMsgManager.GetRichMessage(richMsgID);
        }

        /// <summary>
        /// Post a new message
        /// </summary>
        /// <param name="message">message content</param>
        /// <param name="schemaID">schema id</param>
        /// <param name="eventID">event id</param>
        /// <param name="owner">user id of the owner. Can be a list.</param>
        /// <param name="atUser">user id of related users. Can be a list</param>
        /// <param name="topicName">topic name of related topic. Can be a list</param>
        /// <param name="richMessage">rich message. Up to 992 kb</param>
        /// <param name="attachmentID">attachment id related. Can be a list</param>
        /// <returns></returns>
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

        /// <summary>
        /// Post a new message. Same as the Get Api.
        /// </summary>
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

        private bool GetFilterDateTime(string filter, out DateTime start, out DateTime end)
        {
            start = end = DateTime.UtcNow;

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
                case null:
                case "":
                case "all":
                default:
                    return false;
            }

            return true;
        }
    }

}
