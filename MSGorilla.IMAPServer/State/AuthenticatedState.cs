using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MSGorilla.IMAPServer.Command;
using MSGorilla.IMAPServer.Response;
using MSGorilla.MailStore;

namespace MSGorilla.IMAPServer.State
{
    public class AuthenticatedState : BaseState
    {
        protected string User;
        protected MailStore.MailStore store;
        public AuthenticatedState(IMAPSession session, string username)
            : base(session)
        {
            this.User = username;
            this.store = new MailStore.MailStore(this.User);
        }

        public override void ProcessSelectCommand(SelectCommand cmd)
        {
            MailBox mailbox = store.GetMailbox(cmd.MailBox);
            if (mailbox == null)
            {
                this.Session.AppendResponse(
                    new ServerStatusResponse(
                        cmd.Tag,
                        ServerStatusResponseType.NO,
                        "SELECT fail. Mailbox not found.")
                );
                return;
            }

            this.Session.ChangeState(new SelectedState(this.Session, this.User, mailbox));
            this.Session.AppendResponse(
                new ServerStatusResponse(
                    cmd.Tag,
                    ServerStatusResponseType.OK,
                    "SELECT completed",
                    new ResponseCode(ResponseCodeType.READONLY))
                );
        }

        public override void ProcessExamineCommand(ExamineCommand cmd)
        {
            MailBox mailbox = store.GetMailbox(cmd.MailBox);
            if (mailbox == null)
            {
                this.Session.AppendResponse(
                    new ServerStatusResponse(
                        cmd.Tag,
                        ServerStatusResponseType.NO,
                        "EXAMINE fail. Mailbox not found.")
                );
                return;
            }

            this.Session.ChangeState(new SelectedState(this.Session, this.User, mailbox));
            this.Session.AppendResponse(
                new ServerStatusResponse(
                    cmd.Tag, 
                    ServerStatusResponseType.OK,
                    "EXAMINE completed", 
                    new ResponseCode(ResponseCodeType.READONLY))
                );
        }

        public override void ProcessCreateCommand(CreateCommand cmd)
        {
            this.Session.AppendResponse(
                    new ServerStatusResponse(cmd.Tag,
                        ServerStatusResponseType.NO,
                        "CREATE is unsupportted yet")
                );
        }

        public override void ProcessDeleteCommand(DeleteCommand cmd)
        {
            this.Session.AppendResponse(
                    new ServerStatusResponse(cmd.Tag,
                        ServerStatusResponseType.NO,
                        "DELETE is unsupportted yet")
                );
        }

        public override void ProcessRenameCommand(RenameCommand cmd)
        {
            this.Session.AppendResponse(
                    new ServerStatusResponse(cmd.Tag,
                        ServerStatusResponseType.NO,
                        "RENAME is unsupportted yet")
                );
        }

        public override void ProcessSubscribeCommand(SubscribeCommand cmd)
        {
            this.Session.AppendResponse(
                    new ServerStatusResponse(cmd.Tag,
                        ServerStatusResponseType.NO,
                        "SUBSCRIBE is unsupportted yet")
                );
        }

        public override void ProcessUnsubscribeCommand(UnsubscribeCommand cmd)
        {
            this.Session.AppendResponse(
                    new ServerStatusResponse(cmd.Tag,
                        ServerStatusResponseType.NO,
                        "UNSUBSCRIBE is unsupportted yet")
                );
        }

        public override void ProcessListCommand(ListCommand cmd)
        {
            List<MailBox> boxes;
            if (cmd.MailBoxWithWildcards == "*")
            {
                boxes = store.GetAllMailbox();
            }
            else
            {
                boxes = store.SearchMailbox(cmd.MailBoxWithWildcards);
            }

            foreach (var box in boxes)
            {
                this.Session.AppendResponse(new ListResponse(box.Path));
            }
            this.Session.AppendResponse(new ServerStatusResponse(cmd.Tag, ServerStatusResponseType.OK, "LIST completed"));
        }

        public override void ProcessLsubCommand(LsubCommand cmd)
        {
            List<MailBox> boxes;
            if (cmd.MailBoxWithWildcards == "*")
            {
                boxes = store.GetAllMailbox();
            }
            else
            {
                boxes = store.SearchMailbox(cmd.MailBoxWithWildcards);
            }

            foreach (var box in boxes)
            {
                this.Session.AppendResponse(new LsubResponse(box.Path));
            }
            this.Session.AppendResponse(new ServerStatusResponse(cmd.Tag, ServerStatusResponseType.OK, "LSUB completed"));
        }

        public override void ProcessStatusCommand(StatusCommand cmd)
        {
            MailBox mailbox = store.GetMailbox(cmd.MailBox);
            if (mailbox == null)
            {
                this.Session.AppendResponse(
                    new ServerStatusResponse(
                        cmd.Tag, 
                        ServerStatusResponseType.NO, 
                        "STATUS fail. Mailbox not found.")
                );
                return;
            }

            StatusResponse response = new StatusResponse(mailbox.Path);
            foreach (var item in cmd.StatusDataItems)
            {
                switch (item)
                {
                    case StatusCommand.StatusItem.MESSAGES:
                        response.StatusItems.Add(
                            new StatusResponseItem(
                                StatusCommand.StatusItem.MESSAGES, mailbox.Exist.ToString()
                                ));
                        break;
                    case StatusCommand.StatusItem.RECENT:
                        response.StatusItems.Add(
                            new StatusResponseItem(
                                StatusCommand.StatusItem.RECENT, mailbox.Recent.ToString()
                                ));
                        break;
                    case StatusCommand.StatusItem.UIDVALIDITY:
                        response.StatusItems.Add(
                            new StatusResponseItem(
                                StatusCommand.StatusItem.UIDVALIDITY, mailbox.UidValidity.ToString()
                                ));
                        break;
                    case StatusCommand.StatusItem.UIDNEXT:
                        response.StatusItems.Add(
                            new StatusResponseItem(
                                StatusCommand.StatusItem.UIDNEXT, mailbox.UidNext.ToString()
                                ));
                        break;
                    case StatusCommand.StatusItem.UNSEEN:
                        response.StatusItems.Add(
                            new StatusResponseItem(
                                StatusCommand.StatusItem.UNSEEN, store.GetUnseenMailCount(mailbox).ToString()
                                ));
                        break;
                    default:
                        break;
                }
            }

            this.Session.AppendResponse(response);
            this.Session.AppendResponse(
                new ServerStatusResponse(cmd.Tag, ServerStatusResponseType.OK, "STATUS completed")
            );
        }

        public override void ProcessAppendCommand(AppendCommand cmd)
        {
            this.Session.AppendResponse(
                    new ServerStatusResponse(cmd.Tag,
                        ServerStatusResponseType.NO,
                        "APPEND is unsupportted yet")
                );
        }
    }
}
