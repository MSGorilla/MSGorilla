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
    public class ProfileController : Controller
    {
        AccountManager _accManager = new AccountManager();

        [TokenAuthAttribute]
        public ActionResult Index(string user, string group = null)
        {
            string myid = this.Session["userid"].ToString();
            UserProfile me = _accManager.FindUser(myid);
            ViewBag.Myid = me.Userid;
            ViewBag.Me = me;

            if (string.IsNullOrEmpty(user) || user.Equals(myid, StringComparison.CurrentCultureIgnoreCase))
            {
                ViewBag.IsMe = true;
                user = myid;
            }
            else{
                ViewBag.IsMe = false;
            }

            ViewBag.UserId = user;

            ViewBag.FeedCategory = "userline";
            ViewBag.FeedId = user;

            if (string.IsNullOrEmpty(group))
            {
                group = "";
            }
            ViewBag.Group = group;

            return View();
        }

        [TokenAuthAttribute]
        public ActionResult Following(string user)
        {
            string myid = this.Session["userid"].ToString();
            UserProfile me = _accManager.FindUser(myid);
            ViewBag.Myid = me.Userid;
            ViewBag.Me = me;

            if (string.IsNullOrEmpty(user) || user.Equals(myid, StringComparison.CurrentCultureIgnoreCase))
            {
                ViewBag.IsMe = true;
                user = myid;
            }
            else
            {
                ViewBag.IsMe = false;
            }

            ViewBag.UserId = user;

            ViewBag.FeedCategory = "following";
            ViewBag.FeedId = user;

            return View();
        }

        [TokenAuthAttribute]
        public ActionResult Followers(string user)
        {
            string myid = this.Session["userid"].ToString();
            UserProfile me = _accManager.FindUser(myid);
            ViewBag.Myid = me.Userid;
            ViewBag.Me = me;

            if (string.IsNullOrEmpty(user) || user.Equals(myid, StringComparison.CurrentCultureIgnoreCase))
            {
                ViewBag.IsMe = true;
                user = myid;
            }
            else
            {
                ViewBag.IsMe = false;
            }

            ViewBag.UserId = user;

            ViewBag.FeedCategory = "followers";
            ViewBag.FeedId = user;

            return View();
        }

        [TokenAuthAttribute]
        public ActionResult Favorites(string user)
        {
            string myid = this.Session["userid"].ToString();
            UserProfile me = _accManager.FindUser(myid);
            ViewBag.Myid = me.Userid;
            ViewBag.Me = me;

            if (string.IsNullOrEmpty(user) || user.Equals(myid, StringComparison.CurrentCultureIgnoreCase))
            {
                ViewBag.IsMe = true;
                user = myid;
            }
            else
            {
                ViewBag.IsMe = false;
            }

            ViewBag.UserId = user;

            ViewBag.FeedCategory = "favorites";
            ViewBag.FeedId = user;

            return View();
        }

        [TokenAuthAttribute]
        public ActionResult Owned(string user)
        {
            string myid = this.Session["userid"].ToString();
            UserProfile me = _accManager.FindUser(myid);
            ViewBag.Myid = me.Userid;
            ViewBag.Me = me;

            if (string.IsNullOrEmpty(user) || user.Equals(myid, StringComparison.CurrentCultureIgnoreCase))
            {
                ViewBag.IsMe = true;
                user = myid;
            }
            else
            {
                ViewBag.IsMe = false;
            }

            ViewBag.UserId = user;

            ViewBag.FeedCategory = "ownerline";
            ViewBag.FeedId = user;

            return View();
        }
    }
}