using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MSGorilla.IMAPServer.Response;

namespace MSGorilla.IMAPServer.State
{
    public class LogoutState : BaseState
    {
        public LogoutState(IMAPSession session)
            : base(session)
        {

        }

        public override void Enter()
        {
            this.Session.AppendResponse(new ServerStatusResponse("*", ServerStatusResponseType.BYE, "logging out"));
            this.Session.Flush();
        }
    }
}
