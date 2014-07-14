using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace MSGorilla.Library.Models
{
    public class Reply
    {
        public string FromUser { get; set; }
        public string[] ToUser { get; set; }
        public string Message { get; set; }
        public DateTime PostTime { get; set; }
        public string MessageUser { get; set; }
        public string MessageID { get; set; }
        public string ReplyID { get; set; }

        public Reply(string fromUser, string[] toUser, string message, DateTime timestamp, string messageUser, string messageID, string replyID = null)
        {
            FromUser = fromUser;
            ToUser = toUser;
            Message = message;
            PostTime = timestamp.ToUniversalTime();
            MessageUser = messageUser;
            MessageID = messageID;
            if (string.IsNullOrEmpty(replyID))
            {
                ReplyID = string.Format("{0}_{1}",
                    Utils.ToAzureStorageSecondBasedString(PostTime.ToUniversalTime()),
                    Guid.NewGuid().ToString());
            }
            else
            {
                ReplyID = replyID;
            }            
        }
        public string toJsonString(){
            return JsonConvert.SerializeObject(this);
        }
    }
}
