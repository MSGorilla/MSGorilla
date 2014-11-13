using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSGorilla.Library.Models.ViewModels
{
    public class DisplayCounterChart
    {
        public string Title;
        public DisplayCounter MainCounter;
        public List<DisplayCounter> RelatedCounter;
    }
}
