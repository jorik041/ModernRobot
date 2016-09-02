using System.Runtime.Serialization;

namespace Calculator.Calculation
{
    [DataContract]
    public class CalculationResult
    {
        [DataMember]
        public float[] Balances { get; set; }
        [DataMember]
        public string[][] OutData { get; set; }
        [DataMember]
        public string[] OutDataDescription { get; set; }
    }
}
