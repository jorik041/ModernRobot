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
        private object _lock = new object();

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
                lock (_lock)
                {
                    return _finishedOrders.ToArray();
                }
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
            Logger.Log(string.Format("Calculation order {0} from {1} to {2}", order.Id, order.DateFrom, order.DateTo));
            var sw = new Stopwatch();
            var datefrom = order.DateFrom.AddMonths(-3);
            if (_reader.GetMinDateTimeStamp(order.InstrumentName) > datefrom)
            {
                Logger.Log(" ERROR: incorrect data in order DATE FROM.");
                return;
            }
            var candles = _reader.GetCandles(order.InstrumentName, order.Period, datefrom, order.DateTo);
            sw.Start();
            try
            {
                var tickers = candles.Select(o => o.Ticker).Distinct()
                    .OrderBy(o => candles.Where(c => c.Ticker == o).Max(d => d.DateTimeStamp)).ToArray();
                var strategy = (IStrategy)Activator.CreateInstance(_strategyType);
                var outDatas = new List<object[]>();
                var balances = new List<float>();
                var lastResult = StrategyResult.Exit;
                var lastEnterPrice = 0f;
                var balance = 0f;

                foreach (var ticker in tickers)
                {
                    var tc = candles.Where(o => o.Ticker == ticker).OrderBy(o => o.DateTimeStamp).ToList();
                    var itemDateFrom = _reader.GetItemDateFrom(ticker);
                    if (order.DateFrom > itemDateFrom)
                        itemDateFrom = order.DateFrom;
                    var startIndex = tc.FindIndex(o => o.DateTimeStamp >= itemDateFrom);
                    if (startIndex == -1)
                        continue;
                    if (strategy.AnalysisDataLength > startIndex)
                    {
                        Logger.Log(string.Format("ERROR: order id {0}, preload data count < data count needed for analysis!", order.Id));
                        return;
                    }
                    strategy.Initialize();

                    for (var i = startIndex; i < tc.Count; i++)
                    {
                        var data = tc.GetRange(i - strategy.AnalysisDataLength+1, strategy.AnalysisDataLength).ToArray();
                        object[] outData;
                        var result = strategy.Analyze(data, out outData);
                        outDatas.Add(outData);
                        if (i == tc.Count - 1)
                            result = StrategyResult.Exit;
                        if (lastResult != result)
                        {
                            if (lastResult == StrategyResult.Long)
                            {
                                balance = balance - (lastEnterPrice - tc[i].Close);
                            }
                            if (lastResult == StrategyResult.Short)
                            {
                                balance = balance + (lastEnterPrice - tc[i].Close);
                            }

                            lastResult = result;
                            lastEnterPrice = tc[i].Close;
                        }
                        balances.Add(balance);
                    }
                }
                order.Result = new CalculationResult() { OutData = outDatas.ToArray(), Balances = balances.ToArray() };
            }
            catch (Exception ex)
            {
                Logger.Log(string.Format(" ERROR on calculation: {0}", ex));
            }
            sw.Stop();
            Logger.Log(string.Format("Order {0} calculation finished in [{1} ms]", order.Id, sw.ElapsedMilliseconds));
            order.Status = CalculationOrderStatus.Finished;
            lock (_lock)
            {
                _finishedOrders.Add(order);
            }
        }
    }
}
