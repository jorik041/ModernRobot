using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Calculator.Calculation;

namespace ModernServer.WCFEntities
{
    public class RemoteCalculation : RemoteCalculationInfo
    {
        public new Guid Id { get; private set; }
        public CalculationOrdersPool OrdersPool { get; private set; }

        public RemoteCalculation(string name, Type strategyType)
        {
            Id = Guid.NewGuid();
            Name = name;
            OrdersPool = new CalculationOrdersPool(strategyType);
        }
    }
}
