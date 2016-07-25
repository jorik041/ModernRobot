using Calculator.Strategies;
using DBAccess;
using System;

namespace Calculator.Calculation
{
    public interface ICalculationOrdersPool
    {
        CalculationOrder[] FinishedOrders { get; }
        int WaitingOrdersCount { get; }
        Guid AddNewOrderForCalculation(string insName, DateTime dateFrom, DateTime dateTo, TimePeriods period, float[] parameters);
        void ProcessOrders();
        void Flush();
    }
}
