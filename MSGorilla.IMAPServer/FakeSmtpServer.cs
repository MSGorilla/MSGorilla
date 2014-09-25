using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace MSGorilla.IMAPServer
{
    public class FakeSmtpServer
    {
        public IPAddress IP { get; private set; }
        public int Port { get; private set; }

        private volatile bool _running = false;
        public FakeSmtpServer(IPAddress ip, int port)
        {
            this.IP = ip;
            this.Port = port;
        }

        void _serve(Object client)
        {
            try
            {
                NetworkStream stream = ((TcpClient)client).GetStream(); 
                StreamReader sr = new StreamReader(stream);
                StreamWriter sw = new StreamWriter(stream);

                sw.WriteLine("220 smtp.msgorilla.cloudapp.net ESMTP Postfix");
                sw.Flush();

                while (true)
                {
                    string line = sr.ReadLine();
                    sw.WriteLine("250 OK");
                    sw.Flush();
                }
            }
            catch { }
        }

        public void Start()
        {
            this._running = true;

            TcpListener smtpServer = new TcpListener(IP, Port);
            smtpServer.Start();

            Trace.TraceInformation("Fake stmp server started. Listening on {0}:{1}.", IP, Port);
            while (this._running)
            {
                TcpClient client = smtpServer.AcceptTcpClient();
                Trace.TraceInformation("{0} connected.", client.Client.RemoteEndPoint.ToString());
                Thread serveThread = new Thread(new ParameterizedThreadStart(this._serve));
                serveThread.Start(client);
            }
        }

        public void Stop()
        {
            Trace.TraceInformation("Fake stmp server stopped.");
            this._running = false;
        }
    }
}
