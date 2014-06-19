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
    public class SearchController : Controller
    {
        AccountManager _accManager = new AccountManager();
        
        // GET: Search
        [TokenAuthAttribute]
        public ActionResult Index(string keyword)
        {
            string myid = this.Session["userid"].ToString();
            UserProfile me = _accManager.FindUser(myid);

            ViewBag.Myid = me.Userid;
            ViewBag.Me = me;

            ViewBag.FeedCategory = "topicline";

            if (string.IsNullOrEmpty(keyword))
            {
                keyword = "";
            }
            ViewBag.SearchKeyword = keyword;

            return View();
        }
    }
}