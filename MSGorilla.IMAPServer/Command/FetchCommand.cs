using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MSGorilla.IMAPServer.DataType;

namespace MSGorilla.IMAPServer.Command
{
    [CommandName("FETCH")]
    public class FetchCommand : BaseCommand, IMailProcessCommand
    {
        public bool IsUIDCommand { get; set; }
        public MessageIDList MessageID { get; set; }
        public FetchDataList fetchData { get; private set; }

        public override void Parse(string Tag, string Data)
        {
            base.Parse(Tag, Data);
            this.IsUIDCommand = false;

            string messageIDs = Data.Substring(0, Data.IndexOf(' '));
            string fetchListStr = Data.Substring(Data.IndexOf(' ') + 1);

            this.MessageID = new MessageIDList(messageIDs);
            this.fetchData = new FetchDataList(fetchListStr);
        }
    }
}
