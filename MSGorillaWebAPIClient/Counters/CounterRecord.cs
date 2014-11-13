using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSGorilla.Library.Models
{
    [Serializable]
    public class CounterRecord
    {
        [Serializable]
        public class ComplexValue
        {
            public string Name { get; set; }
            public string Type { get; set; }
            public object Value { get; set; }
            public List<ComplexValue> RelatedValues { get; set; }

            public ComplexValue(string name, string type, object value = null)
            {
                this.Name = name;
                if (string.IsNullOrEmpty(name))
                {
                    this.Name = "None";
                }

                this.Type = type;
                this.Value = value;
                this.RelatedValues = new List<ComplexValue>();
            }
        }

        public string Key { get; set; }

        /// <summary>
        /// The Value of the CounterRecord should be a virtual root.
        /// The Name and Value of the virtual root doesn't matter but the Subvalues matters
        /// </summary>
        public ComplexValue Value { get; set; }

        public CounterRecord(string key)
        {
            this.Key = key;
            this.Value = new ComplexValue("vroot", "none");
        }
    }
}
