using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSGorilla.IMAPServer.Command
{
    public sealed class CommandName : Attribute
    {
        public CommandName(string commandToken)
        {
            this._name = commandToken;
		}

		private string _name;

		public string Name {
			get {
                return this._name;
			}
		}
    }
}
