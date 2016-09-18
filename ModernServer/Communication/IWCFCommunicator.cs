using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using ModernServer.WCFEntities;
using Calculator.Strategies;
using DBAccess;
using Calculator.Calculation;

namespace ModernServer.Communication
{
    [ServiceContract]
    public interface IWCFCommunicator
    {
        [OperationContract]
        ActualizedInstrument[] GetActualizedInstruments();

        [OperationContract]
        string[] GetAvaliableStrategies();

        [OperationContract]
        RemoteCalculationInfo[] GetRemoteCalculationsInfo();

        [OperationContract]
        RemoteCalculationInfo AddRemoteCalculation(string name, string strategyName);

        [OperationContract]
        void RemoveRemoteCalculation(Guid id);

        [OperationContract]
        void AddOrderToRemoteCalulation(Guid idCalculation, string insName, DateTime dateFrom, DateTime dateTo, TimePeriods period, float[] parameters, float stopLoss, bool ignoreNightCandles);

        [OperationContract]
        void AddOrdersToRemoteCalulation(Guid idCalculation, string insName, DateTime dateFrom, DateTime dateTo, TimePeriods period, FromToValue[] parameters, float stopLoss, bool ignoreNightCandles);

        [OperationContract]
        void StartRemoteCalculation(Guid idCalculation);

        [OperationContract]
        void StopRemoteCalculation(Guid id);

        [OperationContract]
        CalculationOrder[] GetFinishedOrdersForRemoteCalculation(Guid idCalculation, int pointsCount);

        [OperationContract]
        CalculationResult GetFinishedOrderResult(Guid idCalculation, Guid idOrder);

        [OperationContract]
        int GetWaitingOrdersForRemoteCalculation(Guid idCalculation);

        [OperationContract]
        int GetFinishedOrdersCountForRemoteCalculation(Guid idCalculation);

        [OperationContract]
        string[] GetStrategyParametersDescription(string strategyName);
    }
}
