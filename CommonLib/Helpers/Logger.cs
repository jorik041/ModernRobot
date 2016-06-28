using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;

namespace CommonLib.Helpers
{
    public class Logger
    {
        private string _logPath;
        private static object _lockObject = new object();

        private const string LOGSFOLDER = "Logs";
        private const string LOGFILENAME = "modernServer.log";

        private void CheckLogFile()
        {
            var fInfo = new FileInfo(_logPath);
            if (fInfo.Exists)
            {
                if (fInfo.Length / 1024 / 1024  >= 10)
                {
                    File.Move(_logPath, string.Format("{0}_{1}.backup.log", _logPath.Split('.').First(), DateTime.Now.Ticks));
                    File.WriteAllLines(_logPath, new List<string>() { string.Format("Log continued at {0}", DateTime.Now) });
                }
            }
        }

        public Logger()
        {
            var logFolder = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), LOGSFOLDER);
            if (!Directory.Exists(logFolder))
                Directory.CreateDirectory(logFolder);
            _logPath = Path.Combine(logFolder, LOGFILENAME);
            Log("Started.");
        }

        public void Log(string info)
        {
            Console.WriteLine(info);
            lock (_lockObject)
            {
                CheckLogFile();
                using (var stream = new StreamWriter(_logPath, true))
                {
                    stream.WriteLine(string.Format("{0} {1}", DateTime.Now, info));
                    stream.Close();
                }
            }
        }
    }
}
