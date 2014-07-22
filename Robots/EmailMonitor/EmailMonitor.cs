using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Exchange.WebServices.Data;
using MSGorilla.EmailMonitor.Sql;
namespace MSGorilla.EmailMonitor
{
    /// <summary>
    /// Monitor email account and post mail content to MSGorilla if the receiver or ccer contain _monitorEmailAddress
    /// </summary>
    public class EmailMonitor
    {
        private EmailManager _emailRetriever;
        private MSGorillaEmailReporter _emailReporter;
        private string _monitorEmailAddress;

        public EmailMonitor(string emailAddress, 
            string emailPassword, 
            string gorillaUsername, 
            string gorillaPassword,
            string monitorEmailAddress)
        {
            _emailRetriever = new EmailManager(emailAddress, emailPassword);
            _emailReporter = new MSGorillaEmailReporter(gorillaUsername, gorillaPassword);
            _monitorEmailAddress = monitorEmailAddress;
        }

        public int ScanAndReport()
        {
            int ret = 0;
            try
            {
                using (var ctx = new EmailMonitorContext())
                {
                    DateTime start;
                    DateTime now = DateTime.UtcNow;
                    EmailMonitoringHistory history = ctx.EmailMonitoringHistory.Find(_monitorEmailAddress);

                    if (history == null)
                    {
                        Logger.Info("Haven't monitor this email address yet.");
                        history = new EmailMonitoringHistory();
                        history.Address = _monitorEmailAddress;
                        ctx.EmailMonitoringHistory.Add(history);
                        start = now.AddDays(-30);
                    }
                    else
                    {
                        //timestamp in sql is Universal time.However somehow sql server lose the timezone
                        //User the following to create a correct Utc Datetime
                        start = history.LastUpdateTimestamp.ToLocalTime().ToUniversalTime();
                    }


                    Logger.Debug(string.Format("Start to scan new email on {0} from {1:u} to {2:u}", _monitorEmailAddress, start, now));
                    List<EmailMessage> emails = _emailRetriever.RetrieveEmailMessage(_monitorEmailAddress, start, now);

                    Logger.Info(string.Format("{0} new emails found related to {1}", emails.Count, _monitorEmailAddress));
                    foreach (EmailMessage mail in emails)
                    {
                        history.LastUpdateTimestamp = mail.DateTimeReceived;
                        ctx.SaveChanges();
                        _emailReporter.Report(mail);
                        ret++;
                    }

                    history.LastUpdateTimestamp = now;
                    ctx.SaveChanges();
                }
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
                Logger.Error(e.StackTrace);
            }

            return ret;
        }
    }
}
