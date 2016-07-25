using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Calculator.Calculation;
using Calculator.Strategies;

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

        public RemoteCalculation(string name, Type strategyType) : base(Guid.NewGuid(), name, string.Empty)
        {
            OrdersPool = new CalculationOrdersPool(strategyType);
            var strategy = Activator.CreateInstance(strategyType);
            StrategyName = ((IStrategy)strategy).Name;
        }
    }
}
