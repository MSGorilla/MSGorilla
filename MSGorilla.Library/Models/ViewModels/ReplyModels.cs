using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSGorilla.Library.Models.ViewModels
{
    public class ReplyPagiantion
    {
        public List<Reply> reply { get; set; }
        public string continuationToken { get; set; }
    }

    public class DisplayReplyPagination
    {
        public string continuationToken { get; set; }
        public List<DisplayReply> reply { get; set; }

        public DisplayReplyPagination(ReplyPagiantion rpl)
        {
            continuationToken = rpl.continuationToken;
            var replylist = rpl.reply;
            AccountManager accManager = new AccountManager();
            reply = new List<DisplayReply>();
            foreach (var r in replylist)
            {
                reply.Add(new DisplayReply(r, accManager));
            }
        }
    }

    public class DisplayReply
    {
        public SimpleUserProfile FromUser { get; set; }
        public List<SimpleUserProfile> ToUser { get; set; }
        public string Message { get; set; }
        public DateTime PostTime { get; set; }
        public SimpleUserProfile MessageUser { get; set; }
        public string MessageID { get; set; }
        public string ReplyID { get; set; }

        public DisplayReply(Reply rpl, AccountManager accManager)
        {
            // use old id
            FromUser = new SimpleUserProfile(accManager.FindUser(rpl.FromUser));
            ToUser = new List<SimpleUserProfile>();
            if (rpl.ToUser != null)
            {
                foreach (string userid in rpl.ToUser)
                {
                    ToUser.Add(new SimpleUserProfile(accManager.FindUser(userid)));
                }
            }            
            Message = rpl.Message;
            PostTime = rpl.PostTime;
            MessageUser = new SimpleUserProfile(accManager.FindUser(rpl.MessageUser));
            MessageID = rpl.MessageID;
            ReplyID = rpl.ReplyID;
        }
    }
}
