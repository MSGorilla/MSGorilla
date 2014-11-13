using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSGorilla.Library.Models.ViewModels
{
    public class DisplayCounter
    {
        public string Path;
        public string Type;

        public DisplayCounter() { }
        public DisplayCounter(string path, string type)
        {
            this.Path = path;
            this.Type = type;
        }
    }
}
