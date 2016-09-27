using Calculator.Strategies;
using DBAccess;
using System;

namespace Calculator.Calculation
{
    public interface ICalculationOrdersPool
    {
        CalculationOrder[] FinishedOrders { get; }
        int WaitingOrdersCount { get; }
        Guid AddNewOrderForCalculation(string insName, DateTime dateFrom, DateTime dateTo, TimePeriods period, float[] parameters, float stopLoss, bool ignoreNightCandles, float daySpread, float nightSpread);
        void ProcessOrders();
        bool IsProcessingOrders { get; }
        void GetFinishedOrderResults(Guid orderId);
        void Flush();
    }
}
