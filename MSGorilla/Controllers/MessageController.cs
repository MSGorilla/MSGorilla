using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MSGorilla.Filters;
using MSGorilla.Library;
using MSGorilla.Library.Models.SqlModels;

namespace MSGorilla.Views
{
    public class MessageController : Controller
    {
        AccountManager _accManager = new AccountManager();
        
        // GET: Search
        [TokenAuthAttribute]
        public ActionResult Index(string msgID)
        {
            string myid = this.Session["userid"].ToString();
            UserProfile me = _accManager.FindUser(myid);
            ViewBag.Myid = me.Userid;
            ViewBag.Me = me;

            ViewBag.FeedCategory = "message";
            if (string.IsNullOrEmpty(msgID))
            {
                msgID = "";
            }
            ViewBag.FeedId = msgID;

            return View();
        }
    }
}