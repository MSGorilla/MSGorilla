using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Runtime.Serialization;


namespace MSGorilla.WebAPI.Models.ViewModels
{
    public class Message
    {
        public string User { get; set; }
        public string ID { get; set; }
        public string Group { get; set; }
        public string EventID { get; set; }
        public string SchemaID { get; set; }
        public string[] Owner { get; set; }
        public string[] AtUser { get; set; }
        public string[] TopicName { get; set; }
        public string MessageContent { get; set; }
        public DateTime PostTime { get; set; }
        public string RichMessageID { get; set; }
        public string[] AttachmentID { get; set; }
        public int Importance { get; set; }

        public Message() { }
    }
}
