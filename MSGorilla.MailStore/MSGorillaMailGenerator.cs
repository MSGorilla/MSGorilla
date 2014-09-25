using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Net.Mail;

using MSGorilla.Library.Models;
using MSGorilla.MailStore.Helper;

namespace MSGorilla.MailStore
{
    public class MSGorillaMailGenerator
    {
        private const string _topicRegex = @"(\s|^)#([\w][\w \-\.,:&\*\+]*)?[\w]#(?=\s|$)";
        private const string _userRegex = @"(\s|^)@[0-9a-zA-Z\-]+(?=\s|$)";

        private const string _topicUrl = @"<a href=""https://msgorilla.cloudapp.net/topic/index?topic={0}&group={1}"">#{0}#</a>";
        private const string _userUrl = @"<a href=""https://msgorilla.cloudapp.net/profile/index?user={0}"">@{0}</a>";


        const string HtmlMailBodyFormat =
            @"  <html>
                    <head>
                        <style>
                            body {{
                                background: #fff;
                                color: #505050;
                                font: 14px 'Segoe UI', tahoma, arial, helvetica, sans-serif;
                                margin: 20px;
                                padding: 0;
                            }}
                        </style>
                    </head>
                    <body>
                        {0}
                        </br>
                        <p>=================================================================</p>
                        </br>
                        <div>
                            {1}
                        </div>
                    </body>
                </html>
            ";


        public static string CreateTextMessageMail(string msgID, string toUser = "no-reply")
        {
            if (string.IsNullOrEmpty(msgID))
            {
                return "";
            }

            Message msg = MSGorillaHelper.GetMessage(msgID);
            string richMessage = MSGorillaHelper.GetRichmessage(msg.RichMessageID);
            return CreateTextMessageMail(msg, richMessage, toUser);           
        }

        public static string CreateTextMessageMail(
            Message msg, 
            string richMessage, 
            string toUser = "me")
        {
            return CreateTextMessageMail(
                    msg.MessageContent,
                    msg.Group,
                    msg.PostTime,
                    toUser,
                    msg.User,
                    richMessage
                );
        }

        public static string CreateTextMessageMail(
            string content, 
            string group,
            DateTime timestamp,
            string to, 
            string from = "no-reply", 
            string richMessage = null)
        {
            System.Net.Mail.MailMessage mailmessage = new System.Net.Mail.MailMessage();
            mailmessage.From = new MailAddress(from + "@msgorilla.cloudapp.net");
            mailmessage.To.Add(to + "@msgorilla.cloudapp.net");
            mailmessage.Subject = "";
            mailmessage.Body = string.Format(
                HtmlMailBodyFormat,
                CreateMessageHtmlBody(content, group),
                richMessage);
            mailmessage.IsBodyHtml = true;

            return MailMessageExtensions.RawText(mailmessage, timestamp);
        }

        static string CreateMessageHtmlBody(string message, string groupID)
        {
            if(string.IsNullOrEmpty(message)){
                return "";
            }

            StringBuilder sb = new StringBuilder();
            Regex topicRegex = new Regex(_topicRegex, RegexOptions.IgnoreCase);
            Regex userRegex = new Regex(_userRegex, RegexOptions.IgnoreCase);
            foreach (string line in message.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None))
            {
                string temp = line;
                //Filter topic
                MatchCollection matches = topicRegex.Matches(line);
                foreach (Match m in matches)
                {
                    string match = m.Value.Trim();
                    temp = temp.Replace(match, string.Format(_topicUrl, match.Replace("#", ""), groupID));
                }

                //Filter userid
                matches = userRegex.Matches(line);
                foreach (Match m in matches)
                {
                    string match = m.Value.Trim();
                    temp = temp.Replace(match, string.Format(_userUrl, match.Replace("@", "")));
                }

                sb.Append("<p>");
                sb.Append(temp);
                sb.Append("</p>");
            }

            return sb.ToString();
        }
    }
}
