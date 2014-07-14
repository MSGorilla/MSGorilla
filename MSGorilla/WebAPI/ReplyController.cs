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
            foreach (var r in replylist)
            {
                reply.Add(new DisplayReply(r, accManager));
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
        ///             "FromUser": {
        ///                 "Userid": "user1",
        ///                 "DisplayName": "User1",
        ///                 "PortraitUrl": null,
        ///                 "Description": "user for test"
        ///             },
        ///             "ToUser": {
        ///                 "Userid": "user1",
        ///                 "DisplayName": "User1",
        ///                 "PortraitUrl": null,
        ///                 "Description": "user for test"
        ///             },
        ///             "Message": "good",
        ///             "PostTime": "2014-06-13T03:04:12.1416528Z",
        ///             "MessageUser": {
        ///                 "Userid": "user1",
        ///                 "DisplayName": "User1",
        ///                 "PortraitUrl": null,
        ///                 "Description": "user for test"
        ///             },
        ///             "MessageID": "251999672156363_6e66aa08-6382-4ee4-bc1a-dea53dcf4e69",
        ///             "ReplyID": "251999672147858_3a8e1a0a-e2b6-46aa-be3f-644bd2f372df"
        ///         },
        ///         {
        ///             "FromUser": {
        ///                 "Userid": "fdy",
        ///                 "DisplayName": "fdy",
        ///                 "PortraitUrl": "/Content/Images/default_avatar.jpg",
        ///                 "Description": "fdy"
        ///             },
        ///             "ToUser": {
        ///                 "Userid": "user1",
        ///                 "DisplayName": "User1",
        ///                 "PortraitUrl": null,
        ///                 "Description": "user for test"
        ///             },
        ///             "Message": "@us #top3 @fdy http://google.com/cn?aaa=baidu.com",
        ///             "PostTime": "2014-06-12T02:45:47.890376Z",
        ///             "MessageUser": {
        ///                 "Userid": "user1",
        ///                 "DisplayName": "User1",
        ///                 "PortraitUrl": null,
        ///                 "Description": "user for test"
        ///             },
        ///             "MessageID": "252000855072961_987ef186-1fd0-48f0-a344-b750cd6f29ab",
        ///             "ReplyID": "251999759652109_0c7bb049-13dd-40ad-9a80-b9eba809f2b9"
        ///         },
        ///			......
        ///     ]
        /// }
        /// </summary>
        /// <param name="count">count of replies in the list </param>
        /// <param name="token">continuous token</param>
        /// <returns></returns>
        [HttpGet]
        public DisplayReplyPagination GetMyReply(int count = 25, string token = null)
        {
            string me = whoami();
            if(token == null){
                _notifManager.clearReplyNotifCount(me);
            }            
            return new DisplayReplyPagination(_replyManager.GetReply(me, count, Utils.String2Token(token)));
        }

        /// <summary>
        /// Post a reply to somebody
        /// 
        /// Example output:
        /// {
        ///     "FromUser": {
        ///         "Userid": "user2",
        ///         "DisplayName": "User22",
        ///         "PortraitUrl": null,
        ///         "Description": "user22"
        ///     },
        ///     "ToUser": {
        ///         "Userid": "user1",
        ///         "DisplayName": "User1",
        ///         "PortraitUrl": null,
        ///         "Description": "user for test"
        ///     },
        ///     "Message": "a new reply",
        ///     "PostTime": "2014-06-24T06:02:38.5769194Z",
        ///     "MessageUser": {
        ///         "Userid": "user1",
        ///         "DisplayName": "User1",
        ///         "PortraitUrl": null,
        ///         "Description": "user for test"
        ///     },
        ///     "MessageID": "251998712005492_f4a6056b-0ac5-4b46-9d69-c8d5aa1a355c",
        ///     "ReplyID": "251998711041423_42b1f6b8-8861-4924-be0f-bd82f1d776da"
        /// }
        /// </summary>
        /// <param name="to">to user id</param>
        /// <param name="message">reply content</param>
        /// <param name="messageUser">user id of whom posted the origin message</param>
        /// <param name="messageID">origin message id</param>
        /// <returns></returns>
        [HttpGet, HttpPost]
        public DisplayReply PostReply([FromUri]string[] to, string message, string messageUser, string messageID)
        {
            return new DisplayReply(_replyManager.PostReply(whoami(), to, message, DateTime.UtcNow, messageUser, messageID), new AccountManager());
        }

        public class ReplyModel
        {
            public string[] To { get; set; }
            public string Message { get; set; }
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
            return PostReply(reply.To, reply.Message, reply.MessageUser, reply.MessageID);
        }
    }
}
