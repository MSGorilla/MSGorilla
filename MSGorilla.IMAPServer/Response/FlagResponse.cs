using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSGorilla.IMAPServer.Response
{
    public class FlagResponse : BaseResponse
    {
        public override string ToString()
        {
            return "* FLAGS (\\Answered \\Flagged \\Deleted \\Seen \\Draft)";
        }
    }
}
