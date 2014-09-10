using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Threading.Tasks;

using Microsoft.IdentityModel.Claims;
using System.Configuration;
using System.Net.Http.Headers;
//using System.Security.Claims;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using MSGorilla.Utility;
using MSGorilla.Filters;
using MSGorilla.Library;
using MSGorilla.Library.Exceptions;
using MSGorilla.Library.Models.SqlModels;
using MSGorilla.Library.Models;


//using Microsoft.AspNet.Identity;
//using Microsoft.AspNet.Identity.EntityFramework;
//using Microsoft.Owin.Security;
//using Microsoft.Owin.Security.Cookies;

namespace MSGorilla.WebAPI
{
    public class BaseController : ApiController
    {
        protected AccountManager _accountManager = new AccountManager();

        //
        // Retrieve the user's name, tenantID, and access token since they are parameters used to query the Graph API.
        //
        private string graphResourceId = ConfigurationManager.AppSettings["ida:GraphUrl"];
        private string graphUserUrl = "https://graph.windows.net/{0}/me?api-version=" + ConfigurationManager.AppSettings["ida:GraphApiVersion"];
        private const string TenantIdClaimType = "http://schemas.microsoft.com/identity/claims/tenantid";

        /// <summary>
        /// Return the current authenticated user id
        /// 
        /// Example output:
        /// "user1"
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public string whoami()
        {
            return MembershipHelper.whoami();
        }
    }
}
