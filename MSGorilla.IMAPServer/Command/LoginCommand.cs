using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSGorilla.IMAPServer.Command
{
    [CommandName("LOGIN")]
    public class LoginCommand : BaseCommand
    {
        private string _username;
        private string _password;
        public override void Parse(string Tag, string Data)
        {
            base.Parse(Tag, Data);
            string[] args = GetDataTokens(2);
            this._username = args[0];
            this._password = args[1];
        }

        public string Username
        {
            get
            {
                return this._username;
            }
        }

        public string Password
        {
            get
            {
                return this._password;
            }
        }
    }
}
