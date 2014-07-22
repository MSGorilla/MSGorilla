using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Exchange.WebServices.Data;

namespace MSGorilla.EmailMonitor
{
    /// <summary>
    /// Send or receive mails. 
    /// </summary>
    public class EmailManager
    {
        private ExchangeService _service;
        private PropertySet _propSet = new PropertySet(
                BasePropertySet.IdOnly, ItemSchema.Subject, ItemSchema.DateTimeReceived, ItemSchema.ConversationId
            );
        private string _address;

        public EmailManager(string address, string password)
        {
            _address = address;

            _service = new ExchangeService(ExchangeVersion.Exchange2010_SP2);
            _service.Credentials = new WebCredentials(address, password);
            _service.TraceEnabled = false;
            _service.TraceFlags = TraceFlags.All;

            _service.AutodiscoverUrl(address, RedirectionUrlValidationCallback);
        }

        public void SendMail(string subject, string body, string[] to)
        {
            if (to == null || to.Length == 0)
            {
                return;
            }
            EmailMessage message = new EmailMessage(_service);

            message.Subject = subject;
            message.Body = body;

            foreach (string address in to)
            {
                message.ToRecipients.Add(address);
            }          

            message.SendAndSaveCopy();
        }

        public void ReplyConversation(string from, string replyBody, string convID)
        {
            Conversation conv = GetConversationById(convID);
            EmailMessage lastMail = null;
            foreach (ItemId itemid in conv.ItemIds)
            {
                lastMail = GetMailByID(itemid);
                if (lastMail != null)
                {
                    break;
                }
            }

            if (lastMail == null)
            {
                return;
            }

            //ResponseMessage responseMsg = lastMail.CreateReply(true);
            ResponseMessage responseMsg = lastMail.CreateReply(false);
            responseMsg.ToRecipients.Add("t-yig@microsoft.com");

            responseMsg.BodyPrefix = string.Format(
                "This reply was sent by <a href=\"https://msgorilla.cloudapp.net/profile/index?user={0}\">{0}</a>"
                    + "on <a href=\"https://msgorilla.cloudapp.net/\">MSGorilla</a></br>"
                    + "================================================================================</br>"
                    +"{1}",
                        from,
                        replyBody
                );
            //responseMsg.ToRecipients.Add("t-yig@microsoft.com");
            responseMsg.SendAndSaveCopy();
        }

        public EmailMessage GetMailByID(ItemId id)
        {
            return EmailMessage.Bind(_service, id);
        }
        public Conversation GetConversationById(string convID)
        {
            int pagesize = 25;
            int offset = 0;
            Conversation conv = null;

            while (conv == null)
            {
                ConversationIndexedItemView view = new ConversationIndexedItemView(pagesize, offset, OffsetBasePoint.Beginning);
                ICollection<Conversation> conversations = _service.FindConversation(view, WellKnownFolderName.Inbox);

                if (conversations.Count == 0)
                {
                    break;
                }
                foreach (Conversation conversation in conversations)
                {
                    string id = conversation.Id;
                    if (id.Equals(convID))
                    {
                        conv = conversation;
                        break;
                    }
                }

                offset += pagesize;
            }

            return conv;
        }

        public List<EmailMessage> RetrieveEmailMessage(string monitorAddress, DateTime start, DateTime end)
        {
            //Folder inbox = Folder.Bind(_service, WellKnownFolderName.Inbox, _propSet);

            SearchFilter.SearchFilterCollection sf = new SearchFilter.SearchFilterCollection(LogicalOperator.And);
            sf.Add(new SearchFilter.IsGreaterThan(EmailMessageSchema.DateTimeReceived, start));
            sf.Add(new SearchFilter.IsLessThanOrEqualTo(EmailMessageSchema.DateTimeReceived, end));
            //Ignore email sent by monitor account. Those emails may already send by EmailSender;
            sf.Add(new SearchFilter.IsNotEqualTo(EmailMessageSchema.Sender, _address));

            sf.Add(new SearchFilter.SearchFilterCollection(LogicalOperator.Or,
                    new SearchFilter.ContainsSubstring(EmailMessageSchema.ToRecipients, monitorAddress),
                    new SearchFilter.ContainsSubstring(EmailMessageSchema.CcRecipients, monitorAddress)
                ));

            ItemView view = new ItemView(Int32.MaxValue);
            view.OrderBy.Add(ItemSchema.DateTimeReceived, SortDirection.Ascending);

            FindItemsResults<Item> findResults = _service.FindItems(WellKnownFolderName.Inbox, sf, view);
            Logger.Debug("Server return " + findResults.TotalCount + " records.");

            List<EmailMessage> result = new List<EmailMessage>();
            foreach(Item item in findResults)
            {
                if(item is EmailMessage)
                {
                    EmailMessage emailMessage = EmailMessage.Bind(_service, item.Id);
                    if (emailMessage.DateTimeReceived > start && emailMessage.DateTimeReceived <= end)
                    {
                        result.Add(emailMessage);
                    }
                    else
                    {
                        Logger.Debug(string.Format(@"Exchange server filter error. Find mail subject:{0} received {1:s} is not filted by DatetimeReceived from {2:s} to {3:s}",
                        emailMessage.Subject, emailMessage.DateTimeReceived, start, end));
                    }                    
                }
            }

            return result;
        }

        private static bool RedirectionUrlValidationCallback(string redirectionUrl)
        {
            // The default for the validation callback is to reject the URL.
            bool result = false;
            Uri redirectionUri = new Uri(redirectionUrl);

            // Validate the contents of the redirection URL. In this simple validation
            // callback, the redirection URL is considered valid if it is using HTTPS
            // to encrypt the authentication credentials. 
            if (redirectionUri.Scheme == "https")
            {
                result = true;
            }
            return result;
        }
    }
}
