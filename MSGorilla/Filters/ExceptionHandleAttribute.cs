using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net;
using System.Net.Http;

using MSGorilla.Library.Exceptions;
using MSGorilla.Library.Models;

using System.Diagnostics;

namespace MSGorilla.Filters
{
    public class ExceptionHandleAttribute : System.Web.Http.Filters.ExceptionFilterAttribute
    {
        public override void OnException(System.Web.Http.Filters.HttpActionExecutedContext actionExecutedContext)
        {
            base.OnException(actionExecutedContext);          

            Exception e = actionExecutedContext.Exception;
            ActionResult result;

            if (e is AccessDenyException)
            {
                result = ((AccessDenyException)e).toActionResult();
            }
            else if (e is MSGorillaBaseException)
            {
                result = ((MSGorillaBaseException)e).toActionResult();
            }
            else
            {
                result = new MSGorillaBaseException().toActionResult();
                Trace.TraceError("Server internal error.", e);
            }

            //System.Net.Http.HttpContent
            actionExecutedContext.Response = actionExecutedContext.Request.CreateResponse(HttpStatusCode.BadRequest, result);
        }
    }
}