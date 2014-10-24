using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSGorilla.WebAPI.Models.ViewModels
{
    public partial class DisplayCategory
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string GroupID { get; set; }
        public string Description { get; set; }
        public string Creater { get; set; }
        public DateTime CreateTimestamp { get; set; }
        public int EventCount { get; set; }
    }
}
