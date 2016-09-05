using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Calculator.Calculation;
using Calculator.Strategies;
using System.Runtime.Serialization;

namespace ModernServer.WCFEntities
{
    public class RemoteCalculation : RemoteCalculationInfo
    {
        public CalculationOrdersPool OrdersPool { get; private set; }
        public new int WaitingOrdersCount
        {
            get
            {
                if (OrdersPool == null)
                    return 0;
                return OrdersPool.WaitingOrdersCount;
            }
        }
        public new int FinishedOrdersCount
        {
            get
            {
                if (OrdersPool == null)
                    return 0;
                return OrdersPool.FinishedOrders.Count();
            }
        }

        public new bool IsWaiting
        {
            get
            {
                return !OrdersPool.IsProcessingOrders && OrdersPool.WaitingOrdersCount > 0;
            }
        }

        public new bool IsDone
        {
            get
            {
                return !OrdersPool.IsProcessingOrders && OrdersPool.FinishedOrders.Count() > 0;
            }
        }

        public new bool IsRunning
        {
            get
            {
                return OrdersPool.IsProcessingOrders;
            }
        }

        public RemoteCalculation(string name, Type strategyType) : base(Guid.NewGuid(), name, string.Empty)
        {
            OrdersPool = new CalculationOrdersPool(strategyType);
            var strategy = Activator.CreateInstance(strategyType);
            StrategyName = ((IStrategy)strategy).Name;
        }
    }
}
