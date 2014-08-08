using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MSGorilla.Library.Models;

namespace MSGorilla.Library.Models.ViewModels
{
    public partial class DisplayMessagePagination
    {
        public string continuationToken { get; set; }
        public List<DisplayMessage> message { get; set; }

        public DisplayMessagePagination() { }
    }

    public partial class DisplayMessage
    {
        public string Type
        {
            get
            {
                return "message";
            }
        }
        public SimpleUserProfile User { get; set; }
        public string ID { get; set; }
        public string Group { get; set; }
        public string EventID { get; set; }
        public string SchemaID { get; set; }
        public string[] Owner { get; set; }
        public string[] AtUser { get; set; }
        public string[] TopicName { get; set; }
        public string MessageContent { get; set; }
        public string RichMessageID { get; set; }
        public List<Attachment> Attachment { get; set; } 
        public DateTime PostTime { get; set; }
        public int Importance { get; set; }

        public DisplayMessage() { }
    }
}
