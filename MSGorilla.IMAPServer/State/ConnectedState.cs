using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MSGorilla.IMAPServer.Response;
using MSGorilla.IMAPServer.Command;

namespace MSGorilla.IMAPServer.State
{
    /// <summary>
    /// Equals to Unauthenticated state
    /// </summary>
    public class ConnectedState : BaseState
    {
        public ConnectedState(IMAPSession session) : base(session)
        {

        }

        public override void Enter()
        {
            this.Session.AppendResponse(new ServerStatusResponse("*", ServerStatusResponseType.OK, "MSGorilla IMAP Server ready for requests"));
            this.Session.Flush();
        }

        public override void ProcessLoginCommand(LoginCommand cmd)
        {
            //User authentication......
            //
            //
            if (cmd.Password.Equals(cmd.Username))
            {
                this.Session.ChangeState(new AuthenticatedState(this.Session, cmd.Username));
                this.Session.AppendResponse(new ServerStatusResponse(cmd.Tag, ServerStatusResponseType.OK, "LOGIN completed"));
            }
            else
            {
                this.Session.AppendResponse(new ServerStatusResponse(cmd.Tag, ServerStatusResponseType.NO, "LOGIN Fail. User not found or wrong password."));
            }
        }

        public override void ProcessStartTlsCommand(StartTLSCommand cmd)
        {
            this.Session.AppendResponse(
                    new ServerStatusResponse(cmd.Tag,
                        ServerStatusResponseType.NO,
                        "STARTTLS is unsupportted yet")
                );
        }
        public override void ProcessAuthenticateCommand(AuthenticateCommand cmd)
        {
            this.Session.AppendResponse(
                    new ServerStatusResponse(cmd.Tag,
                        ServerStatusResponseType.NO,
                        "AUTHENTICATE is unsupportted yet")
                );
        }
    }
}
