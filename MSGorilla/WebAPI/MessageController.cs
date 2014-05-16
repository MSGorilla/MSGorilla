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
        public List<Tweet> UserLine()
        {
            return _messageManager.UserLine(whoami());
        }
        [HttpGet]
        public List<Tweet> UserLine(DateTime before, DateTime after)
        {
            return _messageManager.UserLine(whoami(), before, after);
        }
        [HttpGet]
        public List<Tweet> HomeLine()
        {
            return _messageManager.HomeLine(whoami());
        }
        [HttpGet]
        public List<Tweet> HomeLine(DateTime before, DateTime after)
        {
            return _messageManager.HomeLine(whoami(), before, after);
        }
        [HttpGet]
        public TweetDetail GetMessage(string userid, string tweetID)
        {
            return _messageManager.GetMessageDetail(userid, tweetID);
        }
        [HttpGet]
        public ActionResult PostMessage(string type, string message, string url = "")
        {
            _postManager.PostTweet(whoami(), type, message, DateTime.UtcNow, url);
            return new ActionResult();
        }

        [HttpGet]
        public ActionResult PostRetweet(string tweetUser, string tweetID)
        {
            _postManager.PostRetweet(whoami(), tweetUser, tweetID, DateTime.UtcNow);
            return new ActionResult();
        }

        [HttpGet]
        public ActionResult PostReply(string to, string message, string tweetUser, string tweetID)
        {
            _postManager.PostReply(whoami(), to, message, DateTime.UtcNow, tweetUser, tweetID);
            return new ActionResult();
        }
    }
}
