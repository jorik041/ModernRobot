using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DBAccess;
using CommonLib.Helpers;
using Calculator.Strategies;
using System.Collections.ObjectModel;
using System.Threading;
using System.Collections.Specialized;
using DBAccess.Database;
using System.Diagnostics;

namespace Calculator.Calculation
{
    public class CalculationOrdersPool : ICalculationOrdersPool, IDisposable
    {
        #region Logging

        private Logger _logger = new Logger();
        private void Log(string contents)
        {
            var strToLog = string.Format("Calc orders pool: {0}", contents);
            Console.WriteLine(strToLog);
            _logger.Log(strToLog);         
        }

        #endregion

        private ObservableCollection<CalculationOrder> _orders = new ObservableCollection<CalculationOrder>();
        private DBDataReader _dbReader = new DBDataReader();

        public IStrategy Strategy { get; private set; }

        public CalculationOrdersPool(IStrategy strategy)
        {
            Strategy = strategy;
            Log(string.Format("Opened pool for strategy {0}", strategy.Name));
            _orders.CollectionChanged += OrdersChanged;
        }

        private void OrdersChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            foreach (var order in _orders.Where(o => o.Status == CalculationOrderStatus.Waiting))
                CalculateSingleOrder(order);
        }

        public CalculationOrder[] FinishedOrders
        {
            get
            {
                return _orders.Where(o => o.Status == CalculationOrderStatus.Finished).ToArray();
            }
        }

        public int ProcessingOrdersCount
        {
            get
            {
                return _orders.Where(o => o.Status == CalculationOrderStatus.Processing).Count();
            }
        }
        public int OrdersCount
        {
            get
            {
                return _orders.Count;
            }
        }
        public bool AllOrdersFinished
        {
            get
            {
                return _orders.All(o => o.Status == CalculationOrderStatus.Finished);
            }
        }

        public Guid AddNewOrderForCalculation(string insName, DateTime dateFrom, DateTime dateTo, TimePeriods period, float[] parameters)
        {
            var order = CalculationOrder.CreateNew(insName, dateFrom, dateTo, period, parameters);
            _orders.Add(order);
            return order.Id;
        }

        public void Flush()
        {
            _orders.Clear();
        }

        private void GetResultsForOrder(CalculationOrder order)
        {
            try
            {
                var sw = Stopwatch.StartNew();
                var candles = _dbReader.GetCandles(order.InstrumentName, order.Period, order.DateFrom, order.DateTo);
                sw.Stop();
                Log(string.Format("Succesfully obtained DB data for order id {0} [{1} ms]", order.Id, sw.ElapsedMilliseconds));

            }
            catch (Exception ex)
            {
                Log(string.Format(" Error on calculation: {0}", ex.ToString()));
            }    
        }

        private void CalculateSingleOrder(CalculationOrder order)
        {
            if (Strategy.Parameters.Count() != order.Parameters.Count())
                return;
            order.Status = CalculationOrderStatus.Processing;
            ThreadPool.QueueUserWorkItem((obj) => GetResultsForOrder(order));
        }

        public void Dispose()
        {
            _dbReader.Dispose();
        }
    }
}
