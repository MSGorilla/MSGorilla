using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using MSGorilla.Library;
using MSGorilla.WebApi;
using MSGorilla.Library.Exceptions;
using MSGorilla.Library.Models.SqlModels;

namespace MSGorilla.Filters
{
    public class TokenAuthAttribute : AuthorizeAttribute 
    {
        private BaseController _baseController;
        private AccountManager _accManager;
        public TokenAuthAttribute()
        {
            _baseController = new BaseController();
            _accManager = new AccountManager();
        }

        //protected override bool AuthorizeCore(HttpContextBase httpContext) 

        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            try
            {
                string userid = null;
                if (filterContext.HttpContext.Session["userid"] != null)
                {
                    userid = filterContext.HttpContext.Session["userid"].ToString();
                    if (_accManager.FindUser(userid) == null)
                    {
                        UserProfile profile = Utils.CreateNewUser(userid);
                        _accManager.AddUser(profile).Wait();
                    }
                    return;
                }
                userid = _baseController.whoami();
                filterContext.HttpContext.Session.Add("userid", userid);
            }
            catch
            {
                filterContext.Result = new RedirectResult("/Account/Login"); 
            }
        }
    }
}