using Calculator.Strategies;
using DBAccess;
using System;

namespace Calculator.Calculation
{
    public interface ICalculationOrdersPool
    {
        IStrategy Strategy { get; }
        CalculationOrder[] FinishedOrders { get; }
        int ProcessingOrdersCount { get; }
        int OrdersCount { get; }
        bool AllOrdersFinished { get; }
        Guid AddNewOrderForCalculation(string insName, DateTime dateFrom, DateTime dateTo, TimePeriods period, float[] parameters);
        void Flush();
    }
}
