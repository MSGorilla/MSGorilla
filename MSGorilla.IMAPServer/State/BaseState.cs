using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MSGorilla.IMAPServer.Command;
using MSGorilla.IMAPServer.Response;

namespace MSGorilla.IMAPServer.State
{
    public class BaseState
    {
        private IMAPSession _session;
        public IMAPSession Session
        {
            get
            {
                return this._session;
            }
        }
        public BaseState(IMAPSession session)
        {
            this._session = session;
        }

        public virtual void Enter()
        {

        }

        public virtual void Leave()
        {

        }

        public void ProcessCommand(BaseCommand cmd)
        {
            if (cmd is CapabilityCommand)
            {
                ProcessCapabilityCommand(cmd as CapabilityCommand);
            }
            else if (cmd is NoopCommand)
            {
                ProcessNoopCommand(cmd as NoopCommand);
            }
            else if (cmd is LogoutCommand)
            {
                ProcessLogoutCommand(cmd as LogoutCommand);
            }
            else if (cmd is StartTLSCommand)
            {
                ProcessStartTlsCommand(cmd as StartTLSCommand);
            }
            else if (cmd is AuthenticateCommand)
            {
                ProcessAuthenticateCommand(cmd as AuthenticateCommand);
            }
            else if (cmd is LoginCommand)
            {
                ProcessLoginCommand(cmd as LoginCommand);
            }
            else if (cmd is SelectCommand)
            {
                ProcessSelectCommand(cmd as SelectCommand);
            }
            else if (cmd is ExamineCommand)
            {
                ProcessExamineCommand(cmd as ExamineCommand);
            }
            else if (cmd is CreateCommand)
            {
                ProcessCreateCommand(cmd as CreateCommand);
            }
            else if (cmd is DeleteCommand)
            {
                ProcessDeleteCommand(cmd as DeleteCommand);
            }
            else if (cmd is RenameCommand)
            {
                ProcessRenameCommand(cmd as RenameCommand);
            }
            else if (cmd is SubscribeCommand)
            {
                ProcessSubscribeCommand(cmd as SubscribeCommand);
            }
            else if (cmd is UnsubscribeCommand)
            {
                ProcessUnsubscribeCommand(cmd as UnsubscribeCommand);
            }
            else if (cmd is ListCommand)
            {
                ProcessListCommand(cmd as ListCommand);
            }
            else if (cmd is LsubCommand)
            {
                ProcessLsubCommand(cmd as LsubCommand);
            }
            else if (cmd is StatusCommand)
            {
                ProcessStatusCommand(cmd as StatusCommand);
            }
            else if (cmd is AppendCommand)
            {
                ProcessAppendCommand(cmd as AppendCommand);
            }
            else if (cmd is CheckCommand)
            {
                ProcessCheckCommand(cmd as CheckCommand);
            }
            else if (cmd is CloseCommand)
            {
                ProcessCloseCommand(cmd as CloseCommand);
            }
            else if (cmd is ExpungeCommand)
            {
                ProcessExpungeCommand(cmd as ExpungeCommand);
            }
            else if (cmd is SearchCommand)
            {
                ProcessSearchCommand(cmd as SearchCommand);
            }
            else if (cmd is FetchCommand)
            {
                ProcessFetchCommand(cmd as FetchCommand);
            }
            else if (cmd is StoreCommand)
            {
                ProcessStoreCommand(cmd as StoreCommand);
            }
            else if (cmd is CopyCommand)
            {
                ProcessCopyCommand(cmd as CopyCommand);
            }
            else
            {
                this.Session.AppendResponse(
                    new ServerStatusResponse(cmd.Tag, 
                        ServerStatusResponseType.NO, 
                        "This command is unsupportted yet")
                );
            }

            this.Session.Flush();
        }

        public virtual void ProcessCapabilityCommand(CapabilityCommand cmd)
        {
            this.Session.AppendResponse(new CapabilityResponse());
            this.Session.AppendResponse(new ServerStatusResponse(cmd.Tag, ServerStatusResponseType.OK, "CAPABILITY completed"));
        }

        public virtual void ProcessNoopCommand(NoopCommand cmd)
        {
            this.Session.AppendResponse(new ServerStatusResponse(cmd.Tag, ServerStatusResponseType.OK, "NOOP completed"));
        }

        public virtual void ProcessLogoutCommand(LogoutCommand cmd)
        {
            this.Session.ChangeState(new LogoutState(this.Session));
            this.Session.AppendResponse(new ServerStatusResponse(cmd.Tag, ServerStatusResponseType.OK, "LOGOUT completed"));
        }

        public virtual void ProcessLoginCommand(LoginCommand cmd)
        {
            this.Session.AppendResponse(
                    new ServerStatusResponse(cmd.Tag,
                        ServerStatusResponseType.NO,
                        "LOGIN State Error")
                );
        }

        public virtual void ProcessStartTlsCommand(StartTLSCommand cmd)
        {
            this.Session.AppendResponse(
                    new ServerStatusResponse(cmd.Tag,
                        ServerStatusResponseType.NO,
                        "STARTTLS State Error")
                );
        }
        public virtual void ProcessAuthenticateCommand(AuthenticateCommand cmd)
        {
            this.Session.AppendResponse(
                    new ServerStatusResponse(cmd.Tag,
                        ServerStatusResponseType.NO,
                        "AUTHENTICATE State Error")
                );
        }
 
        //Authenticated Status
        public virtual void ProcessSelectCommand(SelectCommand cmd)
        {
            this.Session.AppendResponse(
                    new ServerStatusResponse(cmd.Tag,
                        ServerStatusResponseType.NO,
                        "SELECT State Error")
                );
        }
        public virtual void ProcessExamineCommand(ExamineCommand cmd)
        {
            this.Session.AppendResponse(
                    new ServerStatusResponse(cmd.Tag,
                        ServerStatusResponseType.NO,
                        "EXAMINE State Error")
                );
        }
        public virtual void ProcessCreateCommand(CreateCommand cmd)
        {
            this.Session.AppendResponse(
                    new ServerStatusResponse(cmd.Tag,
                        ServerStatusResponseType.NO,
                        "CREATE State Error")
                );
        }
        public virtual void ProcessDeleteCommand(DeleteCommand cmd)
        {
            this.Session.AppendResponse(
                    new ServerStatusResponse(cmd.Tag,
                        ServerStatusResponseType.NO,
                        "DELETE State Error")
                );
        }
        public virtual void ProcessRenameCommand(RenameCommand cmd)
        {
            this.Session.AppendResponse(
                    new ServerStatusResponse(cmd.Tag,
                        ServerStatusResponseType.NO,
                        "RENAME State Error")
                );
        }
        public virtual void ProcessSubscribeCommand(SubscribeCommand cmd)
        {
            this.Session.AppendResponse(
                    new ServerStatusResponse(cmd.Tag,
                        ServerStatusResponseType.NO,
                        "SUBSCRIBE State Error")
                );
        }
        public virtual void ProcessUnsubscribeCommand(UnsubscribeCommand cmd)
        {
            this.Session.AppendResponse(
                    new ServerStatusResponse(cmd.Tag,
                        ServerStatusResponseType.NO,
                        "UNSUBSCRIBE State Error")
                );
        }
        public virtual void ProcessListCommand(ListCommand cmd)
        {
            this.Session.AppendResponse(
                    new ServerStatusResponse(cmd.Tag,
                        ServerStatusResponseType.NO,
                        "LIST State Error")
                );
        }
        public virtual void ProcessLsubCommand(LsubCommand cmd)
        {
            this.Session.AppendResponse(
                    new ServerStatusResponse(cmd.Tag,
                        ServerStatusResponseType.NO,
                        "LSUB State Error")
                );
        }
        public virtual void ProcessStatusCommand(StatusCommand cmd)
        {
            this.Session.AppendResponse(
                    new ServerStatusResponse(cmd.Tag,
                        ServerStatusResponseType.NO,
                        "STATUS State Error")
                );
        }
        public virtual void ProcessAppendCommand(AppendCommand cmd)
        {
            this.Session.AppendResponse(
                    new ServerStatusResponse(cmd.Tag,
                        ServerStatusResponseType.NO,
                        "APPEND State Error")
                );
        }

        //Selected State
        public virtual void ProcessCheckCommand(CheckCommand cmd)
        {
            this.Session.AppendResponse(
                    new ServerStatusResponse(cmd.Tag,
                        ServerStatusResponseType.NO,
                        "CHECK State Error")
                );
        }
        public virtual void ProcessCloseCommand(CloseCommand cmd)
        {
            this.Session.AppendResponse(
                    new ServerStatusResponse(cmd.Tag,
                        ServerStatusResponseType.NO,
                        "CLOSE State Error")
                );
        }
        public virtual void ProcessExpungeCommand(ExpungeCommand cmd)
        {
            this.Session.AppendResponse(
                    new ServerStatusResponse(cmd.Tag,
                        ServerStatusResponseType.NO,
                        "EXPUNGE State Error")
                );
        }
        public virtual void ProcessSearchCommand(SearchCommand cmd)
        {
            this.Session.AppendResponse(
                       new ServerStatusResponse(cmd.Tag,
                           ServerStatusResponseType.NO,
                           "SEARCH State Error")
                   );
        }
        public virtual void ProcessFetchCommand(FetchCommand cmd)
        {
            this.Session.AppendResponse(
                    new ServerStatusResponse(cmd.Tag,
                        ServerStatusResponseType.NO,
                        "FETCH State Error")
                );
        }
        public virtual void ProcessStoreCommand(StoreCommand cmd)
        {
            this.Session.AppendResponse(
                    new ServerStatusResponse(cmd.Tag,
                        ServerStatusResponseType.NO,
                        "STORE State Error")
                );
        }
        public virtual void ProcessCopyCommand(CopyCommand cmd)
        {
            this.Session.AppendResponse(
                    new ServerStatusResponse(cmd.Tag,
                        ServerStatusResponseType.NO,
                        "COPY State Error")
                );
        }
    }
}
