using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MSGorilla.Library.DAL;
using MSGorilla.Library.Exceptions;
using MSGorilla.Library.Models.SqlModels;

namespace MSGorilla.Library
{
    public class GroupManager
    {
        public Group FindGroupByID(string GroupID, MSGorillaContext ctx = null)
        {
            if (ctx == null)
            {
                using (ctx = new MSGorillaContext())
                {
                    return ctx.Groups.Find(GroupID);
                }
            }
            return ctx.Groups.Find(GroupID);
        }

        public Group AddGroup(string groupID, string displayName, string description, bool isOpen)
        {
            if (Utils.IsValidID(groupID))
            {
                throw new InvalidIDException();
            }

            using (var ctx = new MSGorillaContext())
            {
                Group group = FindGroupByID(groupID);
                if (group != null)
                {
                    throw new GroupAlreadyExistException(groupID);
                }

                group = new Group();
                group.GroupID = groupID;
                group.DisplayName = displayName;
                group.IsOpen = isOpen;

                ctx.Groups.Add(group);
                ctx.SaveChanges();
                return group;
            }
        }
    }
}
