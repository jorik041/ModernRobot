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

namespace DBAccess
{
    public class DBDataReader : IDBDataReader, IDisposable
    {
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
            var cmd = new StringBuilder();
            cmd.AppendLine("SELECT * FROM StockDataSet");
            cmd.AppendLine(string.Format("JOIN ItemsSet ON StockDataSet.ItemId = ItemsSet.Id AND ItemsSet.Period = {0}", (int)period));
            cmd.AppendLine(string.Format("JOIN InstrumentsSet ON InstrumentsSet.Id = ItemsSet.InstrumentId AND InstrumentsSet.Name='{0}'", instrumentName));
            cmd.AppendLine(string.Format("WHERE StockDataSet.DateTimeStamp >= CONVERT(DATETIME, '{0}-{1}-{2} {3}:{4}:{5}.000', 121)", dateFrom.Year, dateFrom.Month, dateFrom.Day, dateFrom.Hour, dateFrom.Minute, dateFrom.Second));
            cmd.AppendLine(string.Format("AND StockDataSet.DateTimeStamp <=  CONVERT(DATETIME, '{0}-{1}-{2} {3}:{4}:{5}.000', 121) ORDER BY DateTimeStamp;", dateTo.Year, dateTo.Month, dateTo.Day, dateTo.Hour, dateTo.Minute, dateTo.Second));

            var query = new SqlCommand(cmd.ToString(),_connection);
            var candles = new Stack<Candle>();
            var dataReader = query.ExecuteReader();

            while (dataReader.Read())
            {
                candles.Push(new Candle()
                {
                    DateTimeStamp = (DateTime)dataReader["DateTimeStamp"],
                    Close = (float)dataReader["Close"],
                    Open = (float)dataReader["Open"],
                    High = (float)dataReader["High"],
                    Low = (float)dataReader["Low"]
                });
            }
            dataReader.Close();

            return candles.ToArray(); 
        }
    }
}
