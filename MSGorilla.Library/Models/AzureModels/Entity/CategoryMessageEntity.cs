using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using MSGorilla.Library.Models;
using Microsoft.WindowsAzure.Storage.Table;

namespace MSGorilla.Library.Models.AzureModels.Entity
{
    public class CategoryMessageEntity : TableEntity
    {
        public string User { get; set; }
        public string ID { get; set; }
        public DateTime PostTime { get; set; }
        public string Group { get; set; }
        public string CategoryName { get; set; }
        public int CategoryID { get; set; }
        public string NotifyTo { get; set; }
        public string EventIDs { get; set; }

        public static string ToPartitionKey(string notifyTo, string groupID, string categeryName)
        {
            return string.Format("{0}_{1}_{2}", groupID, categeryName, notifyTo);
        }

        public static implicit operator CategoryMessageEntity(CategoryMessage msg)
        {
            if (msg == null)
            {
                return null;
            }

            CategoryMessageEntity entity = new CategoryMessageEntity();
            entity.User = msg.User;
            entity.ID = msg.ID;
            entity.PostTime = msg.PostTime;
            entity.Group = msg.Group;
            entity.CategoryName = msg.CategoryName;
            entity.CategoryID = msg.CategoryID;
            entity.NotifyTo = msg.NotifyTo;

            string[] ids = (from id in msg.EventIDs select HttpUtility.UrlEncode(id)).ToArray();
            entity.EventIDs = Utils.Array2String(ids);

            entity.PartitionKey = ToPartitionKey(entity.NotifyTo, entity.Group, entity.CategoryName);
            entity.RowKey = msg.ID;

            return entity;
        }
    }
}
