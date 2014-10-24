using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MSGorilla.WebAPI.Client;
using Microsoft.Exchange.WebServices.Data;
using MSGorilla.EmailMonitor.Sql;
//using MSGorilla.WebAPI.Models.ViewModels;
using Newtonsoft.Json;
using mshtml;

namespace MSGorilla.EmailMonitor
{
    public class MSGorillaEmailReporter
    {
        GorillaWebAPI _client;
        public MSGorillaEmailReporter(string username, string password)
        {
            _client = new GorillaWebAPI(username, password);
        }

        public static string CaseInsenstiveReplace(string originalString, string oldValue, string newValue)
        {
            Regex regEx = new Regex(oldValue,
               RegexOptions.IgnoreCase | RegexOptions.Multiline);
            return regEx.Replace(originalString, newValue);
        }

        public static String BytesToString(long byteCount)
        {
            string[] suf = { "B", "KB", "MB", "GB", "TB", "PB", "EB" }; //Longs run out around EB
            if (byteCount == 0)
                return "0" + suf[0];
            long bytes = Math.Abs(byteCount);
            int place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
            double num = Math.Round(bytes / Math.Pow(1024, place), 1);
            return (Math.Sign(byteCount) * num).ToString() + suf[place];
        }

        public void Report(EmailMessage email)
        {
            try
            {
                Sql.Conversation conversation = GetConversationByID(email.ConversationId);
                if (conversation == null)
                {
                    //new conversation, post message
                    PostEmailMessage(email);
                }
                else
                {
                    //old conversation, post reply
                    PostEmailReply(email, conversation);
                }
            }
            catch (Exception e)
            {
                Logger.Error("Fail to process email : \r\n" + GetEmailThumbnail(email));
                throw e;
            }
        }

        public MSGorilla.WebAPI.Models.ViewModels.Attachment UploadAttachment2MSGorilla(FileAttachment attachment)
        {
            string filename = attachment.Name;
            attachment.Load(filename);
            MSGorilla.WebAPI.Models.ViewModels.Attachment att = _client.UploadAttachment(filename);
            System.IO.File.Delete(filename);
            return att;
        }

        public string UploadAttachment(string html, EmailMessage email)
        {
            string body = email.Body;
            List<MSGorilla.WebAPI.Models.ViewModels.Attachment> fileAtts = new List<MSGorilla.WebAPI.Models.ViewModels.Attachment>();

            foreach (Attachment attachment in email.Attachments)
            {
                if (attachment is FileAttachment)
                {
                    FileAttachment fileAttachment = (FileAttachment)attachment;
                    if (string.IsNullOrEmpty(fileAttachment.ContentId))
                    {
                        //real attachment
                        MSGorilla.WebAPI.Models.ViewModels.Attachment att = UploadAttachment2MSGorilla(fileAttachment);
                        fileAtts.Add(att);
                    }
                    else if (html.Contains(fileAttachment.ContentId))
                    {
                        //Figure attachment in mail body
                        MSGorilla.WebAPI.Models.ViewModels.Attachment att = UploadAttachment2MSGorilla(fileAttachment);
                        html = html.Replace("cid:" + fileAttachment.ContentId, 
                            "/api/attachment/Download?attachmentID=" + att.AttachmentID);
                    }
                    
                }
            }

            if (fileAtts.Count > 0)
            {
                string tag = CreateAttachmentTag(fileAtts);
                html = CaseInsenstiveReplace(
                        html, "<div class=WordSection1>", "<div class=WordSection1>" + tag);
            }

            return html;
        }

        public static string CreateAttachmentTag(List<MSGorilla.WebAPI.Models.ViewModels.Attachment> fileAtts)
        {
            string attachmentTemplate = "<a href=\"/api/attachment/Download?attachmentID={0}\">{1}({2})</a>";
            StringBuilder attachments = new StringBuilder("");
            foreach (MSGorilla.WebAPI.Models.ViewModels.Attachment attach in fileAtts)
            {
                attachments.Append(string.Format(attachmentTemplate, 
                    attach.AttachmentID, 
                    attach.Filename,
                    BytesToString(attach.Filesize)));
            }

            return "<div>" + 
                        "<div style='border:none;border-top:solid #E1E1E1 1.0pt;padding:3.0pt 0in 0in 0in'>"+
                            "<p class=MsoNormal>" + 
                                "<b>Attachment:</b> " + 
                                attachments.ToString() + 
                            "</p>" + 
                        "</div>" + 
                    "</div>";
        }

        public static string GetReplyMailBodyHtml(EmailMessage email)
        {
            string html = GetCompleteMailBodyHtml(email);

            //html start from <div> is the replied mail
            int index = html.IndexOf("<div>", StringComparison.CurrentCultureIgnoreCase);
            if (index >= 0)
            {
                html =  html.Substring(0, index) + "</div></body>";
            }

            return html;
        }

        public string CreateMailRichMessage(string html, EmailMessage email)
        {
            html = UploadAttachment(html, email);

            string tag = CreateEmailTag(email);

            return CaseInsenstiveReplace(
                html, "<div class=WordSection1>", "<div class=WordSection1>" + tag);
        }

        private static string CreateEmailTag(EmailMessage email)
        {
            string vector = @"<div>
                                <div style='border:none;border-top:solid #E1E1E1 1.0pt;padding:3.0pt 0in 0in 0in'>
                                    <p class=MsoNormal>
                                        <b>From:</b> {0}<br>
                                        <b>Sent:</b> {1:f}<br>
                                        <b>To:</b> {2}<br>
                                        <b>CC:</b> {3}<br>
                                        <b>Subject:</b> {4}<o:p></o:p>
                                    </p>
                                </div>
                              </div>";
            string from = email.From.Name;
            string to = EmailAddrCollection2Str(email.ToRecipients);
            string cc = EmailAddrCollection2Str(email.CcRecipients);
            return string.Format(vector, from, email.DateTimeSent, to, cc, email.Subject);
        }

        private static string GetCompleteMailBodyHtml(EmailMessage email)
        {
            string body = email.Body;

            HTMLDocumentClass doc = new HTMLDocumentClass();
            doc.designMode = "on";
            doc.IHTMLDocument2_write(body);

            return doc.body.outerHTML;
        }

        private static string InnerHtmlText(string html)
        {
            HTMLDocumentClass doc = new HTMLDocumentClass();
            doc.designMode = "on";
            doc.IHTMLDocument2_write(html);

            return doc.body.innerText;
        }

        public static string GetEmailThumbnail(EmailMessage email)
        {
            string thumbnail = string.Format(
                    "Subject: {0}\r\nSent: {1:u}\r\nFrom: {2}",
                    email.Subject,
                    email.DateTimeSent,
                    email.From.Address
                );

            if (thumbnail.Length > 512)
            {
                return thumbnail.Substring(0, 500) + "......";
            }
            return thumbnail;
        }

        private static string EmailAddrCollection2Str(EmailAddressCollection collection)
        {
            if (collection.Count == 0)
            {
                return "";
            }
            StringBuilder sb = new StringBuilder();
            foreach (EmailAddress addr in collection)
            {
                sb.Append(addr.Name);
                sb.Append("; ");
            }
            return sb.ToString();
        }

        private void PostEmailMessage(EmailMessage email)
        {
            Logger.Debug("Post email message: \r\n" + GetEmailThumbnail(email));

            string htmlBody = GetCompleteMailBodyHtml(email);
            htmlBody = CreateMailRichMessage(htmlBody, email);

            string result = _client.PostMessage("none", null, "none", email.ConversationId, null, null, null, htmlBody, null);
            MSGorilla.WebAPI.Models.ViewModels.DisplayMessage displayMessage = JsonConvert.DeserializeObject<MSGorilla.WebAPI.Models.ViewModels.DisplayMessage>(result);

            Sql.Conversation conv = new Sql.Conversation();
            conv.ConversationID = email.ConversationId;
            conv.MessageID = displayMessage.ID;
            conv.MessageUser = displayMessage.User.Userid;
            
            SaveConversation(conv);
        }

        private void PostEmailReply(EmailMessage email, Sql.Conversation conv)
        {
            Logger.Debug("Post email reply: \r\n" + GetEmailThumbnail(email));

            string htmlBody = GetReplyMailBodyHtml(email);
            htmlBody = CreateMailRichMessage(htmlBody, email);

            _client.PostReply(null, "", conv.MessageID, htmlBody, null);
        }

        private static Sql.Conversation GetConversationByID(ConversationId id)
        {
            using (var ctx = new EmailMonitorContext())
            {
                Sql.Conversation conversation = ctx.Conversation.Find(id.ToString());
                return conversation;
            }
        }

        private static void SaveConversation(Sql.Conversation conv)
        {
            if (conv == null)
            {
                return;
            }
            Logger.Debug("Save Conversation: " + JsonConvert.SerializeObject(conv));
            using (var ctx = new EmailMonitorContext())
            {
                ctx.Conversation.Add(conv);
                ctx.SaveChanges();
            }
        }
    }
}
