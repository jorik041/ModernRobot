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

namespace Calculator.Calculation
{
    public class CalculationOrdersPool : ICalculationOrdersPool
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

        public void AddNewOrderForCalculation(string insName, DateTime dateFrom, DateTime dateTo, TimePeriods period, float[] parameters)
        {
            _orders.Add(CalculationOrder.CreateNew(insName, dateFrom, dateTo, period, parameters));
        }

        public void Flush()
        {
            _orders.Clear();
        }

        private void GetResultsForOrder(CalculationOrder order)
        {
            
        }

        private void CalculateSingleOrder(CalculationOrder order)
        {
            if (Strategy.Parameters.Count() != order.Parameters.Count())
                return;
            Log(string.Format("Calculating order with id {0}", order.Id));
            order.Status = CalculationOrderStatus.Processing;
            ThreadPool.QueueUserWorkItem((obj) => GetResultsForOrder(order));
        }
    }
}
