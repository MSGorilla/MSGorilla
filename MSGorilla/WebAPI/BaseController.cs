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
            if (HttpContext.Current.Session != null)
            {
                var session = HttpContext.Current.Session;
                if (session["userid"] != null)
                {
                    return session["userid"].ToString();
                }
            }

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
                            return _accountManager.FindUser(user).Userid;
                        }
                    }
                }
            }

            if (System.Web.HttpContext.Current.User.Identity.IsAuthenticated)
            {
                var identity = (ClaimsIdentity)User.Identity;
                IEnumerable<Claim> claims = identity.Claims;
                string userid = GetAlias(claims);

                if (userid == null)
                {
                    throw new AccessDenyException();
                }

                //string userid = System.Web.HttpContext.Current.User.Identity.Name;
                //if (userid.Contains("@microsoft.com"))
                //{
                //    userid = userid.Replace("@microsoft.com", "");
                //}

                UserProfile profile = _accountManager.FindUser(userid);
                if (profile == null)
                {
                    UserProfile newUser = Utils.CreateNewUser(userid, GetDisplayName(claims), GetUserTitle(claims));
                    _accountManager.AddUser(newUser);
                    profile = newUser;
                }
                else if (profile.Password != null)
                {
                    string description = string.Format("A local account names {0} already exists "
                     + "so you can't use your MS account to login. "
                     + "Register a local account or contact MSGorilla Admin to solve this problem."
                     , profile.Userid);
                    throw new UserAlreadyExistException(profile.Userid, description);
                }
                else if (string.IsNullOrEmpty(profile.PortraitUrl))
                {
                    //Check Thumbnail photo
                    try
                    {
                        byte[] photoBytes = GetThumbnailPhoto(claims);
                        if (photoBytes != null)
                        {
                            Attachment thumbnail = UploadThumbnailPhoto(photoBytes, profile.Userid);
                            profile.PortraitUrl = Utils.GetDownloadAttachmentUri(thumbnail.AttachmentID);
                            _accountManager.UpdateUser(profile);
                        }
                    }
                    catch (Exception e)
                    {
                        
                    }
                    
                }
                return userid;
            }

            throw new AccessDenyException();
        }

        public static string GetAlias(IEnumerable<Claim> claims)
        {
            Claim claim = claims.First(c => c.ClaimType.Contains("Alias"));
            if (claim == null)
            {
                return null;
            }
            return claim.Value;
        }

        public static string GetDisplayName(IEnumerable<Claim> claims)
        {
            string displayname = "";
            Claim claim = claims.FirstOrDefault(c => c.ClaimType.Contains("FirstName"));
            displayname += claim.Value;
            displayname += " ";
            claim = claims.FirstOrDefault(c => c.ClaimType.Contains("LastName"));
            displayname += claim.Value;
            return displayname;
        }

        public static string GetUserTitle(IEnumerable<Claim> claims)
        {
            Claim claim = claims.FirstOrDefault(c => c.ClaimType.Contains("Title"));
            if (claim == null)
            {
                return null;
            }
            return claim.Value;
        }

        public static byte[] GetThumbnailPhoto(IEnumerable<Claim> claims)
        {
            Claim claim = claims.FirstOrDefault(c => c.ClaimType.Contains("ThumbnailPhoto"));
            if (claim == null)
            {
                return null;
            }

            byte[] photoBytes = null;
            try
            {
                photoBytes = Utils.Base64Decode(claim.Value);
            }
            catch
            {

            }
            return photoBytes;
        }

        Attachment UploadThumbnailPhoto(byte[] photoBytes, string userid)
        {
            AttachmentManager attachmentManager = new AttachmentManager();
            Attachment thumbnail = attachmentManager.Upload(string.Format("Thumbnail_{0}.jpg", userid), "image/jpeg", photoBytes, userid);
            return thumbnail;
        }
    }
}
