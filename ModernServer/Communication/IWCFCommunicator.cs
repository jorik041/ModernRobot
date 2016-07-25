﻿using System;
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
        void AddOrderToRemoteCalulation(Guid idCalculation, string insName, DateTime dateFrom, DateTime dateTo, TimePeriods period, float[] parameters);

        [OperationContract]
        void StartRemoteCalculation(Guid idCalculation);

        [OperationContract]
        void StopRemoteCalculation(Guid id);

        [OperationContract]
        CalculationOrder[] GetFinishedOrdersForRemoteCalculation(Guid idCalculation);

        [OperationContract]
        int GetWaitingOrdersForRemoteCalculation(Guid idCalculation);

        [OperationContract]
        string[] GetStrategyParametersDescription(string strategyName);
    }
}
