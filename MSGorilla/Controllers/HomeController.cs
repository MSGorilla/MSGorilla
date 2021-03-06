﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web;
using System.Web.Mvc;

using MSGorilla.Filters;
using MSGorilla.Library;
using MSGorilla.Library.Models.SqlModels;

namespace MSGorilla.Controllers
{
    public class HomeController : Controller
    {
        AccountManager _accManager = new AccountManager();

        [TokenAuthAttribute]
        public ActionResult Index(string group = null)
        {
            string myid = this.Session["userid"].ToString();
            UserProfile me = _accManager.FindUser(myid);
            ViewBag.Myid = me.Userid;
            ViewBag.Me = me;

            ViewBag.FeedCategory = "homeline";
            ViewBag.FeedId = me.Userid;

            if (string.IsNullOrEmpty(group))
            {
                group = "";
            }
            ViewBag.Group = group;

            ViewBag.PerfChartName = "perf_chart_test";

            return View();
        }
    }
}
