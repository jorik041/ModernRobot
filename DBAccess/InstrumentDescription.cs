using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBAccess
{
    public class InstrumentDescription
    {
        public string Ticker { get; set; }
        public string InstrumentCode { get; set; }
        public DateTime DateTo { get; set; }
        public DateTime DateFrom { get; set; }
        public string MarketCode { get; set; }
    }
}
