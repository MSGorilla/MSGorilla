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

namespace MSGorilla.WebApi
{
    public class ReplyController : BaseController
    {
        ReplyManager _replyManager = new ReplyManager();

        [HttpGet]
        public List<Reply> Replies()
        {
            return _replyManager.GetAllReply(whoami());
        }

        //[HttpGet]
        //public List<Reply> ReplyNotification()
        //{
        //    return _replyManager.GetReplyNotif(whoami());
        //}

        [HttpGet]
        public ReplyPagiantion GetMyReply(int count = 25, string token = null)
        {
            return _replyManager.GetReply(whoami(), count, Utils.String2Token(token));
        }

        [HttpGet, HttpPost]
        public Reply PostReply(string to, string message, string messageUser, string messageID)
        {
            return _replyManager.PostReply(whoami(), to, message, DateTime.UtcNow, messageUser, messageID);
            //return new ActionResult();
        }

        public class ReplyModel
        {
            public string To { get; set; }
            public string Message { get; set; }
            public string MessageUser { get; set; }
            public string MessageID { get; set; }
        }

        [HttpPost]
        public Reply PostReply(ReplyModel reply)
        {
            return PostReply(reply.To, reply.Message, reply.MessageUser, reply.MessageID);
        }
    }
}
