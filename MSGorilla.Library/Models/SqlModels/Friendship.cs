using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MSGorilla.Library.Models
{
    [Serializable]
    public class Friendship
    {
        public int Id { get; set; }
        public string Userid { get; set; }
        public string FollowingUserid { get; set; }
        public string FollowingUserDisplayName { get; set; }
    }
}