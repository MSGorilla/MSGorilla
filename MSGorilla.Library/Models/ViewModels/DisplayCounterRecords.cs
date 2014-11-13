using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSGorilla.Library.Models.ViewModels
{
    public class DisplayRecord
    {
        public string Key;
        public object Value;
        public DisplayRecord() { }
        public DisplayRecord(string key, object value)
        {
            this.Key = key;
            this.Value = value;
        }
    }

    public class DisplayCounterRecords
    {
        public string Name;
        public string Group;
        public string Path;
        public int StartIndex;
        public List<DisplayRecord> Records;
        public DisplayCounterRecords() { }
        public DisplayCounterRecords(
            string name, 
            string group, 
            string path, 
            int startIndex,
            List<DisplayRecord> records)
        {
            this.Name = name;
            this.Group = group;
            this.Path = path;
            this.StartIndex = startIndex;
            this.Records = records;
        }
    }
}
