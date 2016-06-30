using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DBAccess;
using CommonLib.Helpers;
using Calculator.Strategies;
using System.Threading;
using System.Diagnostics;

namespace Calculator.Calculation
{
    public class CalculationOrdersPool : ICalculationOrdersPool, IDisposable
    {
        private DBDataReader _reader = new DBDataReader();
        private Type _strategyType;
        private Queue<CalculationOrder> _ordersQueue = new Queue<CalculationOrder>();
        private List<CalculationOrder> _finishedOrders = new List<CalculationOrder>();

        public CalculationOrdersPool(Type strategyType)
        {
            _strategyType = strategyType;
        }

        public bool AllOrdersFinished
        {
            get
            {
                return _ordersQueue.Count == 0 && !_ordersQueue.Any();
            }
        }

        public CalculationOrder[] FinishedOrders
        {
            get
            {
                return _finishedOrders.ToArray();
            }
        }

        public Guid AddNewOrderForCalculation(string insName, DateTime dateFrom, DateTime dateTo, TimePeriods period, float[] parameters)
        {
            var order = CalculationOrder.CreateNew("SI", dateFrom, dateTo, period, parameters);
            _ordersQueue.Enqueue(order);
            return order.Id;
        }

        public void Dispose()
        {
            _reader.Dispose();
        }

        public void Flush()
        {
            _ordersQueue.Clear();
            _finishedOrders.Clear();
        }

        public void ProcessOrders()
        {
            ThreadPool.QueueUserWorkItem(obj =>
            {
                while (_ordersQueue.Any())
                {
                    var order = _ordersQueue.Dequeue();
                    ThreadPool.QueueUserWorkItem(o => { CalculateNextOrder(order); });
                }
            });
        }

        private void CalculateNextOrder(CalculationOrder order)
        {
            order.Status = CalculationOrderStatus.Processing;
            Logger.Log(string.Format("Calculation order {0}", order.Id));
            var datefrom = order.DateFrom.AddMonths(-3);
            if (_reader.GetMinDateTimeStamp(order.InstrumentName) > datefrom)
            {
                Logger.Log(" ERROR: incorrect data in order DATE FROM.");
                return;
            }
            var candles = _reader.GetCandles(order.InstrumentName, TimePeriods.Hour, datefrom, order.DateTo);
            var sw = new Stopwatch();

            sw.Start();
            try
            {
                var tickers = candles.Select(o => o.Ticker).Distinct()
                    .OrderBy(o => candles.Where(c => c.Ticker == o).Max(d => d.DateTimeStamp)).ToArray();
                Logger.Log(string.Format("Process tickers:{0}", string.Join(", ", tickers)));

            }
            catch (Exception ex)
            {
                Logger.Log(string.Format(" ERROR on calculation: {0}", ex));
            }
            finally
            {
                sw.Stop();
                Logger.Log(string.Format("Order {0} calculation finished in [{1} ms]", order.Id, sw.ElapsedMilliseconds));
                order.Status = CalculationOrderStatus.Finished;
                _finishedOrders.Add(order);
            }
        }
    }
}
