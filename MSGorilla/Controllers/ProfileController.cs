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
        public ActionResult Index(string user)
        {
            string myid = this.Session["userid"].ToString();
            UserProfile me = _accManager.FindUser(myid);

            ViewBag.Myid = me.Userid;
            ViewBag.Me = me;

            if (string.IsNullOrEmpty(user))
            {
                user = "";
            }
            //user = user.ToLower();
            if (user == "" || user.Equals(myid, StringComparison.CurrentCultureIgnoreCase))
            {
                ViewBag.IsMe = true;
                user = "";
            }
            else{
                ViewBag.IsMe = false;
            }

            ViewBag.UserId = user;

            return View();
        }

    }
}