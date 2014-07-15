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
    public class BaseReplyEntity : BaseMessageEntity
    {
        public string MessageUser { get; set; }
        public string MessageID { get; set; }

        public BaseReplyEntity() { }

        public BaseReplyEntity(Reply reply, string pk = null, string rk = null ):
            base(reply, pk, rk)
        {
            this.MessageUser = reply.MessageUser;
            this.MessageID = reply.MessageID;
        }

        public Reply ToReply()
        {
            //if (string.IsNullOrEmpty(this.Content))
            //{
            Reply reply = new Reply(
                this.User,
                this.MessageContent,
                this.PostTime,
                this.MessageUser,
                this.MessageID,
                Utils.String2StringArray(this.AtUser),
                this.RichMessageID,
                Utils.String2StringArray(this.AttachmentID),
                this.ID
            );
            return reply;
            //}

            ////compatible with old version one column format
            //JObject obj = JObject.Parse(this.Content);
            //Reply oldFormatReply = new Reply(
            //        (string)obj["FromUser"],
            //        new string[] { (string)obj["ToUser"] },
            //        (string)obj["Message"],
            //        DateTime.Parse((string)obj["PostTime"]),
            //        (string)obj["MessageUser"],
            //        (string)obj["MessageID"],
            //        (string)obj["ReplyID"]
            //    );
            //return oldFormatReply;
        }
    }
}
