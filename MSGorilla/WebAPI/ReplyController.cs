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

        [HttpGet]
        public DisplayReplyPagination GetMyReply(int count = 25, string token = null)
        {
            return new DisplayReplyPagination(_replyManager.GetReply(whoami(), count, Utils.String2Token(token)));
        }

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

        [HttpPost]
        public DisplayReply PostReply(ReplyModel reply)
        {
            return PostReply(reply.To, reply.Message, reply.MessageUser, reply.MessageID);
        }
    }
}
