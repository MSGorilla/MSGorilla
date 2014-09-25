using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MSGorilla.IMAPServer.Command;

namespace MSGorilla.IMAPServer.Response
{
    public class StatusResponseItem
    {
        public StatusCommand.StatusItem Type;
        public string Value;

        public StatusResponseItem(StatusCommand.StatusItem type, string value)
        {
            this.Type = type;
            this.Value = value;
        }

        public StatusResponseItem()
        {

        }

        public string ToString()
        {
            return string.Format("{0} {1}", this.Type, this.Value);
        }
    }
    public class StatusResponse : BaseResponse
    {
        public string FolderPath;
        public List<StatusResponseItem> StatusItems;
        public StatusResponse(string folderPath)
        {
            this.FolderPath = folderPath;
            this.StatusItems = new List<StatusResponseItem>();
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder("* STATUS ");
            sb.Append(this.FolderPath);
            sb.Append(" (");
            if (this.StatusItems.Count > 0)
            {
                for (int i = 0; i < this.StatusItems.Count - 1; i++)
                {
                    sb.Append(this.StatusItems[i].ToString());
                    sb.Append(" ");
                }
                sb.Append(this.StatusItems[this.StatusItems.Count - 1].ToString());
            }
            sb.Append(")");

            return sb.ToString();
        }
    }
}
