using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DBAccess.Database;
using DBAccess.Interfaces;

namespace DBAccess
{
    /// <summary>
    /// Reads data from database
    /// </summary>
    public class DBDataReader : IDBDataReader
    {
        /// <summary>
        /// Gets candles for specifiend instrument from database
        /// </summary>
        /// <param name="instrumentName">Instrument name</param>
        /// <param name="period">Time period</param>
        /// <param name="dateFrom">Date From</param>
        /// <param name="dateTo">Date To</param>
        /// <param name="tickers">Tickers list</param>
        /// <returns></returns>
        public StockData[] GetCandles(string instrumentName, TimePeriods period, DateTime dateFrom, DateTime dateTo)
        {
            using (var context = new DatabaseContainer())
            {
                return context.StockDataSet.Where(o => o.DateTimeStamp >= dateFrom && o.DateTimeStamp <= dateTo && o.Item.Period == (int)period).OrderBy(o => o.DateTimeStamp).ToArray();
            }    
        }
        /// <summary>
        /// Returns ticker for Date Time Stamp
        /// </summary>
        /// <param name="dateTimeStamp"></param>
        /// <returns></returns>
        public string GetTickerForCandle(DateTime dateTimeStamp)
        {
            using (var context = new DatabaseContainer())
            {
                var candle = context.StockDataSet.FirstOrDefault(o => o.DateTimeStamp == dateTimeStamp);
                if (candle == null)
                    return string.Empty;
                return candle.Item.Ticker;
            }
        }
    }
}
