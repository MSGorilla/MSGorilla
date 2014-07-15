using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace MSGorilla.StatusReporter
{
    public class HistoryData
    {
        [Key]
        public DateTime Date {get; set;}
        public int UserCount {get; set;}
        public int RobotCount { get; set; }
        public int TopicCount { get; set; }
        public int CountOfMsgPostedByRobot { get; set; }
    }
}
