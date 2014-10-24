using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSGorilla.WebAPI.Models.ViewModels
{
    public partial class DisplayMetricChart
    {
        public string Name { get; set; }
        public string Title { get; set; }
        public string SubTitle { get; set; }
        public string GroupID { get; set; }

        public List<DisplayChartDataSet> DataSet { get; set; }
    }
}
