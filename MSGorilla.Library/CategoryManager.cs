using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MSGorilla.Library.Azure;
using MSGorilla.Library.Exceptions;
using MSGorilla.Library.Models;
using MSGorilla.Library.Models.SqlModels;
using MSGorilla.Library.Models.AzureModels.Entity;

using Microsoft.WindowsAzure.Storage.Table;

namespace MSGorilla.Library
{
    public class CategoryManager
    {
        private AWCloudTable _categoryMsgTable = AzureFactory.GetTable(AzureFactory.MSGorillaTable.CategoryMessage);

        public Category CreateCategory(string name, string groupID, string creater, string description = null)
        {
            if(!Utils.IsValidID(name))
            {
                throw new InvalidIDException(name, "name");
            }

            using(var _gorillaCtx = new MSGorillaEntities())
            {
                Category category = GetCategory(name, groupID, _gorillaCtx);
                if(category != null){
                    throw new CategoryAlreadyExistException(name, groupID);
                }

                category = new Category();
                category.Name = name;
                category.GroupID = groupID;
                category.Creater = creater;
                category.CreateTimestamp = DateTime.UtcNow;
                category.Description = description;

                _gorillaCtx.Categories.Add(category);
                _gorillaCtx.SaveChanges();
                return category;
            }
        }

        public Category GetCategory(int id, MSGorillaEntities _gorillaCtx)
        {
            return _gorillaCtx.Categories.Find(id);
        }

        public Category GetCategory(string name, string groupID, MSGorillaEntities _gorillaCtx)
        {
            return _gorillaCtx.Categories.Where( c => c.Name == name && c.GroupID == groupID).FirstOrDefault();
        }

        public Category GetCategory(int id)
        {
            using(var _gorillaCtx = new MSGorillaEntities())
            {
                return GetCategory(id, _gorillaCtx);
            }
        }

        public Category GetCategory(string name, string groupID)
        {
            using(var _gorillaCtx = new MSGorillaEntities())
            {
                return GetCategory(name, groupID, _gorillaCtx);
            }
        }

        public List<Category> GetCategoryByGroup(string groupID)
        {
            using (var _gorillaCtx = new MSGorillaEntities())
            {
                return _gorillaCtx.Groups.Find(groupID).Categories.ToList();
            }
        }

        public List<Category> GetCategoryByName(string categoryName, string[] groups)
        {
            if (string.IsNullOrEmpty(categoryName) || groups == null || groups.Length == 0)
            {
                return new List<Category>();
            }

            using (var _gorillaCtx = new MSGorillaEntities())
            {
                var matches = from category in _gorillaCtx.Categories
                              where groups.Contains(category.GroupID) && category.Name == categoryName
                              select category;
                return matches.ToList();
            }
        }

        public CategoryMessage UpdateCategoryMessage(string[] eventIDs, string from, string to, Category category, DateTime timestamp)
        {
            CategoryMessage message = new CategoryMessage(eventIDs, from, to, category, timestamp);
            TableOperation insertOperation = TableOperation.InsertOrReplace((CategoryMessageEntity)message);
            _categoryMsgTable.Execute(insertOperation);
            return message;
        }

        public CategoryMessage RetriveCategoryMessage(string notifyTo, Category category)
        {
            string query = TableQuery.GenerateFilterCondition
                (
                    "PartitionKey",
                    QueryComparisons.Equal,
                    CategoryMessageEntity.ToPartitionKey(notifyTo, category.GroupID, category.Name)
                );

            TableQuery<CategoryMessageEntity> tableQuery = new TableQuery<CategoryMessageEntity>().Where(query).Take(1);
            TableQuerySegment<CategoryMessageEntity> queryResult = _categoryMsgTable.ExecuteQuerySegmented(tableQuery, null);

            CategoryMessageEntity entity = queryResult.FirstOrDefault();
            return entity;
        }
    }
}
