using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace MSGorilla.WebAPI.Models.ViewModels
{
    public partial class DisplayUserProfile
    {
        public string Userid { get; set; }

        public string DisplayName { get; set; }

        public string PortraitUrl { get; set; }

        public string Description { get; set; }

        public int FollowingsCount { get; set; }

        public int FollowersCount { get; set; }

        public int MessageCount { get; set; }

        public int IsFollowing { get; private set; }

        public DisplayUserProfile() { }
    }

}
