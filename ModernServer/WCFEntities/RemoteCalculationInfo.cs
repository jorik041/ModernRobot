using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModernServer.WCFEntities
{
    public class RemoteCalculationInfo
    {
        public Guid Id { get; private set; }
        public string Name { get; set; }
        public Type StrategyType { get; private set; }
    }
}
