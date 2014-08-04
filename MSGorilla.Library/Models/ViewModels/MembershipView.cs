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
        public string MemberID { get; set; }
        public string Role { get; set; }

        public static implicit operator MembershipView(MSGorilla.Library.Models.SqlModels.Membership member)
        {
            if (member == null)
            {
                return null;
            }
            MembershipView view = new MembershipView();
            view.GroupID = member.GroupID;
            view.MemberID = member.MemberID;
            view.Role = member.Role;
            return view;
        }
    }
}
