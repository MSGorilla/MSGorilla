using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MSGorillaIMAPServer;
using MSGorilla.MailStore;
using MSGorilla.IMAPServer.Response;
using MSGorilla.IMAPServer.Command;
using MSGorilla.IMAPServer.DataType;
using MSGorilla.MailStore.Helper;

using MSGorilla.Library;
using MSGorilla.Library.Models;
using System.Runtime.Caching;

namespace MSGorilla.IMAPServer.State
{
    public class SelectedState : AuthenticatedState
    {
        private MailBox _mailbox;

        /// <summary>
        /// Search command and Fetch command are the most important 2 commands in our senario
        /// </summary>
        /// <param name="session"></param>
        /// <param name="username"></param>
        /// <param name="mailbox"></param>
        public SelectedState(IMAPSession session, string username, MailBox mailbox)
            : base(session, username)
        {
            this._mailbox = mailbox;
        }

        public override void Enter()
        {
            this.Session.AppendResponse(new ExistResponse(_mailbox.Exist));
            this.Session.AppendResponse(new RecentResponse(_mailbox.Recent));
            ServerStatusResponse rep = new ServerStatusResponse(
                "*", 
                ServerStatusResponseType.OK, 
                "UIDs valid", 
                new ResponseCode(ResponseCodeType.UIDVALIDITY, this._mailbox.UidValidity.ToString())
                );
            this.Session.AppendResponse(rep);
            this.Session.AppendResponse(new FlagResponse());
            rep = new ServerStatusResponse(
                "*", 
                ServerStatusResponseType.OK, 
                "Limited", 
                new ResponseCode(ResponseCodeType.PERMANENTFLAGS, "(\\Answered \\Seen \\Deleted \\Draft \\Flagged)"));
            this.Session.AppendResponse(rep);
        }

        public override void ProcessNoopCommand(NoopCommand cmd)
        {
            using (var ctx = new MSGorillaMailEntities())
            {
                MailBox box = this.store.GetMailbox(_mailbox.MailboxID, ctx);
                if (box.Exist != _mailbox.Exist)
                {
                    _mailbox = box;
                    this.Enter();
                }
                else{
                    _mailbox = box;
                }
            }

            base.ProcessNoopCommand(cmd);
        }



        #region Search Command
        private string _createQuerySql(SearchCommand cmd)
        {
            StringBuilder sql = new StringBuilder(
                string.Format("select * from mailmessage where mailboxid={0}", this._mailbox.MailboxID)
                );

            foreach (var item in cmd.SearchItems)
            {
                switch (item.SearchType)
                {
                    case SearchItemType.All:
                        return sql.ToString();
                    case SearchItemType.Before:
                        DateTime timestamp = DateTime.Parse(item.Argument);                        
                        sql.Append(" and Timestamp < ");
                        sql.Append(string.Format("'{0}'", timestamp));
                        break;
                    case SearchItemType.Flagged:
                        sql.Append(" and Important = 0");
                        break;
                    case SearchItemType.Larger:
                        sql.Append(" and size >= ");
                        sql.Append(int.Parse(item.Argument));
                        break;
                    case SearchItemType.New:
                        sql.Append(" and recent=1 and seen=0");
                        break;
                    case SearchItemType.Old:
                        sql.Append(" and recent=0");
                        break;
                    case SearchItemType.On:
                        DateTime day = DateTime.Parse(item.Argument);
                        DateTime nextDay = day.AddDays(1);
                        sql.Append(" and timestamp > ");
                        sql.Append(string.Format("'{0}'", day));
                        sql.Append(" and timestamp < ");
                        sql.Append(string.Format("'{0}'", nextDay));
                        break;
                    case SearchItemType.Recent:
                        sql.Append(" and recent=1");
                        break;
                    case SearchItemType.Seen:
                        sql.Append(" and seen=1");
                        break;
                    case SearchItemType.SentBefore:
                        timestamp = DateTime.Parse(item.Argument);                        
                        sql.Append(" and Timestamp < ");
                        sql.Append(string.Format("'{0}'", timestamp));
                        break;
                    case SearchItemType.SentSince:
                        timestamp = DateTime.Parse(item.Argument);                        
                        sql.Append(" and Timestamp > ");
                        sql.Append(string.Format("'{0}'", timestamp));
                        break;
                    case SearchItemType.Since:
                        timestamp = DateTime.Parse(item.Argument);                        
                        sql.Append(" and Timestamp > ");
                        sql.Append(string.Format("'{0}'", timestamp));
                        break;
                    case SearchItemType.Smaller:
                        sql.Append(" and size <= ");
                        sql.Append(int.Parse(item.Argument));
                        break;
                    case SearchItemType.Unflagged:
                        sql.Append(" and important > 0");
                        break;
                    case SearchItemType.Unseen:
                        sql.Append(" and seen=1");
                        break;
                    default:
                        return null;
                }
            }
            return sql.ToString();
        }

        private List<MailMessage> Search(SearchCommand cmd)
        {
            List<MailMessage> ret = new List<MailMessage>();
            string sql = _createQuerySql(cmd);
            if (!string.IsNullOrEmpty(sql))
            {
                using (var ctx = new MSGorillaMailEntities())
                {
                    ret = ctx.MailMessages.SqlQuery(sql).ToList();
                }
            }
            return ret;
        }

        public override void ProcessSearchCommand(SearchCommand cmd)
        {
            SearchResponse response = new SearchResponse();
            List<MailMessage> mails = Search(cmd);
            if (cmd.IsUIDCommand)
            {
                foreach (var mail in mails)
                {
                    response.MessageIDList.Add(mail.Uid);
                }
            }
            else
            {
                foreach (var mail in mails)
                {
                    response.MessageIDList.Add(mail.SequenceNumber);
                }
            }

            this.Session.AppendResponse(response);
            this.Session.AppendResponse(
                       new ServerStatusResponse(cmd.Tag,
                           ServerStatusResponseType.OK,
                           "SEARCH completed")
                   );
        }

        #endregion

        #region Fetch Command

        private List<MailMessage> GetMailMessagesFromCmd(IMailProcessCommand cmd)
        {
            List<MailMessage> mails = new List<MailMessage>();
            using (var ctx = new MSGorillaMailEntities())
            {
                if (cmd.IsUIDCommand)
                {
                    foreach (var item in cmd.MessageID)
                    {
                        if (item.ItemType == MessageIDListItemType.Single)
                        {
                            mails.Add(this.store.GetMailMessageByUid(item.Start, this._mailbox, ctx));
                        }
                        else if (item.ItemType == MessageIDListItemType.BoundedRange)
                        {
                            mails = mails.Concat(this.store.GetMailMessageByUidRange(item.Start, item.End, this._mailbox, ctx)).ToList();
                        }
                        else if (item.ItemType == MessageIDListItemType.UnboundedRange)
                        {
                            mails = mails.Concat(this.store.GetMailMessageByUidRange(item.Start, int.MaxValue, this._mailbox, ctx)).ToList();
                        }
                    }
                }
                else
                {
                    foreach (var item in cmd.MessageID)
                    {
                        if (item.ItemType == MessageIDListItemType.Single)
                        {
                            mails.Add(this.store.GetMailMessageBySeqID(item.Start, this._mailbox, ctx));
                        }
                        else if (item.ItemType == MessageIDListItemType.BoundedRange)
                        {
                            mails = mails.Concat(this.store.GetMailMessageBySeqIDRange(item.Start, item.End, this._mailbox, ctx)).ToList();
                        }
                        else if (item.ItemType == MessageIDListItemType.UnboundedRange)
                        {
                            mails = mails.Concat(this.store.GetMailMessageBySeqIDRange(item.Start, int.MaxValue, this._mailbox, ctx)).ToList();
                        }
                    }
                }
            }

            return mails;
        }

        public static string CreateFlaggedString(MailMessage mail)
        {
            StringBuilder flags = new StringBuilder("(");
            if (mail.Recent)
            {
                flags.Append("\\Recent ");
            }
            if (mail.Important == 0)
            {
                flags.Append("\\Flagged ");
            }
            if (mail.Seen)
            {
                flags.Append("\\Seen");
            }
            return flags.ToString().TrimEnd() + ")";
        }

        private static FetchDataItem CreateFlagDateItem(MailMessage mail)
        {
            FetchDataItem item = new FetchDataItem(FetchDataItemType.Flags);
            item.Text = CreateFlaggedString(mail);
            return item;
        }

        private FetchDataItem CreateMailBodyDataItem(MailMessage mail)
        {
            FetchDataItem item = new FetchDataItem(FetchDataItemType.Body);

            string mailEnvelope = MSGorillaMailGenerator.CreateTextMessageMail(mail.MSGorillaMessageID, this.User);

            StringBuilder sb = new StringBuilder("{");
            sb.Append(mailEnvelope.Length);
            sb.Append("}\r\n");

            sb.Append(mailEnvelope);
            //sb.Append("\r\n");

            item.Text = sb.ToString();
            //Console.WriteLine("================={0}=======================", item.Text.Length);
            return item;
        }

        private FetchResponse Fetch(MailMessage mail, FetchDataList fetchDataList)
        {
            FetchResponse response = new FetchResponse(mail.SequenceNumber);

            FetchDataItem body = null;
            foreach (var data in fetchDataList)
            {
                switch (data)
                {
                    case FetchDataItemType.All:
                        break;
                    case FetchDataItemType.Body:
                        if (body == null)
                        {
                            body = CreateMailBodyDataItem(mail);
                        }
                        break;
                    case FetchDataItemType.BodyPeek:
                        if (body == null)
                        {
                            body = CreateMailBodyDataItem(mail);
                        }
                        break;
                    case FetchDataItemType.BodyStructure:
                        break;
                    case FetchDataItemType.Envelope:
                        break;
                    case FetchDataItemType.Fast:
                        break;
                    case FetchDataItemType.Flags:
                        response.Items.Add(new FetchDataItem(FetchDataItemType.Flags, CreateFlaggedString(mail)));
                        break;
                    case FetchDataItemType.Full:
                        break;
                    case FetchDataItemType.InternalDate:
                        response.Items.Add(
                            new FetchDataItem(
                                FetchDataItemType.InternalDate, 
                                string.Format("\"{0:d-MMM-yyyy HH:mm:ss} {1}\"", mail.Timestamp, mail.Timestamp.ToString("zzz").Replace(":", ""))
                            )
                        );
                        break;
                    case FetchDataItemType.Rfc822:
                        break;
                    case FetchDataItemType.Rfc822Header:
                        break;
                    case FetchDataItemType.Rfc822Size:
                        response.Items.Add(new FetchDataItem(FetchDataItemType.Rfc822Size, mail.Size.ToString()));
                        break;
                    case FetchDataItemType.Rfc822Text:
                        break;
                    case FetchDataItemType.Uid:
                        response.Items.Add(new FetchDataItem(FetchDataItemType.Uid, mail.Uid.ToString()));
                        break;
                    default:
                        break;
                }
            }
            //put the BODY[] item to the last
            if (body != null)
            {
                response.Items.Add(body);
            }
            return response;
        }

        public override void ProcessFetchCommand(FetchCommand cmd)
        {
            List<MailMessage> mails = GetMailMessagesFromCmd(cmd);

            foreach (var mail in mails)
            {
                this.Session.AppendResponse(Fetch(mail, cmd.fetchData));
            }
            
            this.Session.AppendResponse(
                    new ServerStatusResponse(cmd.Tag,
                        ServerStatusResponseType.OK,
                        "FETCH completed")
                );
        }
        #endregion

        public override void ProcessCheckCommand(CheckCommand cmd)
        {
            this.Session.AppendResponse(
                    new ServerStatusResponse(cmd.Tag,
                        ServerStatusResponseType.OK,
                        "CHECK completed")
                );
        }
        public override void ProcessCloseCommand(CloseCommand cmd)
        {
            this.Session.ChangeState(new AuthenticatedState(this.Session, this.User));
            this.Session.AppendResponse(
                    new ServerStatusResponse(cmd.Tag,
                        ServerStatusResponseType.OK,
                        "CLOSE completed")
                );
        }
        public override void ProcessExpungeCommand(ExpungeCommand cmd)
        {
            this.Session.AppendResponse(
                    new ServerStatusResponse(cmd.Tag,
                        ServerStatusResponseType.NO,
                        "EXPUNGE unsupported")
                );
        }

        #region Store Command

        private void Store(StoreCommand cmd, List<MailMessage> mails)
        {
            using (var ctx = new MSGorillaMailEntities())
            {
                foreach (var mail in mails)
                {
                    var curMail = ctx.MailMessages.Find(mail.ID);
                    if (cmd.Action == StoreCommand.ActionType.AddFlag)
                    {
                        AddFlags(curMail, cmd.Flags);
                    }
                    else if (cmd.Action == StoreCommand.ActionType.RemoveFlag)
                    {
                        RemoveFlags(curMail, cmd.Flags);
                    }
                    else
                    {
                        SetFlags(curMail, cmd.Flags);
                    }

                    if (cmd.IsSilent == false)
                    {
                        FetchResponse response = new FetchResponse(curMail.SequenceNumber);
                        response.Items.Add(new FetchDataItem(FetchDataItemType.Flags, CreateFlaggedString(curMail)));
                        this.Session.AppendResponse(response);
                    }
                }

                ctx.SaveChanges();
            }
        }

        private void RemoveFlags(MailMessage mail, List<FlagType> flags)
        {
            foreach (var flag in flags)
            {
                switch (flag)
                {
                    case FlagType.Seen:
                        mail.Seen = false;
                        break;
                    case FlagType.Flagged:
                        mail.Important = 2;
                        break;
                    case FlagType.Recent:
                        mail.Recent = false;
                        break;
                    default:
                        break;
                }
            }
        }

        private void AddFlags(MailMessage mail, List<FlagType> flags)
        {
            foreach (var flag in flags)
            {
                switch (flag)
                {
                    case FlagType.Seen:
                        mail.Seen = true;
                        break;
                    case FlagType.Flagged:
                        mail.Important = 0;
                        break;
                    case FlagType.Recent:
                        mail.Recent = true;
                        break;
                    default:
                        break;
                }
            }
        }

        private void SetFlags(MailMessage mail, List<FlagType> flags)
        {
            mail.Important = 2;
            mail.Recent = false;
            mail.Seen = false;
            AddFlags(mail, flags);
        }

        public override void ProcessStoreCommand(StoreCommand cmd)
        {
            List<MailMessage> mails = GetMailMessagesFromCmd(cmd);

            Store(cmd, mails);

            this.Session.AppendResponse(
                    new ServerStatusResponse(cmd.Tag,
                        ServerStatusResponseType.OK,
                        "STORE completed")
                );
        }

        #endregion

        public override void ProcessCopyCommand(CopyCommand cmd)
        {
            this.Session.AppendResponse(
                    new ServerStatusResponse(cmd.Tag,
                        ServerStatusResponseType.NO,
                        "COPY unsupportted.")
                );
        }
    }
}
