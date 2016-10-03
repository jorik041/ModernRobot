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
        public void FortsBasicTest()
        {
            using (var pool = new CalculationOrdersPool(typeof(FortsBasic)))
            {
                pool.AddNewOrderForCalculation("SI", new DateTime(2015, 1, 1), new DateTime(2015, 6, 1), TimePeriods.Hour, new float[] { 10, 5, 1, 8}, 0, false, 0, 0);
                pool.ProcessOrders();
                while (pool.FinishedOrders.Length != 1)
                {
                    Thread.Sleep(100);
                }
                Assert.AreEqual(25056, pool.FinishedOrders[0].TotalBalance);
                Assert.AreEqual(5249, pool.FinishedOrders[0].Gap);
                pool.GetFinishedOrderResults(pool.FinishedOrders[0].Id);
                Assert.AreEqual(pool.FinishedOrders[0].Result.Balances.Count(), pool.FinishedOrders[0].Result.OutData.Count());
            }
        }

        [TestMethod]
        public void SimpleTrendTest()
        {
            using (var pool = new CalculationOrdersPool(typeof(SimpleTrend)))
            {
                pool.AddNewOrderForCalculation("SI", new DateTime(2015, 1, 1), new DateTime(2015, 6, 1), TimePeriods.Hour, new float[] { 2, 6, 4}, 0, false, 0, 0);
                pool.ProcessOrders();
                while (pool.FinishedOrders.Length != 1)
                {
                    Thread.Sleep(100);
                }
                Assert.AreEqual(-2730, pool.FinishedOrders[0].TotalBalance);
                Assert.AreEqual(17963, pool.FinishedOrders[0].Gap);
                pool.GetFinishedOrderResults(pool.FinishedOrders[0].Id);
                Assert.AreEqual(pool.FinishedOrders[0].Result.Balances.Count(), pool.FinishedOrders[0].Result.OutData.Count());
            }
        }
    }
}
