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
using MSGorilla.Library.Models.ViewModels;

namespace MSGorilla.WebApi
{
    public class ReplyController : BaseController
    {
        ReplyManager _replyManager = new ReplyManager();
        NotifManager _notifManager = new NotifManager();

        /// <summary>
        /// Return current user's all replies in a  list.
        /// Deprecated.
        /// 
        /// Example output:
        /// [
        ///     {
        ///         "FromUser": {
        ///             "Userid": "user1",
        ///             "DisplayName": "User1",
        ///             "PortraitUrl": null,
        ///             "Description": "user for test"
        ///         },
        ///         "ToUser": {
        ///             "Userid": "user1",
        ///             "DisplayName": "User1",
        ///             "PortraitUrl": null,
        ///             "Description": "user for test"
        ///         },
        ///         "Message": "asdf",
        ///         "PostTime": "2014-05-29T07:19:07.2070991Z",
        ///         "MessageUser": {
        ///             "Userid": "user1",
        ///             "DisplayName": "User1",
        ///             "PortraitUrl": null,
        ///             "Description": "user for test"
        ///         },
        ///         "MessageID": "252000953520305_afbf6c35-4229-435f-8ed8-045c172176a8",
        ///         "ReplyID": "252000952852792_5da7331e-322c-4638-a340-c47d55773649"
        ///     },
        ///     {
        ///         "FromUser": {
        ///             "Userid": "user2",
        ///             "DisplayName": "User22",
        ///             "PortraitUrl": null,
        ///             "Description": "user22"
        ///         },
        ///         "ToUser": {
        ///             "Userid": "user1",
        ///             "DisplayName": "User1",
        ///             "PortraitUrl": null,
        ///             "Description": "user for test"
        ///         },
        ///         "Message": "111",
        ///         "PostTime": "2014-05-29T08:04:14.8203109Z",
        ///         "MessageUser": {
        ///             "Userid": "user1",
        ///             "DisplayName": "User1",
        ///             "PortraitUrl": null,
        ///             "Description": "user for test"
        ///         },
        ///         "MessageID": "252000953520305_afbf6c35-4229-435f-8ed8-045c172176a8",
        ///         "ReplyID": "252000950145179_7a0cfdb0-1b2e-4ae8-bb8d-ed2e5e1a0217"
        ///     },
        ///     ......
        /// ]
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public List<DisplayReply> Replies()
        {
            var replylist = _replyManager.GetAllReply(whoami());
            var reply = new List<DisplayReply>();
            AccountManager accManager = new AccountManager();
            AttachmentManager attManager = new AttachmentManager();
            foreach (var r in replylist)
            {
                reply.Add(new DisplayReply(r, accManager, attManager));
            }
            return reply;
        }

        /// <summary>
        /// Return current user's reply list.
        /// 
        /// Example output:
        /// {
        ///     "continuationToken": "1!8!dXNlcjE-;1!72!MjUxOTk5ODEzMzAwNzc5XzNhMDNlZTM0LWRjZjEtNGRlOS1iYjM4LWFjZmRhMzAxYjMyNw--;;Primary;",
        ///     "reply": [
        ///         {
        ///             "type": "reply",
        ///             "MessageUser": "user1",
        ///             "MessageID": "251997929671837_bdb3414b-3232-48fc-be5b-b6c15d48902f",
        ///             "User": {
        ///                 "Userid": "user1",
        ///                 "DisplayName": "User1",
        ///                 "PortraitUrl": null,
        ///                 "Description": "user for test"
        ///             },
        ///             "ID": "251996885624787_a9ce32f7-5409-496f-a320-3f0283144540",
        ///             "EventID": "none",
        ///             "SchemaID": "none",
        ///             "Owner": null,
        ///             "AtUser": [
        ///                 "user2"
        ///             ],
        ///             "TopicName": null,
        ///             "MessageContent": "test new reply",
        ///             "RichMessageID": null,
        ///             "Attachment": null,
        ///             "PostTime": "2014-07-15T09:06:15.2123811Z",
        ///             "Importance": 2
        ///         },
        ///			......
        ///     ]
        /// }
        /// </summary>
        /// <param name="count">count of replies in the list </param>
        /// <param name="token">continuous token</param>
        /// <returns></returns>
        [HttpGet]
        public DisplayReplyPagination GetMyReply(int count = 25, string token = null, bool keepUnread = false)
        {
            string me = whoami();
            if (!keepUnread && token == null)
            {
                _notifManager.clearReplyNotifCount(me);
            }            
            return _replyManager.GetReply(me, count, Utils.String2Token(token));
        }

        /// <summary>
        /// Post a reply to somebody
        /// 
        /// Example output:
        /// {
        ///     "type": "reply",
        ///     "MessageUser": "user1",
        ///     "MessageID": "251997929671837_bdb3414b-3232-48fc-be5b-b6c15d48902f",
        ///     "User": {
        ///         "Userid": "user1",
        ///         "DisplayName": "User1",
        ///         "PortraitUrl": null,
        ///         "Description": "user for test"
        ///     },
        ///     "ID": "251996885624787_a9ce32f7-5409-496f-a320-3f0283144540",
        ///     "EventID": "none",
        ///     "SchemaID": "none",
        ///     "Owner": null,
        ///     "AtUser": [
        ///         "user2"
        ///     ],
        ///     "TopicName": null,
        ///     "MessageContent": "test new reply",
        ///     "RichMessageID": null,
        ///     "Attachment": null,
        ///     "PostTime": "2014-07-15T09:06:15.2123811Z",
        ///     "Importance": 2
        /// }
        /// </summary>
        /// <param name="to">to user id</param>
        /// <param name="message">reply content</param>
        /// <param name="messageUser">user id of whom posted the origin message</param>
        /// <param name="messageID">origin message id</param>
        /// <param name="richMessage">rich message</param>
        /// <param name="attachmentID">origin message id</param>
        /// <returns></returns>
        [HttpGet, HttpPost]
        public DisplayReply PostReply([FromUri]string[] to, string message, string messageUser, string messageID, string richMessage = null, [FromUri]string[] attachmentID = null)
        {
            return new DisplayReply(_replyManager.PostReply(whoami(), to, message, richMessage, attachmentID, DateTime.UtcNow, messageUser, messageID), new AccountManager(), new AttachmentManager());
        }

        public class ReplyModel
        {
            public string[] To { get; set; }
            public string Message { get; set; }
            public string richMessage { get; set; }
            public string[] attachmentID { get; set; }
            public string MessageUser { get; set; }
            public string MessageID { get; set; }
        }

        /// <summary>
        /// Post a reply to somebody. Same as the Get API
        /// Same as the Get API
        /// </summary>
        /// <param name="reply"></param>
        /// <returns></returns>
        [HttpPost]
        public DisplayReply PostReply(ReplyModel reply)
        {
            return PostReply(reply.To, reply.Message, reply.MessageUser, reply.MessageID, reply.richMessage, reply.attachmentID);
        }
    }
}
