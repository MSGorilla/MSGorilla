using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSGorilla.WebAPI.Models.ViewModels
{
    public class DisplayCategoryMessage
    {
        public string User { get; set; }
        public string ID { get; set; }
        public DateTime PostTime { get; set; }
        public DisplayGroup Group { get; set; }
        public string CategoryName { get; set; }
        public int CategoryID { get; set; }
        public SimpleUserProfile NotifyTo { get; set; }
        public List<string> EventIDs { get; set; }
    }
}
