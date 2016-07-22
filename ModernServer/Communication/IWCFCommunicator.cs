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
        IStrategy[] GetAvaliableStrategies();

        [OperationContract]
        RemoteCalculationInfo[] GetRemoteCalculationsInfo();

        [OperationContract]
        RemoteCalculationInfo AddRemoteCalculation(string name, Type strategyType);

        [OperationContract]
        void AddOrderToRemoteCalulation(Guid idCalculation, string insName, DateTime dateFrom, DateTime dateTo, TimePeriods period, float[] parameters);

        [OperationContract]
        void StartRemoteCalculation(Guid idCalculation);

        [OperationContract]
        CalculationOrder[] GetFinishedOrdersForRemoteCalculation(Guid idCalculation);
    }
}
