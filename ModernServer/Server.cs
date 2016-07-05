﻿using System;
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

        private static DBActualizer _dbActualizer;

        static void Main(string[] args)
        { 
            Logger.Log(string.Format("Started server v.{0}", Assembly.GetExecutingAssembly().GetName().Version));
            _dbActualizer = new DBActualizer();
            _dbActualizer.Start();

            var calcPool = new Calculator.Calculation.CalculationOrdersPool(typeof(Calculator.Strategies.FortsBasic));
            calcPool.AddNewOrderForCalculation("SI", new DateTime(2015,1,1), new DateTime(2015,2,1), TimePeriods.Hour, new float[5]);
            calcPool.ProcessOrders();

            Console.ReadLine();
            _dbActualizer.Stop();
        }
    }
}
