using System;
using System.Collections.Generic;
using System.Linq;
using DBAccess.Entities;
using Calculator.Helpers;

namespace Calculator.Strategies
{
    public class SimpleTrendInv : IStrategy
    {
        public string Name
        {
            get { return "Simple Trend (Inverted)"; }
        }

        public int AnalysisDataLength
        {
            get { return 50; }
        }

        private StrategyParameter[] _defaultParameters
        {
            get
            {
                return new StrategyParameter[]
                {
                    new StrategyParameter("Размер шага 1")
                    {
                        Value = 2
                    },
                    new StrategyParameter("Размер шага 2")
                    {
                        Value = 6
                    },
                    new StrategyParameter("Шаг усреднения")
                    {
                        Value = 4
                    }
                };
            }
        }
        public StrategyParameter[] Parameters { get; private set; }

        public string[] OutDataDescription
        {
            get
            {
                return new string[] { "Trend 1", "Trend 2", "Average", "Result" };
            }
        }

        public event EventHandler<float> OnStopLossChanged;
        public float StopLossValue { get; set; }

        private void ChangeStopLoss(float newValue)
        {
            if (OnStopLossChanged != null)
                OnStopLossChanged(this, newValue);
        }

        public SimpleTrendInv()
        {
            Parameters = _defaultParameters;
        }

        private List<float[]> _matrix;

        public void Initialize()
        {
            _matrix = new List<float[]>();
        }


        private void AddMatrixRow(Candle c)
        {
            var newCol = new float[9];
            newCol[0] = _matrix.Count() + 1;
            newCol[1] = c.Open;
            newCol[2] = c.High;
            newCol[3] = c.Low;
            newCol[4] = c.Close;
            _matrix.Add(newCol);
        }

        private void InitiateCandles(Candle[] candles)
        {
            foreach (var c in candles)
                AddMatrixRow(c);
            for (var i = (int)Parameters.Select(o => o.Value).Max() + 1; i < candles.Count(); i++)
                ProcessRow(i);
        }

        /// <summary>
        /// 0. Index
        /// 1. Open
        /// 2. High
        /// 3. Low
        /// 4. Close
        /// 5. Trend 1
        /// 6. Trend 2
        /// 7. Average
        /// 8. Result
        /// </summary>
        /// <param name="number"></param>
        private void ProcessRow(int number)
        {
            var row = _matrix[number];
            var steps1 = (int)Parameters[0].Value;
            var points1 = _matrix.Skip(number - steps1 - 1).Take(steps1).Select(o => new Point() { X = o[0], Y = o[4] }).ToArray();
            if (points1.Count() > 1)
                row[5] = MathHelper.LeastSquaresValueAtX(points1, number);
            var steps2 = (int)Parameters[1].Value;
            if (_matrix.Count(o => o[5] > 0) > steps1 + steps2)
            {
                var points2 = _matrix.Skip(number - steps2 - 1).Take(steps2).Select(o => new Point() { X = o[0], Y = o[5] }).ToArray();
                if (points2.Count() > 1)
                    row[6] = MathHelper.LeastSquaresValueAtX(points2, number);
            }
            var avCount = (int)Parameters[2].Value;
            if (_matrix.Count() > steps1 + steps2 + avCount)
            {
                row[7] = _matrix.Skip(number - avCount).Take(avCount).SelectMany(o => new float[3] { o[4], o[5], o[6] }).Average();
                row[8] = row[7] > _matrix[number - 1][7] ? 1 : (row[7] < _matrix[number - 1][7] ? -1 : 0);
            }
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
            
            StrategyResult result = StrategyResult.Exit;

            if (_matrix.Last().Last() == 1)
                result = StrategyResult.Long;
            if (_matrix.Last().Last() == -1)
                result = StrategyResult.Short;

            if (result == StrategyResult.Long)
                result = StrategyResult.Short;
            else
                if (result == StrategyResult.Short)
                    result = StrategyResult.Long;

            outData = new object[] { _matrix.Last()[4], _matrix.Last()[5], _matrix.Last()[6], result };
            return result;
        }
    }
}
