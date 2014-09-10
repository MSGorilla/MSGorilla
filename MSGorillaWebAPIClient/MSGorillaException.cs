using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MSGorilla.WebAPI.Client
{
    public class MSGorillaException : Exception
    {
        private int _code;
        private string _description;
        public int ErrorCode
        {
            get
            {
                return _code;
            }
        }

        public string Description
        {
            get
            {
                return _description;
            }
        }
        public MSGorillaException(string text, Exception innerException) : base(
            JObject.Parse(text)["Message"].Value<string>(),
            innerException = null
            )
        {
            JObject obj = JObject.Parse(text);
            _code = obj["ActionResultCode"].Value<int>();
            _description = obj["Message"].Value<string>();
        }
    }
}
