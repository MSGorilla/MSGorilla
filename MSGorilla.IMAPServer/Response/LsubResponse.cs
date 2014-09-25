using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSGorilla.IMAPServer.Response
{
    public class LsubResponse : BaseResponse
    {
        public const string delimiter = "/";
        public string FolderPath;
        public LsubResponse(string folderPath)
        {
            this.FolderPath = folderPath;
        }

        public override string ToString()
        {
            return string.Format("* LSUB () \"{0}\" \"{1}\"", delimiter, this.FolderPath);
        }
    }
}
