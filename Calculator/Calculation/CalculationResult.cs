using System.Runtime.Serialization;

namespace Calculator.Calculation
{
    [DataContract]
    public struct CalculationResult
    {
        [DataMember]
        public float[] Balances { get; set; }
        [DataMember]
        public string[][] OutData { get; set; }
    }
}
