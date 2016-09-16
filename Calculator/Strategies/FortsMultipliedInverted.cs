using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DBAccess.Entities;

namespace Calculator.Strategies
{
    public class FortsMultipliedInverted : IStrategy
    {
        public string Name
        {
            get { return "FORTS Multiplied (inverted)"; }
        }

        public int AnalysisDataLength
        {
            get { return 300; }
        }

        public int HistoryPointsCount
        {
            get
            {
                return 2 * AnalysisDataLength / 3;
            }
        }

        private StrategyParameter[] _defaultParameters
        {
            get
            {
                return new StrategyParameter[]
                {
                    new StrategyParameter("Период MIN и MAX (14)")
                    {
                        Value = 10
                    },
                    new StrategyParameter("Число шагов фильтра 1")
                    {
                        Value = 3
                    },
                    new StrategyParameter("Процент урезания")
                    {
                        Value = 50
                    },
                    new StrategyParameter("Период средней (14)")
                    {
                        Value = 1
                    },
                    new StrategyParameter("Число шагов фильтра 2")
                    {
                        Value = 8
                    },
                    new StrategyParameter("Параметр 3")
                    {
                        Value = 4
                    },
                    new StrategyParameter("Параметр 4")
                    {
                        Value = 17
                    },
                    new StrategyParameter("Параметр 5")
                    {
                        Value = 5
                    },
                    new StrategyParameter("Параметр 6")
                    {
                        Value = 13
                    },
                    new StrategyParameter("Параметр 7")
                    {
                        Value = 5
                    },
                    new StrategyParameter("Параметр 8")
                    {
                        Value = 3
                    }
                };
            }
        }
        public StrategyParameter[] Parameters { get; private set; }

        public string[] OutDataDescription
        {
            get
            {
                return new string[] { "Average", "MAX", "MIN", "FMAX", "FMIN", "R", "Filter", "R1", "R2", "Result", "Min", "Max", "Average", "Result 2", "Min", "Max", "Average", "Result 3", "RESULT" };
            }
        }

        private List<float> _filterValues;
        private List<List<float>> _minFilters;
        private List<List<float>> _maxFilters;
        private List<float> _mins2;
        private List<float> _maxs2;
        private List<float> _avs2;
        private List<float> _mins3;
        private List<float> _maxs3;
        private List<float> _avs3;
        private int _lastResult;
        private float? _newSLValue = null;

        public event EventHandler<float> OnStopLossChanged;
        public float StopLossValue { get; set; }

        private void ChangeStopLoss(float newValue)
        {
            if (OnStopLossChanged != null)
                OnStopLossChanged(this, newValue);
        }

        public FortsMultipliedInverted()
        {
            Parameters = _defaultParameters;
        }

        public void Initialize()
        {
            _filterValues = new List<float>();
            _minFilters = new List<List<float>>();
            _maxFilters = new List<List<float>>();
            _lastResult = 0;
            _newSLValue = null;
            _mins2 = new List<float>();
            _mins3 = new List<float>();
            _maxs2 = new List<float>();
            _maxs3 = new List<float>();
            _avs2 = new List<float>();
            _avs3 = new List<float>();
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

        private int GetR(Candle[] initialCandles, out float min, out float max, out float mav, out float close)
        {
            var candles = initialCandles.ToList();
            var maxs = new List<float>();
            var mins = new List<float>();
            var delta = (int)Parameters[0].Value;

            for (var i = candles.Count(); i >= candles.Count() - 1 - Math.Max((int)Parameters[0].Value, (int)Parameters[3].Value); i--)
            {
                maxs.Add(candles.GetRange(i - delta, delta).Max(o => o.High));
                mins.Add(candles.GetRange(i - delta, delta).Min(o => o.Low));
            }

            var maxsForAv = maxs.Take((int)Parameters[3].Value).ToList();
            var minsForAv = mins.Take((int)Parameters[3].Value).ToList();

            maxs = maxs.Take((int)Parameters[0].Value).ToList();
            mins = mins.Take((int)Parameters[0].Value).ToList();

            max = maxs.First();
            min = mins.First();

            var avs = new List<float>();

            avs.AddRange(maxsForAv);
            avs.AddRange(minsForAv);

            mav = CalculateTrimmedMean(avs.ToArray(), Parameters[2].Value);

            close = candles.Last().Close;

            var r = 0;

            if (close >= mav)
                r = 1;
            else
                r = -1;

            return r;
        }

        private void GetLastMinMax(float[] closes, out List<float> lastMax, out List<float> lastMin)
        {
            for (var i = 0; i < Parameters[4].Value; i++)
            {
                if (_minFilters.Count() < i + 1)
                    _minFilters.Add(new List<float>());
                if (_maxFilters.Count() < i + 1)
                    _maxFilters.Add(new List<float>());
                var prevMaxList = i == 0 ? closes.ToList() : _maxFilters.ElementAt(i - 1);
                var prevMinList = i == 0 ? closes.ToList() : _minFilters.ElementAt(i - 1);

                if (_maxFilters[i].Count() == 0)
                    _maxFilters[i].Add(prevMaxList.Last());
                else
                {
                    _maxFilters[i].Add(prevMaxList.Last() > prevMaxList.ElementAt(prevMaxList.Count() - 2) ? prevMaxList.Last() : _maxFilters[i].Last());
                }

                if (_minFilters[i].Count() == 0)
                    _minFilters[i].Add(prevMinList.Last());
                else
                {
                    _minFilters[i].Add(prevMinList.Last() < prevMinList.ElementAt(prevMinList.Count() - 2) ? prevMinList.Last() : _minFilters[i].Last());
                }
            }
            lastMax = _maxFilters[((int)Parameters[4].Value - 1)];
            lastMin = _minFilters[((int)Parameters[4].Value - 1)];
        }

        public StrategyResult Analyze(Candle[] candles, out object[] outData)
        {

            float max;
            float min;
            float mav;
            float close;

            var r = GetR(candles, out min, out max, out mav, out close);

            float filter = 0;

            if (_filterValues.Count() == 0)
            {
                _filterValues = new List<float>();
                for (var i = (int)Parameters[1].Value; i > 0; i--)
                {
                    var data = candles.Take(candles.Count() - i).ToArray();
                    var newR = GetR(data, out min, out max, out mav, out close);
                    _filterValues.Add(newR);
                }
            }
            _filterValues.Add(r);
            if (_filterValues.Count() > Parameters[1].Value)
            {
                _filterValues.RemoveAt(0);
                filter = _filterValues.Sum();
            }

            var closes = candles.Select(o => o.Close).Skip(candles.Count() - AnalysisDataLength).ToArray();

            List<float> lastMax;
            List<float> lastMin;

            if ((_maxFilters.Count() == 0) && (_minFilters.Count() == 0))
            {
                for (var i = HistoryPointsCount; i > 0; i--)
                    GetLastMinMax(closes.Take(closes.Count() - i).ToArray(), out lastMax, out lastMin);
            }

            GetLastMinMax(closes, out lastMax, out lastMin);

            var r1 = 0;

            if ((lastMax.Count() > 1) && (lastMin.Count() > 1))
            {
                var curr = (lastMax.Last() + lastMin.Last()) / 2;
                var prev = (lastMax.ElementAt(lastMax.Count() - 2) + lastMin.ElementAt(lastMin.Count() - 2)) / 2;
                if (curr > prev)
                    r1 = 1;
                if (curr < prev)
                    r1 = -1;
            }

            var r2 = 0;
            if (filter > 0)
                r2 = 1;
            else
                if (filter < 0)
                r2 = -1;

            var result = 0;

            if (r1 == r2)
            {
                if (r1 == 1)
                    result = 1;
                if (r1 == -1)
                    result = -1;
            }
            else
            {
                var res = r1 + r2;
                if (res == 0)
                    result = 0;
                if (res == 1)
                    result = 0;
                if (res == -1)
                    result = -1;
            }

            if (result != _lastResult)
                _newSLValue = null;

            _lastResult = result;

            if (StopLossValue != 0)
            {
                if (result == -1)
                {
                    var newValue = Math.Max(closes.Last() + StopLossValue, mav);
                    if ((_newSLValue == null) || (newValue < _newSLValue))
                    {
                        _newSLValue = newValue;
                        ChangeStopLoss((float)_newSLValue);
                    }
                }

                if (result == 1)
                {
                    var newValue = Math.Min(closes.Last() - StopLossValue, mav);
                    if ((_newSLValue == null) || (newValue > _newSLValue))
                    {
                        _newSLValue = newValue;
                        ChangeStopLoss((float)_newSLValue);
                    }
                }
            }

            float min2;
            float max2;
            if (!_avs2.Any())
                for (var i = AnalysisDataLength / 2; i < closes.Count() - 1; i++)
                {
                    Alg2Execute(closes.Take(i).ToArray(), out min2, out max2);
                }

            var result2 = Alg2Execute(closes, out min2, out max2);

            float min3;
            float max3;
            if (!_avs3.Any())
                for (var i = AnalysisDataLength / 2; i < closes.Count() - 1; i++)
                {
                    Alg3Execute(closes.Take(i).ToArray(), out min3, out max3);
                }
            var result3 = Alg3Execute(closes, out min3, out max3);

            StrategyResult finalResult;

            switch (result * result2 * result3)
            {
                case -1:
                    finalResult = StrategyResult.Long;
                    break;
                case 1:
                    finalResult = StrategyResult.Short;
                    break;
                default:
                    finalResult = StrategyResult.Exit;
                    break;
            }

            outData = new object[] { mav, max, min,
                _maxFilters.ElementAt((int)Parameters[4].Value - 1).Last(),
                _minFilters.ElementAt((int)Parameters[4].Value - 1).Last(),
                r, filter, r1, r2, result, min2, max2, _avs2.Last(), result2, min3, max3, _avs3.Last(), result3, finalResult };

            return finalResult;
        }

        private int Alg2Execute(float[] closes, out float min, out float max)
        {
            min = closes.Skip(closes.Count() - (int)Parameters[5].Value).Min();
            max = closes.Skip(closes.Count() - (int)Parameters[6].Value).Max();
            _mins2.Add(min);
            _maxs2.Add(max);
            var means2 = _mins2.Skip(_mins2.Count() - (int)Parameters[7].Value).ToList();
            means2.AddRange(_maxs2.Skip(_maxs2.Count() - (int)Parameters[7].Value));
            _avs2.Add(means2.Average());
            if (_avs2.Count() > 1)
                if (_avs2.Last() > _avs2[_avs2.Count() - 2])
                    return 1;
                else
                    return -1;
            return 0;
        }

        private int Alg3Execute(float[] closes, out float min, out float max)
        {
            min = closes.Skip(closes.Count() - (int)Parameters[8].Value).Min();
            max = closes.Skip(closes.Count() - (int)Parameters[9].Value).Max();
            _mins3.Add(min);
            _maxs3.Add(max);
            var means3 = _mins3.Skip(_mins3.Count() - (int)Parameters[10].Value).ToList();
            means3.AddRange(_maxs3.Skip(_maxs3.Count() - (int)Parameters[10].Value));
            _avs3.Add(means3.Average());
            if (closes.Last() > _avs3.Last())
                return 1;
            else
                return -1;
        }
    }
}