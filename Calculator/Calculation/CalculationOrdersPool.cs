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
using System.IO;

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

        public Guid AddNewOrderForCalculation(string insName, DateTime dateFrom, DateTime dateTo, TimePeriods period, float[] parameters, float stopLoss)
        {
            var order = CalculationOrder.CreateNew("SI", dateFrom, dateTo, period, parameters);
            order.StopLoss = stopLoss;
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

        public void GetFinishedOrderResults(Guid orderId)
        {
            var order = FinishedOrders.SingleOrDefault(o => o.Id == orderId);
            if (order != null)
                CalculateNextOrder(order, true);
        }

        private void CalculateNextOrder(CalculationOrder order, bool saveResults = false)
        {
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
                var tickers = candles.Select(o => o.Ticker).Distinct()
                    .OrderBy(o => candles.Where(c => c.Ticker == o).Max(d => d.DateTimeStamp)).ToArray();
                var strategy = (IStrategy)Activator.CreateInstance(_strategyType);
                for (var i = 0; i < order.Parameters.Count(); i++)
                    strategy.Parameters[i].Value = order.Parameters[i];
                var outDatas = new List<object[]>();
                var balances = new List<float>();
                var lastResult = StrategyResult.Exit;
                var balance = 0f;
                var priceDiff = 0f;
                var lotSize = 0;
                var lastPrice = 0f;
                var stopPrice = 0f;

                foreach (var ticker in tickers)
                {
                    var tc = candles.Where(o => o.Ticker == ticker).OrderBy(o => o.DateTimeStamp).ToList();
                    lastResult = StrategyResult.Exit;
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
                    var currentSL = 0f;
                    strategy.StopLossValue = order.StopLoss;

                    EventHandler<float> stopLossChanged = (sender, newVal) =>
                    {
                        if (currentSL != 0)
                        {
                            if (lastResult == StrategyResult.Long)
                                if (newVal > lastPrice - order.StopLoss)
                                    currentSL = newVal;
                            if (lastResult == StrategyResult.Short)
                                if (newVal < lastPrice + order.StopLoss)
                                    currentSL = newVal;
                        }
                    };

                    strategy.OnStopLossChanged += stopLossChanged;

                    for (var i = startIndex; i < tc.Count; i++)
                    {
                        var data = tc.GetRange(i - strategy.AnalysisDataLength + 1, strategy.AnalysisDataLength).ToArray();
                        object[] outData;
                        var result = strategy.Analyze(data, out outData);
                        if (i == tc.Count - 1)
                            result = StrategyResult.Exit;

                        if (lastResult != result)
                        {
                            balance = balance + priceDiff + lotSize * tc[i].Close;
                            if (result == StrategyResult.Long)
                            {
                                lotSize = 1;
                                priceDiff = -tc[i].Close * lotSize;
                                if (order.StopLoss == 0)
                                    currentSL = 0;
                                else
                                    currentSL = tc[i].Close - order.StopLoss;
                            }
                            if (result == StrategyResult.Short)
                            {
                                lotSize = -1;
                                priceDiff = tc[i].Close * (-lotSize);
                                if (order.StopLoss == 0)
                                    currentSL = 0;
                                else 
                                    currentSL = tc[i].Close + order.StopLoss;
                            }
                            if (result == StrategyResult.Exit)
                            {
                                priceDiff = 0;
                                lotSize = 0;
                                currentSL = 0;
                            }
                            if (saveResults)
                                balances.Add(balance);
                            lastPrice = tc[i].Close;
                            stopPrice = 0;
                        }
                        else
                        {
                            if (saveResults)
                                balances.Add(balance + priceDiff + lotSize * tc[i].Close);
                            if (currentSL != 0)
                            {
                                if (result == StrategyResult.Long)
                                    if (tc[i].Close <= currentSL)
                                    {
                                        balance = balance + priceDiff + lotSize * tc[i].Close;
                                        currentSL = 0;
                                        priceDiff = 0;
                                        lotSize = 0;
                                        stopPrice = tc[i].Close;
                                    }
                                if (result == StrategyResult.Short)
                                    if (tc[i].Close >= currentSL)
                                    {
                                        balance = balance + priceDiff + lotSize * tc[i].Close;
                                        currentSL = 0;
                                        priceDiff = 0;
                                        lotSize = 0;
                                        stopPrice = tc[i].Close;
                                    }
                            }
                        }
                        lastResult = result;

                        var outList = new List<object>() { data.Last().DateTimeStamp, tc[i].Ticker, tc[i].Open, tc[i].High, tc[i].Low, tc[i].Close };
                        outList.AddRange(outData);
                        outList.Add(lotSize);
                        if (result == StrategyResult.Long)
                            outList.Add(currentSL);
                        if (result == StrategyResult.Short)
                            outList.Add(currentSL);
                        if (result == StrategyResult.Exit)
                            outList.Add(0);
                        if (stopPrice == 0)
                            outList.Add("No");
                        else
                            outList.Add(string.Format("Yes ({0})",stopPrice));
                        outList.Add(balance);
                        if (saveResults)
                            outDatas.Add(outList.ToArray());
                    }
                    strategy.OnStopLossChanged -= stopLossChanged;
                }
                var outDataDescription = new List<string>();
                if (saveResults)
                {
                    outDataDescription.AddRange(new string[6] { "Date Time", "Ticker", "Open", "High", "Low", "Close" });
                    outDataDescription.AddRange(strategy.OutDataDescription);
                    outDataDescription.Add("LOT Size");
                    outDataDescription.Add("STOP price");
                    outDataDescription.Add("STOPPED");
                    outDataDescription.Add("Balance per deal");
                }
                order.Result = new CalculationResult() { OutData = outDatas.Select(o => o.Select(obj => obj.ToString()).ToArray()).ToArray(), Balances = balances.ToArray(), OutDataDescription = outDataDescription.ToArray()};
                order.TotalBalance = balance;
            }
            catch (Exception ex)
            {
                Logger.Log(string.Format(" ERROR on calculation: {0}", ex));
            }
            //Logger.Log(string.Format("Order {0} calculation finished in [{1} ms]", order.Id, sw.ElapsedMilliseconds));
            order.Status = CalculationOrderStatus.Finished;
            lock (_lock)
            {
                if (!saveResults)
                    _finishedOrders.Add(order);
            }
        }
    }
}
