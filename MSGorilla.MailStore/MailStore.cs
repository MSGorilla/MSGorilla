using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSGorilla.MailStore
{
    public class MailStore
    {
        public string Userid { get; set; }
        public MailStore(string userid)
        {
            this.Userid = userid;
            this.CreateMailboxIfNotExist("Inbox");
        }

        public List<MailBox> GetAllMailbox(MSGorillaMailEntities ctx)
        {
            return ctx.MailBoxes.Where(mailbox => mailbox.Userid == Userid).ToList();
        }
        public List<MailBox> GetAllMailbox()
        {
            using (var ctx = new MSGorillaMailEntities())
            {
                return GetAllMailbox(ctx);
            }
        }

        public MailBox GetMailbox(int id, MSGorillaMailEntities ctx)
        {
            return ctx.MailBoxes.Find(id);
        }

        public MailBox GetMailbox(int id)
        {
            using (var ctx = new MSGorillaMailEntities())
            {
                return GetMailbox(id, ctx);
            }
        }

        public MailBox GetMailbox(string path, MSGorillaMailEntities ctx)
        {
            return ctx.MailBoxes.Where(mailbox => mailbox.Path == path && mailbox.Userid == Userid).FirstOrDefault();
        }
        public MailBox GetMailbox(string path)
        {
            using (var ctx = new MSGorillaMailEntities())
            {
                return GetMailbox(path, ctx);
            }
        }

        public int GetUnseenMailCount(MailBox mailbox, MSGorillaMailEntities ctx)
        {
            return ctx.MailMessages.Count(mail => mail.MailboxID == mailbox.MailboxID && mail.Seen == false);
                //(from mailMessage in ctx.MailMessages 
                //    where mailMessage.MailboxID == mailbox.MailboxID
                //        && mailMessage.Seen == false)
        }

        public int GetUnseenMailCount(MailBox mailbox)
        {
            using (var ctx = new MSGorillaMailEntities())
            {
                return GetUnseenMailCount(mailbox);
            }
        }

        public List<MailBox> SearchMailbox(string name, MSGorillaMailEntities ctx)
        {
            if (name.Contains('%') || name.Contains('*'))
            {
                name = name.Replace('*', '%');
                return ctx.MailBoxes.SqlQuery(
                        "select * from Mailbox where Userid = {0} and Path like {1}",
                        this.Userid,
                        name
                    ).ToList();

            }
            return ctx.MailBoxes.Where(
                mailbox => mailbox.Userid == Userid && mailbox.Path == name
            ).ToList();
        }

        public List<MailBox> SearchMailbox(string name)
        {
            using (var ctx = new MSGorillaMailEntities())
            {
                return SearchMailbox(name, ctx);
            }
        }

        protected MailBox CreateSingleMailboxIfNotExist(string path, MSGorillaMailEntities ctx)
        {
            if (GetMailbox(path, ctx) == null)
            {
                MailBox mailbox = new MailBox();
                mailbox.Path = path;
                mailbox.Exist = 0;
                mailbox.Recent = 0;
                mailbox.UidNext = 1;
                mailbox.UidValidity = 1;
                mailbox.Userid = this.Userid;

                ctx.MailBoxes.Add(mailbox);
                return mailbox;
            }
            return ctx.MailBoxes.Where(mailbox => mailbox.Path == path && mailbox.Userid == Userid).FirstOrDefault();
        }

        public MailBox CreateMailboxIfNotExist(string path, MSGorillaMailEntities ctx)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new Exception("Path can't be null");
            }
            if (path.Length > 256)
            {
                throw new Exception("Folder path too long. No more than 256 chars");
            }

            MailBox box = null;
            string[] split = path.Split('/');
            string curPath = "";
            foreach (string str in split)
            {
                if (!string.IsNullOrEmpty(str))
                {
                    curPath += str;
                    box = CreateSingleMailboxIfNotExist(curPath, ctx);
                    curPath += '/';
                }
            }
            ctx.SaveChanges();
            return box;
        }

        public MailBox CreateMailboxIfNotExist(string path)
        {
            using (var ctx = new MSGorillaMailEntities())
            {
                MailBox mailbox = CreateMailboxIfNotExist(path, ctx);
                ctx.SaveChanges();
                return mailbox;
            }
        }

        public void DeleteMailboxIfExist(string path, MSGorillaMailEntities ctx)
        {
            MailBox mailbox = GetMailbox(path, ctx);
            if (mailbox != null)
            {
                ctx.MailBoxes.Remove(mailbox);
            }
        }

        public void DeleteMailboxIfExist(string path)
        {
            using (var ctx = new MSGorillaMailEntities())
            {
                DeleteMailboxIfExist(path, ctx);
                ctx.SaveChanges();
            }
        }

        /// <summary>
        /// the size is wrong. This function need to be refined
        /// </summary>
        /// <param name="msgorillaMessageID"></param>
        /// <param name="mailbox"></param>
        /// <param name="ctx"></param>
        public void AddMailMessage(string msgorillaMessageID, 
            int size, 
            int importance, 
            MailBox mailbox, 
            MSGorillaMailEntities ctx)
        {
            AddMailMessage(msgorillaMessageID, size, importance, mailbox.MailboxID, ctx);
        }

        public void AddMailMessage(string msgorillaMessageID, 
            int size, 
            int importance,
            int mailboxID, 
            MSGorillaMailEntities ctx)
        {
            MailBox mailbox = ctx.MailBoxes.Find(mailboxID);

            MailMessage message = new MailMessage();
            message.MailboxID = mailbox.MailboxID;
            message.MSGorillaMessageID = msgorillaMessageID;
            message.Recent = true;
            message.Seen = false;
            message.SequenceNumber = mailbox.Exist + 1;
            message.Uid = mailbox.UidNext;
            message.Important = importance;
            message.Timestamp = DateTime.UtcNow;
            message.Size = size;

            ctx.MailMessages.Add(message);

            mailbox.UidNext++;
            mailbox.Recent++;
            mailbox.Exist++;
            ctx.SaveChanges();
        }

        public void AddMailMessage(string msgorillaMessageID, int size, int importance, MailBox mailbox)
        {
            using (var ctx = new MSGorillaMailEntities())
            {
                AddMailMessage(msgorillaMessageID, size, importance, mailbox, ctx);
            }
        }

        public void AddMailMessage(string msgorillaMessageID, int size, int importance, int mailboxID)
        {
            using (var ctx = new MSGorillaMailEntities())
            {
                AddMailMessage(msgorillaMessageID, size, importance, mailboxID, ctx);
            }
        }

        public MailMessage GetMailMessageByUid(int uid, MailBox mailbox, MSGorillaMailEntities ctx)
        {
            return ctx.MailMessages.Where(
                mail => mail.Uid == uid && mail.MailboxID == mailbox.MailboxID
                ).FirstOrDefault();
        }

        public List<MailMessage> GetMailMessageByUidRange(int start, int end, MailBox mailbox, MSGorillaMailEntities ctx)
        {
            return ctx.MailMessages.Where(
                mail => mail.Uid >= start && mail.Uid <= end && mail.MailboxID == mailbox.MailboxID
                ).ToList();
        }

        public MailMessage GetMailMessageBySeqID(int seqID, MailBox mailbox, MSGorillaMailEntities ctx)
        {
            return ctx.MailMessages.Where(
                mail => mail.MailboxID == mailbox.MailboxID && mail.SequenceNumber == seqID
                ).FirstOrDefault();
        }

        public List<MailMessage> GetMailMessageBySeqIDRange(int start, int end, MailBox mailbox, MSGorillaMailEntities ctx)
        {
            return ctx.MailMessages.Where(
                mail => mail.MailboxID == mailbox.MailboxID && mail.SequenceNumber >= start && mail.SequenceNumber <= end
                ).ToList();
        }
    }
}