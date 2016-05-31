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
        private Timer _checkTimer;

        private const int DBCHECKPERIOD = 3600 * 24 * 1000;
        private const int DOWNLOADPERIOD = 24; // hrs
        private const string DATEFORMAT = "dd.MM.yyy";

        private void Log(string contents)
        {
            _log?.Invoke(contents);
        }
        public DBActualizer(Action<string> logAction)
        {
            _log = logAction;
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
                foreach (var ins in context.InstrumentsSet)
                {
                    var downloader = new FuturesDownloader();
                    foreach (var item in ins.Items)
                    {
                        Log(string.Format(" Checking ticker {0} for instrument {1} from {2} to {3} for period {4}", item.Ticker, ins.Name, item.DateFrom, item.DateTo, (TimePeriods)item.Period));
                        if (!item.StockData.Any() || (item.StockData.Max(o => o.DateTimeStamp) < item.DateTo))
                        {
                            Log(string.Format(" Need actualizaion for {0}", item.Ticker));
                            var ct = item.DateFrom;
                            while (ct <= item.DateTo)
                            {
                                if (!item.StockData.Any(o => o.DateTimeStamp == ct))
                                {
                                    try
                                    {
                                        var data = downloader.LoadFinamData(item.Ticker, item.Period, item.MarketCode, item.InstrumentCode, ct, ct.AddHours(DOWNLOADPERIOD));
                                        foreach (var d in data)
                                        {
                                            Log(string.Format("Actualizing : {0}", string.Join(", ", d)));
                                            var dtStamp = DateTime.ParseExact(d[0], FuturesDownloader.DateTemplate, CultureInfo.InvariantCulture);
                                            if (!item.StockData.Any(o => o.DateTimeStamp == dtStamp))
                                                item.StockData.Add(new StockData()
                                                {
                                                    DateTimeStamp = dtStamp,
                                                    Open = float.Parse(d[1],CultureInfo.InvariantCulture),
                                                    High = float.Parse(d[2], CultureInfo.InvariantCulture),
                                                    Low = float.Parse(d[3], CultureInfo.InvariantCulture),
                                                    Close = float.Parse(d[4], CultureInfo.InvariantCulture),
                                                    Volume = float.Parse(d[5], CultureInfo.InvariantCulture),
                                                    ItemId = item.Id
                                                });
                                        }
                                    }
                                    catch (InvalidOperationException ex)
                                    {
                                        Log("ERROR loading data.");
                                        Log(ex.ToString());
                                    }
                                }
                                ct = ct.AddHours(DOWNLOADPERIOD);
                            }
                        }
                    }
                }
                context.SaveChanges();
            }
        }

        /// <summary>
        ///  Checks DB integrity
        /// </summary>
        public void CheckDBIntegrity()
        {

        }

        public void Dispose()
        {
            _checkTimer.Dispose();
        }

        #endregion
    }
}
