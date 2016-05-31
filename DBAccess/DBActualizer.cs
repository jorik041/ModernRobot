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
        public bool ParseItemsStrings(string instrumentName, string[] lines, string formatLine)
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
        public string[] GetItemsStrings(string instrumentName, string formatLine)
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
        private FuturesDownloader _downloader;
        private Timer _checkTimer;

        private const int DBCHECKPERIOD = 3600 * 24 * 1000;

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
                    foreach (var item in ins.Items)
                    {
                        Log(string.Format(" Checking ticker {0} for instrument {1} from {2} to {3}", item.Ticker, ins.Name, item.DateFrom, item.DateTo));
                        if (!item.StockData.Any() || (item.StockData.Max(o => o.DateTimeStamp) < item.DateTo))
                        {
                            Log(string.Format(" Need actualizaion for {0}", item.Ticker));
                        }
                    }
                }
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
