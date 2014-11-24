using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

using MSGorilla.Library.Models;
using MSGorilla.Library.Models.SqlModels;
using MSGorilla.Library.Exceptions;

namespace MSGorilla.Library
{
    public class AccountManager
    {
        //private MSGorillaEntities _gorillaCtx;
        public AccountManager(){
            //_accountCtx = new MSGorillaEntities();
        }

        public List<UserProfile> GetActiveUsers(int count)
        {
            using (var _gorillaCtx = new MSGorillaEntities())
            {
                return _gorillaCtx.UserProfiles.OrderByDescending(user => user.MessageCount).Take(count).ToList();
                //return _gorillaCtx.UserProfiles.SqlQuery(
                //    @"select top({0}) * from [UserProfile] order by MessageCount desc",
                //    new object[] { count }
                //).ToList();
            }
        }

        public List<UserProfile> GetAllUsers(){
            using (var _gorillaCtx = new MSGorillaEntities())
            {
                return _gorillaCtx.UserProfiles.ToList();
            }            
        }

        public bool AuthenticateUser(string userid, string password)
        {
            if (string.IsNullOrEmpty(userid))
            {
                return false;
            }

            using (var _gorillaCtx = new MSGorillaEntities())
            {
                UserProfile user = _gorillaCtx.UserProfiles.Find(userid);

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
            using (var _gorillaCtx = new MSGorillaEntities())
            {
                return _gorillaCtx.UserProfiles.Find(userid);
            }            
        }

        public List<UserProfile> SearchUser(string keyword)
        {
            using (var _gorillaCtx = new MSGorillaEntities())
            {
                var users = _gorillaCtx.UserProfiles.Where(u => u.Userid.Contains(keyword));
                return users.ToList<UserProfile>();
            }            
        }

        public UserProfile AddUser(UserProfile user)
        {
            if (!Utils.IsValidID(user.Userid))
            {
                throw new InvalidIDException("User");
            }

            using (var _gorillaCtx = new MSGorillaEntities())
            {
                UserProfile temp = _gorillaCtx.UserProfiles.Find(user.Userid);
                if (temp != null)
                {
                    throw new UserAlreadyExistException(user.Userid);
                }
                user.MessageCount = 0;
                if (string.IsNullOrEmpty(user.DefaultGroup))
                {
                    user.DefaultGroup = "default";
                }
                _gorillaCtx.UserProfiles.Add(user);
                _gorillaCtx.SaveChanges();

                new GroupManager().AddMember(user.DefaultGroup, user.Userid);

                return user;
            }            
        }

        public UserProfile UpdateUser(UserProfile user)
        {
            using (var _gorillaCtx = new MSGorillaEntities())
            {
                UserProfile originUser = _gorillaCtx.UserProfiles.Find(user.Userid);
                if (originUser == null)
                {
                    throw new UserNotFoundException(user.Userid);
                }

                originUser.Password = user.Password;
                originUser.DisplayName = user.DisplayName;
                originUser.Description = user.Description;
                originUser.PortraitUrl = user.PortraitUrl;
                originUser.MessageCount = user.MessageCount;

                _gorillaCtx.SaveChanges();

                return _gorillaCtx.UserProfiles.Find(user.Userid);
            }
        }

        public async Task<Boolean> Follow(string userid, string followingUserid)
        {
            using (var _gorillaCtx = new MSGorillaEntities())
            {
                UserProfile user = _gorillaCtx.UserProfiles.Find(userid);
                UserProfile followingUser = _gorillaCtx.UserProfiles.Find(followingUserid);
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
            using (var _gorillaCtx = new MSGorillaEntities())
            {
                Subscription f = _gorillaCtx.Subscriptions.Where(ff => ff.Userid == userid && ff.FollowingUserid == followingUserid).FirstOrDefault();
                if (f != null)
                {
                    _gorillaCtx.Subscriptions.Remove(f);

                    UserProfile user = _gorillaCtx.UserProfiles.Find(userid);
                    UserProfile followingUser = _gorillaCtx.UserProfiles.Find(followingUserid);
                    user.FollowingsCount--;
                    followingUser.FollowersCount--;

                    await _gorillaCtx.SaveChangesAsync();
                }
                return true;
            }            
        }

        public List<UserProfile> Followings(string userid)
        {
            using (var _gorillaCtx = new MSGorillaEntities())
            {
                UserProfile user = _gorillaCtx.UserProfiles.Find(userid);
                if (user == null)
                {
                    throw new UserNotFoundException(userid);
                }

                return  (
                            from u in _gorillaCtx.UserProfiles join su in _gorillaCtx.Subscriptions 
                                on u.Userid equals su.FollowingUserid 
                                where su.Userid == userid select u
                        ).ToList<UserProfile>();

//                return _gorillaCtx.UserProfiles.SqlQuery(
//                    @"select FollowingUserid as Userid, DisplayName, PortraitUrl, Description, FollowingsCount, FollowersCount, Password, MessageCount, IsRobot from (
//	                select f.FollowingUserid, f.Userid, DisplayName, PortraitUrl, Description, FollowingsCount, FollowersCount, Password, MessageCount, IsRobot from 
//		                [Subscription] f
//		                join
//		                [UserProfile] u
//		                on f.FollowingUserid = u.Userid 
//		                ) ff 
//	                where ff.userid = {0}",
//                        new object[] { userid }
//                    ).ToList();
            }
        }

        public List<UserProfile> Followers(string userid)
        {
            using (var _gorillaCtx = new MSGorillaEntities())
            {
                UserProfile user = _gorillaCtx.UserProfiles.Find(userid);
                if (user == null)
                {
                    throw new UserNotFoundException(userid);
                }

                return (
                            from u in _gorillaCtx.UserProfiles
                            join su in _gorillaCtx.Subscriptions
                                on u.Userid equals su.Userid
                            where su.FollowingUserid == userid
                            select u
                        ).ToList<UserProfile>();

//                return _gorillaCtx.UserProfiles.SqlQuery(
//                    @"select Userid, DisplayName, PortraitUrl, Description, FollowingsCount, FollowersCount, Password, MessageCount from (
//		                select f.FollowingUserid, f.Userid, DisplayName, PortraitUrl, Description, FollowingsCount, FollowersCount, Password, MessageCount from 
//			                [Subscription] f
//			                join
//			                [UserProfile] u
//			                on f.Userid = u.Userid 
//	                ) ff 
//	                where ff.FollowingUserid = {0}",
//                        new object[] { userid }
//                    ).ToList();
            }
        }

        public bool IsFollowing(string userid, string followingUserID)
        {
            using (var _gorillaCtx = new MSGorillaEntities())
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
            using (var _gorillaCtx = new MSGorillaEntities())
            {
                UserProfile user = _gorillaCtx.UserProfiles.Find(userid);
                if (user != null)
                {
                    _gorillaCtx.UserProfiles.Remove(user);
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
