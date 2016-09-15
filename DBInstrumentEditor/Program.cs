using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using DBAccess.Database;
using DBAccess;

namespace DBInstrumentEditor
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(string.Format(" DB Instruments editor v. {0}", Assembly.GetExecutingAssembly().GetName().Version));
            Console.WriteLine();
            using (var context = new DatabaseContainer())
            {
                Console.WriteLine(string.Format(" Connected to database {0}.", context.Database.Connection.Database));
                var avaliableInstruments = context.InstrumentsSet.Select(o => o.Name).ToArray();
                Console.Write(string.Format(" Specify instrument to edit [ {0} ]:", string.Join(",", avaliableInstruments)));
                var instrumentToEdit = Console.ReadLine();
                Console.Write(" Enter ticker:");
                var ticker = Console.ReadLine();
                Console.Write(" Enter market code:");
                var marketCode = Console.ReadLine();
                Console.Write(" Enter instrument code:");
                var insCode = Console.ReadLine();
                Console.Write(" Enter date from:");
                DateTime dateFrom;
                if (!DateTime.TryParse(Console.ReadLine(), out dateFrom))
                {
                    Console.WriteLine(" Error parsing date.");
                    return;
                }
                else
                {
                    Console.WriteLine(" Date parsed succesfully: {0}.", dateFrom);
                }
                Console.Write(" Enter date to:");
                DateTime dateTo;
                if (!DateTime.TryParse(Console.ReadLine(), out dateTo))
                {
                    Console.WriteLine(" Error parsing date.");
                    return;
                }
                else
                {
                    Console.WriteLine(" Date parsed succesfully: {0}.", dateTo);
                }

                if (dateFrom > dateTo)
                {
                    Console.WriteLine(" Invalid dates.");
                    return;
                }
                var data = (new FuturesDownloader()).LoadFinamData(ticker, 5, marketCode, insCode, dateFrom, dateFrom.AddDays(14));
                if (!data.Any())
                {
                    Console.WriteLine(" No data avaliable.");
                    Console.ReadLine();
                    return;
                }

                foreach (var str in data)
                    Console.WriteLine(string.Join(" ", str));
                Console.WriteLine();

                var currIns = context.InstrumentsSet.SingleOrDefault(o => o.Name == instrumentToEdit);
                if (currIns == null)
                    context.InstrumentsSet.Add(currIns = new Instruments() { Name = instrumentToEdit });

                if (currIns.Items.Any())
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    var item = currIns.Items.OrderBy(o => dateFrom - o.DateTo).First();
                    Console.WriteLine("{0} {1} {2}", item.Ticker, item.DateFrom, item.DateTo);
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("{0} {1} {2}", ticker, dateFrom, dateTo);
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.WriteLine();
                }
                Console.WriteLine();
                for (byte i=0; i <=5; i++)
                    currIns.Items.Add(new Items() { DateFrom = dateFrom, DateTo = dateTo, MarketCode = marketCode, Ticker = ticker, InstrumentCode = insCode, Period = i });
                Console.WriteLine(string.Format(" Are you sure you want to add {0} to instrument {1} from {2} to {3}? [y/n]", ticker, instrumentToEdit, dateFrom, dateTo));
                var ans = Console.ReadKey();
                if ((ans.KeyChar != 'Y') && (ans.KeyChar != 'y'))
                    return;
                try
                {
                    context.SaveChanges();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }

                new DBActualizer().ActualizeDB();
                Console.WriteLine();
                Console.WriteLine("Success. ");
                Console.ReadLine();
            }
        }
    }
}
