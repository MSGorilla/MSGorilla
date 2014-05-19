using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

using MSGorilla.Library.Models;
using MSGorilla.Library.Models.SqlModels;
using MSGorilla.Library.Exceptions;
using MSGorilla.Library.DAL;

namespace MSGorilla.Library
{
    public class AccountManager
    {
        private MSGorillaContext _accountCtx;
        public AccountManager(){
            _accountCtx = new MSGorillaContext();
        }

        public List<UserProfile> GetAllUsers(){
            return _accountCtx.Users.ToList();
        }

        public bool AuthenticateUser(string userid, string password)
        {
            if (string.IsNullOrEmpty(userid))
            {
                return false;
            }
            UserProfile user = _accountCtx.Users.Find(userid);
            if (user == null)
            {
                throw new UserNotFoundException(userid);
            }

            if (string.IsNullOrEmpty(user.Password))
            {
                return true;
            }
            return user.Password.Equals(password);
        }

        public UserProfile FindUser(string userid)
        {
            return _accountCtx.Users.Find(userid);
        }

        public async Task<UserProfile> AddUser(UserProfile user)
        {
            UserProfile temp = _accountCtx.Users.Find(user.Userid);
            if (temp != null)
            {
                throw new UserAlreadyExistException(user.Userid);
            }
            _accountCtx.Users.Add(user);
            await _accountCtx.SaveChangesAsync();
            return  _accountCtx.Users.Find(user.Userid);
        }

        public async Task<Boolean> UpdateUser(UserProfile user)
        {
            UserProfile originUser = _accountCtx.Users.Find(user.Userid);
            if (originUser == null)
            {
                throw new UserNotFoundException(user.Userid);
            }

            originUser.DisplayName = user.DisplayName;
            originUser.PortraitUrl = user.PortraitUrl;
            originUser.Description = user.Description;
            await _accountCtx.SaveChangesAsync();
            return true;
        }

        public async Task<Boolean> Follow(string userid, string followingUserid)
        {
            UserProfile user = _accountCtx.Users.Find(userid);
            UserProfile followingUser = _accountCtx.Users.Find(followingUserid);
            if (user == null || followingUser == null)
            {
                throw new UserNotFoundException(string.Format("{0} or {1}", userid, followingUserid));
            }

            if(_accountCtx.Subscriptions.Where(f => f.Userid == userid && f.FollowingUserid == followingUserid).ToList().Count > 0){
                return true;
            }

            Subscription follow = new Subscription();
            follow.Userid = userid;
            follow.FollowingUserid = followingUserid;
            follow.FollowingUserDisplayName = "";
            _accountCtx.Subscriptions.Add(follow);

            user.FollowingsCount++;
            followingUser.FollowersCount++;
            await _accountCtx.SaveChangesAsync();

            return true;
        }

        public async Task<Boolean> UnFollow(string userid, string followingUserid)
        {
            Subscription f = _accountCtx.Subscriptions.Where(ff => ff.Userid == userid && ff.FollowingUserid == followingUserid).First();
            if(f != null){
                _accountCtx.Subscriptions.Remove(f);

                UserProfile user = _accountCtx.Users.Find(userid);
                UserProfile followingUser = _accountCtx.Users.Find(followingUserid);
                user.FollowingsCount--;
                followingUser.FollowersCount--;

                await _accountCtx.SaveChangesAsync();
            }
            return true;
        }

        public List<UserProfile> Followings(string userid)
        {
            UserProfile user = _accountCtx.Users.Find(userid);
            if (user == null)
            {
                throw new UserNotFoundException(userid);
            }

            return _accountCtx.Users.SqlQuery(
                @"select FollowingUserid as Userid, DisplayName, PortraitUrl, Description, FollowingsCount, FollowersCount, Password from (
	                select f.FollowingUserid, f.Userid, DisplayName, PortraitUrl, Description, FollowingsCount, FollowersCount, Password from 
		                [MSGorilla.Library.DAL.MSGorillaContext].[dbo].[Subscription] f
		                join
		                [MSGorilla.Library.DAL.MSGorillaContext].[dbo].[UserProfile] u
		                on f.FollowingUserid = u.Userid 
		                ) ff 
	                where ff.userid = {0}",
                    new object[] { userid }
                ).ToList();
        }

        public List<UserProfile> Followers(string userid)
        {
            UserProfile user = _accountCtx.Users.Find(userid);
            if (user == null)
            {
                throw new UserNotFoundException(userid);
            }

            return _accountCtx.Users.SqlQuery(
                @"select Userid, DisplayName, PortraitUrl, Description, FollowingsCount, FollowersCount, Password from (
		                select f.FollowingUserid, f.Userid, DisplayName, PortraitUrl, Description, FollowingsCount, FollowersCount, Password from 
			                [MSGorilla.Library.DAL.MSGorillaContext].[dbo].[Subscription] f
			                join
			                [MSGorilla.Library.DAL.MSGorillaContext].[dbo].[UserProfile] u
			                on f.Userid = u.Userid 
	                ) ff 
	                where ff.FollowingUserid = {0}",
                    new object[] { userid }
                ).ToList();
        }
    }
}
