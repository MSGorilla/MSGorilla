using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MSGorilla.Library.Models.SqlModels
{
    [Serializable]
    public class Subscription
    {
        public int Id { get; set; }
        public string Userid { get; set; }
        public string FollowingUserid { get; set; }
        public string FollowingUserDisplayName { get; set; }
    }
}