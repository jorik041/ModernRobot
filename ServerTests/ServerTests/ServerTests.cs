using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Calculator.Strategies;
using Calculator.Calculation;
using DBAccess;
using System.Threading;
using System.Linq;

namespace ServerTests
{
    [TestClass]
    public class ServerTests
    {
        [TestMethod]
        public void PassStrategies()
        {
            using (var pool = new CalculationOrdersPool(typeof(FortsBasic)))
            {
                pool.AddNewOrderForCalculation("SI", new DateTime(2015, 1, 1), new DateTime(2015, 6, 1), TimePeriods.Hour, new float[] { 10, 5, 1, 8}, 0, false);
                pool.ProcessOrders();
                while (pool.FinishedOrders.Length != 1)
                {
                    Thread.Sleep(100);
                }
                Assert.AreEqual(25056, pool.FinishedOrders[0].TotalBalance);
                Assert.AreEqual(5249, pool.FinishedOrders[0].Gap);
                pool.GetFinishedOrderResults(pool.FinishedOrders[0].Id);
                Assert.AreEqual(pool.FinishedOrders[0].Result.Balances.Count(), pool.FinishedOrders[0].Result.OutData.Count());
                pool.AddNewOrderForCalculation("SI", new DateTime(2015, 1, 1), new DateTime(2015, 6, 1), TimePeriods.FifteenMinutes, new float[] { 10, 5, 1, 8 }, 0, false);
                pool.ProcessOrders();
                while (pool.FinishedOrders.Length != 2)
                {
                    Thread.Sleep(100);
                }
                Assert.AreEqual(4227, pool.FinishedOrders[1].TotalBalance);
                Assert.AreEqual(15405, pool.FinishedOrders[1].Gap);
                pool.GetFinishedOrderResults(pool.FinishedOrders[1].Id);
                Assert.AreEqual(pool.FinishedOrders[1].Result.Balances.Count(), pool.FinishedOrders[1].Result.OutData.Count());
            }
        }
    }
}
