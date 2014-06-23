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
        /// </summary>
        /// <param name="to">to user id</param>
        /// <param name="message">reply message content</param>
        /// <param name="messageUser">user id of whom posted the message</param>
        /// <param name="messageID">message id</param>
        /// <returns></returns>
        [HttpGet, HttpPost]
        public DisplayReply PostReply(string to, string message, string messageUser, string messageID)
        {
            return new DisplayReply(_replyManager.PostReply(whoami(), to, message, DateTime.UtcNow, messageUser, messageID), new AccountManager());
        }

        public class ReplyModel
        {
            public string To { get; set; }
            public string Message { get; set; }
            public string MessageUser { get; set; }
            public string MessageID { get; set; }
        }

        /// <summary>
        /// Post a reply to somebody. Same as the Get API
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
