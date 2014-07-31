using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using MSGorilla.Library.Models.SqlModels;
using MSGorilla.Library.Exceptions;

namespace MSGorilla.Library
{
    public class SchemaManager
    {
        private MSGorillaEntities _gorillaCtx;

        public SchemaManager()
        {
            using (var _gorillaCtx = new MSGorillaEntities())
            {
                Schema defaultSchema = _gorillaCtx.Schemata.Find("none");
                if (defaultSchema == null)
                {
                    defaultSchema = new Schema();
                    defaultSchema.SchemaID = "none";
                    defaultSchema.SchemaContent = "";
                    _gorillaCtx.Schemata.Add(defaultSchema);
                    _gorillaCtx.SaveChanges();
                }
            }            
        }

        public bool Contain(string schemaID)
        {
            using (var _gorillaCtx = new MSGorillaEntities())
            {
                return !(_gorillaCtx.Schemata.Find(schemaID) == null);
            }
        }

        public List<Schema> GetSchema()
        {
            using (var _gorillaCtx = new MSGorillaEntities())
            {
                return _gorillaCtx.Schemata.ToList();
            }            
        }

        public Schema GetSchema(string schemaID)
        {
            using (var _gorillaCtx = new MSGorillaEntities())
            {
                Schema schema = _gorillaCtx.Schemata.Find(schemaID);
                if (schema == null)
                {
                    throw new SchemaNotFoundException();
                }

                return schema;
            }            
        }

        public void PostSchema(Schema schema)
        {
            using (var _gorillaCtx = new MSGorillaEntities())
            {
                if (Contain(schema.SchemaID))
                {
                    throw new SchemaAlreadyExistException();
                }
                _gorillaCtx.Schemata.Add(schema);
                _gorillaCtx.SaveChanges();
            }            
        }
    }
}
