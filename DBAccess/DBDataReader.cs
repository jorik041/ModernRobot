using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DBAccess.Database;
using DBAccess.Interfaces;
using DBAccess.Entities;

namespace DBAccess
{
    /// <summary>
    /// Reads data from database
    /// </summary>
    public class DBDataReader : IDBDataReader
    {
        public Candle[] GetCandles(string instrumentName, TimePeriods period, DateTime dateFrom, DateTime dateTo)
        {
            return new Candle[0]; 
        }

        public string GetTickerForCandle(DateTime dateTimeStamp)
        {
            return string.Empty;
        }
    }
}
