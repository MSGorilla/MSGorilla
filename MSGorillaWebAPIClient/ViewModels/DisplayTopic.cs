using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSGorilla.WebAPI.Models.ViewModels
{
    public partial class DisplayTopic
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int MsgCount { get; set; }
        public string GroupID { get; set; }
        public bool IsLiked { get; private set; }
    }
}
