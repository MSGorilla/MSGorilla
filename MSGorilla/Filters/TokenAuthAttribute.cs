using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using MSGorilla.Library;
using MSGorilla.WebApi;
using MSGorilla.Library.Exceptions;

namespace MSGorilla.Filters
{
    public class TokenAuthAttribute : AuthorizeAttribute 
    {
        private BaseController _baseController;
        public TokenAuthAttribute()
        {
            _baseController = new BaseController();
        }

        //protected override bool AuthorizeCore(HttpContextBase httpContext) 

        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            try
            {
                if (filterContext.HttpContext.Session["userid"] != null)
                {
                    return;
                }
                string userid = _baseController.whoami();
                filterContext.HttpContext.Session.Add("userid", userid);
            }
            catch
            {
                filterContext.Result = new RedirectResult("/Account/Login"); 
            }
        }
    }
}