using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSGorilla.Library.Models.ViewModels
{
    public class DisplayChartDataSet
    {
        public int ID { get; set; }
        public string Legend { get; set; }
        public string Type { get; set; }
        public int MetricDataSetID { get; set; }
        public string MetricChartName { get; set; }

        public static implicit operator DisplayChartDataSet(MSGorilla.Library.Models.SqlModels.Chart_DataSet dataset)
        {
            if (dataset == null)
            {
                return null;
            }

            DisplayChartDataSet d = new DisplayChartDataSet();
            d.ID = dataset.ID;
            d.Legend = dataset.Legend;
            d.Type = dataset.Type;
            d.MetricDataSetID = dataset.MetricDataSetID;
            d.MetricChartName = dataset.MetricChartName;

            return d;
        }
    }
}
