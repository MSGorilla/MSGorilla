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

        // GET: Profile
        [TokenAuthAttribute]
        public ActionResult Index(string user)
        {
            string myid = this.Session["userid"].ToString();
            UserProfile me = _accManager.FindUser(myid);

            ViewBag.Myid = myid;
            ViewBag.Me = me;

            if (string.IsNullOrEmpty(user))
            {
                user = myid;
            }
            ViewBag.UserId = user;
            ViewBag.IsFollowing = false;

            if (user == myid)
            {
                ViewBag.IsMe = true;
            }
            else{
                ViewBag.IsMe = false;

                var myfollowers = _accManager.Followers(myid);
                foreach (var u in myfollowers)
                {
                    if (u.Userid == user)
                    {
                        ViewBag.IsFollowing = true;
                        break;
                    }
                }
            }

            return View();
        }

    }
}