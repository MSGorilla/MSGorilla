using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSGorilla.IMAPServer.Response
{
    public class ExistResponse : BaseResponse
    {
        public int ExistCount { get; set; }
        public ExistResponse(int existCount = 0)
        {
            this.ExistCount = existCount;
        }

        public override string ToString()
        {
            return string.Format("* {0} EXISTS", this.ExistCount);
        }
    }
}
