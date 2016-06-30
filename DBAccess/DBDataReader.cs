using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DBAccess.Database;
using DBAccess.Interfaces;
using DBAccess.Entities;
using System.Data.SqlClient;
using System.Configuration;
using CommonLib.Helpers;
using System.Diagnostics;

namespace DBAccess
{
    public class DBDataReader : IDBDataReader, IDisposable
    {
        private Logger _logger = new Logger();
        private List<DBCandlesCache> _cache = new List<DBCandlesCache>();
        private const int maxCacheSize = 10;

        private SqlConnection _connection;
        public DBDataReader()
        {
            var conString = ConfigurationManager.ConnectionStrings["readerConnectionString"].ConnectionString;
            _connection = new SqlConnection(conString);
            _connection.Open();
        }

        public void Dispose()
        {
            _connection.Close();
            _connection.Dispose();
        }

        public Candle[] GetCandles(string instrumentName, TimePeriods period, DateTime dateFrom, DateTime dateTo)
        {
            var sw = new Stopwatch();
            sw.Start();

            if (_cache.Any(o => o.InstrumentName == instrumentName && o.DateFrom == dateFrom && o.DateTo == dateTo && o.Period == period))
            {
                sw.Stop();
                _logger.Log(string.Format("Obtained data from CACHE for {0}, period {1} from {2} to {3} [{4} ms]", instrumentName, period, dateFrom, dateTo, sw.ElapsedMilliseconds));
                return _cache.First(o => o.InstrumentName == instrumentName && o.DateFrom == dateFrom && o.DateTo == dateTo && o.Period == period).Candles;
            }

            var cmd = new StringBuilder();
            cmd.AppendLine("SELECT [Open], [High], [Low], [Close], [DateTimeStamp], [Ticker] FROM StockDataSet");
            cmd.AppendLine(string.Format("JOIN ItemsSet ON StockDataSet.ItemId = ItemsSet.Id AND ItemsSet.Period = {0}", (int)period));
            cmd.AppendLine(string.Format("JOIN InstrumentsSet ON InstrumentsSet.Id = ItemsSet.InstrumentId AND InstrumentsSet.Name='{0}'", instrumentName));
            cmd.AppendLine(string.Format("WHERE StockDataSet.DateTimeStamp >= CONVERT(DATETIME, '{0}-{1}-{2} {3}:{4}:{5}.000', 121)", dateFrom.Year, dateFrom.Month, dateFrom.Day, dateFrom.Hour, dateFrom.Minute, dateFrom.Second));
            cmd.AppendLine(string.Format("AND StockDataSet.DateTimeStamp <=  CONVERT(DATETIME, '{0}-{1}-{2} {3}:{4}:{5}.000', 121) ORDER BY DateTimeStamp ASC;", dateTo.Year, dateTo.Month, dateTo.Day, dateTo.Hour, dateTo.Minute, dateTo.Second));

            var query = new SqlCommand(cmd.ToString(),_connection);
            var candles = new List<Candle>();
            using (var dataReader = query.ExecuteReader())
            {

                while (dataReader.Read())
                {
                    candles.Add(new Candle()
                    {
                        DateTimeStamp = (DateTime)dataReader["DateTimeStamp"],
                        Close = (float)dataReader["Close"],
                        Open = (float)dataReader["Open"],
                        High = (float)dataReader["High"],
                        Low = (float)dataReader["Low"],
                        Ticker = dataReader["Ticker"].ToString()
                    });
                }
                dataReader.Close();
            }
            if (!_cache.Any(o => o.InstrumentName == instrumentName && o.DateFrom == dateFrom && o.DateTo == dateTo && o.Period == period))
                _cache.Add(new DBCandlesCache()
                {
                    InstrumentName = instrumentName,
                    DateFrom = dateFrom,
                    DateTo = dateTo,
                    Candles = candles.ToArray(),
                    Period = period
                });

            if (_cache.Count > maxCacheSize)
                _cache = _cache.Skip(maxCacheSize - 1).ToList();
            sw.Stop();

            _logger.Log(string.Format("Obtained data from DB for {0}, period {1} from {2} to {3} [{4} ms]", instrumentName, period, dateFrom, dateTo, sw.ElapsedMilliseconds));

            return candles.ToArray(); 
        }

        public DateTime GetMaxDateTimeStamp(string instrumentName)
        {
            var query = new SqlCommand(string.Format(" SELECT MAX([DateTimeStamp]) from StockDataSet JOIN ItemsSet ON StockDataSet.ItemId = ItemsSet.Id JOIN InstrumentsSet ON InstrumentsSet.Id = ItemsSet.InstrumentId AND InstrumentsSet.Name='{0}'; ", instrumentName), _connection);
            return (DateTime)query.ExecuteScalar();
        }

        public DateTime GetMinDateTimeStamp(string instrumentName)
        {
            var query = new SqlCommand(string.Format(" SELECT MIN([DateTimeStamp]) from StockDataSet JOIN ItemsSet ON StockDataSet.ItemId = ItemsSet.Id JOIN InstrumentsSet ON InstrumentsSet.Id = ItemsSet.InstrumentId AND InstrumentsSet.Name='{0}'; ", instrumentName), _connection);
            return (DateTime)query.ExecuteScalar();
        }
    }
}
