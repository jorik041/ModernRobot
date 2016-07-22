using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using CommonLib.Helpers;
using ModernServer.WCFEntities;
using DBAccess;
using Calculator.Strategies;
using Calculator.Calculation;

namespace ModernServer.Communication
{
    public class WCFCommunicator : IWCFCommunicator, IDisposable
    {
        private static List<RemoteCalculation> _remoteCalculators = new List<RemoteCalculation>();

        private DBDataReader _reader = new DBDataReader();
        private readonly IStrategy[] _avaliableStrategies = { new FortsBasic() };

        public ActualizedInstrument[] GetActualizedInstruments()
        {
            return
            _reader.GetAvaliableInstrumentNames().Select(o => new ActualizedInstrument()
            {
                Name = o,
                DateFrom = _reader.GetMinDateTimeStamp(o).AddMonths(3),
                DateTo = _reader.GetMaxDateTimeStamp(o)
            }).ToArray();
        }

        public void Dispose()
        {
            _reader.Dispose();
        }

        public IStrategy[] GetAvaliableStrategies()
        {
            return _avaliableStrategies;    
        }

        public RemoteCalculationInfo[] GetRemoteCalculationsInfo()
        {
            return _remoteCalculators.ToArray();
        }

        public RemoteCalculationInfo AddRemoteCalculation(string name, Type strategyType)
        {
            var rCalc = new RemoteCalculation(name, strategyType);
            _remoteCalculators.Add(rCalc);
            return rCalc;
        }

        public void AddOrderToRemoteCalulation(Guid idCalculation, string insName, DateTime dateFrom, DateTime dateTo, TimePeriods period, float[] parameters)
        {
            _remoteCalculators.Single(o => o.Id == idCalculation)
                .OrdersPool.AddNewOrderForCalculation(insName, dateFrom, dateTo, period, parameters);
        }

        public void StartRemoteCalculation(Guid idCalculation)
        {
            _remoteCalculators.Single(o => o.Id == idCalculation).OrdersPool.ProcessOrders();
        }

        public CalculationOrder[] GetFinishedOrdersForRemoteCalculation(Guid idCalculation)
        {
            return _remoteCalculators.Single(o => o.Id == idCalculation).OrdersPool.FinishedOrders;
        }
    }
}
