using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Runtime.Serialization;
using System.Web;
//using System.Web.Mvc;


using MSGorilla.Library;
using MSGorilla.Filters;
using MSGorilla.Library.Models;
using MSGorilla.Library.Exceptions;
using MSGorilla.Library.Models.SqlModels;
using MSGorilla.Library.Models.ViewModels;

using System.Threading;
using System.Threading.Tasks;

using System.Data.Entity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace MSGorilla.WebApi
{
    public class AccountController : BaseController
    {
        private NotifManager _notifManager = new NotifManager();

        /// <summary>
        /// Return a list of all users in the system. 
        /// 
        /// Example output:
        /// [
        ///     {
        ///         "IsFollowing": 1,
        ///         "Userid": "abc",
        ///         "DisplayName": "aaa",
        ///         "PortraitUrl": null,
        ///         "Description": "aaa",
        ///         "FollowingsCount": 0,
        ///         "FollowersCount": 2,
        ///         "MessageCount": 0
        ///     },
        ///     {
        ///         "IsFollowing": 0,
        ///         "Userid": "bin",
        ///         "DisplayName": "bin",
        ///         "PortraitUrl": "/Content/Images/default_avatar.jpg",
        ///         "Description": "bin",
        ///         "FollowingsCount": 0,
        ///         "FollowersCount": 0,
        ///         "MessageCount": 1
        ///     },
        ///     ......
        /// ]
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// Return profile of a user.
        /// 
        /// Example output:
        /// {
        ///     "IsFollowing": 0,
        ///     "Userid": "bin",
        ///     "DisplayName": "bin",
        ///     "PortraitUrl": "/Content/Images/default_avatar.jpg",
        ///     "Description": "bin",
        ///     "FollowingsCount": 0,
        ///     "FollowersCount": 0,
        ///     "MessageCount": 1
        /// }
        /// </summary>
        /// <param name="userid">user id</param>
        /// <returns></returns>
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

        /// <summary>
        /// Return profile of the current user
        /// 
        /// Example output:
        /// {
        ///     "IsFollowing": -1,
        ///     "Userid": "user1",
        ///     "DisplayName": "User1",
        ///     "PortraitUrl": null,
        ///     "Description": "User1",
        ///     "FollowingsCount": 6,
        ///     "FollowersCount": 5,
        ///     "MessageCount": 73
        /// }
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// Register a local account. Return the profile of the registered user.
        /// User https tunnel for security reason.
        /// 
        /// Example output:
        /// {
        ///     "Userid": "newuser1",
        ///     "DisplayName": "New user 1",
        ///     "PortraitUrl": null,
        ///     "Description": "test add new user",
        ///     "FollowingsCount": 0,
        ///     "FollowersCount": 0,
        ///     "MessageCount": 0
        /// }
        /// </summary>
        /// <param name="Username">user id, should only contain [0-9a-zA-Z\-]</param>
        /// <param name="DisplayName">user display name</param>
        /// <param name="Password">password of the user</param>
        /// <param name="Description">description of the user</param>
        /// <returns></returns>
        [HttpGet, System.Web.Mvc.RequireHttps]
        public UserProfile Register(string Username, string DisplayName, string Password, string Description)
        {
            UserProfile user = new UserProfile();
            //account.Userid = 0;
            user.Userid = Username;
            user.DisplayName = DisplayName;
            user.Password = Utils.MD5Encoding(Password);
            user.Description = Description;

            UserProfile createdUser = _accountManager.AddUser(user);
            return createdUser;
        }

        /// <summary>
        /// Update user profile. Return the profile after updated.
        /// 
        /// Example output:
        /// {
        ///     "Userid": "user1",
        ///     "DisplayName": "User1",
        ///     "PortraitUrl": null,
        ///     "Description": "user for test",
        ///     "FollowingsCount": 6,
        ///     "FollowersCount": 5,
        ///     "MessageCount": 73
        /// }
        /// </summary>
        /// <param name="DisplayName">user display name</param>
        /// <param name="Description">description of the user</param>
        /// <param name="portraitUrl">portrait url of the user</param>
        /// <returns></returns>
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

        /// <summary>
        /// Update user password
        /// 
        /// Example output:
        /// {
        ///     "ActionResultCode": 0,
        ///     "Message": "success"
        /// }
        /// </summary>
        /// <param name="password">password of the user</param>
        /// <returns></returns>
        [HttpGet, System.Web.Mvc.RequireHttps]
        public ActionResult UpdatePassword(string password)
        {
            UserProfile user = _accountManager.FindUser(whoami());
            if (user.Password == null)
            {
                throw new NoAccessToUpdatePasswordException();
            }
            user.Password = Utils.MD5Encoding(password);
            _accountManager.UpdateUser(user);
            return new ActionResult();
        }

        //public class RegisterModel
        //{
        //    public string Username { get; set; }
        //    public string DisplayName { get; set; }
        //    public string Password { get; set; }
        //    public string Description { get; set; }
        //}
        //[HttpPost]
        //public UserProfile Register(RegisterModel registerModel)
        //{
        //    return Register(registerModel.Username, registerModel.DisplayName, registerModel.Password, registerModel.Description);
        //}

        /// <summary>
        /// Set current user following another user
        /// 
        /// Example output:
        /// {
        ///     "ActionResultCode": 0,
        ///     "Message": "success"
        /// }
        /// </summary>
        /// <param name="userid">another user id</param>
        /// <returns></returns>
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

        /// <summary>
        /// Set current user unfollowing of another user
        /// 
        /// Example output:
        /// {
        ///     "ActionResultCode": 0,
        ///     "Message": "success"
        /// }
        /// </summary>
        /// <param name="userid">another user id</param>
        /// <returns></returns>
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

        /// <summary>
        /// Return following user list of the current user.
        /// 
        /// Example output:
        /// [
        ///     {
        ///         "IsFollowing": 1,
        ///         "Userid": "abc",
        ///         "DisplayName": "aaa",
        ///         "PortraitUrl": null,
        ///         "Description": "aaa",
        ///         "FollowingsCount": 0,
        ///         "FollowersCount": 2,
        ///         "MessageCount": 0
        ///     },
        ///     {
        ///         "IsFollowing": 1,
        ///         "Userid": "fdy",
        ///         "DisplayName": "fdy",
        ///         "PortraitUrl": "/Content/Images/default_avatar.jpg",
        ///         "Description": "fdy",
        ///         "FollowingsCount": 5,
        ///         "FollowersCount": 0,
        ///         "MessageCount": 9
        ///     },
        ///     ......
        /// ]
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// Return following user list of a user
        /// 
        /// Example output:
        /// [
        ///     {
        ///         "IsFollowing": 1,
        ///         "Userid": "abc",
        ///         "DisplayName": "aaa",
        ///         "PortraitUrl": null,
        ///         "Description": "aaa",
        ///         "FollowingsCount": 0,
        ///         "FollowersCount": 2,
        ///         "MessageCount": 0
        ///     },
        ///     {
        ///         "IsFollowing": 1,
        ///         "Userid": "fdy",
        ///         "DisplayName": "fdy",
        ///         "PortraitUrl": "/Content/Images/default_avatar.jpg",
        ///         "Description": "fdy",
        ///         "FollowingsCount": 5,
        ///         "FollowersCount": 0,
        ///         "MessageCount": 9
        ///     },
        ///     ......
        /// ]
        /// </summary>
        /// <param name="userid">user id</param>
        /// <returns></returns>
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

        /// <summary>
        /// Return follower user list of the current user
        /// 
        /// Example output:
        /// [
        ///     {
        ///         "IsFollowing": 0,
        ///         "Userid": "user2",
        ///         "DisplayName": "User22",
        ///         "PortraitUrl": null,
        ///         "Description": "user22",
        ///         "FollowingsCount": 5,
        ///         "FollowersCount": 3,
        ///         "MessageCount": 10
        ///     },
        ///     {
        ///         "IsFollowing": 1,
        ///         "Userid": "fdy",
        ///         "DisplayName": "fdy",
        ///         "PortraitUrl": "/Content/Images/default_avatar.jpg",
        ///         "Description": "fdy",
        ///         "FollowingsCount": 5,
        ///         "FollowersCount": 0,
        ///         "MessageCount": 9
        ///     },
        ///     ......
        /// ]
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public List<DisplayUserProfile> Followers()
        {
            return Followers(whoami());
        }

        /// <summary>
        /// Return follower user list of a user
        /// 
        /// Example output:
        /// [
        ///     {
        ///         "IsFollowing": 0,
        ///         "Userid": "user2",
        ///         "DisplayName": "User22",
        ///         "PortraitUrl": null,
        ///         "Description": "user22",
        ///         "FollowingsCount": 5,
        ///         "FollowersCount": 3,
        ///         "MessageCount": 10
        ///     },
        ///     {
        ///         "IsFollowing": 1,
        ///         "Userid": "fdy",
        ///         "DisplayName": "fdy",
        ///         "PortraitUrl": "/Content/Images/default_avatar.jpg",
        ///         "Description": "fdy",
        ///         "FollowingsCount": 5,
        ///         "FollowersCount": 0,
        ///         "MessageCount": 9
        ///     },
        ///     ......
        /// ]
        /// </summary>
        /// <param name="userid">user id</param>
        /// <returns></returns>
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

        /// <summary>
        /// Return whether current is following another user.
        /// 0 for false , 1 for true and -1 for users are the same.
        /// 
        /// Example output:
        /// 0
        /// </summary>
        /// <param name="followingUserID">another user id</param>
        /// <returns></returns>
        [HttpGet]
        public int IsFollowing(string followingUserID)
        {
            return IsFollowing(whoami(), followingUserID);
        }

        /// <summary>
        /// Return whether a user is following another user.
        /// 0 for false , 1 for true and -1 for users are the same.
        /// 
        /// Example output:
        /// 1
        /// </summary>
        /// <param name="userid">user id</param>
        /// <param name="followingUserID">another user id</param>
        /// <returns></returns>
        [HttpGet]
        public int IsFollowing(string userid, string followingUserID)
        {
            whoami();
            // 0: not following, 1: following, -1: myself
            if (userid.Equals(followingUserID, StringComparison.CurrentCultureIgnoreCase))
            {
                return -1;
            }

            return _accountManager.IsFollowing(userid, followingUserID) ? 1 : 0;
        }

        /// <summary>
        /// Return count of new messages of the current user
        /// 
        /// Example output:
        /// {
        ///    "Userid": "user1",
        ///    "UnreadHomelineMsgCount": 8,
        ///    "UnreadOwnerlineMsgCount": 0,
        ///    "UnreadAtlineMsgCount": 0,
        ///    "UnreadReplyCount": 0
        /// }
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public NotificationCount GetNotificationCount()
        {
            string me = whoami();
            return _notifManager.FindUserNotif(me);
        }

        /// <summary>
        /// Return count of new messages of a user
        /// 
        /// Example output:
        /// {
        ///    "Userid": "user1",
        ///    "UnreadHomelineMsgCount": 8,
        ///    "UnreadOwnerlineMsgCount": 0,
        ///    "UnreadAtlineMsgCount": 0,
        ///    "UnreadReplyCount": 0
        /// }
        /// </summary>
        /// <param name="userid">user id</param>
        /// <returns></returns>
        [HttpGet]
        public NotificationCount GetNotificationCount(string userid)
        {
            string me = whoami();
            if (string.IsNullOrEmpty(userid))
            {
                userid = me;
            }
            return _notifManager.FindUserNotif(userid);
        }

        /// <summary>
        /// Return a user list the userid of which contains key word
        /// 
        /// Example output:
        /// [
        ///     {
        ///         "IsFollowing": 0,
        ///         "Userid": "WossBuildMonitor",
        ///         "DisplayName": "WossBuildMonitor",
        ///         "PortraitUrl": "/Content/Images/default_avatar.jpg",
        ///         "Description": "WossBuildMonitor",
        ///         "FollowingsCount": 0,
        ///         "FollowersCount": 4,
        ///         "MessageCount": 0
        ///     },
        ///     {
        ///         "IsFollowing": 1,
        ///         "Userid": "WossTFSMonitor",
        ///         "DisplayName": "WossTFSMonitor",
        ///         "PortraitUrl": "/Content/Images/default_avatar.jpg",
        ///         "Description": "WossTFSMonitor",
        ///         "FollowingsCount": 0,
        ///         "FollowersCount": 5,
        ///         "MessageCount": 0
        ///     },
        /// 
        /// ]
        /// </summary>
        /// <param name="keyword">key word</param>
        /// <returns></returns>
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

        //[HttpGet]
        //public ActionResult DeleteUser(string userid)
        //{
        //    _accountManager.DeleteUser(userid);
        //    return new ActionResult();
        //}

        /// <summary>
        /// Return a user list which posted the most messages order by message count desc.
        /// 
        /// </summary>
        /// <param name="count">count of users in the list</param>
        /// <returns></returns>
        [HttpGet]
        public List<DisplayUserProfile> ActiveUsers(int count = 5)
        {
            var userlist = _accountManager.GetActiveUsers(count);
            var dispusers = new List<DisplayUserProfile>();

            foreach (var u in userlist)
            {
                dispusers.Add(new DisplayUserProfile(u, IsFollowing(u.Userid)));
            }

            return dispusers;
        }
    }
}
