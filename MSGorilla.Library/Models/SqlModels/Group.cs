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
    
    public partial class Group
    {
        public Group()
        {
            this.Memberships = new HashSet<Membership>();
            this.MetricDataSets = new HashSet<MetricDataSet>();
            this.Topics = new HashSet<Topic>();
            this.UserProfiles = new HashSet<UserProfile>();
        }
    
        public string GroupID { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public bool IsOpen { get; set; }
    
        public virtual ICollection<Membership> Memberships { get; set; }
        public virtual ICollection<MetricDataSet> MetricDataSets { get; set; }
        public virtual ICollection<Topic> Topics { get; set; }
        public virtual ICollection<UserProfile> UserProfiles { get; set; }
    }
}
