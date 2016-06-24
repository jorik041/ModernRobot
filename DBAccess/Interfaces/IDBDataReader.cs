using DBAccess.Database;
using System.Collections.Generic;
using System;
using DBAccess.Entities;

namespace DBAccess.Interfaces
{
    public interface IDBDataReader
    {
        Candle[] GetCandles(string instrumentName, TimePeriods period, DateTime dateFrom, DateTime dateTo);
        string GetTickerForCandle(DateTime dateTimeStamp);        
    }
}
