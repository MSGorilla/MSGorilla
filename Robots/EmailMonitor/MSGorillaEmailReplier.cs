using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MSGorilla.WebAPI.Client;
using MSGorilla.EmailMonitor.Sql;
using MSGorilla.WebAPI.Models.ViewModels;



namespace MSGorilla.EmailMonitor
{
    /// <summary>
    /// Get new replies from MSGorilla and send reply email to the conversation
    /// </summary>
    public class MSGorillaEmailReplier
    {
        private string _userid;
        GorillaWebAPI _client;
        EmailManager _replySender;

        public MSGorillaEmailReplier(string mailAddress, string mailPassword, string gorillaUserid, string gorillaPassword)
        {
            _userid = gorillaUserid;
            _client = new GorillaWebAPI(gorillaUserid, gorillaPassword);
            _replySender = new EmailManager(mailAddress, mailPassword);

        }

        public List<DisplayReply> GetNewReply()
        {
            Logger.Debug("Query new replies.");
            NotificationCount notif = _client.GetNotificationCount();
            if (notif.UnreadReplyCount == 0)
            {
                Logger.Debug("No reply found.");
                return new List<DisplayReply>();
            }

            Logger.Debug(_client.GetReply(notif.UnreadReplyCount).message.Count + " replies found");
            return _client.GetReply(notif.UnreadReplyCount).message;
        }

        public void CheckReply()
        {
            List<DisplayReply> replies = GetNewReply();
            replies.Reverse();
            foreach (DisplayReply reply in replies)
            {
                Logger.Debug(string.Format("Processing reply\n From: {0}\n Content:{1}", reply.User.Userid, reply.MessageContent));
                try
                {
                    Message msg = _client.GetRawMessage(reply.MessageID);
                    if (msg != null && !string.IsNullOrEmpty(msg.EventID))
                    {
                        _replySender.ReplyConversation(reply.User.Userid, reply.MessageContent, msg.EventID);
                    }
                }
                catch(Exception e){
                    Logger.Error(e.Message);
                    Logger.Error(e.StackTrace);
                }
            }
        }
    }
}
