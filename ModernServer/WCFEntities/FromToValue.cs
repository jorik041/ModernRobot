using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace ModernServer.WCFEntities
{
    [DataContract]
    public class FromToValue
    {
        [DataMember]
        public float From { get; set; }
        [DataMember]
        public float To { get; set; }
    }
}
