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

namespace ModernServer
{
    class Server
    {
        #region Logger settings
        private static Logger logger = new Logger();

        private static void Log(string contents)
        {
            Console.WriteLine(contents);
            logger.Log(contents);
        }

        #endregion

        private static DBActualizer _dbActualizer;

        static void Main(string[] args)
        { 
            Log(string.Format("Started server v.{0}", Assembly.GetExecutingAssembly().GetName().Version));
            _dbActualizer = new DBActualizer();
            _dbActualizer.Start();

            /*var pool = new Calculator.Calculation.CalculationOrdersPool(new Calculator.Strategies.FortsBasic());
            for (int i=0; i < 100;i++)
            {
                pool.AddNewOrderForCalculation("SI", DateTime.Now.AddMonths(-3), DateTime.Now.AddMonths(-2), TimePeriods.Minute, new float[5]);
            }*/

            Console.ReadLine();
            _dbActualizer.Stop();
        }
    }
}
