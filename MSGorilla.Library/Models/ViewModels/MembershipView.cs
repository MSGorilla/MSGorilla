using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSGorilla.Library.Models.ViewModels
{
    public class MembershipView
    {
        public string GroupID { get; set; }
        public DisplayUserProfile User { get; set; }
        public string Role { get; set; }
    }
}
