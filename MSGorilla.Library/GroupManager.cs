using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MSGorilla.Library.DAL;
using MSGorilla.Library.Exceptions;
using MSGorilla.Library.Models.SqlModels;
using MSGorilla.Library.Models.ViewModels;

namespace MSGorilla.Library
{
    public class GroupManager
    {
        public Group GetGroupByID(string GroupID, MSGorillaContext ctx = null)
        {
            if (ctx == null)
            {
                using (ctx = new MSGorillaContext())
                {
                    return ctx.Groups.Find(GroupID);
                }
            }
            return ctx.Groups.Find(GroupID);
        }

        public List<Group> GetAllGroup()
        {
            using (var ctx = new MSGorillaContext())
            {
                return ctx.Groups.ToList();
            }
        }

        public Group AddGroup(string creater, string groupID, string displayName, string description, bool isOpen)
        {
            if (!Utils.IsValidID(groupID))
            {
                throw new InvalidIDException();
            }

            using (var ctx = new MSGorillaContext())
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
                ctx.Groups.Add(group);
                //add creater as default admin
                Membership member = new Membership();
                member.GroupID = groupID;
                member.MemberID = creater;
                member.Role = "admin";
                ctx.Memberships.Add(member);

                ctx.SaveChanges();
                return group;
            }
        }

        public Group UpdateGroup(string admin, Group group)
        {
            using (var ctx = new MSGorillaContext())
            {
                Group groupUpdate = ctx.Groups.Find(group.GroupID);
                if (groupUpdate == null)
                {
                    throw new GroupNotExistException();
                }
                groupUpdate.Description = group.Description;
                groupUpdate.DisplayName = group.DisplayName;
                groupUpdate.IsOpen = group.IsOpen;

                ctx.SaveChanges();
                return groupUpdate;
            }
        }

        public Membership JoinGroup(string userid, string groupID)
        {
            using(var ctx = new MSGorillaContext())
            {
                Group group = GetGroupByID(groupID, ctx);
                if (group == null)
                {
                    throw new GroupNotExistException();
                }

                UserProfile user = ctx.Users.Find(userid);
                if (user == null)
                {
                    throw new UserNotFoundException(userid);
                }

                if (user.IsRobot)
                {
                    throw new HandleRobotMembershipException();
                }

                Membership member = ctx.Memberships.SqlQuery(
                    "select * from membership where groupid={0} and memberid={1}",
                    groupID,
                    userid).FirstOrDefault();

                if (member != null)
                {
                    return member;
                }

                if (group.IsOpen == false)
                {
                    throw new UnauthroizedActionException();
                }

                member = new Membership();
                member.GroupID = groupID;
                member.MemberID = userid;
                member.Role = "user";
                ctx.Memberships.Add(member);
                ctx.SaveChanges();

                return member;
            }
        }

        public Membership CheckMembership(string groupID, string userid)
        {
            using (var ctx = new MSGorillaContext())
            {
                return CheckMembership(groupID, userid, ctx);
            }
        }

        public Membership CheckMembership(string groupID, string userid, MSGorillaContext ctx)
        {
            Group group = ctx.Groups.Find(groupID);
            if (group == null)
            {
                throw new GroupNotExistException();
            }

            Membership member = ctx.Memberships.SqlQuery(
                "select * from membership where groupid={0} and memberid={1}",
                groupID,
                userid).FirstOrDefault();

            if (member == null)
            {
                throw new UnauthroizedActionException();
            }

            return member;
        }

        public void CheckAdmin(string groupID, string userid)
        {
            using (var ctx = new MSGorillaContext())
            {
                CheckAdmin(groupID, userid, ctx);
            }
        }

        public void CheckAdmin(string groupID, string userid, MSGorillaContext ctx)
        {
            if (!"admin".Equals(CheckMembership(groupID, userid, ctx).Role))
            {
                throw new UnauthroizedActionException();
            }
        }

        public Membership AddMember(string groupID, string userid, string role = "user")
        {
            using (var ctx = new MSGorillaContext())
            {
                UserProfile user = ctx.Users.Find(userid);
                if (user == null)
                {
                    throw new UserNotFoundException(userid);
                }

                if (user.IsRobot == true)
                {
                    throw new HandleRobotMembershipException();
                }

                Membership member = ctx.Memberships.Where(m => m.GroupID == groupID && m.MemberID == userid).FirstOrDefault();
                if (member == null)
                {
                    member = new Membership();
                    member.GroupID = groupID;
                    member.MemberID = user.Userid;
                    member.Role = role;
                    ctx.Memberships.Add(member);

                    ctx.SaveChanges();
                }
                
                return member;
            }
        }

        public void RemoveMember(string groupID, string admin, string userid)
        {
            using (var ctx = new MSGorillaContext())
            {
                Membership member = ctx.Memberships.SqlQuery(
                    "select * from membership where groupid={0} and memberid={1}",
                    groupID,
                    userid).FirstOrDefault();
                if (member != null)
                {
                    ctx.Memberships.Remove(member);
                }
                ctx.SaveChanges();
            }
        }

        public Membership UpdateMembership(string groupID, string admin, string userid, string role)
        {
            using (var ctx = new MSGorillaContext())
            {
                Membership member = CheckMembership(groupID, userid, ctx);
                if ("robot".Equals(member.Role))
                {
                    throw new HandleRobotMembershipException();
                }
                member.Role = role;
                ctx.SaveChanges();
                return member;
            }
        }

        public List<Membership> GetAllGroupMember(string groupID, string userid)
        {
            using (var ctx = new MSGorillaContext())
            {
                return ctx.Memberships.SqlQuery("select * from membership where groupid={0}", groupID).ToList<Membership>();
            }
        }

        public List<DisplayMembership> GetJoinedGroup(string userid)
        {
            using (var ctx = new MSGorillaContext())
            {
                return ctx.Database.SqlQuery<DisplayMembership>(
                    @"select g.GroupID, g.DisplayName, g.Description, g.IsOpen, m.MemberID, m.Role 
                        from membership m join [group] g on m.groupid = g.groupid where memberid={0}",
                    userid
                    ).ToList();
            }
        }

        public List<DisplayMembership> GetOwnedGroup(string userid)
        {
            using (var ctx = new MSGorillaContext())
            {
                return ctx.Database.SqlQuery<DisplayMembership>(
                    @"select g.GroupID, g.DisplayName, g.Description, g.IsOpen, m.MemberID, m.Role 
                        from membership m join [group] g on m.groupid = g.groupid where memberid={0} and role='admin'",
                    userid
                    ).ToList();
            }
        }

        public UserProfile CreateGroupRobotAccount(string groupID, string admin, UserProfile robot)
        {
            using (var ctx = new MSGorillaContext())
            {
                robot.IsRobot = true;
                new AccountManager().AddUser(robot);

                Membership member = new Membership();
                member.GroupID = groupID;
                member.MemberID = robot.Userid;
                member.Role = "robot";
                ctx.Memberships.Add(member);

                ctx.SaveChanges();

                return robot;
            }
        }
    }
}
