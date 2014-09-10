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
        GroupManager _groupManag = new GroupManager();

        [TokenAuthAttribute]
        public ActionResult Index(string topic, string group = null)
        {
            string myid = this.Session["userid"].ToString();
            UserProfile me = _accManager.FindUser(myid);
            ViewBag.Myid = me.Userid;
            ViewBag.Me = me;

            ViewBag.FeedCategory = "topicline";

            Topic t = null;
            if (string.IsNullOrEmpty(group))
            {
                t = _topicManager.FindTopicByName(topic, MembershipHelper.JoinedGroup(myid));
            }
            else
            {
                t = _topicManager.FindTopicByName(topic, group);
            }
            if (t == null)
            {
                ViewBag.TopicId = -1;
                ViewBag.Topic = "";
                ViewBag.FeedId = "";
                ViewBag.IsLiked = false;
                ViewBag.Group = "";
                ViewBag.GroupName = "";
                ViewBag.PerfChartName = "";
            }
            else
            {
                var g = _groupManag.GetGroupByID(t.GroupID);
                ViewBag.TopicId = t.Id;
                ViewBag.Topic = t.Name;
                ViewBag.FeedId = t.Name;
                ViewBag.IsLiked = _topicManager.IsFavouriteTopic(me.Userid, t.Id) ? 1 : 0;
                ViewBag.Group = group;
                ViewBag.GroupName = g.DisplayName;
                ViewBag.PerfChartName = group + "_" + t.Name;
                //ViewBag.PerfChartName = "perf_chart_test";

            }

            return View();
        }

    }
}