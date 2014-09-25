using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSGorilla.IMAPServer.Response
{
    public enum FolderAttribute
    {
        Noinferiors,
        Noselect,
        Marked,
        Unmarked
    }

    public class ListResponse : BaseResponse
    {
        public const string delimiter = "\"/\"";
        public string FolderPath;
        public List<FolderAttribute> Attributes;

        public ListResponse(string folderPath)
        {
            this.FolderPath = folderPath;
            this.Attributes = new List<FolderAttribute>();
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder("* LIST (");

            if (Attributes.Count > 0)
            {
                for (int i = 0; i < Attributes.Count - 1; i++)
                {
                    sb.Append('/');
                    sb.Append(Attributes[i].ToString());
                    sb.Append(' ');
                }
                sb.Append('/');
                sb.Append(Attributes[Attributes.Count - 1].ToString());
            }

            sb.Append(") " + delimiter + " \"");
            sb.Append(FolderPath);
            sb.Append('"');

            return sb.ToString();
        }
    }
}
