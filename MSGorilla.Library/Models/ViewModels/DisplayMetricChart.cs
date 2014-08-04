using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSGorilla.Library.Models.ViewModels
{
    public class DisplayMetricChart
    {
        public string Name { get; set; }
        public string Title { get; set; }
        public string SubTitle { get; set; }
        public string GroupID { get; set; }

        public List<DisplayChartDataSet> DataSet { get; set; }

        public static implicit operator DisplayMetricChart(MSGorilla.Library.Models.SqlModels.MetricChart chart)
        {
            if (chart == null)
            {
                return null;
            }

            DisplayMetricChart temp = new DisplayMetricChart();
            temp.Name = chart.Name;
            temp.Title = chart.Title;
            temp.SubTitle = chart.SubTitle;
            temp.GroupID = chart.GroupID;

            temp.DataSet = new List<DisplayChartDataSet>();
            foreach (var set in chart.Chart_DataSet)
            {
                temp.DataSet.Add(set);
            }

            return temp;
        }
    }
}
