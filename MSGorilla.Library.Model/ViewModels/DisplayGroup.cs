using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSGorilla.Library.Models.ViewModels
{
    public partial class DisplayGroup
    {
        public string GroupID { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public bool IsOpen { get; set; }
        public bool IsJoined { get; set; }

        public DisplayGroup() { }
    }
}
