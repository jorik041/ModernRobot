using DBAccess.Database;
using DBAccess.Entities;

namespace Calculator.Strategies
{
    public interface IStrategy
    {
        string Name { get; }
        bool SingleLOT { get;  }
        void Initialize();
        int AnalysisDataLength { get; }
        StrategyParameter[] Parameters { get; }
        StrategyResult Analyze(Candle[] candles, out object[] outData);
        string[] OutDataDescription { get; }
    }
}
