using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;

using MSGorilla.IMAPServer.Command;

namespace MSGorilla.IMAPServer
{
    class Program
    {
        static void Main(string[] args)
        {
            //BaseCommand cmd = new BaseCommand();
            //cmd.Parse("jx1d", " SELECT \"INBOX\"");
            //foreach (string s in cmd.GetDataTokens(100, false))
            //{
            //    Console.WriteLine(s);
            //}

            //StatusEnum status1 = (StatusEnum)Enum.Parse(typeof(StatusEnum), "Active", true);
            //StatusEnum status2 = (StatusEnum)Enum.Parse(typeof(StatusEnum), "active", true);
            //StatusEnum status3 = (StatusEnum)Enum.Parse(typeof(StatusEnum), "Inactive", true);

            //Console.WriteLine(StatusEnum.Active.ToString());

            //CommandParser parser = new CommandParser();

            //Type cmdType;
            //BaseCommand cmd = parser.Parse("aaaa uid search ANSWERED larger 100000 1:100 uid 3:20", out cmdType);

            //if (cmd is SearchCommand)
            //{
            //    SearchCommand c = cmd as SearchCommand;

            //    Console.WriteLine(cmd.Tag);
            //    Console.WriteLine(cmd.Data);
            //}


            //IPAddress myIP = IPAddress.Parse("127.0.0.1");
            //TcpListener imapServer = new TcpListener(myIP, 143);

            //imapServer.Start();
            //Console.WriteLine(string.Format("Start imap server on {0}:{1}", myIP.ToString(), 143));
            //while (true)
            //{
            //    TcpClient client = imapServer.AcceptTcpClient();
            //    IMAPSession session = new IMAPSession(client);
            //    session.CommandLoop();
            //}

            IPAddress addr = Dns.Resolve(Dns.GetHostName()).AddressList[0];

            FakeSmtpServer smtpServer = new FakeSmtpServer(addr, 25);
            new Thread(new ThreadStart(smtpServer.Start)).Start();

            //Thread smtp = new Thread(SimpleSMTPLoginServer);
            //smtp.Start();

            //IPAddress myIP = IPAddress.Parse("127.0.0.1");

            IMAPServer server = new IMAPServer(addr, 143);
            server.Start();


            //Console.WriteLine(DateTime.Parse("02-Sep-2014").ToString());
        }
    }
}
