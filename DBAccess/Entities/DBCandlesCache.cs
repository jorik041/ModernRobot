using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBAccess.Entities
{
    public struct DBCandlesCache
    {
        public string InstrumentName { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
        public TimePeriods Period { get; set; }
        public Candle[] Candles { get; set; }
    }
}
