using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSGorilla.IMAPServer.Response
{
    public class SearchResponse : BaseResponse
    {
        public List<int> MessageIDList;
        public SearchResponse(List<int> idlist)
        {
            this.MessageIDList = idlist;
        }
        public SearchResponse()
        {
            this.MessageIDList = new List<int>();
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder("* SEARCH");
            if (MessageIDList != null && MessageIDList.Count > 0)
            {
                foreach (int id in MessageIDList)
                {
                    sb.Append(' ');
                    sb.Append(id);
                }
            }

            return sb.ToString();
        }
    }
}
