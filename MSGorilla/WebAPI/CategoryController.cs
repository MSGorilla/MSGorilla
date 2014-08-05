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
        ///     "CreateTimestamp": "2014-08-05T01:09:54.3483546Z"
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
        ///     "CreateTimestamp": "2014-08-05T01:09:54.3483546Z"
        /// }
        /// </summary>
        /// <param name="name">category name</param>
        /// <param name="group">group id</param>
        /// <returns></returns>
        [HttpGet]
        public DisplayCategory GetCategory(string name, string group = null)
        {
            return GetCategoryModel(name, group);
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
        ///     "CreateTimestamp": "2014-08-05T01:09:54.3483546Z"
        /// }
        /// </summary>
        /// <param name="id">category id</param>
        /// <returns></returns>
        [HttpGet]
        public DisplayCategory GetCategory(int id)
        {
            string me = whoami();
            DisplayCategory category = _categoryManager.GetCategory(id);
            if (category == null)
            {
                throw new CategoryNotFoundException();
            }

            if (!MembershipHelper.JoinedGroup(me).Contains(category.GroupID))
            {
                throw new UnauthroizedActionException();
            }

            return category;
        }

        /// <summary>
        /// Retrive the top message in the category.
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
        ///     "Message": "second category message"
        /// }
        /// </summary>
        /// <param name="name">category name</param>
        /// <param name="group">group id</param>
        /// <returns></returns>
        [HttpGet]
        public CategoryMessage RetriveCategoryMessage(string name, string group = null)
        {
            string me = whoami();            
            Category category = GetCategoryModel(name, group);
            return _categoryManager.RetriveCategoryMessage(me, category);
        }

        /// <summary>
        /// Retrive all category messages in the group
        /// 
        /// example output:
        /// [
        ///     {
        ///         "User": "user1",
        ///         "ID": "251995098518041",
        ///         "PostTime": "2014-08-05T01:31:21.9587913Z",
        ///         "Group": "msgorilladev",
        ///         "CategoryName": "testcategory",
        ///         "CategoryID": 1,
        ///         "NotifyTo": "user1",
        ///         "Message": "second category message"
        ///     },
        ///     ......
        ///     {
        ///         "User": "user1",
        ///         "ID": "251995097347218",
        ///         "PostTime": "2014-08-05T01:50:52.7815261Z",
        ///         "Group": "msgorilladev",
        ///         "CategoryName": "testcategory2",
        ///         "CategoryID": 2,
        ///         "NotifyTo": "user1",
        ///         "Message": "first  message in category2"
        ///     }
        /// ]
        /// </summary>
        /// <param name="group"></param>
        /// <returns></returns>
        [HttpGet]
        public List<CategoryMessage> RetriveAllCategoryMessage(string group = null)
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

            List<CategoryMessage> msgs = new List<CategoryMessage>();
            List<Category> categories = _categoryManager.GetCategoryByGroup(group);
            foreach (var category in categories)
            {
                msgs.Add(_categoryManager.RetriveCategoryMessage(me, category));
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
        ///     "Message": "second category message"
        /// }
        /// </summary>
        /// <param name="message">message content</param>
        /// <param name="to">notify to whom</param>
        /// <param name="categoryName">category name</param>
        /// <param name="group">group id</param>
        /// <returns></returns>
        [HttpGet, HttpPost]
        public CategoryMessage UpdateCategoryMessage(string message, string to, string categoryName, string group = null)
        {
            string me = whoami();
            Category category = GetCategoryModel(categoryName, group);
            return _categoryManager.UpdateCategoryMessage(message, me, to, category, DateTime.UtcNow);
        }

        public class MessageModel
        {
            public string Message { get; set; }
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
        ///     "Message": "second category message"
        /// }
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        [HttpPost]
        public CategoryMessage UpdateCategoryMessage(MessageModel msg)
        {
            return UpdateCategoryMessage(msg.Message, msg.To, msg.CategoryName, msg.Group);
        }
    }
}