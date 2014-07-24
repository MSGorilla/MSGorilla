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
    public class Group
    {
        [Key, DataMember]
        public string GroupID { get; set; }
        [DataMember]
        public string DisplayName { get; set; }
        [DataMember]
        public string Description { get; set; }
        [DataMember]
        public bool IsOpen { get; set; }

        public Group() { }
        public Group(string id, string name, string description, bool isOpen)
        {
            this.GroupID = id;
            this.DisplayName = name;
            this.Description = description;
            this.IsOpen = IsOpen;
        }
    }
}
