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
    public class MessageController : BaseController
    {
        MessageManager _messageManager = new MessageManager();
        PostManager _postManager = new PostManager();

        [HttpGet]
        public List<Message> UserLine()
        {
            return _messageManager.UserLine(whoami());
        }
        [HttpGet]
        public List<Message> UserLine(DateTime before, DateTime after)
        {
            return _messageManager.UserLine(whoami(), before, after);
        }

        [HttpGet]
        public List<Message> UserLine(string userid)
        {
            string me = whoami();
            return _messageManager.UserLine(userid);
        }
        [HttpGet]
        public List<Message> UserLine(string userid, DateTime before, DateTime after)
        {
            string me = whoami();
            return _messageManager.UserLine(userid, before, after);
        }

        [HttpGet]
        public List<Message> HomeLine()
        {
            return _messageManager.HomeLine(whoami());
        }
        [HttpGet]
        public List<Message> HomeLine(DateTime before, DateTime after)
        {
            return _messageManager.HomeLine(whoami(), before, after);
        }
        [HttpGet]
        public MessageDetail GetMessage(string userid, string tweetID)
        {
            return _messageManager.GetMessageDetail(userid, tweetID);
        }
        [HttpGet]
        public ActionResult PostMessage(string eventID, string schemaID, string message)
        {
            _postManager.PostMessage(whoami(), eventID, schemaID, message, DateTime.UtcNow);
            return new ActionResult();
        }

        //[HttpGet]
        //public ActionResult PostRetweet(string tweetUser, string tweetID)
        //{
        //    _postManager.PostRetweet(whoami(), tweetUser, tweetID, DateTime.UtcNow);
        //    return new ActionResult();
        //}

        [HttpGet]
        public ActionResult PostReply(string to, string message, string messageUser, string messageID)
        {
            _postManager.PostReply(whoami(), to, message, DateTime.UtcNow, messageUser, messageID);
            return new ActionResult();
        }
    }
}
