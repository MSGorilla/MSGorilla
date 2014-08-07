using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MSGorilla.Library.Models.SqlModels;
using MSGorilla.Library.Models.SqlModels;
namespace MSGorilla.Library.Models.ViewModels
{
    public class DisplayGroup
    {
        public string GroupID { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public bool IsOpen { get; set; }
        public bool IsJoined { get; set; }

        public static implicit operator DisplayGroup(Group group)
        {
            if (group == null)
            {
                return null;
            }

            DisplayGroup dgroup = new DisplayGroup();
            dgroup.GroupID = group.GroupID;
            dgroup.DisplayName = group.DisplayName;
            dgroup.Description = group.Description;
            dgroup.IsOpen = group.IsOpen;
            dgroup.IsJoined = false;
            return dgroup;
        }

        public DisplayGroup() { }

        public DisplayGroup(Group group, string userid, MSGorillaEntities _gorillaCtx)
        {
            this.GroupID = group.GroupID;
            this.DisplayName = group.DisplayName;
            this.Description = group.Description;
            this.IsOpen = group.IsOpen;

            if (_gorillaCtx == null)
            {
                using (_gorillaCtx = new MSGorillaEntities())
                {
                    this.IsJoined = _gorillaCtx.Memberships.Where(
                                        m => m.GroupID == group.GroupID && m.MemberID == userid
                                    ).FirstOrDefault() != null;
                }
            }
            else
            {
                this.IsJoined = _gorillaCtx.Memberships.Where(
                            m => m.GroupID == group.GroupID && m.MemberID == userid
                        ).FirstOrDefault() != null;
            }
        }

        public DisplayGroup(Group group, bool isJoined = false)
        {
            this.GroupID = group.GroupID;
            this.DisplayName = group.DisplayName;
            this.Description = group.Description;
            this.IsOpen = group.IsOpen;
            this.IsJoined = isJoined;
        }
    }
}
