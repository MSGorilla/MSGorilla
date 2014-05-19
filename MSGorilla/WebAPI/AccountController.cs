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
                //UserProfile newuser = new UserProfile { Userid = userid, DisplayName = userid, PortraitUrl = "" };
                //await _accountManager.AddUser(newuser);
                //return _accountManager.FindUser(userid);
            }
            else
            {
                return user;
            }            
        }

        


        [AcceptVerbs("GET", "POST")]
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

        [HttpGet, HttpPost]
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

        [HttpGet, HttpPost]
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


        //[HttpGet]
        //public object user()
        //{
        //    try
        //    {
        //        return db.users.ToList();
        //    }
        //    catch (Exception e)
        //    {
        //        Console.WriteLine(e.ToString());
        //        return null;
        //    }            
        //}

        //[HttpGet]
        //public object user(int id)
        //{
        //    try
        //    {
        //        return db.users.Find(id);
        //    }
        //    catch (Exception e)
        //    {
        //        Console.WriteLine(e.ToString());
        //        return null;
        //    }
        //}
    }
}
