using DBAccess.Database;

namespace Calculator.Strategies
{
    public interface IStrategy
    {
        string Name { get; }
        int AnalysisDataLength { get; }
        StrategyParameter[] Parameters { get; }
        StrategyResult Analyze(StockData[] candles, out object[] outData);
        string[] OutDataDescription { get; }
    }
}
