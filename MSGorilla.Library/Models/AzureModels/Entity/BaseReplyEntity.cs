using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Microsoft.WindowsAzure.Storage.Table;

namespace MSGorilla.Library.Models.AzureModels.Entity
{
    public class BaseReplyEntity : TableEntity
    {
        public string FromUser { get; set; }
        public string ToUser { get; set; }
        public string Message { get; set; }
        public DateTime PostTime { get; set; }
        public string MessageUser { get; set; }
        public string MessageID { get; set; }
        public string ReplyID { get; set; }
        public string Content { get; set; }

        public BaseReplyEntity() { }

        public BaseReplyEntity(Reply reply, string pk = null, string rk = null )
        {
            this.PartitionKey = pk;
            this.RowKey = rk;

            this.FromUser = reply.FromUser;
            this.ToUser = Utils.StringArray2String(reply.ToUser);
            this.Message = reply.Message;
            this.PostTime = reply.PostTime;
            this.MessageUser = reply.MessageUser;
            this.MessageID = reply.MessageID;
            this.ReplyID = reply.ReplyID;
        }

        public Reply ToReply()
        {
            if (string.IsNullOrEmpty(this.Content))
            {
                Reply reply = new Reply(
                    this.FromUser,
                    Utils.String2StringArray(this.ToUser),
                    this.Message,
                    this.PostTime,
                    this.MessageUser,
                    this.MessageID,
                    this.ReplyID
                );
                return reply;
            }

            //compatible with old version one column format
            JObject obj = JObject.Parse(this.Content);
            Reply oldFormatReply = new Reply(
                    (string)obj["FromUser"],
                    new string[] { (string)obj["ToUser"] },
                    (string)obj["PostTime"],
                    DateTime.Parse((string)obj["PostTime"]),
                    (string)obj["MessageUser"],
                    (string)obj["MessageID"],
                    (string)obj["ReplyID"]
                );
            return oldFormatReply;
        }
    }
}
