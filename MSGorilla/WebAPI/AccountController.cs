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



namespace MSGorilla.WebApi
{
    public class AccountController : BaseController
    {
        [HttpGet]
        public Object User()
        {
            return _accountManager.GetAllUsers();
        }

        //[HttpGet]
        //public Object CurrentUser()
        //{
        //    return User(HttpContext.Current.User.Identity.Name);
        //}

        [HttpGet]
        public UserProfile User(string userid)
        {
            var user = _accountManager.FindUser(userid);
            if (user == null)
            {
                throw new UserNotFoundException(userid);
            }
            else
            {
                return user;
            }
        }

        [HttpGet]
        public UserProfile Me()
        {
            string userid = whoami();
            var user = _accountManager.FindUser(userid);
            if (user == null)
            {
                throw new UserNotFoundException(userid);
            }
            else
            {
                return user;
            }
        }

        [HttpGet]
        public async Task<object> Register(string Username, string DisplayName, string Password, string Description)
        {
            UserProfile user = new UserProfile();
            //account.Userid = 0;
            user.Userid = Username;
            user.DisplayName = DisplayName;
            user.Password = Utils.MD5Encoding(Password);
            user.Description = Description;

            Task<UserProfile> createdUser = _accountManager.AddUser(user);
            UserProfile u = await createdUser;
            return u;            
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
        public Task<object> Register(RegisterModel registerModel)
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
        public List<UserProfile> Followings(string userid)
        {
            return _accountManager.Followings(userid);
        }

        [HttpGet]
        public List<UserProfile> Followers(string userid)
        {
            return _accountManager.Followers(userid);
        }
    }
}
