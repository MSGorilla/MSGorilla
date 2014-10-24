using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSGorilla.WebAPI.Models.ViewModels
{
    public partial class DisplayReplyPagination
    {
        public string continuationToken { get; set; }
        public List<DisplayReply> message { get; set; }

        public DisplayReplyPagination() { }

    }

    public partial class DisplayReply : DisplayMessage
    {
        public new string Type
        {
            get
            {
                return "reply";
            }
        }
        //public string MessageUser { get; set; }
        public string MessageID { get; set; }

        public DisplayReply() { }
    }
}
