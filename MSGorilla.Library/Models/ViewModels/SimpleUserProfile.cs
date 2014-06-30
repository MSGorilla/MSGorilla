using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MSGorilla.Library.Models.SqlModels;

namespace MSGorilla.Library.Models.ViewModels
{
    public class SimpleUserProfile
    {
        public string Userid { get; set; }
        public string DisplayName { get; set; }
        public string PortraitUrl { get; set; }
        public string Description { get; set; }

        public SimpleUserProfile(UserProfile userinfo)
        {
            if (userinfo == null)
            {
                Userid = "";
                DisplayName = "";
                PortraitUrl = "";
                Description = "";
            }
            else
            {
                Userid = userinfo.Userid;
                DisplayName = userinfo.DisplayName;
                PortraitUrl = userinfo.PortraitUrl;
                Description = userinfo.Description;
            }
        }

        public SimpleUserProfile() { }
    }
}
