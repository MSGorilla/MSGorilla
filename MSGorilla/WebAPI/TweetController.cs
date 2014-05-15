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
    public class TweetController : BaseController
    {
        TweetManager _tweetManager = new TweetManager();
        PostManager _postManager = new PostManager();

        [HttpGet, BasicAuthAttribute]
        public List<Tweet> UserLine()
        {
            return _tweetManager.UserLine(whoami());
        }
        [HttpGet, BasicAuthAttribute]
        public List<Tweet> UserLine(DateTime before, DateTime after)
        {
            return _tweetManager.UserLine(whoami(), before, after);
        }
        [HttpGet, BasicAuthAttribute]
        public List<Tweet> HomeLine()
        {
            return _tweetManager.HomeLine(whoami());
        }
        [HttpGet, BasicAuthAttribute]
        public List<Tweet> HomeLine(DateTime before, DateTime after)
        {
            return _tweetManager.HomeLine(whoami(), before, after);
        }
        [HttpGet, BasicAuthAttribute]
        public TweetDetail GetTweet(string userid, string tweetID)
        {
            return _tweetManager.GetTweetDetail(userid, tweetID);
        }
        [HttpGet, BasicAuthAttribute]
        public ActionResult PostTweet(string type, string message, string url = "")
        {
            _postManager.PostTweet(whoami(), type, message, DateTime.UtcNow, url);
            return new ActionResult();
        }
        [HttpGet, BasicAuthAttribute]
        public ActionResult PostRetweet(string tweetUser, string tweetID)
        {
            _postManager.PostRetweet(whoami(), tweetUser, tweetID, DateTime.UtcNow);
            return new ActionResult();
        }
        [HttpGet, BasicAuthAttribute]
        public ActionResult PostReply(string to, string message, string tweetUser, string tweetID)
        {
            _postManager.PostReply(whoami(), to, message, DateTime.UtcNow, tweetUser, tweetID);
            return new ActionResult();
        }
    }
}
