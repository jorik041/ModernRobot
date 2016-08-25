using DBAccess.Database;
using DBAccess.Entities;
using System;

namespace Calculator.Strategies
{
    public interface IStrategy
    {
        string Name { get; }
        void Initialize();
        int AnalysisDataLength { get; }
        StrategyParameter[] Parameters { get; }
        StrategyResult Analyze(Candle[] candles, out object[] outData);
        string[] OutDataDescription { get; }

        event EventHandler<float> OnStopLossChanged;
        float StopLossValue { get; set; }
    }
}
