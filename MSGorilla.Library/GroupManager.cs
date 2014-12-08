using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using MSGorilla.Library.Exceptions;
using MSGorilla.Library.Models.SqlModels;
using MSGorilla.Library.Models.ViewModels;

namespace MSGorilla.Library
{
    public class GroupManager
    {
        public Group GetGroupByID(string GroupID, MSGorillaEntities _gorillaCtx = null)
        {
            if (_gorillaCtx == null)
            {
                using (_gorillaCtx = new MSGorillaEntities())
                {
                    return _gorillaCtx.Groups.Find(GroupID);
                }
            }
            return _gorillaCtx.Groups.Find(GroupID);
        }

        public List<Group> GetAllGroup()
        {
            using (var _gorillaCtx = new MSGorillaEntities())
            {
                return GetAllGroup(_gorillaCtx);
            }
        }

        public List<Group> GetAllGroup(MSGorillaEntities _gorillaCtx)
        {
            return _gorillaCtx.Groups.ToList();
        }

        public Group CreateGroup(string creater, string groupID, string displayName, string description, bool isOpen)
        {
            if (!Utils.IsValidID(groupID))
            {
                throw new InvalidIDException();
            }

            using (var _gorillaCtx = new MSGorillaEntities())
            {
                Group group = GetGroupByID(groupID);
                if (group != null)
                {
                    throw new GroupAlreadyExistException(groupID);
                }

                //add group
                group = new Group();
                group.GroupID = groupID;
                group.DisplayName = displayName;
                group.Description = description;
                group.IsOpen = isOpen;
                _gorillaCtx.Groups.Add(group);
                //add creater as default admin
                Membership member = new Membership();
                member.GroupID = groupID;
                member.MemberID = creater;
                member.Role = "admin";
                _gorillaCtx.Memberships.Add(member);

                _gorillaCtx.SaveChanges();
                return group;
            }
        }

        public Group UpdateGroup(string admin, Group group)
        {
            using (var _gorillaCtx = new MSGorillaEntities())
            {
                Group groupUpdate = _gorillaCtx.Groups.Find(group.GroupID);
                if (groupUpdate == null)
                {
                    throw new GroupNotExistException();
                }
                groupUpdate.Description = group.Description;
                groupUpdate.DisplayName = group.DisplayName;
                groupUpdate.IsOpen = group.IsOpen;

                _gorillaCtx.SaveChanges();
                return groupUpdate;
            }
        }

        public Membership JoinGroup(string userid, string groupID)
        {
            using(var _gorillaCtx = new MSGorillaEntities())
            {
                Group group = GetGroupByID(groupID, _gorillaCtx);
                if (group == null)
                {
                    throw new GroupNotExistException();
                }

                UserProfile user = _gorillaCtx.UserProfiles.Find(userid);
                if (user == null)
                {
                    throw new UserNotFoundException(userid);
                }

                if (user.IsRobot)
                {
                    throw new UpdateRobotMembershipException();
                }

                Membership member = _gorillaCtx.Memberships.SqlQuery(
                    "select * from membership where groupid={0} and memberid={1}",
                    groupID,
                    userid).FirstOrDefault();

                if (member != null)
                {
                    return member;
                }

                member = new Membership();
                member.GroupID = groupID;
                member.MemberID = userid;
                member.Role = group.IsOpen ? "user" : "pending";
                _gorillaCtx.Memberships.Add(member);
                _gorillaCtx.SaveChanges();

                return member;
            }
        }

        public void LeaveGroup(string userid, string groupID)
        {
            using (var _gorillaCtx = new MSGorillaEntities())
            {
                foreach (var member in _gorillaCtx.Memberships.Where(f => f.MemberID == userid && f.GroupID == groupID))
                {
                    _gorillaCtx.Memberships.Remove(member);
                }

                UserProfile user = _gorillaCtx.UserProfiles.Find(userid);
                if (user.DefaultGroup.Equals(groupID, StringComparison.InvariantCultureIgnoreCase))
                {
                    user.DefaultGroup = "microsoft";
                }
                _gorillaCtx.SaveChanges();
            }
        }

        public Membership CheckMembership(string groupID, string userid)
        {
            using (var _gorillaCtx = new MSGorillaEntities())
            {
                return CheckMembership(groupID, userid, _gorillaCtx);
            }
        }

        public Membership CheckMembership(string groupID, string userid, MSGorillaEntities _gorillaCtx)
        {
            Group group = _gorillaCtx.Groups.Find(groupID);
            if (group == null)
            {
                throw new GroupNotExistException();
            }

            Membership member = _gorillaCtx.Memberships.SqlQuery(
                "select * from membership where groupid={0} and memberid={1}",
                groupID,
                userid).FirstOrDefault();

            if (member == null || "pending".Equals(member.Role))
            {
                throw new UnauthroizedActionException();
            }

            return member;
        }

        public void CheckAdmin(string groupID, string userid)
        {
            using (var _gorillaCtx = new MSGorillaEntities())
            {
                CheckAdmin(groupID, userid, _gorillaCtx);
            }
        }

        public void CheckAdmin(string groupID, string userid, MSGorillaEntities ctx)
        {
            if (!"admin".Equals(CheckMembership(groupID, userid, ctx).Role))
            {
                throw new UnauthroizedActionException();
            }
        }

        public Membership AddMember(string groupID, string userid, string role = "user")
        {
            using (var _gorillaCtx = new MSGorillaEntities())
            {
                UserProfile user = _gorillaCtx.UserProfiles.Find(userid);
                if (user == null)
                {
                    throw new UserNotFoundException(userid);
                }

                if (user.IsRobot == true)
                {
                    throw new UpdateRobotMembershipException();
                }

                Membership member = _gorillaCtx.Memberships.Where(m => m.GroupID == groupID && m.MemberID == userid).FirstOrDefault();
                if (member == null)
                {
                    member = new Membership();
                    member.GroupID = groupID;
                    member.MemberID = user.Userid;
                    member.Role = role;
                    _gorillaCtx.Memberships.Add(member);

                    _gorillaCtx.SaveChanges();
                }
                else
                {
                    member.Role = "user";
                    _gorillaCtx.SaveChanges();
                }
                
                return member;
            }
        }

        public void RemoveMember(string groupID, string admin, string userid)
        {
            using (var _gorillaCtx = new MSGorillaEntities())
            {
                Membership member = _gorillaCtx.Memberships.SqlQuery(
                    "select * from membership where groupid={0} and memberid={1}",
                    groupID,
                    userid).FirstOrDefault();
                if (member != null)
                {
                    _gorillaCtx.Memberships.Remove(member);
                    UserProfile removedUser = _gorillaCtx.UserProfiles.Find(userid);
                    removedUser.DefaultGroup = "microsoft";
                }
                _gorillaCtx.SaveChanges();
            }
        }

        public Membership UpdateMembership(string groupID, string admin, string userid, string role)
        {
            using (var _gorillaCtx = new MSGorillaEntities())
            {
                Membership member = CheckMembership(groupID, userid, _gorillaCtx);
                if ("robot".Equals(member.Role))
                {
                    throw new UpdateRobotMembershipException();
                }
                member.Role = role;
                _gorillaCtx.SaveChanges();
                return member;
            }
        }

        public List<Membership> GetAllGroupMember(string groupID)
        {
            using (var _gorillaCtx = new MSGorillaEntities())
            {
                return GetAllGroupMember(groupID, _gorillaCtx);
            }
        }

        public List<Membership> GetAllGroupMember(string groupID, MSGorillaEntities _gorillaCtx)
        {
            return _gorillaCtx.Memberships.SqlQuery("select * from membership where groupid={0}", groupID).ToList<Membership>();
        }

        public List<MembershipView> GetGroupMembershipView(string groupID, string userid)
        {
            using (var _gorillaCtx = new MSGorillaEntities())
            {
                UserProfile user = _gorillaCtx.UserProfiles.Find(userid);
                if (user == null)
                {
                    throw new UserNotFoundException(userid);
                }

                HashSet<string> followings = new HashSet<string>();
                foreach (var u in user.Subscriptions)
                {
                    followings.Add(u.FollowingUserid);
                }

                List<Membership> members = GetAllGroupMember(groupID, _gorillaCtx);
                List<string> users = new List<string>();
                foreach(var mem in members){
                    users.Add(mem.MemberID);
                }

                Dictionary<string, UserProfile> map = new Dictionary<string, UserProfile>();
                foreach(var userprofile in _gorillaCtx.UserProfiles.Where(u => users.Contains(u.Userid)))
                {
                    map.Add(userprofile.Userid, userprofile);
                }

                List<MembershipView> memberViews = new List<MembershipView>();
                foreach (var m in members)
                {
                    user = map[m.MemberID];
                    int isfollowing = userid.Equals(user.Userid) ? -1 : (followings.Contains(user.Userid) ? 1 : 0);
                    DisplayUserProfile dup = new DisplayUserProfile(user, isfollowing);
                    MembershipView mv = new MembershipView();
                    mv.GroupID = groupID;
                    mv.Role = m.Role;
                    mv.User = dup;

                    memberViews.Add(mv);
                }
                return memberViews;
            }
        }

        public List<DisplayMembership> GetJoinedGroup(string userid)
        {
            using (var _gorillaCtx = new MSGorillaEntities())
            {
                List<DisplayMembership> result =  _gorillaCtx.Database.SqlQuery<DisplayMembership>(
                    @"select g.GroupID, g.DisplayName, g.Description, g.IsOpen, m.MemberID, m.Role 
                        from membership m join [group] g on m.groupid = g.groupid where memberid={0} and role != 'pending'",
                    userid
                    ).ToList();
                UserProfile user = _gorillaCtx.UserProfiles.Find(userid);

                for (int i = 0; i < result.Count; i++ )
                {
                    if (result[i].GroupID.Equals(user.DefaultGroup, StringComparison.InvariantCultureIgnoreCase))
                    {
                        DisplayMembership temp = result[i];
                        result[i] = result[0];
                        result[0] = temp;
                        break;
                    }
                }
                return result;
            }
        }

        public List<DisplayMembership> GetOwnedGroup(string userid)
        {
            using (var _gorillaCtx = new MSGorillaEntities())
            {
                return _gorillaCtx.Database.SqlQuery<DisplayMembership>(
                    @"select g.GroupID, g.DisplayName, g.Description, g.IsOpen, m.MemberID, m.Role 
                        from membership m join [group] g on m.groupid = g.groupid where memberid={0} and role='admin'",
                    userid
                    ).ToList();
            }
        }

        public void SetDefaultGroup(string groupID, string userid)
        {
            using (var _gorillaCtx = new MSGorillaEntities())
            {
                UserProfile user = _gorillaCtx.UserProfiles.Find(userid);
                if (user == null)
                {
                    throw new UserNotFoundException(userid);
                }

                user.DefaultGroup = groupID;
                _gorillaCtx.SaveChanges();
            }
        }

        public Group GetDefaultGroup(string userid)
        {
            using (var _gorillaCtx = new MSGorillaEntities())
            {
                UserProfile user = _gorillaCtx.UserProfiles.Find(userid);
                if (user == null)
                {
                    throw new UserNotFoundException(userid);
                }

                return user.Group;
            }
        }

        public UserProfile CreateGroupRobotAccount(string groupID, string admin, UserProfile robot)
        {
            using (var _gorillaCtx = new MSGorillaEntities())
            {
                robot.IsRobot = true;
                try
                {
                    new AccountManager().AddUser(robot);
                }
                catch (UpdateRobotMembershipException e) { } 

                Membership member = new Membership();
                member.GroupID = groupID;
                member.MemberID = robot.Userid;
                member.Role = "robot";
                _gorillaCtx.Memberships.Add(member);

                _gorillaCtx.SaveChanges();

                return robot;
            }
        }
    }
}
