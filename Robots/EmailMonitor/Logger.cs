using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;

namespace MSGorilla.EmailMonitor
{
    public static class Logger
    {
        private static string LogPath;
        private static StreamWriter _sw;
        static Logger()
        {
            LogPath = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), "Log");
            if (!Directory.Exists(LogPath))
                Directory.CreateDirectory(LogPath);

            _sw = new StreamWriter(Path.Combine(LogPath, "StatusReporter.log"), true);
        }

        public static void WriteRawLog(string message)
        {
            lock (_sw)
            {
                _sw.WriteLine(message);
                _sw.Flush();
                Console.WriteLine(message);
            }
        }

        public static void WriteLog(string message)
        {
            WriteRawLog(string.Format("[{0}]{1}", DateTime.Now, message));
        }

        public static void Debug(string message)
        {
            WriteLog(string.Format("[Debug]{0}", message));
        }

        public static void Info(string message)
        {
            WriteLog(string.Format("[Info]{0}", message));
        }
        public static void Warning(string message)
        {
            WriteLog(string.Format("[Warning]{0}", message));
        }
        public static void Error(string message)
        {
            WriteLog(string.Format("[Error]{0}", message));
        }
    }
}
