using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSGorilla.IMAPServer.Command
{
    [CommandName("RENAME")]
    public class RenameCommand : BaseCommand
    {
        private string _curMailBox;
        private string _newMailBox;
        public string CurMailBox
        {
            get
            {
                return this._curMailBox;
            }
        }

        public string NewMailBox
        {
            get
            {
                return this._newMailBox;
            }
        }

        public override void Parse(string Tag, string Data)
        {
            base.Parse(Tag, Data);
            string[] args = GetDataTokens(2);
            this._curMailBox = args[0];
            this._newMailBox = args[1];
        }
    }
}
