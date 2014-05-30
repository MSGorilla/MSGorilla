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
    public class DisplayReplyPagination
    {
        public string continuationToken { get; set; }
        public List<DisplayReply> reply { get; set; }

        public DisplayReplyPagination(ReplyPagiantion rpl)
        {
            continuationToken = rpl.continuationToken;
            var replylist = rpl.reply;

            reply = new List<DisplayReply>();
            foreach (var r in replylist)
            {
                reply.Add(new DisplayReply(r));
            }
        }
    }

    public class DisplayReply : Reply
    {
        public string DisplayName { get; private set; }

        public string PortraitUrl { get; private set; }

        public string Description { get; private set; }

        public DisplayReply(Reply rpl)
            : base(rpl.FromUser, rpl.ToUser, rpl.Message, rpl.PostTime, rpl.MessageUser, rpl.MessageID)
        {
            // use old id
            ReplyID = rpl.ReplyID;

            AccountManager accmng = new AccountManager();
            var userinfo = accmng.FindUser(FromUser);
            if (userinfo == null)
            {
                DisplayName = FromUser;
                PortraitUrl = "";
                Description = FromUser;
            }
            else
            {
                DisplayName = userinfo.DisplayName;
                PortraitUrl = userinfo.PortraitUrl;
                Description = userinfo.Description;
            }
        }
    }

    public class ReplyController : BaseController
    {
        ReplyManager _replyManager = new ReplyManager();

        [HttpGet]
        public List<DisplayReply> Replies()
        {
            var replylist = _replyManager.GetAllReply(whoami());
            var reply = new List<DisplayReply>();
            foreach (var r in replylist)
            {
                reply.Add(new DisplayReply(r));
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
            return new DisplayReply(_replyManager.PostReply(whoami(), to, message, DateTime.UtcNow, messageUser, messageID));
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
        public DisplayReply PostReply(ReplyModel reply)
        {
            return PostReply(reply.To, reply.Message, reply.MessageUser, reply.MessageID);
        }
    }
}
