using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MSGorilla.IMAPServer.DataType;

namespace MSGorilla.IMAPServer.Command
{
    [CommandName("STORE")]
    public class StoreCommand : BaseCommand, IMailProcessCommand
    {
        public enum ActionType{
            SetFlag,
            AddFlag,
            RemoveFlag
        }

        public bool IsUIDCommand { get; set; }
        public MessageIDList MessageID { get; set; }
        public ActionType Action { get; private set; }
        public bool IsSilent { get; private set; }
        public List<FlagType> Flags { get; private set; }
        public override void Parse(string Tag, string Data)
        {
            base.Parse(Tag, Data);
            this.IsUIDCommand = false;
            this.IsSilent = false;
            this.Flags = new List<FlagType>();

            string[] parts = Data.Split(new char[] { ' ' }, 3);
            MessageID = new MessageIDList(parts[0]);

            if (parts[1].StartsWith("FLAGS", StringComparison.InvariantCultureIgnoreCase))
            {
                this.Action = ActionType.SetFlag;
            }
            else if(parts[1].StartsWith("+FLAGS", StringComparison.InvariantCultureIgnoreCase))
            {
                this.Action = ActionType.AddFlag;
            }
            else
            {
                this.Action = ActionType.RemoveFlag;
            }

            if (parts[1].EndsWith("SILENT", StringComparison.InvariantCultureIgnoreCase))
            {
                this.IsSilent = true;
            }

            if (parts[2].StartsWith("(") && parts[2].EndsWith(")"))
            {
                parts[2] = parts[2].Substring(1, parts[2].Length - 2);
            }

            foreach (string flags in parts[2].Split(' '))
            {
                this.Flags.Add((FlagType)Enum.Parse(typeof(FlagType), parts[2].Replace("\\", ""), true));
            }
        }
    }
}
