using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSGorilla.Library.Models.SqlModels
{
    [Serializable]
    public class Schema
    {
        public int SchemaID { get; set;}
        public string SchemaText { get; set;}
    }
}
