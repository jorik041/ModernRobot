using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBAccess.Interfaces
{
    public interface IItemsManager
    {
        bool ParseItemsStrings(string instrumentName, string[] lines, string formatLine);
        string[] GetItemsStrings(string instrumentName, string formatLine);
    }
}
