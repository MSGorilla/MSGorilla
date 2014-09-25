using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSGorilla.IMAPServer.Response
{
    public enum ServerStatusResponseType
    {
        OK, NO, BAD, PREAUTH, BYE, 
    }

    public enum ResponseCodeType
    {
        ALERT,
        BADCHARSET,
        CAPABILITY,
        PARSE,
        PERMANENTFLAGS,
        READONLY,
        READWRITE,
        TRYCREATE,
        UIDNEXT,
        UIDVALIDITY,
        UNSEEN
    }

    public class ResponseCode
    {
        public ResponseCodeType Type;
        public string Argument;
        public ResponseCode(ResponseCodeType type, string argument = null)
        {
            this.Type = type;
            this.Argument = argument;
        }

        public string ToString()
        {
            string type = "";
            if (this.Type == ResponseCodeType.READONLY)
            {
                type = "READ-ONLY";
            }
            else if (this.Type == ResponseCodeType.READWRITE)
            {
                type = "READ-WRITE";
            }
            else
            {
                type = this.Type.ToString();
            }

            if(string.IsNullOrEmpty(this.Argument))
            {
                return string.Format("[{0}]", type);
            }
            return string.Format("[{0} {1}]", type, this.Argument);
        }
    }

    public class ServerStatusResponse : BaseResponse
    {
        public string Tag;
        public ServerStatusResponseType Type;
        public ResponseCode Responsecode;
        public string Text;

        public ServerStatusResponse(string tag = "*",
            ServerStatusResponseType type = ServerStatusResponseType.OK,
            string text = "complete",
            ResponseCode responseCode = null)
        {
            this.Tag = tag;
            this.Type = type;
            this.Responsecode = responseCode;
            this.Text = text;
        }

        public override string ToString()
        {
            if (this.Responsecode == null)
            {
                return string.Format("{0} {1} {2}", 
                    this.Tag, 
                    this.Type.ToString(), 
                    this.Text);
            }

            return string.Format("{0} {1} {2} {3}", 
                this.Tag, 
                this.Type.ToString(), 
                this.Responsecode.ToString(), 
                this.Text);
        }
    }
}
