using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MSGorilla.Library.Models;

namespace MSGorilla.Library.Models.ViewModels
{
    public class DisplayCounterSet : CounterSet
    {
        public List<DisplayCounter> Entry;

        public DisplayCounterSet() { }

        public DisplayCounterSet(CounterSet counterSet)
        {
            this.Group = counterSet.Group;
            this.Name = counterSet.Name;
            this.Description = counterSet.Description;
            this.LastUpdateTime = counterSet.LastUpdateTime;
            this.RecordCount = counterSet.RecordCount;
            this.Entry = new List<DisplayCounter>();
        }
    }
}
