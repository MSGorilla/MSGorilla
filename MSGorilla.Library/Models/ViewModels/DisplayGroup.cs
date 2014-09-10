using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MSGorilla.Library.Models.SqlModels;

namespace MSGorilla.Library.Models.ViewModels
{
    public class DisplayGroup
    {
        public string GroupID { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public bool IsOpen { get; set; }
        public string Status { get; set; }


        public const string NotJoinedStatus = "Not Joined";
        public const string PendingStatus = "Pending";
        public const string JoinedStatus = "Joined";
        public const string AdminStatus = "Admin";

        private static string Role2Status(string role)
        {
            if ("admin".Equals(role))
            {
                return AdminStatus;
            }
            else if ("user".Equals(role) || "robot".Equals(role))
            {
                return JoinedStatus;
            }
            else if ("pending".Equals(role))
            {
                return PendingStatus;
            }
            else
            {
                return NotJoinedStatus;
            }
        }

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
            dgroup.Status = "Not Joined";
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
                    Membership membership = _gorillaCtx.Memberships.Where(
                                        m => m.GroupID == group.GroupID && m.MemberID == userid
                                    ).FirstOrDefault();
                    if (membership == null)
                    {
                        this.Status = Role2Status(null);
                    }
                    else
                    {
                        this.Status = Role2Status(membership.Role);
                    }
                }
            }
            else
            {
                Membership membership = _gorillaCtx.Memberships.Where(
                            m => m.GroupID == group.GroupID && m.MemberID == userid
                        ).FirstOrDefault();
                if (membership == null)
                {
                    this.Status = Role2Status(null);
                }
                else
                {
                    this.Status = Role2Status(membership.Role);
                }
            }
        }

        public DisplayGroup(Group group, string status = null)
        {
            this.GroupID = group.GroupID;
            this.DisplayName = group.DisplayName;
            this.Description = group.Description;
            this.IsOpen = group.IsOpen;
            this.Status = Role2Status(status);
        }
    }
}
