using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace CommonLib.Helpers
{
    public class Logger
    {
        private string _logPath;

        private void CheckLogFile()
        {
            var fInfo = new FileInfo(_logPath);
            if (fInfo.Exists)
            {
                if (fInfo.Length / 1024 / 1024  >= 1)
                {
                    File.Move(_logPath, string.Format("{0}_{1}.backup.log", _logPath.Split('.').First(), DateTime.Now.Ticks));
                    File.WriteAllLines(_logPath, new List<string>() { string.Format("Log continued at {0}", DateTime.Now) });
                }
            }
        }

        public Logger(string logPath)
        {
            _logPath = logPath;
            Log("Started.");
        }

        public void Log(string info)
        {
            lock (this)
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
