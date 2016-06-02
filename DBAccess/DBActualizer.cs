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

        private const int DBCHECKPERIOD = 3600 * 24 * 1000;
        private const int DOWNLOADPERIOD = 24; // hrs
        private const string DATEFORMAT = "dd.MM.yyy";

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
            _checkTimer = new Timer(o => ActualizeDB(), null, 0, DBCHECKPERIOD);
        }

        /// <summary>
        /// Actualizes DB
        /// </summary>
        public void ActualizeDB()
        {
            Log("Checking DB to be actual..");
            using (var context = new DatabaseContainer())
            {
                var itemIds = context.ItemsSet.Select(o => o.Id).ToList();
                var itemDatesFrom = context.ItemsSet.Select(o => o.DateFrom).ToArray();
                var itemDatesTo = context.ItemsSet.Select(o => o.DateTo).ToArray();
                var itemNames = context.ItemsSet.Select(o => o.Ticker).ToArray();
                var itemPeriods = context.ItemsSet.Select(o => o.Period).ToArray();
                var itemMarketCodes = context.ItemsSet.Select(o => o.MarketCode).ToArray();
                var itemInsCodes = context.ItemsSet.Select(o => o.InstrumentCode).ToArray();

                foreach (var itemId in itemIds)
                {
                    var i = itemIds.IndexOf(itemId);
                    _logToScreen?.Invoke(string.Format(" Actualizing item {0} from {1}", i+1, itemIds.Count()));
                    var date = itemDatesFrom[i];
                    var dateFrom = itemDatesFrom[i];
                    var dateTo = itemDatesTo[i];
                    while (date <= dateTo)
                    {
                        var maxId = context.StockDataSet.Any() ? context.StockDataSet.Max(o => o.Id) : 0;
                        if (date >= DateTime.Now)
                            break;
                        var newDate = date.AddHours(DOWNLOADPERIOD);
                        var needsActualization = !context.StockDataSet.Where(o => o.ItemId == itemId)
                            .Any(o => o.DateTimeStamp >= date && o.DateTimeStamp <= newDate);
                        if (needsActualization)
                        {
                            var dataToAdd = _downloader.LoadFinamData(itemNames[i], itemPeriods[i], itemMarketCodes[i], itemInsCodes[i], date, newDate)
                                .Select(o => new StockData()
                                {
                                    DateTimeStamp = DateTime.ParseExact(o[0],FuturesDownloader.DateTemplate, CultureInfo.InvariantCulture),
                                    Open = float.Parse(o[1], CultureInfo.InvariantCulture),
                                    High = float.Parse(o[2], CultureInfo.InvariantCulture),
                                    Low = float.Parse(o[3], CultureInfo.InvariantCulture),
                                    Close = float.Parse(o[4], CultureInfo.InvariantCulture),
                                    Volume = float.Parse(o[5], CultureInfo.InvariantCulture),
                                    ItemId = itemId
                                });
                            var idc = 0;
                            if (dataToAdd.Count() > 0)
                            {
                                Log(string.Format("Need actualization for item {0} from {1} to {2} for {3} data", itemNames[i], date, newDate, (TimePeriods)itemPeriods[i]));
                                foreach (var d in dataToAdd)
                                {
                                    idc++;
                                    d.Id = maxId + idc;
                                    if (!context.StockDataSet.Any(o => o.DateTimeStamp == d.DateTimeStamp))
                                        context.StockDataSet.Add(d);
                                }
                                Log("Done.");
                            }
                        }
                        date = date.AddHours(DOWNLOADPERIOD);
                        context.SaveChanges();
                    }
                }

                _logToScreen?.Invoke("");
                Log("Actualization completed.");

                if (!CheckDBIntegrity())
                    Log("DB integrity check: DB corruped.");
                else
                    Log("DB integrity check: ok.");
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
