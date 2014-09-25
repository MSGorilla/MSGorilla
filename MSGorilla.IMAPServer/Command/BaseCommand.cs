using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MSGorilla.IMAPServer.Helper;

namespace MSGorilla.IMAPServer.Command
{
    public class BaseCommand
    {
        public string Tag;
        public string Data;
        public virtual void Parse(string Tag, string Data)
        {
            this.Tag = Tag;
            this.Data = Data;
        }

        public string[] GetDataTokens(int maximumTokens = 100
            //, bool enforceSingleSpace = false
            )
        {
            return StringHelper.GetQuotedTokens(this.Data, new char[]{' '}, maximumTokens, true);
        }

        public string GetCommandName()
        {
            System.Reflection.MemberInfo info = this.GetType();
            object[] attributes = info.GetCustomAttributes(true);
            if (attributes.Length == 1)
            {
                CommandName attr = attributes[0] as CommandName;
                return attr.Name;
            }
            return "Unrecognized";
        }
    }
}
