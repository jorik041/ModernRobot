using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace ModernServer.WCFEntities
{
    [DataContract]
    public class RemoteCalculationInfo
    {
        [DataMember]
        public Guid Id { get; protected set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string StrategyName { get; protected set; }
        [DataMember]
        public int WaitingOrdersCount { get; set; }
        [DataMember]
        public int FinishedOrdersCount { get; set; }
        [DataMember]
        public bool IsRunning { get; set; }
        [DataMember]
        public bool IsDone { get; set; }
        [DataMember]
        public bool IsWaiting { get; set; }

        public RemoteCalculationInfo(Guid id, string name, string strategyName)
        {
            Id = id;
            Name = name;
            StrategyName = strategyName;
        }
    }
}
