using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace MSGorilla.Library.Models.SqlModels
{
    [Serializable, DataContract]
    public class Membership
    {
        [DataMember]
        public int Id { get; set; }
        [DataMember]
        public string GroupID { get; set; }
        [DataMember]
        public string EntityID { get; set; }
        [DataMember]
        public string Role { get; set; }
    }
}
