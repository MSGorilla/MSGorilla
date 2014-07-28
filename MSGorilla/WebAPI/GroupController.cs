﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net;
using System.Web.Http;
using MSGorilla.Library;
using MSGorilla.Library.Models.SqlModels;
using MSGorilla.Library.Exceptions;
using MSGorilla.Library.Models.ViewModels;
using MSGorilla.Library.Models;

using MSGorilla.Utility;

namespace MSGorilla.WebAPI
{
    public class GroupController : BaseController
    {
        private GroupManager _groupManager = new GroupManager();

        /// <summary>
        /// Add a new Group. You'll be the default admin of the group
        /// 
        /// Example output:
        /// {
        ///     "GroupID": "microsoft",
        ///     "DisplayName": "Microsoft",
        ///     "Description": "default group for all active users",
        ///     "IsOpen": true
        /// }
        /// </summary>
        /// <param name="groupID">group id</param>
        /// <param name="displayName">display group name</param>
        /// <param name="description">description of the group</param>
        /// <param name="isOpen">If the group is open, userid do not need admin's permission to join the group</param>
        /// <returns></returns>
        [HttpGet, HttpPost]
        public Group AddGroup(string groupID, string displayName = null, string description = null, bool isOpen = false)
        {
            string me = whoami();

            if(string.IsNullOrEmpty(displayName)){
                displayName = groupID;
            }
            Group group =  _groupManager.AddGroup(me, groupID, displayName, description, isOpen);

            MembershipHelper.RefreshJoinedGroup(me);
            return group;
        }

        /// <summary>
        /// Find a Group by id.
        /// 
        /// Example output:
        /// {
        ///     "GroupID": "microsoft",
        ///     "DisplayName": "Microsoft",
        ///     "Description": "default group for all active users",
        ///     "IsOpen": true
        /// }
        /// </summary>
        /// <param name="groupID">group id</param>
        /// <returns></returns>
        [HttpGet]
        public Group GetGroup(string groupID)
        {
            string me = whoami();
            Group group = _groupManager.GetGroupByID(groupID);
            if (group == null)
            {
                throw new GroupNotExistException();
            }
            return group;
        }

        [HttpGet]
        public List<Group> GetAllGroup()
        {
            string me = whoami();
            return _groupManager.GetAllGroup();
        }

        [HttpGet]
        public Group UpdateGroup(string groupID, string displayname = null, string description = null, bool isOpen = false)
        {
            string me = whoami();
            MembershipHelper.CheckAdmin(groupID, me);

            Group group = new Group(groupID, displayname, description, isOpen);            
            return _groupManager.UpdateGroup(me, group);
        }

        [HttpGet, HttpPost]
        public Membership JoinGroup(string groupID)
        {
            string me = whoami();
            Membership member = _groupManager.JoinGroup(whoami(), groupID);
            MembershipHelper.RefreshJoinedGroup(me);
            return member;
        }

        [HttpGet, HttpPost, HttpPut]
        public Membership AddMember(string userid, string groupID)
        {
            string me = whoami();
            MembershipHelper.CheckAdmin(groupID, me);
            Membership member = _groupManager.AddMember(groupID, userid);
            MembershipHelper.RefreshJoinedGroup(userid);
            return member;
        }

        [HttpGet, HttpPost, HttpDelete]
        public ActionResult RemoveMember(string userid, string groupID)
        {
            string me = whoami();
            MembershipHelper.CheckAdmin(groupID, me);
            _groupManager.RemoveMember(groupID, whoami(), userid);
            return ActionResult.Success();
        }

        [HttpGet, HttpPost, HttpPut]
        public Membership UpdateMembership(string groupID, string userid, string role)
        {
            string me = whoami();
            MembershipHelper.CheckAdmin(groupID, me);
            return _groupManager.UpdateMembership(groupID, whoami(), userid, role);
        }

        [HttpGet]
        public List<Membership> GetAllGroupMember(string groupID)
        {
            string me = whoami();
            MembershipHelper.CheckMembership(groupID, me);
            return _groupManager.GetAllGroupMember(groupID, whoami());
        }

        [HttpGet]
        public List<DisplayMembership> GetJoinedGroup()
        {
            return _groupManager.GetJoinedGroup(whoami());
        }

        [HttpGet]
        public List<DisplayMembership> GetOwnedGroup()
        {
            return _groupManager.GetOwnedGroup(whoami());
        }

        [HttpGet, HttpPost]
        public UserProfile CreateGroupRobotAccount(string groupID, string userid, string password, string displayname, string description)
        {
            string me = whoami();
            MembershipHelper.CheckAdmin(groupID, me);

            UserProfile robot = new UserProfile();
            robot.Userid = userid;
            robot.DisplayName = displayname;
            robot.Description = description;
            robot.FollowingsCount = 0;
            robot.FollowersCount = 0;
            robot.MessageCount = 0;
            robot.IsRobot = true;
            robot.Password = Utils.MD5Encoding(password);

            return _groupManager.CreateGroupRobotAccount(groupID, me, robot);
        }
    }
}