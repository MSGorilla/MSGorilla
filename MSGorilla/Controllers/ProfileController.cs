using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using MSGorilla.Filters;
using MSGorilla.Library;

namespace MSGorilla.Controllers
{
    public class ProfileController : Controller
    {
        // GET: Profile
        [TokenAuthAttribute]
        public ActionResult Index(string user)
        {
            ViewBag.UserId = user;

            return View();
        }

        //[TokenAuthAttribute]
        //public ActionResult Index()
        //{
        //    ViewBag.UserId = "";

        //    return View();
        //}
    }
}