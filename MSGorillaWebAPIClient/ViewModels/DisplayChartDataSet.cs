using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSGorilla.WebAPI.Models.ViewModels
{
    public partial class DisplayChartDataSet
    {
        public int ID { get; set; }
        public string Legend { get; set; }
        public string Type { get; set; }
        public int MetricDataSetID { get; set; }
        public string MetricChartName { get; set; }
    }
}
