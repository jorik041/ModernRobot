using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DBAccess.Database;

namespace DBDriver.Interfaces
{
    public interface IDBDriver
    {
        void AddInstrument(Instruments instrument);
        void DeleteInstrument(Instruments instrument);
        bool CheckDBIntegrity();
    }
}
