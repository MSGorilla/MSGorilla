using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSGorilla.WebAPI.Models.ViewModels
{
    public partial class SimpleUserProfile
    {
        public string Userid { get; set; }
        public string DisplayName { get; set; }
        public string PortraitUrl { get; set; }
        public string Description { get; set; }

        public SimpleUserProfile() { }
    }
}
