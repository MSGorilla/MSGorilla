using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSGorilla.IMAPServer.Response
{
    public class CapabilityResponse : BaseResponse
    {
        public override string ToString()
        {
            return "* CAPABILITY IMAP4rev1";
        }
    }
}
