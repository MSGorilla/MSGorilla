using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Runtime.Serialization;
using System.Web;

using MSGorilla.Library;
using MSGorilla.Filters;
using MSGorilla.Library.Models;
using MSGorilla.Library.Exceptions;
using MSGorilla.Library.Models.SqlModels;

using System.Threading;
using System.Threading.Tasks;

using System.Data.Entity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace MSGorilla.WebApi
{
    public class DisplayUserProfile : UserProfile
    {
        [DataMember]
        public int IsFollowing { get; private set; }

        public DisplayUserProfile(UserProfile user, int isFollowing)
        {
            Userid = user.Userid;
            DisplayName = user.DisplayName;
            PortraitUrl = user.PortraitUrl;
            Description = user.Description;
            FollowingsCount = user.FollowingsCount;
            FollowersCount = user.FollowersCount;
            MessageCount = user.MessageCount;
            IsFollowing = isFollowing;
        }
    }

    public class AccountController : BaseController
    {
        private NotifManager _notifManager = new NotifManager();

        [HttpGet]
        public List<DisplayUserProfile> User()
        {
            var userlist = _accountManager.GetAllUsers();
            var dispusers = new List<DisplayUserProfile>();

            foreach (var u in userlist)
            {
                dispusers.Add(new DisplayUserProfile(u, IsFollowing(u.Userid)));
            }

            return dispusers;
        }

        [HttpGet]
        public DisplayUserProfile User(string userid)
        {
            var user = _accountManager.FindUser(userid);
            if (user == null)
            {
                throw new UserNotFoundException(userid);
            }
            else
            {
                return new DisplayUserProfile(user, IsFollowing(userid));
            }
        }

        [HttpGet]
        public DisplayUserProfile Me()
        {
            string userid = whoami();
            var user = _accountManager.FindUser(userid);
            if (user == null)
            {
                throw new UserNotFoundException(userid);
            }
            else
            {
                return new DisplayUserProfile(user, -1);
            }
        }

        [HttpGet]
        public UserProfile Register(string Username, string DisplayName, string Password, string Description)
        {
            UserProfile user = new UserProfile();
            //account.Userid = 0;
            user.Userid = Username.ToLower();
            user.DisplayName = DisplayName;
            user.Password = Utils.MD5Encoding(Password);
            user.Description = Description;

            UserProfile createdUser = _accountManager.AddUser(user);
            return createdUser;
        }

        [HttpGet]
        public UserProfile Update(string DisplayName, string Description, string portraitUrl)
        {
            string userid = whoami();
            UserProfile user = _accountManager.FindUser(userid);
            user.DisplayName = DisplayName;
            user.Description = Description;
            user.PortraitUrl = portraitUrl;
            _accountManager.UpdateUser(user);
            return user;

        }

        [HttpGet]
        public ActionResult UpdatePassword(string password)
        {
            UserProfile user = _accountManager.FindUser(whoami());
            user.Password = Utils.MD5Encoding(password);
            _accountManager.UpdateUser(user);
            return new ActionResult();

        }

        public class RegisterModel
        {
            public string Username { get; set; }
            public string DisplayName { get; set; }
            public string Password { get; set; }
            public string Description { get; set; }
        }
        [HttpPost]
        public UserProfile Register(RegisterModel registerModel)
        {
            return Register(registerModel.Username, registerModel.DisplayName, registerModel.Password, registerModel.Description);
        }

        [HttpGet]
        public async Task<ActionResult> Follow(string userid)
        {
            string me = whoami();
            UserProfile user = _accountManager.FindUser(userid);
            if (userid == null)
            {
                throw new UserNotFoundException(userid);
            }

            Task<Boolean> ret = _accountManager.Follow(me, userid);
            Boolean result = await ret;
            if (result)
            {
                return new ActionResult();
            }
            return new MSGorillaBaseException().toActionResult();
        }

        [HttpGet]
        public async Task<ActionResult> UnFollow(string userid)
        {
            string me = whoami();
            UserProfile user = _accountManager.FindUser(userid);
            if (userid == null)
            {
                throw new UserNotFoundException(userid);
            }

            Task<Boolean> ret = _accountManager.UnFollow(me, userid);
            Boolean result = await ret;
            if (result)
            {
                return new ActionResult();
            }
            return new MSGorillaBaseException().toActionResult();
        }

        [HttpGet]
        public List<DisplayUserProfile> Followings()
        {
            string userid = whoami();
            var userlist = _accountManager.Followings(userid);
            var dispusers = new List<DisplayUserProfile>();

            foreach (var u in userlist)
            {
                dispusers.Add(new DisplayUserProfile(u, 1));
            }

            return dispusers;
        }

        [HttpGet]
        public List<DisplayUserProfile> Followings(string userid)
        {
            var userlist = _accountManager.Followings(userid);
            var dispusers = new List<DisplayUserProfile>();

            foreach (var u in userlist)
            {
                dispusers.Add(new DisplayUserProfile(u, IsFollowing(u.Userid)));
            }

            return dispusers;
        }

        [HttpGet]
        public List<DisplayUserProfile> Followers()
        {
            return Followers(whoami());
        }

        [HttpGet]
        public List<DisplayUserProfile> Followers(string userid)
        {
            var userlist = _accountManager.Followers(userid);
            var dispusers = new List<DisplayUserProfile>();

            foreach (var u in userlist)
            {
                dispusers.Add(new DisplayUserProfile(u, IsFollowing(u.Userid)));
            }

            return dispusers;
        }

        [HttpGet]
        public int IsFollowing(string followingUserID)
        {
            return IsFollowing(whoami(), followingUserID);
        }

        [HttpGet]
        public int IsFollowing(string userid, string followingUserID)
        {
            // 0: not following, 1: following, -1: myself
            if (userid.Equals(followingUserID, StringComparison.CurrentCultureIgnoreCase))
            {
                return -1;
            }

            return _accountManager.IsFollowing(userid, followingUserID) ? 1 : 0;
        }

        [HttpGet]
        public NotificationCount GetNotificationCount()
        {
            string me = whoami();
            return _notifManager.FindUserNotif(me);
        }

        [HttpGet]
        public List<DisplayUserProfile> SearchUser(string keyword)
        {
            var userlist = _accountManager.SearchUser(keyword);
            var dispusers = new List<DisplayUserProfile>();

            foreach (var u in userlist)
            {
                dispusers.Add(new DisplayUserProfile(u, IsFollowing(u.Userid)));
            }

            return dispusers;
        }
    }
}
