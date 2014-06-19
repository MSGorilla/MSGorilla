using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MSGorilla.Filters;
using MSGorilla.Library;
using MSGorilla.Library.Models.SqlModels;

namespace MSGorilla.Controllers
{
    [CryptographicExceptionHandlerAttribute]
    public class TopicController : Controller
    {
        AccountManager _accManager = new AccountManager();
        TopicManager _topicManager = new TopicManager();

        [TokenAuthAttribute]
        public ActionResult Index(string topic)
        {
            string myid = this.Session["userid"].ToString();
            UserProfile me = _accManager.FindUser(myid);
            ViewBag.Myid = me.Userid;
            ViewBag.Me = me;

            ViewBag.FeedCategory = "topicline";

            var t = _topicManager.FindTopicByName(topic);
            if (t == null)
            {
                ViewBag.TopicId = -1;
                ViewBag.Topic = "";
            }
            else
            {
                ViewBag.TopicId = t.Id;
                ViewBag.Topic = topic;
            }

            return View();
        }

    }
}