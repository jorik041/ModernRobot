using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Globalization;
using System.Net;

namespace DBAccess
{
    public class FuturesDownloader
    {
        private string _dateTemplate = "dd.MM.yyyy";
        private char _splitSymbol = ',';

        public int PreLoadValuesCount = 10;

        public FuturesDownloader(string insName, string[] lines)
        {
            GetRawInstruments(insName, lines);
        }

        public object LockObject = new object();
        private List<string[]> LoadQoutes(string instrument, string instrumentCode, int marketCode, int timePeriod, DateTime dateFrom, DateTime dateTo)
        {
            var result = LoadFinamData(instrument, timePeriod, marketCode, instrumentCode, dateFrom, dateTo);

            // здесь нужно загрузить еще точки для стратегий
            var preLoadData = new List<string[]>();
            preLoadData = LoadFinamData(instrument, timePeriod, marketCode, instrumentCode, dateFrom.AddDays(-PreLoadValuesCount), dateFrom);

            var dateString = preLoadData.Last()[0].Split(' ')[0];
            preLoadData = preLoadData.Where(o => (!o[0].Contains(dateString) || (o[0] == dateString + " 00:00"))).ToList();

            preLoadData.Reverse();
            preLoadData = preLoadData.Take(PreLoadValuesCount).ToList();
            preLoadData.Reverse();
            preLoadData.AddRange(result);
            if (preLoadData.Count() <= PreLoadValuesCount)
                return null;
            return preLoadData;
        }


        public List<string[]> LoadFinamData(string ticker, int period, int marketCode, string instCode, DateTime dateFrom, DateTime dateTo)
        {
            var link = "http://195.128.78.52/data.txt?market=" + marketCode + "&em=" + instCode +
                       "&code=" + ticker + "&df=" + dateFrom.Day + "&mf=" + (dateFrom.Month-1).ToString() + "&yf=" + dateFrom.Year +
                       "&dt=" + dateTo.Day + "&mt=" + (dateTo.Month-1) + "&yt=" + dateTo.Year + "&p=" + (period + 2).ToString() +
                       "data&e=.txt&cn=GAZP&dtf=4&tmf=4&MSOR=1&sep=1&sep2=1&datf=5&at=1";
            string data = "";

            lock (LockObject)
            {
                HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(link);
                HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
                using (StreamReader stream = new StreamReader(
                     resp.GetResponseStream(), Encoding.UTF8))
                {
                    data = stream.ReadToEnd();
                }
            }

            var splits = new string[] {"\r\n"};
            var strings = data.Split(splits, StringSplitOptions.None);

            //var logPath = Path.Combine(_coreDirectory,"download.log");
            //File.AppendAllLines(logPath, new string[] { "", " ----"+DateTime.Now+"----------- ", " Loading " + ticker + " from " + dateFrom + " to " + dateTo});
            //File.AppendAllLines(logPath, strings);
            // in <- <DATE>,<TIME>,<OPEN>,<HIGH>,<LOW>,<CLOSE>,<VOL>
            // out -> Date, Open, High, Low, Close, Volume, Adj Close
            //File.AppendAllLines(logPath, new string[] { "Date, Open, High, Low, Close, Volume, Adj Close" });
            var result = new List<string[]>();
            for (int i = 1; i < strings.Count()-1; i++)
            {
                var split = strings[i].Split(',');
                var strResult = new string[] { split[0] + " " + split[1], split[2], split[3], split[4], split[5], split[6], split[5] };

                var date = DateTime.ParseExact(split[0] + " " + split[1], "dd/MM/yy HH:mm", CultureInfo.InvariantCulture);

                if ((date <= dateTo)&&(date>dateFrom))
                {
                    result.Add(strResult);
                    //File.AppendAllLines(logPath, new string[] { strings[i] });
                }
            }
            
            return result;
        }


        private Dictionary<string, string[]> _rawInstruments = new Dictionary<string,string[]>();
        public void GetRawInstruments(string insName, string[] lines)
        {
                _rawInstruments.Add(insName, lines.Where(o => o != null && o != "" && o[0] != '#').ToArray());
        }

        public Dictionary<string, List<string[]>> Load(string instrument, int timePeriod, DateTime dateFrom, DateTime dateTo)
        {
            /*var dataFileName = Path.Combine(_coreDirectory,instrument+".dat");
            if (!File.Exists(dataFileName))
                return null;
            string[] rawInstruments;
            rawInstruments = File.ReadAllLines(dataFileName).Where(o => o != null && o != "" && o[0] != '#').ToArray(); */
            if (!_rawInstruments.Keys.Contains(instrument))
                return null;
            var rawInstruments = _rawInstruments[instrument];
            var instruments = new List<InstrumentDescription>();
            try
            {
                foreach (var ri in rawInstruments)
                {
                    instruments.Add(new InstrumentDescription() { 
                        Ticker = ri.Split(_splitSymbol)[0],
                        InstrumentCode = ri.Split(_splitSymbol)[1],
                        MarketCode  = ri.Split(_splitSymbol)[2],
                        DateFrom = DateTime.ParseExact(ri.Split(_splitSymbol)[3],_dateTemplate,CultureInfo.InvariantCulture),
                        DateTo = DateTime.ParseExact(ri.Split(_splitSymbol)[4], _dateTemplate, CultureInfo.InvariantCulture)
                    });        
                }
            }
            catch
            {
                return null;
            }

            instruments = instruments.OrderBy(o => o.DateFrom).ToList();

            // устанавливаем даты
            var startInstrumentIndex = instruments.IndexOf(instruments.Last(o => (o.DateFrom <= dateFrom)));
            var stopInstrumentIndex = instruments.IndexOf(instruments.Last(o => (o.DateFrom < dateTo)));
            if ((dateFrom < instruments.First().DateFrom) || (dateTo > instruments.Last().DateTo) || (dateTo > DateTime.Now) 
                || (startInstrumentIndex==-1) || (stopInstrumentIndex==-1))
            {
                return null;    
            }

            instruments = instruments.Where(o=> (instruments.IndexOf(o)>=startInstrumentIndex) && (instruments.IndexOf(o)<=stopInstrumentIndex)).ToList();
            instruments.First().DateFrom = dateFrom;
            instruments.Last().DateTo = dateTo;

            var result = new Dictionary<string, List<string[]>>();

            foreach (var ins in instruments)
                result.Add(ins.Ticker,LoadQoutes(ins.Ticker,ins.InstrumentCode,Convert.ToInt32(ins.MarketCode),timePeriod,ins.DateFrom,ins.DateTo));

            return result;
        }
    }
}
