using System;
using System.Collections.Generic;
using System.Linq;
using DBAccess.Entities;
using System.Diagnostics;

namespace Calculator.Strategies
{
    public class FortsBasicInv : IStrategy
    {
        public string Name
        {
            get { return "Basic (Inverted)"; }
        }

        public int AnalysisDataLength
        {
            get { return 300; }
        }

        private StrategyParameter[] _defaultParameters
        {
            get
            {
                return new StrategyParameter[]
                {
                    new StrategyParameter("Период MIN и MAX")
                    {
                        Value = 10
                    },
                    new StrategyParameter("Число шагов фильтра 1")
                    {
                        Value = 3
                    },
                    new StrategyParameter("Период средней")
                    {
                        Value = 1
                    },
                    new StrategyParameter("Число шагов фильтра 2")
                    {
                        Value = 8
                    }
                };
            }
        }
        public StrategyParameter[] Parameters { get; private set; }

        public string[] OutDataDescription
        {
            get
            {
                return new string[] { "MAX Highs", "MIN Lows", "Average", "Sign (Close - Average)", "Filter 1", "Filter 2 Average", "Sign (Filter 2 Average)", "R1", "R2", "Result" };
            }
        }

        public event EventHandler<float> OnStopLossChanged;
        public float StopLossValue { get; set; }

        private void ChangeStopLoss(float newValue)
        {
            if (OnStopLossChanged != null)
                OnStopLossChanged(this, newValue);
        }

        public FortsBasicInv()
        {
            Parameters = _defaultParameters;
        }

        private List<float[]> _matrix;
        private StrategyResult _lastResult;
        private float? _newSLValue;

        public void Initialize()
        {
            _matrix = new List<float[]>();
            _lastResult = StrategyResult.None;
            _newSLValue = null;
        }

        private float CalculateTrimmedMean(float[] points, float trimPercent)
        {
            var g = (int)Math.Floor(trimPercent * points.Count() * 0.01 / 2);
            var n = points.Count();
            var sortedPoints = points.OrderBy(o => o).ToList();
            sortedPoints.RemoveRange(n - g, g);
            sortedPoints.RemoveRange(0, g);
            float sum = 0;
            foreach (var p in sortedPoints)
                sum = sum + p;
            return sum / (n - 2 * g);
        }

        private void AddMatrixRow(Candle c)
        {
            var newCol = new float[9 + (int)Parameters[3].Value * 2 + 5];
            newCol[0] = c.Open;
            newCol[1] = c.High;
            newCol[2] = c.Low;
            newCol[3] = c.Close;
            _matrix.Add(newCol);
        }

        private void InitiateCandles(Candle[] candles)
        {
            foreach (var c in candles)
                AddMatrixRow(c);
            for (var i = (int)Parameters.Select(o => o.Value).Max() + 1; i < candles.Count(); i++)
                ProcessRow(i);
        }

        private void ProcessRow(int number)
        {
            // sensitive data (algorithm removed)
        }

        public StrategyResult Analyze(Candle[] candles, out object[] outData)
        {
            if (!_matrix.Any())
                InitiateCandles(candles);
            else
            {
                AddMatrixRow(candles.Last());
                ProcessRow(_matrix.Count() - 1);
            }

            var row = _matrix.Last();
            var intResult = _matrix.Last().Last();
            StrategyResult result = StrategyResult.Exit;
            if (intResult > 0)
                result = StrategyResult.Long;
            if (intResult < 0)
                result = StrategyResult.Short;

            outData = new object[] { row[4], row[5], row[6], row[7], row[8], row[9 + 2 * (int)Parameters[3].Value], row[9 + 2 * (int)Parameters[3].Value + 1], row[9 + 2 * (int)Parameters[3].Value + 2], row[9 + 2 * (int)Parameters[3].Value + 3], result };

            if (result != _lastResult)
                _newSLValue = null;

            if (StopLossValue != 0)
            {
                if (result == StrategyResult.Short)
                {
                    var newValue = Math.Max(_matrix.Last()[3] + StopLossValue, _matrix.Last()[6]);
                    if ((_newSLValue == null) || (newValue < _newSLValue))
                    {
                        _newSLValue = newValue;
                        ChangeStopLoss((float)_newSLValue);
                    }
                }

                if (result == StrategyResult.Long)
                {
                    var newValue = Math.Min(_matrix.Last()[3] - StopLossValue, _matrix.Last()[6]);
                    if ((_newSLValue == null) || (newValue > _newSLValue))
                    {
                        _newSLValue = newValue;
                        ChangeStopLoss((float)_newSLValue);
                    }
                }
            }

            _lastResult = result;

            if (result == StrategyResult.Long)
                result = StrategyResult.Short;
                else
                    if (result == StrategyResult.Short)
                        result = StrategyResult.Long;

            return result;
        }
    }
}
