using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DBAccess.Database;
using System.Globalization;
using DBAccess.Interfaces;
using System.Threading;

namespace DBAccess
{
    public class DBActualizer : IItemsManager, IActualizer, IDisposable
    {

        #region ItemsManager

        /// <summary>
        /// Parces settings file and saves to DB
        /// </summary>
        /// <param name="instrumentName"></param>
        /// <param name="lines"></param>
        public bool ParseItemsStrings(string instrumentName, string[] lines, string formatLine = DATEFORMAT)
        {
            using (var context = new DatabaseContainer())
            {
                try
                {
                    var instrument = context.InstrumentsSet.SingleOrDefault(o => o.Name == instrumentName);
                    if (instrument == null)
                    {
                        instrument = new Instruments() { Name = instrumentName };
                        context.InstrumentsSet.Add(instrument);
                    }

                    foreach (var line in lines.Where(o => o.First() != '#' && o!=""))
                    {
                        var fields = line.Split(',');
                        if (!instrument.Items.Any(o => o.Ticker == fields[0]))
                        {
                            for (var i = 0; i < Enum.GetNames(typeof(TimePeriods)).Length; i++)
                                instrument.Items.Add(new Items()
                                {
                                    Ticker = fields[0],
                                    InstrumentCode = fields[1],
                                    MarketCode = fields[2],
                                    DateFrom = DateTime.ParseExact(fields[3], formatLine, CultureInfo.InvariantCulture),
                                    DateTo = DateTime.ParseExact(fields[4], formatLine, CultureInfo.InvariantCulture),
                                    InstrumentId = instrument.Id,
                                    Period = (byte)i
                                });
                        }
                    }

                    context.SaveChanges();
                }
                catch (Exception ex)
                {
                    Log(ex.ToString());
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Returns items lines for mentioned instrument from DB
        /// </summary>
        /// <param name="instrumentName"></param>
        /// <param name="formatLine"></param>
        /// <returns></returns>
        public string[] GetItemsStrings(string instrumentName, string formatLine = DATEFORMAT)
        {
            var lines = new List<string>();
            using (var context = new DatabaseContainer())
            {
                var ins = context.InstrumentsSet.FirstOrDefault(o => o.Name == instrumentName);
                if (ins == null)
                    return null;
                lines.Add("# ticker, instrumentCode, marketCode, dateFrom, dateTo");
                foreach (var item in ins.Items)
                    lines.Add(string.Format("{0}, {1}, {2}, {3}, {4}",item.Ticker, item.InstrumentCode, item.MarketCode, item.DateFrom.ToString(formatLine), item.DateTo.ToString(formatLine)));
            }
            return lines.ToArray();
        }

        #endregion

        #region Actualizer

        private Action<string> _log;
        private Action<string> _logToScreen;
        private Timer _checkTimer;
        private FuturesDownloader _downloader;

        private const int DBCHECKHOUR = 5;

        private const int DOWNLOADPERIOD = 24; // hrs
        private const string DATEFORMAT = "dd.MM.yyy";
        private object _lockObject = new object();
        private DateTime _dbLastCheck;

        private void Log(string contents)
        {
            _log?.Invoke(contents);
        }
        public DBActualizer(Action<string> logAction, Action<string> logToScreenAction)
        {
            _log = logAction;
            _logToScreen = logToScreenAction;
            _downloader = new FuturesDownloader();
        }

        public void Start()
        {
            Log("Started DB actualizer.");
            _checkTimer = new Timer(o =>
            {
                lock (_lockObject)
                {
                    try
                    {
                        if (DateTime.Now.Subtract(_dbLastCheck).TotalHours > 1)
                            if ((DateTime.Now.Hour == DBCHECKHOUR) || (_dbLastCheck == DateTime.MinValue))
                            {
                                _dbLastCheck = DateTime.Now;
                                Log(string.Format("Started actualization at {0}", _dbLastCheck)); 
                                ActualizeDB();
                            }
                    }
                    catch (Exception ex)
                    {
                        Log(string.Format("Error actualizing DB: {0}", ex));
                    }
                };
            }, null, 0, 1000*60);
        }

        public void ActualizeDB()
        {
            using (var context = new DatabaseContainer())
            {
                var startTime = DateTime.Now;
                foreach (var ins in context.InstrumentsSet.ToArray())
                    foreach (var item in ins.Items.Where(o => context.StockDataSet.Where(sd => sd.ItemId == o.Id).Any() ? ((o.DateTo >= DateTime.Now.AddHours(-DOWNLOADPERIOD) && o.DateFrom <= DateTime.Now)) : o.Id > 0).ToArray())
                    {
                        Log(string.Format("Getting item {0} for instrument {1} for period {2}", item.Ticker, ins.Name, (TimePeriods)item.Period));
                        var maxId = context.StockDataSet.Count();
                        var dataToAdd = _downloader.LoadFinamData(item.Ticker, item.Period, item.MarketCode,
                            item.InstrumentCode, item.DateFrom, item.DateTo > DateTime.Now ? DateTime.Now : item.DateTo)
                            .Select(o => new StockData()
                            {
                                DateTimeStamp = DateTime.ParseExact(o[0], FuturesDownloader.DateTemplate, CultureInfo.InvariantCulture),
                                Open = float.Parse(o[1], CultureInfo.InvariantCulture),
                                High = float.Parse(o[2], CultureInfo.InvariantCulture),
                                Low = float.Parse(o[3], CultureInfo.InvariantCulture),
                                Close = float.Parse(o[4], CultureInfo.InvariantCulture),
                                Volume = float.Parse(o[5], CultureInfo.InvariantCulture),
                                ItemId = item.Id,
                            }).Where(o => !item.StockData.Any(sd => sd.DateTimeStamp == o.DateTimeStamp));
                        if (dataToAdd.Count() > 0)
                            Log(string.Format("Saving {0} candles total.", dataToAdd.Count()));
                        else
                            Log("Item is actual.");
                        foreach (var data in dataToAdd)
                        {
                            maxId++;
                            data.Id = maxId;
                            item.StockData.Add(data);
                            Log(string.Format("Added DateTimeStamp {0} with Id {1} for item {2}", data.DateTimeStamp, data.Id, item.Ticker));
                        }
                        context.SaveChanges();
                        Log("Done.");
                    }
                Log(string.Format("Last actualized date: {0}", context.StockDataSet.Max(o => o.DateTimeStamp)));
                Log("Actualization completed.");
                if (!CheckDBIntegrity())
                    Log("DB corruped.");
                else
                    Log("DB ok.");
            }
        }

 

        /// <summary>
        ///  Checks DB integrity
        /// </summary>
        public bool CheckDBIntegrity()
        {
            using (var context = new DatabaseContainer())
            {
                var corruped = false;
                foreach (var sd in context.StockDataSet)
                    if (context.StockDataSet.Any(o => o.Id!=sd.Id && o.DateTimeStamp == sd.DateTimeStamp))
                        corruped = true;

                return !corruped;
            }
        }

        public void Stop()
        {
            this.Dispose();
        }

        public void Dispose()
        {
            _checkTimer?.Dispose();
        }

        #endregion
    }
}
