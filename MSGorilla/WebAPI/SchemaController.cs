using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

using MSGorilla.Library;
using MSGorilla.Filters;
using MSGorilla.Library.Models;
using MSGorilla.Library.Exceptions;
using MSGorilla.Library.Models.SqlModels;

namespace MSGorilla.WebApi
{
    public class SchemaController : BaseController
    {
        SchemaManager _schemaManager = new SchemaManager();

        [HttpGet]
        public Schema GetSchema(string schemaID)
        {
            return _schemaManager.GetSchema(schemaID);
        }

        [HttpGet]
        public List<Schema> GetSchema()
        {
            return _schemaManager.GetSchema();
        }

        [HttpGet,HttpPost]
        public Schema PostSchema(string schemaID, string schemaContent)
        {
            Schema schema = new Schema(schemaID, schemaContent);
            _schemaManager.PostSchema(schema);
            return schema;
        }
    }
}