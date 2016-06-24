using DBAccess.Database;
using DBAccess.Entities;

namespace Calculator.Strategies
{
    public interface IStrategy
    {
        string Name { get; }
        int AnalysisDataLength { get; }
        StrategyParameter[] Parameters { get; }
        StrategyResult Analyze(Candle[] candles, out object[] outData);
        string[] OutDataDescription { get; }
    }
}
