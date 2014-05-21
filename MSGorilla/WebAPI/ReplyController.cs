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

        [HttpGet]
        public List<Reply> ReplyNotification()
        {
            return _replyManager.GetReplyNotif(whoami());
        }

        [HttpGet, HttpPost]
        public ActionResult PostReply(string to, string message, string messageUser, string messageID)
        {
            _replyManager.PostReply(whoami(), to, message, DateTime.UtcNow, messageUser, messageID);
            return new ActionResult();
        }
    }
}
