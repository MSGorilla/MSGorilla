using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using MSGorilla.Filters;
using MSGorilla.Models;
using MSGorilla.Library;
using MSGorilla.Library.Models.SqlModels;

namespace MSGorilla.Controllers
{
    public class CounterController : Controller
    {
        AccountManager _accManager = new AccountManager();
        // GET: Counter

        [TokenAuthAttribute]
        public ActionResult Index()
        {
            string myid = this.Session["userid"].ToString();
            UserProfile me = _accManager.FindUser(myid);
            ViewBag.Myid = me.Userid;
            ViewBag.Me = me;


            return View();
        }

        [TokenAuthAttribute]
        public ActionResult Chart(string group, string name, string path)
        {
            PerfCounterModel model = new PerfCounterModel();
            model.Group = group;
            model.Name = name;
            model.Path = path;
            return View(model);
        }
    }
}