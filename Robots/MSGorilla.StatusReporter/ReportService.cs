using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace MSGorilla.StatusReporter
{
    partial class ReportService : ServiceBase
    {
        private static System.Timers.Timer serviceTimer = null;

        public ReportService()
        {
            InitializeComponent();
            this.ServiceName = "ReportService";

        }

        protected override void OnStart(string[] args)
        {
            // TODO: Add code here to start your service.
            Logger.Info("---------------------------------------------");
            Logger.Info("Starting service......");
            base.OnStart(args);

            // Create a timer to periodically trigger an action
            serviceTimer = new Timer();

            // Hook up the Elapsed event for the timer.
            serviceTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            serviceTimer.AutoReset = true;

            serviceTimer.Interval = 10000;
            serviceTimer.Start();

            Logger.Info("Service started.");
        }

        protected override void OnStop()
        {
            // TODO: Add code here to perform any tear-down necessary to stop your service.
            serviceTimer.Enabled = false;
            base.OnStop();
            Logger.Info("Service stopped");
        }

        public static string Execute(string command, int seconds)
        {
            string output = ""; //输出字符串  
            if (command != null && !command.Equals(""))
            {
                Process process = new Process();//创建进程对象  
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.FileName = "cmd.exe";//设定需要执行的命令  
                startInfo.Arguments = "/C " + command;//“/C”表示执行完命令后马上退出  
                startInfo.UseShellExecute = false;//不使用系统外壳程序启动  
                startInfo.RedirectStandardInput = false;//不重定向输入  
                startInfo.RedirectStandardOutput = true; //重定向输出  
                startInfo.CreateNoWindow = true;//不创建窗口  
                process.StartInfo = startInfo;
                try
                {
                    if (process.Start())//开始进程  
                    {
                        if (seconds == 0)
                        {
                            process.WaitForExit();//这里无限等待进程结束  
                        }
                        else
                        {
                            process.WaitForExit(seconds); //等待进程结束，等待时间为指定的毫秒  
                        }
                        output = process.StandardOutput.ReadToEnd();//读取进程的输出  
                    }
                }
                catch
                {
                }
                finally
                {
                    if (process != null)
                        process.Close();
                }
            }
            return output;
        }

        public static void OnTimedEvent(object obj, ElapsedEventArgs args)
        {
            serviceTimer.Interval = 3600*1000;   //run once a day

            new StatusReporter().CollectStatusAndReport();
            //string line = Execute("ping localhost", 10);
            //Console.WriteLine(line);
            //Logger.Info(line);
        }
    }
}
