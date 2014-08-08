using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSGorilla.Library.Models.ViewModels
{
    public partial class DisplayMetricDataSet
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public string GroupID { get; set; }
        public string Creater { get; set; }
        public int RecordCount { get; set; }
        public string Category { get; set; }
        public string Counter { get; set; }
        public string Instance { get; set; }
    }
}
