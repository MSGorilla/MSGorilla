using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeFlowMonitor
{
    public class MemberShipHelper
    {
        public static List<string> GetADSecurityGroupUsers(String sgAlias)
        {
            List<string> userList = new List<string>();

            using (PrincipalContext ctx = new PrincipalContext(ContextType.Domain))
            {
                GroupPrincipal grp = GroupPrincipal.FindByIdentity(ctx, sgAlias);

                if (grp != null)
                {
                    foreach (Principal p in grp.GetMembers(true))
                    {
                        userList.Add(p.SamAccountName);
                        Console.WriteLine(p.SamAccountName);
                    }
                    grp.Dispose();
                    ctx.Dispose();
                }
                else
                {
                    Console.WriteLine("\nWe did not find that group in that domain, perhaps the group resides in a different domain?");
                }
            }
            
            return userList;

        } 
        

    }
}
