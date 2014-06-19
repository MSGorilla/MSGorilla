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
    public class ErrorController : Controller
    {       
        public ActionResult Index(string message, string returnUrl)
        {
            ViewBag.ErrorMessage = message;
            ViewBag.returnUrl = returnUrl;

            return View();
        }
    }
}