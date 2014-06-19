using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSGorilla.Library.Models
{
    public class Attachment
    {
        public string AttachmentId { get; set; }          //{pk};{guid}
        public string FileID { get; set; }
        public string Uploader { get; set; }
        public DateTime UploadTimestamp { get; set; }
        public string Filename { get; set; }
        public string Filetype { get; set; }
        public int Filesize { get; set; }
    }
}
