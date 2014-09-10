using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

using MSGorilla.Library.Models.AzureModels.Entity;
using MSGorilla.Library.Models.SqlModels;

namespace MSGorilla.Library.Models
{
    public class CategoryMessage
    {
        public string User { get; set; }
        public string ID { get; set; }
        public DateTime PostTime { get; set; }
        public string Group { get; set; }
        public string CategoryName { get; set; }
        public int CategoryID { get; set; }
        public string NotifyTo { get; set; }
        public List<string> EventIDs { get; set; }

        public CategoryMessage() { }
        public CategoryMessage(IEnumerable<string> eventIDs, string user, string to, Category category, DateTime timestamp)
        {
            this.User = user;
            this.ID = Utils.ToAzureStorageSecondBasedString(timestamp);
            this.PostTime = timestamp;
            this.Group = category.GroupID;
            this.CategoryName = category.Name;
            this.CategoryID = category.ID;
            this.NotifyTo = to;
            this.EventIDs = eventIDs.ToList();
        }

        public static implicit operator CategoryMessage(CategoryMessageEntity entity)
        {
            if (entity == null)
            {
                return null;
            }

            CategoryMessage msg = new CategoryMessage();
            msg.User = entity.User;
            msg.ID = entity.ID;
            msg.PostTime = entity.PostTime;
            msg.Group = entity.Group;
            msg.CategoryName = entity.CategoryName;
            msg.CategoryID = entity.CategoryID;
            msg.NotifyTo = entity.NotifyTo;
            msg.EventIDs = Utils.String2Array<string>(entity.EventIDs, HttpUtility.UrlDecode).ToList();

            return msg;
        }
    }
}
