using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace MSGorilla.Library.Models.SqlModels
{
    [KnownType(typeof(Subscription))]
    [Serializable, DataContract]
    public class UserProfile
    {
        [Key, DataMember]
        public string Userid { get; set; }

        [Required, DataMember]
        public string DisplayName { get; set; }

        [DataMember]
        public string PortraitUrl { get; set; }

        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public int FollowingsCount { get; set; }

        [DataMember]
        public int FollowersCount { get; set; }

        [DataMember]
        public int MessageCount { get; set; }
        [DataMember]
        public bool IsRobot { get; set; }

        public string Password { get; set; }
        
        public virtual ICollection<Subscription> followings { get; set; }
    }
}
