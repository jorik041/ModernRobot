using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonLib.Helpers;
using System.IO;
using System.Reflection;
using DBAccess;
using DBDriver.Interfaces;

namespace ModernServer
{
    class Server
    {
        #region Logger settings
        private static Logger logger;
        private const string LOGSFOLDER = "Logs";
        private const string LOGFILENAME = "modernServer.log";

        private static void InitializeLogger()
        {
            var logFolder = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), LOGSFOLDER);
            if (!Directory.Exists(logFolder))
                Directory.CreateDirectory(logFolder);
            logger = new Logger(Path.Combine(logFolder, LOGFILENAME));
        }

        private static void Log(string contents)
        {
            Console.WriteLine(contents);
            if (logger!=null)
                logger.Log(contents);
        }
        #endregion

        private static IDBDriver _dbDriver;

        static void Main(string[] args)
        {
            InitializeLogger();
            Log(string.Format("Started server v.{0}", Assembly.GetExecutingAssembly().GetName().Version));
            _dbDriver = new DBDriver.DBDriver();
            Log(" Started database driver.");
            var canContinue = _dbDriver.CheckDBIntegrity();
            Log(string.Format(" Checked DB integrity: {0}", canContinue ? "OK" : "Failed"));
            if (!canContinue)
                return;
        }

        ~Server()
        {
            ((IDisposable)_dbDriver).Dispose();
            Log(" Stopped DB Driver.");
        }
    }
}
