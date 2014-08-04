using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSGorilla.Library.Models.ViewModels
{
    public class DisplayCategory
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string GroupID { get; set; }
        public string Description { get; set; }
        public string Creater { get; set; }
        public DateTime CreateTimestamp { get; set; }

        public static implicit operator DisplayCategory(MSGorilla.Library.Models.SqlModels.Category category)
        {
            if (category == null)
            {
                return null;
            }

            DisplayCategory dCat = new DisplayCategory();
            dCat.ID = category.ID;
            dCat.Name = category.Name;
            dCat.GroupID = category.GroupID;
            dCat.Description = category.Description;
            dCat.Creater = category.Creater;
            dCat.CreateTimestamp = category.CreateTimestamp;
            return dCat;
        }
    }
}
