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
        private Logger _logger = new Logger();

        public CalculationOrdersPool(Type strategyType)
        {
            _strategyType = strategyType;
        }

        public bool AllOrdersFinished
        {
            get
            {
                return _ordersQueue.Count == 0;
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
            while (_ordersQueue.Any())
            {
                ThreadPool.QueueUserWorkItem(o => { CalculateNextOrder(); });    
            }
        }

        private void CalculateNextOrder()
        {
            var order = _ordersQueue.Dequeue();
            order.Status = CalculationOrderStatus.Processing;

            _logger.Log(string.Format("Calculation order {0}", order.Id));



            order.Status = CalculationOrderStatus.Finished;
            _finishedOrders.Add(order);
        }
    }
}
