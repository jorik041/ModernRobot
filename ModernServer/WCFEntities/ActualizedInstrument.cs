using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace ModernServer.WCFEntities
{
    [DataContract]
    public struct ActualizedInstrument
    {
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public DateTime DateFrom { get; set; }
        [DataMember]
        public DateTime DateTo { get; set; }
    }
}
