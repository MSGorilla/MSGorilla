using System;
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
        public ActionResult Index()
        {
            string myid = this.Session["userid"].ToString();
            UserProfile me = _accManager.FindUser(myid);

            ViewBag.Myid = me.Userid;
            ViewBag.Me = me;

            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your app description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult Login(string userid, string password = "")
        {
            try
            {
                if (_accManager.AuthenticateUser(userid, Utils.MD5Encoding(password)))
                {
                    this.Session.Add("userid", userid);

                    HttpCookie cookie = new HttpCookie("Authorization",
                        "basic " + Utils.EncodeBase64(string.Format("{0}:{1}", userid, Utils.MD5Encoding(password))));
                    cookie.HttpOnly = true;
                    cookie.Expires = DateTime.UtcNow.AddDays(1);
                    cookie.Path = "/";

                    this.HttpContext.Response.Cookies.Add(cookie);

                    return RedirectToAction("/index");
                }
                return View();
            }
            catch(Exception e)
            {
                return View();
            }
        }

        public ActionResult Logout()
        {
            try
            {
                this.Session.Abandon();
                if (Request.Cookies["Authorization"] != null)
                {
                    HttpCookie myCookie = new HttpCookie("Authorization");
                    myCookie.Expires = DateTime.Now.AddDays(-1d);
                    Response.Cookies.Add(myCookie);
                }
            }
            catch
            {
                
            }

            return RedirectToAction("/index");
        }

    }
}
