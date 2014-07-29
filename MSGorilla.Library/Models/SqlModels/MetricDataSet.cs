using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MSGorilla.Library.Models.SqlModels
{
    public class MetricDataSet
    {
        [Key, DataMember]
        public int Id { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string Description { get; set; }
        [DataMember]
        public string GroupID { get; set; }
        [DataMember]
        public string Creater { get; set; }
        [DataMember]
        public int RecordCount { get; set; }
    }
}
