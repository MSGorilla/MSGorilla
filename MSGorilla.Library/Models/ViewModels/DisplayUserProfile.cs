using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MSGorilla.Library.Models.SqlModels;
using System.Runtime.Serialization;

namespace MSGorilla.Library.Models.ViewModels
{
    public class DisplayUserProfile
    {
        public string Userid { get; set; }

        public string DisplayName { get; set; }

        public string PortraitUrl { get; set; }

        public string Description { get; set; }

        public int FollowingsCount { get; set; }

        public int FollowersCount { get; set; }

        public int MessageCount { get; set; }

        public int IsFollowing { get; private set; }

        public DisplayUserProfile(UserProfile user, int isFollowing)
        {
            Userid = user.Userid;
            DisplayName = user.DisplayName;
            PortraitUrl = user.PortraitUrl;
            Description = user.Description;
            FollowingsCount = user.FollowingsCount;
            FollowersCount = user.FollowersCount;
            MessageCount = user.MessageCount;
            IsFollowing = isFollowing;
        }

        public DisplayUserProfile() { }
    }

}
