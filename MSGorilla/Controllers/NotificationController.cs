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
    public class NotificationController : Controller
    {
        AccountManager _accManager = new AccountManager();

        [TokenAuthAttribute]
        public ActionResult Index(string category)
        {
            string myid = this.Session["userid"].ToString();
            UserProfile me = _accManager.FindUser(myid);
            ViewBag.Myid = me.Userid;
            ViewBag.Me = me;

            if (string.IsNullOrEmpty(category))
            {
                ViewBag.FeedCategory = "atline";
            }
            else
            {
                ViewBag.FeedCategory = category.ToLower();
            }

            return View();
        }
    }
}