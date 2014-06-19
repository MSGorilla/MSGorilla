using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace MSGorilla.Filters
{
    public class CryptographicExceptionHandlerAttribute : System.Web.Mvc.HandleErrorAttribute
    {
        public override void OnException(System.Web.Mvc.ExceptionContext filterContext)
        {
            //If the exeption is already handled we do nothing
            if (filterContext.ExceptionHandled)
            {
                return;
            }
            else if (filterContext.Exception is System.Security.Cryptography.CryptographicException)
            {
                //Make sure that we mark the exception as handled
                filterContext.ExceptionHandled = true;

                filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary(
                new
                {
                    action = "logoff",
                    controller = "Account"
                    //id = filterContext.Exception.Message,
                    //exceptionAction = (string)filterContext.RouteData.Values["action"],
                    //exceptionController = (string)filterContext.RouteData.Values["controller"]
                }));
            }
        }
    }
}