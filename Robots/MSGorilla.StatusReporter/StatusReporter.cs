using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Mail;

namespace MSGorilla.StatusReporter
{
    public class StatusReporter
    {
        protected StatusCollector _collector;

        public StatusReporter()
        {
            _collector = new StatusCollector();
        }

        public void CollectStatusAndReport()
        {
            try
            {
                HistoryData data = _collector.GetLatestData();
                //collect and report status every day
                if (data == null || DateTime.UtcNow.Subtract(data.Date).TotalDays >= 1)
                {
                    Logger.Info("Start to update MSGorilla status.");
                    _collector.UpdateStatusAndSave();
                    Logger.Info("Update complete.");

                    Logger.Info("Start to report MSGorilla status.");
                    this.Report();
                    Logger.Info("Report complete.");
                }
            }
            catch (Exception e)
            {
                Logger.Error(e.StackTrace);
            }
        }

        public virtual void Report()
        {
            throw new Exception("Unsupportted operation");
        }

        private void MailReport()
        {
            MailMessage mail = new MailMessage();
            mail.From = new MailAddress("t-yig@microsoft.com");

            mail.To.Add(new MailAddress("t-yig@microsoft.com"));
            mail.To.Add(new MailAddress("bekimd@microsoft.com"));
            mail.To.Add(new MailAddress("v-dafu@microsoft.com"));
            mail.To.Add(new MailAddress("qiufang.shi@microsoft.com"));
            mail.To.Add(new MailAddress("sijhua@microsoft.com"));

            mail.Subject = string.Format("MSGorilla Status {0:yyyy-MM-dd} UTC", DateTime.UtcNow);

            mail.Body = "";
            StringBuilder sb = new StringBuilder("");

            try
            {
                int count = _collector.GetCurrentUserCount();
                sb.AppendLine(string.Format("{0} User counts exist is at {1}", count, DateTime.UtcNow));
            }
            catch (Exception e)
            {
                Logger.Error(e.ToString());
            }

            try
            {
                int count = _collector.GetCurrentRobotCount();
                sb.AppendLine(string.Format("{0} Robot counts exist at {1}", count, DateTime.UtcNow));
            }
            catch (Exception e)
            {
                Logger.Error(e.ToString());
            }

            try
            {
                int count = _collector.GetCurrentTopicCount();
                sb.AppendLine(string.Format("{0} topic count exist at {1}", count, DateTime.UtcNow));
            }
            catch (Exception e)
            {
                Logger.Error(e.ToString());
            }

            try
            {
                DateTime date = DateTime.UtcNow.AddDays(-1);
                int count = _collector.GetTotalRobotMessageCountByDateUtc(date);
                sb.AppendLine(string.Format("{0} messages was posted by robots in {1:yyyy-MM-dd}", count, date));
            }
            catch (Exception e)
            {
                Logger.Error(e.ToString());
            }
            mail.Body = sb.ToString();

            sendMail(mail);
        }

        private void sendMail(MailMessage mail)
        {
            try
            {
                MailMessage objeto_mail = new MailMessage();
                SmtpClient client = new SmtpClient("smtphost.redmond.corp.microsoft.com");
                //client.EnableSsl = true;
                client.Port = 25;

                client.Timeout = 30000;
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.UseDefaultCredentials = false;
                client.Credentials = new NetworkCredential("t-yig", "0msAd51398929", "fareast");

                client.Send(mail);

            }
            catch (Exception e)
            {
                Console.WriteLine("{0} Exception caught.", e);
            }
        }
    }
}
