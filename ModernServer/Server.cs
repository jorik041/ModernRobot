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
using System.ServiceModel.Description;
using ModernServer.Communication;
using ModernServer.CrossDomainService;

namespace ModernServer
{
    class Server
    {
        private static DBActualizer _dbActualizer;

        static void Main(string[] args)
        { 
            Logger.Log(string.Format("Started server v.{0}", Assembly.GetExecutingAssembly().GetName().Version));
            _dbActualizer = new DBActualizer();
            _dbActualizer.Start();

            ServiceHost wcfHost = null;
            ServiceHost cdHost = null;
            try
            {
                wcfHost = new ServiceHost(typeof(WCFCommunicator));
                cdHost = new ServiceHost(typeof(WCFCrossDomainService));
                wcfHost.Open();
                Logger.Log("WCF service started.");
                cdHost.Open();
                Logger.Log("Crossdomain service started.");
            }
            catch (Exception ex)
            {
                Logger.Log("Error starting WCF server.");
                Logger.Log(ex.ToString());
            }

            Console.ReadLine();
            wcfHost.Close();
            cdHost.Close();
            _dbActualizer.Stop();
        }
    }
}
