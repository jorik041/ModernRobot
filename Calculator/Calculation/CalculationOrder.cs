using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Calculator.Strategies;
using DBAccess;

namespace Calculator.Calculation
{
    public struct CalculationOrder
    {
        public Guid Id { get; private set; }
        public string InstrumentName { get; private set; }
        public DateTime DateFrom { get; private set; }
        public DateTime DateTo { get; private set; }
        public TimePeriods Period { get; private set; }
        public float[] Parameters { get; private set; }
        public CalculationOrderStatus Status { get; internal set; }
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
