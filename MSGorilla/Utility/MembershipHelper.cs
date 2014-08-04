using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using MSGorilla.Library;
using MSGorilla.Filters;
using MSGorilla.Library.Exceptions;
using MSGorilla.Library.Models.SqlModels;
using MSGorilla.Library.Models;
using Microsoft.IdentityModel.Claims;
using System.Web.Http;
using MSGorilla.Library.Models.ViewModels;
using MSGorilla.Library.Models.SqlModels;

namespace MSGorilla.Utility
{
    public static class MembershipHelper
    {
        static AccountManager _accountManager = new AccountManager();
        static GroupManager _groupManager = new GroupManager();

        private static string _defaultGroup = "defaultGroup";

        

        public static void CheckMembership(string groupID, string userid)
        {
            string key = string.Format("{0}{1}@{2}", CacheHelper.MembershipPrefix, userid, groupID);
            if (CacheHelper.Contains(key))
            {
                return;
            }

            _groupManager.CheckMembership(groupID, userid);
            CacheHelper.Add(key, "member");
        }

        public static void CheckAdmin(string groupID, string userid)
        {
            string key = string.Format("{0}{1}@{2}", CacheHelper.MembershipPrefix, userid, groupID);
            if (CacheHelper.Contains(key) && "admin".Equals(CacheHelper.Get<string>(key)))
            {
                return;
            }

            _groupManager.CheckAdmin(groupID, userid);
            CacheHelper.Add(key, "admin");
        }

        public static string[] CheckJoinedGroup(string userid, string[] groups = null)
        {
            if (groups == null || groups.Count() == 0)
            {
                return JoinedGroup(userid).ToArray();
            }

            return JoinedGroup(userid).Intersect(groups).ToArray();
        }

        /// <summary>
        /// return the default of current user
        /// </summary>
        /// <param name="userid">Can only be current user!!! User whoami() to get the current user</param>
        /// <returns></returns>
        public static string DefaultGroup(string userid)
        {
            var session = HttpContext.Current.Session;
            object obj = session[_defaultGroup];
            if (obj != null)
            {
                return obj.ToString();
            }

            UserProfile profile = _accountManager.FindUser(userid);
            if (profile.IsRobot)
            {
                session[_defaultGroup] = _groupManager.GetJoinedGroup(profile.Userid)[0].GroupID;
                return session[_defaultGroup].ToString();
            }
            else
            {
                session[_defaultGroup] = profile.DefaultGroup;
                return profile.DefaultGroup;
            }
        }

        public static void SetDefaultGroup(string userid, string groupID)
        {
            UserProfile user = _accountManager.FindUser(userid);
            if (user.IsRobot)
            {
                throw new UpdateRobotDefaultGroupException();
            }

            MembershipHelper.CheckMembership(groupID, userid);
            _groupManager.SetDefaultGroup(groupID, userid);

            var session = HttpContext.Current.Session;
            session[_defaultGroup] = groupID;
        }

        public static string[] JoinedGroup(string userid)
        {
            string key = CacheHelper.JoinedGroupPrefix + userid;
            if (CacheHelper.Contains(key))
            {
                return CacheHelper.Get<string[]>(key);
            }
            List<DisplayMembership> groups = _groupManager.GetJoinedGroup(userid);
            List<string> groupIDs = new List<string>();
            string defaultGroup = DefaultGroup(userid);

            groupIDs.Add(defaultGroup);
            foreach (DisplayMembership member in groups)
            {
                if (!member.GroupID.Equals(defaultGroup, StringComparison.InvariantCultureIgnoreCase))
                {
                    groupIDs.Add(member.GroupID);
                }
            }

            CacheHelper.Add<string[]>(key, groupIDs.ToArray());
            return groupIDs.ToArray();
        }

        public static string[] RefreshJoinedGroup(string userid)
        {
            string key = CacheHelper.JoinedGroupPrefix + userid;
            CacheHelper.Remove(key);
            return JoinedGroup(userid);
        }
        public static string whoami()
        {
            var session = HttpContext.Current.Session;
            if (session != null && session["userid"] != null)
            {
                return session["userid"].ToString();
            }

            string userid = GetBasicAuthUserid();
            if (string.IsNullOrEmpty(userid))
            {
                userid = GetAdfsUserid();
            }
            
            if(string.IsNullOrEmpty(userid)){
                throw new AccessDenyException();
            }

            return userid;
        }

        public static string GetAdfsUserid()
        {
            string userid = null;
            if (System.Web.HttpContext.Current.User.Identity.IsAuthenticated)
            {
                //string userid = System.Web.HttpContext.Current.User.Identity.Name;
                var identity = (ClaimsIdentity)HttpContext.Current.User.Identity;
                IEnumerable<Claim> claims = identity.Claims;
                userid = GetAlias(claims);

                if (userid == null)
                {
                    return userid;
                }               

                //create user if not exist in local db
                UserProfile profile = _accountManager.FindUser(userid);
                if (profile == null)
                {
                    UserProfile newUser = Utils.CreateNewUser(userid, GetDisplayName(claims), GetUserTitle(claims));
                    _accountManager.AddUser(newUser);

                    //join default microsoft group
                    new GroupManager().AddMember("microsoft", newUser.Userid, "user");

                    profile = newUser;
                }
                if (string.IsNullOrEmpty(profile.PortraitUrl))
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
            }
            return userid;
        }

        public static string GetBasicAuthUserid()
        {
            string authString = null;
            if (HttpContext.Current.Request.Cookies.Get("Authorization") != null
                && !string.IsNullOrEmpty(HttpContext.Current.Request.Cookies.Get("Authorization").Value))
            {
                authString = HttpContext.Current.Request.Cookies.Get("Authorization").Value;
            }
            else
            {
                authString = HttpContext.Current.Request.Headers.Get("Authorization");
            }

            if (string.IsNullOrEmpty(authString))
            {
                return null;
            }

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
            return null;
        }

        private static string GetAlias(IEnumerable<Claim> claims)
        {
            Claim claim = claims.First(c => c.ClaimType.Contains("Alias"));
            if (claim == null)
            {
                return null;
            }
            return claim.Value;
        }

        private static string GetDisplayName(IEnumerable<Claim> claims)
        {
            string displayname = "";
            Claim claim = claims.FirstOrDefault(c => c.ClaimType.Contains("FirstName"));
            displayname += claim.Value;
            displayname += " ";
            claim = claims.FirstOrDefault(c => c.ClaimType.Contains("LastName"));
            displayname += claim.Value;
            return displayname;
        }

        private static string GetUserTitle(IEnumerable<Claim> claims)
        {
            Claim claim = claims.FirstOrDefault(c => c.ClaimType.Contains("Title"));
            if (claim == null)
            {
                return null;
            }
            return claim.Value;
        }

        private static byte[] GetThumbnailPhoto(IEnumerable<Claim> claims)
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

        private static Attachment UploadThumbnailPhoto(byte[] photoBytes, string userid)
        {
            AttachmentManager attachmentManager = new AttachmentManager();
            Attachment thumbnail = attachmentManager.Upload(string.Format("Thumbnail_{0}.jpg", userid), "image/jpeg", photoBytes, userid);
            return thumbnail;
        }
    }
}