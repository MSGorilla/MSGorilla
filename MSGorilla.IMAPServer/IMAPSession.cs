using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.IO;

using MSGorilla.IMAPServer.State;
using MSGorilla.IMAPServer.Command;
using MSGorilla.IMAPServer.Response;

namespace MSGorilla.IMAPServer
{
    public class IMAPSession
    {
        private TcpClient _client;
        private StreamWriter _writer;
        private StreamReader _reader;
        private BaseState _state;
        private CommandParser _parser;

        private const int MaxCommandErrorCount = 20;
        private const int MaxIdleTime = 1000 * 30 * 60; // million second

        public IMAPSession(TcpClient client)
        {
            this._client = client;
            this._state = new BaseState(this);
            _parser = new CommandParser();

            NetworkStream stream = this._client.GetStream();
            _writer = new StreamWriter(stream);
            _reader = new StreamReader(stream);

            this.ChangeState(new ConnectedState(this));
            //30 min timeout
            this._client.ReceiveTimeout = MaxIdleTime;    
        }

        public void ChangeState(BaseState state)
        {
            _state.Leave();
            _state = state;
            _state.Enter();
        }

        public void CommandLoop()
        {
            int badCmdCount = 0;
            while (!(this._state is LogoutState))
            {
                try
                {
                    string line = _reader.ReadLine();
                    if (line == null)
                    {
                        break;
                    }
                    Console.WriteLine("C->S: " + line);

                    Type cmdType;
                    BaseCommand cmd = this._parser.Parse(line, out cmdType);

                    if (cmd == null)
                    {
                        if(!string.IsNullOrEmpty(line))
                        {
                            this.AppendResponse(
                                new ServerStatusResponse(
                                    line.Split(' ')[0], 
                                    ServerStatusResponseType.BAD, 
                                    "command not support")
                            );
                            this.Flush();
                        }
                        
                        badCmdCount++;
                        if (badCmdCount > MaxCommandErrorCount)
                        {
                            this.AppendResponse(
                                new ServerStatusResponse("*", 
                                    ServerStatusResponseType.BAD, 
                                    "Too many bad commands"
                                )
                            );
                            this.ChangeState(new LogoutState(this));
                        }
                    }
                    else
                    {
                        try
                        {
                            this._state.ProcessCommand(cmd);
                        }
                        catch (Exception e)
                        {
                            AppendResponse(new ServerStatusResponse(cmd.Tag,
                                ServerStatusResponseType.NO,
                                cmd.GetCommandName() + " fail. " + e.Message));
                            Flush();
                        }
                    }
                }
                catch (IOException)
                {
                    this.AppendResponse(
                        new ServerStatusResponse("*", ServerStatusResponseType.BYE, "Auto logout, idle for too long")
                    );
                    this.Flush();
                    break;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    Console.WriteLine(e.StackTrace);
                }
            }

            Close();
        }

        public void AppendResponse(BaseResponse response)
        {
            _writer.WriteLine(response.ToString());
            Console.WriteLine("[{0}]S->C: {1}", DateTime.Now, response.ToString());
        }

        public void Flush()
        {
            _writer.Flush();
        }

        public void Close()
        {
            this._reader.Close();
            this._writer.Close();
            this._client.Close();
        }
    }
}
