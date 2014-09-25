using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;
using System.Net.Mail;

namespace MSGorilla.MailStore.Helper
{
    public static class MailMessageExtensions
    {
        private static readonly BindingFlags Flags = BindingFlags.Instance | BindingFlags.NonPublic;
        private static readonly Type MailWriter = typeof(SmtpClient).Assembly.GetType("System.Net.Mail.MailWriter");
        private static readonly ConstructorInfo MailWriterConstructor = MailWriter.GetConstructor(Flags, null, new[] { typeof(Stream) }, null);
        private static readonly MethodInfo CloseMethod = MailWriter.GetMethod("Close", Flags);
        private static readonly MethodInfo SendMethod = typeof(System.Net.Mail.MailMessage).GetMethod("Send", Flags);

        /// <summary>
        /// A little hack to determine the number of parameters that we
        /// need to pass to the SaveMethod.
        /// </summary>
        private static readonly bool IsRunningInDotNetFourPointFive = SendMethod.GetParameters().Length == 3;

        /// <summary>
        /// The raw contents of this MailMessage as a MemoryStream.
        /// </summary>
        /// <param name="self">The caller.</param>
        /// <returns>A MemoryStream with the raw contents of this MailMessage.</returns>
        public static MemoryStream RawMessage(this System.Net.Mail.MailMessage self)
        {
            var result = new MemoryStream();
            var mailWriter = MailWriterConstructor.Invoke(new object[] { result });
            SendMethod.Invoke(self, Flags, null, IsRunningInDotNetFourPointFive ? new[] { mailWriter, true, true } : new[] { mailWriter, true }, null);
            result = new MemoryStream(result.ToArray());
            CloseMethod.Invoke(mailWriter, Flags, null, new object[] { }, null);
            return result;
        }

        /// <summary>
        /// The raw contents of this MailMessage as a text
        /// </summary>
        /// <param name="self">The callers.</param>
        /// <returns>A string with the raw contents of the mail message</returns>
        public static string RawText(this System.Net.Mail.MailMessage self)
        {
            string ret;
            using (var stream = RawMessage(self))
            {
                StreamReader reader = new StreamReader(stream);
                ret = reader.ReadToEnd();
            }
            return ret;
        }

        /// <summary>
        /// Similar to RawText, but the Date is the specified timestamp
        /// </summary>
        /// <param name="self">The callers.</param>
        /// <param name="timestamp">timestamp when the mail is sent</param>
        /// <returns></returns>
        public static string RawText(this System.Net.Mail.MailMessage self, DateTimeOffset timestamp)
        {
            string ret = null;
            StringBuilder sb = new StringBuilder();
            using (var stream = RawMessage(self))
            {
                StreamReader sr = new StreamReader(stream);
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    if (line.StartsWith("Date: "))
                    {
                        sb.Append("Date: ");
                        sb.Append(ToRfc822Timestamp(timestamp));
                        sb.Append("\r\n");
                        break;
                    }
                    else
                    {
                        sb.Append(line);
                        sb.Append("\r\n");
                    }
                }
                sb.Append(sr.ReadToEnd());
                ret = sb.ToString();
            }
            return ret;
        }

        public static string ToRfc822Timestamp(DateTime timestamp)
        {
            return timestamp.ToString("d MMM yyyy HH:mm:ss") + " " + timestamp.ToString("zzz").Replace(":", "");
        }

        public static string ToRfc822Timestamp(DateTimeOffset timestamp)
        {
            return timestamp.ToString("d MMM yyyy HH:mm:ss") + " " + timestamp.ToString("zzz").Replace(":", "");
        }

        public static string SimpleTextMail(
            string from,
            string to,
            string subject,
            string body,
            DateTimeOffset timestamp)
        {
            System.Net.Mail.MailMessage mailmessage = new System.Net.Mail.MailMessage();
            mailmessage.From = new MailAddress(from);
            mailmessage.To.Add(to);
            mailmessage.Subject = subject;
            mailmessage.Body = body;
            return RawText(mailmessage, timestamp);
        }

        //public static string TextMail(
        //    string from,
        //    string to,
        //    string subject,
        //    string message,
        //    string richMessage,
        //    DateTimeOffset timestamp)
        //{
        //    if (string.IsNullOrEmpty(richMessage))
        //    {
        //        return SimpleTextMail(from, to, subject, message, timestamp);
        //    }

        //    System.Net.Mail.MailMessage mailmessage = new System.Net.Mail.MailMessage();
        //    mailmessage.From = new MailAddress(from);
        //    mailmessage.To.Add(to);
        //    mailmessage.Subject = subject;
        //    mailmessage.Body = string.Format(
        //        HtmlMailBodyFormat, message, richMessage);
        //    mailmessage.IsBodyHtml = true;
        //    return RawText(mailmessage, timestamp);
        //}
    }
}
