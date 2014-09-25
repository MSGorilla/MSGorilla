using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MSGorilla.IMAPServer.Helper;

namespace MSGorilla.IMAPServer.Command
{
    [CommandName("STATUS")]
    public class StatusCommand : BaseCommand
    {
        public enum StatusItem
        {
            MESSAGES, RECENT, UIDNEXT, UIDVALIDITY, UNSEEN
        }

        private string _mailBox;
        private List<StatusItem> _statusDataItems;

        public string MailBox
        {
            get
            {
                return _mailBox;
            }
        }

        public List<StatusItem> StatusDataItems
        {
            get
            {
                return _statusDataItems;
            }
        }

        public override void Parse(string Tag, string Data)
        {
            base.Parse(Tag, Data);

            char[] delim = new char[]{' '};
            string[] args = StringHelper.GetQuotedBracketTokens(this.Data, delim, 2);
            this._mailBox = args[0];

            _statusDataItems = new List<StatusItem>();
            string itemnames = args[1];
            if (itemnames.StartsWith("(") && itemnames.EndsWith(")"))
            {
                itemnames = itemnames.Substring(1, itemnames.Length - 2);
            }
            args = StringHelper.GetQuotedTokens(itemnames, delim, 5);
            foreach (string arg in args)
            {
                _statusDataItems.Add((StatusItem)Enum.Parse(typeof(StatusItem), arg, true));
            }
        }
        
    }
}
