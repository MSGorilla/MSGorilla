using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace MSGorilla.IMAPServer
{
    public class IMAPServer
    {
        public IPAddress IP { get; private set; }
        public int Port { get; private set; }

        private bool Running = false;

        public IMAPServer(IPAddress ip, int port)
        {
            this.IP = ip;
            this.Port = port;
        }

        void SessionStart(object session)
        {
            IMAPSession sess = session as IMAPSession;
            sess.CommandLoop();
        }

        public void Start()
        {
            this.Running = true;
            TcpListener imapServer = new TcpListener(this.IP, Port);

            imapServer.Start();
            Console.WriteLine(string.Format("Start imap server on {0}:{1}", IP.ToString(), Port));
            while (this.Running)
            {
                TcpClient client = imapServer.AcceptTcpClient();
                IMAPSession session = new IMAPSession(client);

                Thread thread = new Thread(new ParameterizedThreadStart(this.SessionStart));
                thread.Start(session);
            }
        }

        public void Stop()
        {
            this.Running = false;
        }
    }
}
