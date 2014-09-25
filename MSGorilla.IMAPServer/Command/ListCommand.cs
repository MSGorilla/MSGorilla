using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSGorilla.IMAPServer.Command
{
    [CommandName("LIST")]
    public class ListCommand : BaseCommand
    {
        private string _referenceName;
        private string _mailBoxWithWildcards;
        public string ReferenceName
        {
            get
            {
                return _referenceName;
            }
        }

        public string MailBoxWithWildcards
        {
            get
            {
                return _mailBoxWithWildcards;
            }
        }

        public override void Parse(string Tag, string Data)
        {
            base.Parse(Tag, Data);
            string[] args = GetDataTokens(2);
            this._referenceName = args[0];
            this._mailBoxWithWildcards = args[1];
        }
    }
}
