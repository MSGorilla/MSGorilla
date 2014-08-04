using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSGorilla.Library.Models.ViewModels
{
    public class DisplayMetricDataSet
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public string GroupID { get; set; }
        public string Creater { get; set; }
        public int RecordCount { get; set; }
        public string Category { get; set; }
        public string Counter { get; set; }
        public string Instance { get; set; }

        public static implicit operator DisplayMetricDataSet(MSGorilla.Library.Models.SqlModels.MetricDataSet dataset)
        {
            DisplayMetricDataSet temp = new DisplayMetricDataSet();
            temp.Id = dataset.Id;
            temp.Instance = dataset.Instance;
            temp.Category = dataset.Category;
            temp.Counter = dataset.Counter;
            temp.Description = dataset.Description;
            temp.GroupID = dataset.GroupID;
            temp.Creater = dataset.Creater;
            temp.RecordCount = dataset.RecordCount;
            return temp;
        }
    }
}
