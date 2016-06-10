using DBAccess.Database;
using System.Collections.Generic;
using System;

namespace DBAccess.Interfaces
{
    public interface IDBDataReader
    {
        StockData[] GetCandles(string instrumentName, TimePeriods period, DateTime dateFrom, DateTime dateTo);
        string GetTickerForCandle(DateTime dateTimeStamp);        
    }
}
