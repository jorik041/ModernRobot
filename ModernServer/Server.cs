using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonLib.Helpers;
using System.IO;
using System.Reflection;
using DBAccess;
using DBAccess.Database;
using System.ServiceModel;

namespace ModernServer
{
    class Server
    {

        private static DBActualizer _dbActualizer;

        static void Main(string[] args)
        { 
            Logger.Log(string.Format("Started server application v.{0}", Assembly.GetExecutingAssembly().GetName().Version));
            _dbActualizer = new DBActualizer();
            _dbActualizer.Start();

            Console.ReadLine();

            _dbActualizer.Stop();
        }
    }
}
