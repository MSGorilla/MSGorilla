using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using System.Net;
using System.Net.Http;
using System.Web.Http.Filters;
using System.Diagnostics;

using MSGorilla.Library;
using MSGorilla.Library.Exceptions;
using MSGorilla.Library.Models;

namespace MSGorilla.Filters
{
    public class BasicAuthAttribute : System.Web.Http.Filters.AuthorizationFilterAttribute
    {

        private bool _needAuth;
        private AccountManager _accountManager;
        public BasicAuthAttribute(bool needAuth = true)
        {
            _needAuth = needAuth;
            _accountManager = new AccountManager();
        }


        //Http Header:
        //Authorization: basic hash(base64(userid|md5(password)))
        public override void OnAuthorization(System.Web.Http.Controllers.HttpActionContext actionContext)
        {          
            try
            {
                var authHeader = actionContext.Request.Headers.Authorization;

                if (authHeader == null)
                {
                    throw new AccessDenyException();
                }
                string authString = authHeader.Parameter;

                if (authHeader.Scheme.Equals("basic", StringComparison.OrdinalIgnoreCase) &&
                !String.IsNullOrWhiteSpace(authString))
                {
                    var credArray = GetCredentials(authHeader);
                    var userName = credArray[0];
                    var password = credArray[1];

                    if (_accountManager.AuthenticateUser(userName, password))
                    {
                        return;
                    }
                }
            }
            catch (Exception e)
            {
                if (e is TwitterBaseException)
                {
                    actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.Unauthorized, ((TwitterBaseException)e).toActionResult());
                }
                else
                {
                    Trace.TraceError("Unknown error when login", e);
                    actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.ServiceUnavailable, new TwitterBaseException().toActionResult());
                }
            }            
        }

        private string[] GetCredentials(System.Net.Http.Headers.AuthenticationHeaderValue authHeader)
        {
            var rawCred = authHeader.Parameter;
            string cred = Encoding.UTF8.GetString(Convert.FromBase64String(rawCred));
            var credArray = cred.Split(':');
            return credArray;
        }
    }
}