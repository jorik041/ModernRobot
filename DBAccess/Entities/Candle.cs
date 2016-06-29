using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBAccess.Entities
{
    public struct Candle
    {
        public float Open { get; set; }
        public float High { get; set; }
        public float Low { get; set; }
        public float Close { get; set; }
        public DateTime DateTimeStamp { get; set; }
        public string Ticker { get; set; }
    }
}
