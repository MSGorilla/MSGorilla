using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net;
using System.Net.Http;
using System.Web.Http;

using MSGorilla.Library;
using MSGorilla.Utility;
using MSGorilla.Library.Models.SqlModels;
using MSGorilla.Library.Models.ViewModels;
using MSGorilla.Library.Exceptions;
using MSGorilla.Library.Models;

namespace MSGorilla.WebAPI
{
    public class CategoryController : BaseController
    {
        private CategoryManager _categoryManager = new CategoryManager();

        private DisplayCategoryMessage ToDisplayCategoryMessage(CategoryMessage msg, string userid)
        {
            if (msg == null)
            {
                return null;
            }
            DisplayCategoryMessage displayMsg = new DisplayCategoryMessage();
            displayMsg.User = msg.User;
            displayMsg.ID = msg.ID;
            displayMsg.PostTime = msg.PostTime;
            displayMsg.CategoryID = msg.CategoryID;
            displayMsg.CategoryName = msg.CategoryName;
            displayMsg.EventIDs = msg.EventIDs;

            displayMsg.NotifyTo = AccountController.GetSimpleUserProfile(msg.NotifyTo);
            displayMsg.Group = GroupController.GetDisplayGroup(msg.Group, userid);
            return displayMsg;
        }

        public DisplayCategory ToDisplayCategory(Category category, string userid)
        {
            DisplayCategory dcat = category;
            CategoryMessage msg = _categoryManager.RetriveCategoryMessage(userid, category);
            if (msg != null && msg.EventIDs != null)
            {
                dcat.EventCount = msg.EventIDs.Count;
            }
            return dcat;
        }

        private static string CreateCacheCategoryKey(string name, string group)
        {
            return CacheHelper.CategoryPrefix + name + "@" + group;
        }

        /// <summary>
        /// Create new category, only admin has authorization to do this
        /// 
        /// example output:
        /// {
        ///     "ID": 1,
        ///     "Name": "testcategory",
        ///     "GroupID": "msgorilladev",
        ///     "Description": null,
        ///     "Creater": "user1",
        ///     "CreateTimestamp": "2014-08-05T01:09:54.3483546Z",
        ///     "EventCount": 0
        /// }
        /// </summary>
        /// <param name="name">category name</param>
        /// <param name="group">group id</param>
        /// <param name="description">description</param>
        /// <returns></returns>
        [HttpGet, HttpPost, HttpPut]
        public DisplayCategory CreateCategory(string name, string group = null, string description = null)
        {
            string me = whoami();
            if (string.IsNullOrEmpty(group))
            {
                group = MembershipHelper.DefaultGroup(me);
            }

            MembershipHelper.CheckAdmin(group, me);
            return _categoryManager.CreateCategory(name, group, me, description);
        }

        private Category GetCategoryModel(string name, string group = null)
        {
            string me = whoami();
            if (string.IsNullOrEmpty(group))
            {
                group = MembershipHelper.DefaultGroup(me);
            }
            else
            {
                MembershipHelper.CheckMembership(group, me);
            }

            if (CacheHelper.Contains(CreateCacheCategoryKey(name, group)))
            {
                return CacheHelper.Get<Category>(CreateCacheCategoryKey(name, group));
            }

            Category result = _categoryManager.GetCategory(name, group);
            if (result == null)
            {
                throw new CategoryNotFoundException();
            }

            CacheHelper.Add(CreateCacheCategoryKey(name, group), result);
            return result;
        }

        /// <summary>
        /// Get detail category info by name and group
        /// 
        /// example output:
        /// {
        ///     "ID": 1,
        ///     "Name": "testcategory",
        ///     "GroupID": "msgorilladev",
        ///     "Description": null,
        ///     "Creater": "user1",
        ///     "CreateTimestamp": "2014-08-05T01:09:54.3483546Z",
        ///     "EventCount": 2
        /// }
        /// </summary>
        /// <param name="name">category name</param>
        /// <param name="group">group id</param>
        /// <returns></returns>
        [HttpGet]
        public DisplayCategory GetCategory(string name, string group = null)
        {
            string me = whoami();
            return ToDisplayCategory(GetCategoryModel(name, group), me);
        }

        /// <summary>
        /// Get detail category info by id. You must be in the same group as the category
        /// 
        /// example output:
        /// {
        ///     "ID": 1,
        ///     "Name": "testcategory",
        ///     "GroupID": "msgorilladev",
        ///     "Description": null,
        ///     "Creater": "user1",
        ///     "CreateTimestamp": "2014-08-05T01:09:54.3483546Z",
        ///     "EventCount": 2
        /// }
        /// </summary>
        /// <param name="id">category id</param>
        /// <returns></returns>
        [HttpGet]
        public DisplayCategory GetCategory(int id)
        {
            string me = whoami();
            Category category = _categoryManager.GetCategory(id);
            if (category == null)
            {
                throw new CategoryNotFoundException();
            }

            if (!MembershipHelper.JoinedGroup(me).Contains(category.GroupID))
            {
                throw new UnauthroizedActionException();
            }

            return ToDisplayCategory(category, me);
        }

        /// <summary>
        /// Get my category in all my groups
        /// 
        /// example output:
        /// [
        ///     {
        ///         "ID": 1,
        ///         "Name": "testcategory",
        ///         "GroupID": "msgorilladev",
        ///         "Description": null,
        ///         "Creater": "user1",
        ///         "CreateTimestamp": "2014-08-05T01:09:54.3483546Z",
        ///         "EventCount": 2
        ///     },
        ///     ......
        ///     {
        ///         "ID": 3,
        ///         "Name": "testcategory",
        ///         "GroupID": "woss",
        ///         "Description": null,
        ///         "Creater": "user1",
        ///         "CreateTimestamp": "2014-08-05T05:31:43.5123168Z",
        ///         "EventCount": 0
        ///     }
        /// ]
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public List<DisplayCategory> GetMyCategory()
        {
            string me = whoami();
            string[] groups = MembershipHelper.JoinedGroup(me);

            List<DisplayCategory> list = new List<DisplayCategory>();
            foreach (string group in groups)
            {
                foreach (var category in _categoryManager.GetCategoryByGroup(group))
                {
                    list.Add(ToDisplayCategory(category, me));
                }
            }

            return list;
        }

        /// <summary>
        /// Get category message with the same category name in all my groups
        /// 
        /// example output:
        /// [
        ///     {
        ///         "User": "user1",
        ///         "ID": "251994817405379",
        ///         "PostTime": "2014-08-08T07:36:34.6200627Z",
        ///         "Group": {
        ///             "GroupID": "msgorilladev",
        ///             "DisplayName": "MSGorilla Dev",
        ///             "Description": "MSgorilla Devs and Testers",
        ///             "IsOpen": false,
        ///             "IsJoined": true
        ///         },
        ///         "CategoryName": "testcategory",
        ///         "CategoryID": 1,
        ///         "NotifyTo": {
        ///             "Userid": "user1",
        ///             "DisplayName": "User1",
        ///             "PortraitUrl": null,
        ///             "Description": "user for test"
        ///         },
        ///         "EventIDs": [
        ///             "1|3",
        ///             "2"
        ///         ]
        ///     },
        ///     ......
        /// ]
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        [HttpGet]
        public List<DisplayCategoryMessage> RetriveCategoryMessageByName(string name)
        {
            string me = whoami();
            string[] groups = MembershipHelper.JoinedGroup(me);
            List<DisplayCategoryMessage> list = new List<DisplayCategoryMessage>();

            foreach (var category in _categoryManager.GetCategoryByName(name, groups))
            {
                CategoryMessage msg = _categoryManager.RetriveCategoryMessage(me, category);
                if(msg != null)
                {
                    list.Add(ToDisplayCategoryMessage(msg, me));
                }
                
            }

            return list;
        }

        /// <summary>
        /// Retrive the top message in the category.
        /// 
        /// example output:
        /// [
        ///     {
        ///         "User": "user1",
        ///         "ID": "251994817405379",
        ///         "PostTime": "2014-08-08T07:36:34.6200627Z",
        ///         "Group": {
        ///             "GroupID": "msgorilladev",
        ///             "DisplayName": "MSGorilla Dev",
        ///             "Description": "MSgorilla Devs and Testers",
        ///             "IsOpen": false,
        ///             "IsJoined": true
        ///         },
        ///         "CategoryName": "testcategory",
        ///         "CategoryID": 1,
        ///         "NotifyTo": {
        ///             "Userid": "user1",
        ///             "DisplayName": "User1",
        ///             "PortraitUrl": null,
        ///             "Description": "user for test"
        ///         },
        ///         "EventIDs": [
        ///             "1|3",
        ///             "2"
        ///         ]
        ///     }
        /// ]
        /// </summary>
        /// <param name="name">category name</param>
        /// <param name="group">group id</param>
        /// <returns></returns>
        [HttpGet]
        public List<DisplayCategoryMessage> RetriveCategoryMessage(string name, string group = null)
        {
            string me = whoami();
            List<DisplayCategoryMessage> list = new List<DisplayCategoryMessage>();

            Category category = GetCategoryModel(name, group);
            CategoryMessage msg = _categoryManager.RetriveCategoryMessage(me, category);
            if (msg != null)
            {
                list.Add(ToDisplayCategoryMessage(msg, me));
            }

            return list;
        }

        /// <summary>
        /// Retrive all category messages in the group
        /// 
        /// example output:
        /// [
        ///     {
        ///         "User": "user1",
        ///         "ID": "251994817405379",
        ///         "PostTime": "2014-08-08T07:36:34.6200627Z",
        ///         "Group": {
        ///             "GroupID": "msgorilladev",
        ///             "DisplayName": "MSGorilla Dev",
        ///             "Description": "MSgorilla Devs and Testers",
        ///             "IsOpen": false,
        ///             "IsJoined": true
        ///         },
        ///         "CategoryName": "testcategory",
        ///         "CategoryID": 1,
        ///         "NotifyTo": {
        ///             "Userid": "user1",
        ///             "DisplayName": "User1",
        ///             "PortraitUrl": null,
        ///             "Description": "user for test"
        ///         },
        ///         "EventIDs": [
        ///             "1|3",
        ///             "2"
        ///         ]
        ///     },
        ///     ......
        /// ]
        /// </summary>
        /// <param name="group">group id</param>
        /// <returns></returns>
        [HttpGet]
        public List<DisplayCategoryMessage> RetriveAllCategoryMessage(string group = null)
        {
            string me = whoami();
            if (string.IsNullOrEmpty(group))
            {
                group = MembershipHelper.DefaultGroup(me);
            }
            else
            {
                MembershipHelper.CheckMembership(group, me);
            }

            List<DisplayCategoryMessage> msgs = new List<DisplayCategoryMessage>();
            List<Category> categories = _categoryManager.GetCategoryByGroup(group);
            foreach (var category in categories)
            {
                CategoryMessage msg = _categoryManager.RetriveCategoryMessage(me, category);
                if (msg != null)
                {
                    msgs.Add(ToDisplayCategoryMessage(msg, me));
                }
            }

            return msgs;
        }

        /// <summary>
        /// Update the message in category
        /// 
        /// example output:
        /// {
        ///     "User": "user1",
        ///     "ID": "251995098518041",
        ///     "PostTime": "2014-08-05T01:31:21.9587913Z",
        ///     "Group": "msgorilladev",
        ///     "CategoryName": "testcategory",
        ///     "CategoryID": 1,
        ///     "NotifyTo": "user1",
        ///     "EventIDs": [
        ///         "1|3",
        ///         "2"
        ///     ]
        /// }
        /// </summary>
        /// <param name="eventID">event id list</param>
        /// <param name="to">notify to whom</param>
        /// <param name="categoryName">category name</param>
        /// <param name="group">group id</param>
        /// <returns></returns>
        [HttpGet, HttpPost]
        public CategoryMessage UpdateCategoryMessage([FromUri]string[] eventID, string to, string categoryName, string group = null)
        {
            string me = whoami();
            Category category = GetCategoryModel(categoryName, group);
            return _categoryManager.UpdateCategoryMessage(eventID, me, to, category, DateTime.UtcNow);
        }

        public class MessageModel
        {
            public string[] EventID { get; set; }
            public string To { get; set; }
            public string CategoryName { get; set; }
            public string Group { get; set; }
        };
        /// <summary>
        /// Update the message in category. Same as the HttpGet API
        /// 
        /// example output:
        /// {
        ///     "User": "user1",
        ///     "ID": "251995098518041",
        ///     "PostTime": "2014-08-05T01:31:21.9587913Z",
        ///     "Group": "msgorilladev",
        ///     "CategoryName": "testcategory",
        ///     "CategoryID": 1,
        ///     "NotifyTo": "user1",
        ///     "EventIDs": [
        ///         "1|3",
        ///         "2"
        ///     ]
        /// }
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        [HttpPost]
        public CategoryMessage UpdateCategoryMessage(MessageModel msg)
        {
            return UpdateCategoryMessage(msg.EventID, msg.To, msg.CategoryName, msg.Group);
        }
    }
}