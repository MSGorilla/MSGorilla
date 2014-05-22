using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeFlowMonitor
{
    public static class Logger
    {
        private static string LogPath = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), "Log");
        private static StreamWriter _sw = new StreamWriter(Path.Combine(LogPath, string.Format("CFMonitor_{0}.log", DateTime.Now.ToString("MMddHHmm"))), true);
        static Logger()
        {
            if (!Directory.Exists(LogPath))
                Directory.CreateDirectory(LogPath);
        }
        public static void WriteInfo(string message)
        {
            _sw.WriteLine(string.Format("{0}:{1}", DateTime.Now, message));
            _sw.Flush();
        }
    }
}
