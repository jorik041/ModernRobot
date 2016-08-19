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

        public int WaitingOrdersCount
        {
            get
            {
                return _ordersQueue.Count();
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

        public Guid AddNewOrderForCalculation(string insName, DateTime dateFrom, DateTime dateTo, TimePeriods period, float[] parameters, bool antiTrend = false, int entryLot = 0, int lotIncrement =0)
        {
            var order = CalculationOrder.CreateNew("SI", dateFrom, dateTo, period, parameters);
            order.AntiTrend = antiTrend;
            order.EntryLOT = entryLot;
            order.LOTIncrement = lotIncrement;
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
                    CalculateNextOrder(order); 
                }
            });
        }

        private void CalculateNextOrder(CalculationOrder order)
        {
            var sw = new Stopwatch();
            try
            {
                order.Status = CalculationOrderStatus.Processing;
                //Logger.Log(string.Format("Calculating order {0} from {1} to {2}", order.Id, order.DateFrom, order.DateTo));
                var datefrom = order.DateFrom.AddMonths(-3);
                if (_reader.GetMinDateTimeStamp(order.InstrumentName) > datefrom)
                {
                    Logger.Log(" ERROR: incorrect data in order DATE FROM.");
                    return;
                }
                var candles = _reader.GetCandles(order.InstrumentName, order.Period, datefrom, order.DateTo);
                sw.Start();
                var tickers = candles.Select(o => o.Ticker).Distinct()
                    .OrderBy(o => candles.Where(c => c.Ticker == o).Max(d => d.DateTimeStamp)).ToArray();
                var strategy = (IStrategy)Activator.CreateInstance(_strategyType);
                for (var i = 0; i < order.Parameters.Count(); i++)
                    strategy.Parameters[i].Value = order.Parameters[i];
                var outDatas = new List<object[]>();
                var balances = new List<float>();
                var lastResult = StrategyResult.Exit;
                var lastEnterPrice = 0f;
                var balance = 0f;
                var priceDiff = 0f;
                var lotSize = 0;


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
                        var data = tc.GetRange(i - strategy.AnalysisDataLength + 1, strategy.AnalysisDataLength).ToArray();
                        object[] outData;
                        var result = strategy.Analyze(data, out outData);
                        if (i == tc.Count - 1)
                            result = StrategyResult.Exit;

                        if ((lastResult != result) || ((Math.Abs(lotSize) == 2 * order.EntryLOT) && order.AntiTrend))
                        {
                            balance = balance + priceDiff + lotSize * tc[i].Close;
                            if (result == StrategyResult.Long)
                            {
                                lotSize = order.AntiTrend ? order.EntryLOT : 1;
                                priceDiff = -tc[i].Close * lotSize;
                            }
                            if (result == StrategyResult.Short)
                            {
                                lotSize = order.AntiTrend ? -order.EntryLOT : -1;
                                priceDiff = tc[i].Close * (-lotSize);
                            }
                            if (result == StrategyResult.Exit)
                            {
                                priceDiff = 0;
                                lotSize = 0;
                            }
                            balances.Add(balance);
                        }
                        else
                        {
                            if (order.AntiTrend)
                            {
                                if (result == StrategyResult.Long)
                                {
                                    if (tc[i].Close > tc[i - 1].Close)
                                    {
                                        if (lotSize >= order.LOTIncrement)
                                        {
                                            priceDiff = priceDiff + tc[i].Close * order.LOTIncrement;
                                            lotSize = lotSize - order.LOTIncrement;
                                        }
                                    }
                                    else
                                    {
                                        if (lotSize < order.EntryLOT * 2)
                                        {
                                            priceDiff = priceDiff - tc[i].Close * order.LOTIncrement;
                                            lotSize = lotSize + order.LOTIncrement;
                                        }
                                    }
                                }
                                if (result == StrategyResult.Short)
                                {
                                    if (tc[i].Close < tc[i - 1].Close)
                                    {
                                        if (lotSize > -order.EntryLOT * 2)
                                        {
                                            priceDiff = priceDiff + tc[i].Close * order.LOTIncrement;
                                            lotSize = lotSize - order.LOTIncrement;
                                        }
                                    }
                                    else
                                    {
                                        if (lotSize <= -order.LOTIncrement)
                                        {
                                            priceDiff = priceDiff - tc[i].Close * order.LOTIncrement;
                                            lotSize = lotSize + order.LOTIncrement;
                                        }
                                    }
                                }
                            }
                            balances.Add(balance + priceDiff + lotSize * tc[i].Close);    
                        }
                        lastResult = result;

                        var outList = new List<object>() { data.Last().DateTimeStamp, lotSize };
                        outList.AddRange(outData);
                        outDatas.Add(outList.ToArray());
                    }
                }
                var outDataDescription = new List<string>() { "Date Time", "LOT size" };
                outDataDescription.AddRange(strategy.OutDataDescription);
                order.Result = new CalculationResult() { OutData = outDatas.Select(o => o.Select(obj => obj.ToString()).ToArray()).ToArray(), Balances = balances.ToArray(), OutDataDescription = outDataDescription.ToArray() };
                order.TotalBalance = balances.Last();
            }
            catch (Exception ex)
            {
                Logger.Log(string.Format(" ERROR on calculation: {0}", ex));
            }
            sw.Stop();
            //Logger.Log(string.Format("Order {0} calculation finished in [{1} ms]", order.Id, sw.ElapsedMilliseconds));
            order.Status = CalculationOrderStatus.Finished;
            lock (_lock)
            {
                _finishedOrders.Add(order);
            }
        }
    }
}
