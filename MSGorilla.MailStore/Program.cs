using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MSGorilla.MailStore.Helper;

namespace MSGorilla.MailStore
{
    class Program
    {
        static void Main(string[] args)
        {
            string html = MSGorillaMailGenerator.CreateTextMessageMail(
                    "#WOSS Codeflow# MSFT #WOSS TFS 713969# :Job scheduler scheduling and execution empty framework Iteration 1 Author: lazhang Reviewer: @fanzhang, @shengl, @dac, @justisun, @geshen, @woss-a #WOSS Change 1217711# Review: http://codeflow/Client/CodeFlow2010.application?server=http://codeflow/Services/DiscoveryService.svc&review=lazhang-51d05bb1ffd8470eacb3ccafb0c835a5",
                    "msgorilladev",
                    DateTime.Now,
                    "me",
                    "no-reply",
                    "<p>some rich message</p>"
                );
            Console.WriteLine(html);
            //MailStore store = new MailStore("user1");

            //using (var ctx = new MSGorillaMailEntities())
            //{
            //    MailBox mailbox = store.CreateMailboxIfNotExist("Inbox/Topic/sometopic", ctx);
            //    ctx.SaveChanges();
            //    mailbox = store.GetMailbox(mailbox.Path, ctx);
            //    store.AddMailMessage(
            //        "251993189487170_woss_CFMonitor_90d1e542-c46f-4334-b1f3-8d8dc1c4b6a4",
            //        10,
            //        mailbox,
            //        ctx);
            //    ctx.SaveChanges();
            //}

            //System.Net.Mail.MailMessage mail = new System.Net.Mail.MailMessage();
            //mail.Body = "Hello world";
            //mail.From = new System.Net.Mail.MailAddress("ddd@cloud.msgorilla.com");
            //mail.To.Add(new System.Net.Mail.MailAddress("me@cloud.msgorilla.com"));

            //Console.WriteLine(MailMessageExtensions.RawText(mail, DateTime.Parse("2000-1-1")));


            //DateTime time = DateTime.Parse("16 Sep 2014 10:11:38 +0800ddd");
            //Console.WriteLine(time);
        }
    }
}
