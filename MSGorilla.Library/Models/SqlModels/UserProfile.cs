//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace MSGorilla.Library.Models.SqlModels
{
    using System;
    using System.Collections.Generic;
    
    public partial class UserProfile
    {
        public UserProfile()
        {
            this.FavouriteTopics = new HashSet<FavouriteTopic>();
            this.Memberships = new HashSet<Membership>();
            this.Subscriptions = new HashSet<Subscription>();
        }
    
        public string Userid { get; set; }
        public string DisplayName { get; set; }
        public string PortraitUrl { get; set; }
        public string Description { get; set; }
        public int FollowingsCount { get; set; }
        public int FollowersCount { get; set; }
        public string Password { get; set; }
        public int MessageCount { get; set; }
        public bool IsRobot { get; set; }
        public string DefaultGroup { get; set; }
    
        public virtual ICollection<FavouriteTopic> FavouriteTopics { get; set; }
        public virtual ICollection<Membership> Memberships { get; set; }
        public virtual NotificationCount NotificationCount { get; set; }
        public virtual ICollection<Subscription> Subscriptions { get; set; }
        public virtual Group Group { get; set; }
    }
}
