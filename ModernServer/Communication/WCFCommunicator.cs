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
        private readonly IStrategy[] _avaliableStrategies = { new FortsBasic(), new FortsBasicInv() };
        private List<Guid> _startedCalcIds = new List<Guid>();
        private int[] _collector;

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

        public string[] GetAvaliableStrategies()
        {
            return _avaliableStrategies.Select(o => o.Name).ToArray();    
        }

        public string[] GetStrategyParametersDescription(string strategyName)
        {
            var strategy = _avaliableStrategies.SingleOrDefault(o => o.Name==strategyName);
            if (strategy != null)
                return strategy.Parameters.Select(o => o.Description).ToArray();
            return null;

        }
        public RemoteCalculationInfo[] GetRemoteCalculationsInfo()
        {
            return _remoteCalculators.Select(o =>
            new RemoteCalculationInfo(o.Id, o.Name, o.StrategyName)
            {
                WaitingOrdersCount = o.WaitingOrdersCount,
                FinishedOrdersCount = o.FinishedOrdersCount,
                IsWaiting = o.IsWaiting,
                IsDone = o.IsDone,
                IsRunning = o.IsRunning
            }).ToArray();
        }

        public RemoteCalculationInfo AddRemoteCalculation(string name, string strategyName)
        {
            var strategy = _avaliableStrategies.SingleOrDefault(o => o.Name == strategyName);
            if (strategy == null)
                return null;
            var strategyType = strategy.GetType();
            var rCalc = new RemoteCalculation(name, strategyType);
            _remoteCalculators.Add(rCalc);
            return new RemoteCalculationInfo(rCalc.Id, rCalc.Name, rCalc.StrategyName) { WaitingOrdersCount = rCalc.WaitingOrdersCount, FinishedOrdersCount = rCalc.FinishedOrdersCount };
        }

        public void RemoveRemoteCalculation(Guid id)
        {
            var rCalc = _remoteCalculators.SingleOrDefault(o => o.Id == id);
            if (rCalc != null)
                _remoteCalculators.Remove(rCalc);
        }

        public void AddOrderToRemoteCalulation(Guid idCalculation, string insName, DateTime dateFrom, DateTime dateTo, TimePeriods period, float[] parameters, float stopLoss, bool ignoreNightCandles)
        {
            var rc = _remoteCalculators.Single(o => o.Id == idCalculation);
            if (rc == null)
                return;
            rc.OrdersPool.AddNewOrderForCalculation(insName, dateFrom, dateTo, period, parameters, stopLoss, ignoreNightCandles);
        }

        private void CreateOrdersForMultipleParams(Guid idCalculation, string insName, DateTime dateFrom, DateTime dateTo, TimePeriods period, FromToValue[] parameters, float stopLoss, bool ignoreNightCandles, int num = 0)
        {
            _collector[num] = (int)parameters[num].From - 1;
            while (_collector[num] < parameters[num].To)
            {
                _collector[num]++;
                var newVal = new float[parameters.Count()];
                for (var i = 0; i < _collector.Count(); i++)
                    newVal[i] = _collector[i];
                if (num == _collector.Count() - 1)
                {
                    AddOrderToRemoteCalulation(idCalculation, insName, dateFrom, dateTo, period, newVal, stopLoss, ignoreNightCandles);
                }
                if (num < _collector.Count() - 1)
                    CreateOrdersForMultipleParams(idCalculation, insName, dateFrom, dateTo, period, parameters, stopLoss, ignoreNightCandles, num + 1);
            }
        }

        public void AddOrdersToRemoteCalulation(Guid idCalculation, string insName, DateTime dateFrom, DateTime dateTo, TimePeriods period, FromToValue[] parameters, float stopLoss, bool ignoreNightCandles)
        {
            var rc = _remoteCalculators.Single(o => o.Id == idCalculation);
            if (rc == null)
                return;
            try
            {
                _collector = new int[parameters.Count()];
                rc.OrdersPool.Lock();
                CreateOrdersForMultipleParams(idCalculation, insName, dateFrom, dateTo, period, parameters, stopLoss, ignoreNightCandles);
            }
            catch (Exception ex)
            {
                Logger.Log(ex.ToString());
            }
            finally
            {
                rc.OrdersPool.UnLock();
            }
        }


        public void StartRemoteCalculation(Guid idCalculation)
        {
            var rc = _remoteCalculators.Single(o => o.Id == idCalculation);
            if (rc == null)
                return;
            if (_startedCalcIds.Contains(rc.Id))
                return;
            _startedCalcIds.Add(rc.Id);
            rc.OrdersPool.ProcessOrders();
        }

        public void StopRemoteCalculation(Guid idCalculation)
        {
            var rc = _remoteCalculators.Single(o => o.Id == idCalculation);
            if (rc == null)
                return;
            rc.OrdersPool.Flush();
        }

        public CalculationOrder[] GetFinishedOrdersForRemoteCalculation(Guid idCalculation, int pointsCount)
        {
            var rc = _remoteCalculators.Single(o => o.Id == idCalculation);
            if (rc == null)
                return null;
            return rc.OrdersPool.FinishedOrders.OrderByDescending(o => o.TotalBalance * o.TotalBalance / o.Gap).Take(pointsCount).ToArray();
        }

        public int GetFinishedOrdersCountForRemoteCalculation(Guid idCalculation)
        {
            var rc = _remoteCalculators.Single(o => o.Id == idCalculation);
            if (rc == null)
                return 0;
            return rc.OrdersPool.FinishedOrders.Count();
        }

        public CalculationResult GetFinishedOrderResult(Guid idCalculation, Guid idOrder)
        {
            var calc = _remoteCalculators.SingleOrDefault(o => o.Id == idCalculation);
            if ((calc == null) || (!calc.OrdersPool.FinishedOrders.Any(o => o.Id == idOrder)))
                return new CalculationResult();
            calc.OrdersPool.GetFinishedOrderResults(idOrder);
            return calc.OrdersPool.FinishedOrders.First(o => o.Id == idOrder).Result;
        }

        public int GetWaitingOrdersForRemoteCalculation(Guid idCalculation)
        {
            var rc = _remoteCalculators.Single(o => o.Id == idCalculation);
            if (rc == null)
                return 0;
            return rc.OrdersPool.WaitingOrdersCount;
        }
    }
}
