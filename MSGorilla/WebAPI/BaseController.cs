using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;

using MSGorilla.Filters;
using MSGorilla.Library;
using MSGorilla.Library.Exceptions;

namespace MSGorilla.WebApi
{
    public class BaseController : ApiController
    {
        protected AccountManager _accountManager = new AccountManager();

        [HttpGet]
        public string whoami()
        {
            string authString = null;
            if(HttpContext.Current.Request.Cookies.Get("Authorization") !=null 
                && !string.IsNullOrEmpty(HttpContext.Current.Request.Cookies.Get("Authorization").Value))
            {
                authString = HttpContext.Current.Request.Cookies.Get("Authorization").Value;
            } else{
                authString = HttpContext.Current.Request.Headers.Get("Authorization");
            }

            if (!string.IsNullOrEmpty(authString))
            {
                var array = authString.Split(' ');
                if (array.Length == 2 && array[0].Equals("basic", StringComparison.OrdinalIgnoreCase))
                {
                    string cred = Encoding.UTF8.GetString(Convert.FromBase64String(array[1]));
                    string[] userpass = cred.Split(':');
                    if (userpass.Length == 2)
                    {
                        string user = userpass[0];
                        string password = userpass[1];
                        if (_accountManager.AuthenticateUser(user, password))
                        {
                            return user;
                        }
                    }
                }
            }

            if (System.Web.HttpContext.Current.User.Identity.IsAuthenticated)
            {
                string userid = System.Web.HttpContext.Current.User.Identity.Name;
                if (_accountManager.FindUser(userid) == null)
                {
                    Library.Models.SqlModels.UserProfile newUser = new Library.Models.SqlModels.UserProfile();
                    newUser.Userid = newUser.DisplayName = newUser.Description = userid;
                    newUser.FollowersCount = newUser.FollowingsCount = 0;
                    _accountManager.AddUser(newUser).Wait();
                }
                return System.Web.HttpContext.Current.User.Identity.Name;
            }

            throw new AccessDenyException();            
        }
    }
}
