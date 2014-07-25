using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MSGorilla.Filters;
using MSGorilla.Library;
using MSGorilla.Library.Models.SqlModels;
using MSGorilla.Utility;

namespace MSGorilla.Controllers
{
    public class TopicController : Controller
    {
        AccountManager _accManager = new AccountManager();
        TopicManager _topicManager = new TopicManager();

        [TokenAuthAttribute]
        public ActionResult Index(string topic, string[] group = null)
        {
            string myid = this.Session["userid"].ToString();
            UserProfile me = _accManager.FindUser(myid);
            ViewBag.Myid = me.Userid;
            ViewBag.Me = me;

            ViewBag.FeedCategory = "topicline";

            var t = _topicManager.FindTopicByName(topic, MembershipHelper.CheckJoinedGroup(group));
            if (t == null)
            {
                ViewBag.TopicId = -1;
                ViewBag.Topic = "";
                ViewBag.FeedId = "";
                ViewBag.IsLiked = false;
            }
            else
            {
                ViewBag.TopicId = t.Id;
                ViewBag.Topic = t.Name;
                ViewBag.FeedId = t.Name;
                ViewBag.IsLiked = _topicManager.IsFavouriteTopic(me.Userid, t.Id) ? 1 : 0;
            }

            return View();
        }

    }
}