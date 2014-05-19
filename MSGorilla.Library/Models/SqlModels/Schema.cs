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
    [Serializable, DataContract]
    public class Schema
    {
        [Key, DataMember]
        public string SchemaID { get; set;}
        [DataMember]
        public string SchemaContent { get; set;}

        public Schema(string id, string Content)
        {
            SchemaID = id;
            SchemaContent = Content;
        }

        public Schema()
        {
            ;
        }
    }
}
