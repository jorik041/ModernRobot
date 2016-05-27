using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DBAccess.Database;
using DBAccess.Entities;
using DBDriver.Interfaces;
using DBAccess;
using System.Configuration;

namespace DBDriver
{
    public class DBDriver : IDBDriver, IDisposable
    {
        private DatabaseContainer _context;

        public DBDriver()
        {
            _context = new DatabaseContainer();        
        }

        public void AddInstrument(Instruments instrument)
        {
            _context.InstrumentsSet.Add(instrument);    
        }

        public bool CheckDBIntegrity()
        {
            return true;
        }

        public void DeleteInstrument(Instruments instrument)
        {
            _context.InstrumentsSet.Remove(instrument);    
        }

        public void Dispose()
        {
            _context.Dispose();
        }

    }
}
