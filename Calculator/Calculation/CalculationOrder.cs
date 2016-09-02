using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Calculator.Strategies;
using DBAccess;
using System.Runtime.Serialization;

namespace Calculator.Calculation
{
    [DataContract]
    public class CalculationOrder
    {
        [DataMember]
        public Guid Id { get; private set; }
        [DataMember]
        public string InstrumentName { get; private set; }
        [DataMember]
        public DateTime DateFrom { get; private set; }
        [DataMember]
        public DateTime DateTo { get; private set; }
        [DataMember]
        public TimePeriods Period { get; private set; }
        [DataMember]
        public float[] Parameters { get; private set; }
        [DataMember]
        public CalculationOrderStatus Status { get; internal set; }
        [DataMember]
        public float TotalBalance { get; internal set; }
        [DataMember]
        public float StopLoss { get; set; }

        public CalculationResult Result { get; internal set; }
        

        public static CalculationOrder CreateNew(string insName, DateTime dateFrom, DateTime dateTo, TimePeriods period, float[] parameters)
        {
            return new CalculationOrder()
            {
                InstrumentName = insName,
                DateFrom = dateFrom,
                DateTo = dateTo,
                Status = CalculationOrderStatus.Waiting,
                Period = period,
                Parameters = parameters,
                Id = Guid.NewGuid()
            };
        }
    }
}
