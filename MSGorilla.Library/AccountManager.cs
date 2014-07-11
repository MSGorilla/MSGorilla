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
        //private MSGorillaContext _gorillaCtx;
        public AccountManager(){
            //_accountCtx = new MSGorillaContext();
        }

        public List<UserProfile> GetActiveUsers(int count)
        {
            using (var _gorillaCtx = new MSGorillaContext())
            {
                return _gorillaCtx.Users.SqlQuery(
                    @"select top({0}) Userid, DisplayName, PortraitUrl, Description, FollowingsCount, FollowersCount, Password, MessageCount from [UserProfile] order by MessageCount desc",
                    new object[] { count }
                ).ToList();
            }
        }

        public List<UserProfile> GetAllUsers(){
            using (var _gorillaCtx = new MSGorillaContext())
            {
                return _gorillaCtx.Users.ToList();
            }            
        }

        public bool AuthenticateUser(string userid, string password)
        {
            if (string.IsNullOrEmpty(userid))
            {
                return false;
            }

            using (var _gorillaCtx = new MSGorillaContext())
            {
                UserProfile user = _gorillaCtx.Users.Find(userid);

                if (user.Password == null)
                {
                    return false;
                }
                if ("".Equals(user.Password))
                {
                    return true;
                }
                return user.Password.Equals(password);
            }            
        }

        public UserProfile FindUser(string userid)
        {
            using (var _gorillaCtx = new MSGorillaContext())
            {
                return _gorillaCtx.Users.Find(userid);
            }            
        }

        public List<UserProfile> SearchUser(string keyword)
        {
            using (var _gorillaCtx = new MSGorillaContext())
            {
                var users = _gorillaCtx.Users.Where(u => u.Userid.Contains(keyword));
                return users.ToList<UserProfile>();
            }            
        }

        public UserProfile AddUser(UserProfile user)
        {
            if (!Utils.IsValidID(user.Userid))
            {
                throw new InvalidIDException("User");
            }

            using (var _gorillaCtx = new MSGorillaContext())
            {
                UserProfile temp = _gorillaCtx.Users.Find(user.Userid);
                if (temp != null)
                {
                    throw new UserAlreadyExistException(user.Userid);
                }
                user.MessageCount = 0;
                _gorillaCtx.Users.Add(user);
                _gorillaCtx.SaveChanges();
                return user;
            }            
        }

        public UserProfile UpdateUser(UserProfile user)
        {
            using (var _gorillaCtx = new MSGorillaContext())
            {
                UserProfile originUser = _gorillaCtx.Users.Find(user.Userid);
                if (originUser == null)
                {
                    throw new UserNotFoundException(user.Userid);
                }

                originUser.Password = user.Password;
                originUser.DisplayName = user.DisplayName;
                originUser.Description = user.Description;
                originUser.PortraitUrl = user.PortraitUrl;
                _gorillaCtx.SaveChanges();

                return _gorillaCtx.Users.Find(user.Userid);
            }
        }

        public async Task<Boolean> Follow(string userid, string followingUserid)
        {
            using (var _gorillaCtx = new MSGorillaContext())
            {
                UserProfile user = _gorillaCtx.Users.Find(userid);
                UserProfile followingUser = _gorillaCtx.Users.Find(followingUserid);
                if (user == null || followingUser == null)
                {
                    throw new UserNotFoundException(string.Format("{0} or {1}", userid, followingUserid));
                }

                if (_gorillaCtx.Subscriptions.Where(f => f.Userid == userid && f.FollowingUserid == followingUserid).ToList().Count > 0)
                {
                    return true;
                }

                Subscription follow = new Subscription();
                follow.Userid = userid;
                follow.FollowingUserid = followingUserid;
                follow.FollowingUserDisplayName = "";
                _gorillaCtx.Subscriptions.Add(follow);

                user.FollowingsCount++;
                followingUser.FollowersCount++;
                await _gorillaCtx.SaveChangesAsync();

                return true;
            }
        }

        public async Task<Boolean> UnFollow(string userid, string followingUserid)
        {
            using (var _gorillaCtx = new MSGorillaContext())
            {
                Subscription f = _gorillaCtx.Subscriptions.Where(ff => ff.Userid == userid && ff.FollowingUserid == followingUserid).FirstOrDefault();
                if (f != null)
                {
                    _gorillaCtx.Subscriptions.Remove(f);

                    UserProfile user = _gorillaCtx.Users.Find(userid);
                    UserProfile followingUser = _gorillaCtx.Users.Find(followingUserid);
                    user.FollowingsCount--;
                    followingUser.FollowersCount--;

                    await _gorillaCtx.SaveChangesAsync();
                }
                return true;
            }            
        }

        public List<UserProfile> Followings(string userid)
        {
            using (var _gorillaCtx = new MSGorillaContext())
            {
                UserProfile user = _gorillaCtx.Users.Find(userid);
                if (user == null)
                {
                    throw new UserNotFoundException(userid);
                }

                return _gorillaCtx.Users.SqlQuery(
                    @"select FollowingUserid as Userid, DisplayName, PortraitUrl, Description, FollowingsCount, FollowersCount, Password, MessageCount from (
	                select f.FollowingUserid, f.Userid, DisplayName, PortraitUrl, Description, FollowingsCount, FollowersCount, Password, MessageCount from 
		                [Subscription] f
		                join
		                [UserProfile] u
		                on f.FollowingUserid = u.Userid 
		                ) ff 
	                where ff.userid = {0}",
                        new object[] { userid }
                    ).ToList();
            }
        }

        public List<UserProfile> Followers(string userid)
        {
            using (var _gorillaCtx = new MSGorillaContext())
            {
                UserProfile user = _gorillaCtx.Users.Find(userid);
                if (user == null)
                {
                    throw new UserNotFoundException(userid);
                }

                return _gorillaCtx.Users.SqlQuery(
                    @"select Userid, DisplayName, PortraitUrl, Description, FollowingsCount, FollowersCount, Password, MessageCount from (
		                select f.FollowingUserid, f.Userid, DisplayName, PortraitUrl, Description, FollowingsCount, FollowersCount, Password, MessageCount from 
			                [Subscription] f
			                join
			                [UserProfile] u
			                on f.Userid = u.Userid 
	                ) ff 
	                where ff.FollowingUserid = {0}",
                        new object[] { userid }
                    ).ToList();
            }
        }

        public bool IsFollowing(string userid, string followingUserID)
        {
            using (var _gorillaCtx = new MSGorillaContext())
            {
                if (_gorillaCtx.Subscriptions.Where(f => f.Userid == userid && f.FollowingUserid == followingUserID).ToList().Count > 0)
                {
                    return true;
                }
                return false;
            }            
        }

        public void DeleteUser(string userid)
        {
            using (var _gorillaCtx = new MSGorillaContext())
            {
                UserProfile user = _gorillaCtx.Users.Find(userid);
                if (user != null)
                {
                    _gorillaCtx.Users.Remove(user);
                    _gorillaCtx.SaveChanges();
                }
                else
                {
                    throw new UserNotFoundException(userid);
                }
            }
        }
    }
}
