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
        public const string DateTemplate = "dd/MM/yy HH:mm";

        public List<string[]> LoadFinamData(string ticker, int period, string marketCode, string instCode, DateTime dateFrom, DateTime dateTo)
        {
            var link = "http://195.128.78.52/data.txt?market=" + marketCode + "&em=" + instCode +
                       "&code=" + ticker + "&df=" + dateFrom.Day + "&mf=" + (dateFrom.Month-1).ToString() + "&yf=" + dateFrom.Year +
                       "&dt=" + dateTo.Day + "&mt=" + (dateTo.Month-1) + "&yt=" + dateTo.Year + "&p=" + (period + 2).ToString() +
                       "data&e=.txt&cn=GAZP&dtf=4&tmf=4&MSOR=1&sep=1&sep2=1&datf=5&at=1";
            string data = "";

                HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(link);
                req.Timeout = int.MaxValue;
                HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
                using (StreamReader stream = new StreamReader(
                     resp.GetResponseStream(), Encoding.UTF8))
                {
                    data = stream.ReadToEnd();
                }

            var splits = new string[] {"\r\n"};
            var strings = data.Split(splits, StringSplitOptions.None);

            var result = new List<string[]>();
            for (int i = 1; i < strings.Count()-1; i++)
            {
                var split = strings[i].Split(',');
                var strResult = new string[] { split[0] + " " + split[1], split[2], split[3], split[4], split[5], split[6], split[5] };

                var date = DateTime.ParseExact(split[0] + " " + split[1], "dd/MM/yy HH:mm", CultureInfo.InvariantCulture);

                if ((date <= dateTo)&&(date>dateFrom))
                {
                    result.Add(strResult);
                }
            }
            
            return result;
        }
    }
}
