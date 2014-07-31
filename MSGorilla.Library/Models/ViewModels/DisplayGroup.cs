using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSGorilla.Library.Models.ViewModels
{
    public class DisplayGroup
    {
        public string GroupID { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public bool IsOpen { get; set; }

        public static implicit operator DisplayGroup(MSGorilla.Library.Models.SqlModels.Group group)
        {
            DisplayGroup dgroup = new DisplayGroup();
            dgroup.GroupID = group.GroupID;
            dgroup.DisplayName = group.DisplayName;
            dgroup.Description = group.Description;
            dgroup.IsOpen = group.IsOpen;
            return dgroup;
        }
    }
}
