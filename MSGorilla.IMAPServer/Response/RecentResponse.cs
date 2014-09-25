using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSGorilla.IMAPServer.Response
{
    public class RecentResponse : BaseResponse
    {
        public int RecentCount { get; set; }
        public RecentResponse(int recentCount = 0)
        {
            this.RecentCount = recentCount;
        }

        public override string ToString()
        {
            return string.Format("* {0} RECENT", this.RecentCount);
        }
    }
}
