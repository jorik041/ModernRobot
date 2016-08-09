using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using ModernClient.WCFCommunicator;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Browser;
using System.Windows.Threading;
using System.Collections.Generic;
using System.Threading;
using ModernClient.Helpers;

namespace ModernClient.ViewModels
{
    public class vmMainPage : INotifyPropertyChanged
    {
        public RelayCommand RunCalculation { get; set; }
        public RelayCommand DeleteCalculation { get; set; }
        public RelayCommand AddNewCalculation { get; set; }
        public RelayCommand ClearNewCalculation { get; set; }
        public RelayCommand GoBackCommand { get; set; }
        public RelayCommand GetDetailedResultsCommand { get; set; }

        private ObservableCollection<ActualizedInstrument> _actualizedInstruments;
        private WCFCommunicatorClient _client = new WCFCommunicatorClient();
        private ObservableCollection<string> _avaliableStrategies;
        private ObservableCollection<RemoteCalculationInfo> _calculators;
        private RemoteCalculationInfo _selectedCalc;
        private string _selectedStrategy;
        private ObservableCollection<ParametersDescription> _selectedStrategyParameters;
        private string _newCalculationName;
        private TimePeriods _selectedPeriod;
        private int[] _collector;
        private List<float[]> _combinations;
        private ActualizedInstrument _selectedInstrument;
        private DateTime _dateFrom;
        private DateTime _dateTo;
        private DateTime _displayDateFrom;
        private DateTime _displayDateTo;
        private object _lockObj= new object();
        private DispatcherTimer _timer;
        private bool _canAddCalc = true;
        private UserControl _selectedContent;
        private Guid _selectedCalcId;
        private ObservableCollection<ResultDescription> _results;
        private ResultDescription _selectedResult;
        
        public UserControl SelectedContent
        {
            get
            {
                return _selectedContent;
            }
            set
            {
                _selectedContent = value;
                OnPropertyChanged("SelectedContent");
            }
        }
        public Dictionary<TimePeriods, string> PeriodsList
        {
            get
            {
                return new Dictionary<TimePeriods, string>()
                {
                    {TimePeriods.Hour, "1 час" },
                    {TimePeriods.HalfHour, "30 минут" },
                    {TimePeriods.FifteenMinutes, "15 минут" },
                    {TimePeriods.TenMinutes, "10 минут" },
                    {TimePeriods.FiveMinutes, "5 минут" },
                    {TimePeriods.Minute, "1 минута" }
                };
            }
        }

        public ObservableCollection<ActualizedInstrument> ActualizedInstruments
        {
            get
            {
                return _actualizedInstruments;
            }

            set
            {
                _actualizedInstruments = value;
                OnPropertyChanged("ActualizedInstruments");
            }
        }
        public ObservableCollection<string> AvaliableStrategies
        {
            get
            {
                return _avaliableStrategies;
            }
            set
            {
                _avaliableStrategies = value;
                OnPropertyChanged("AvaliableStrategies");
            }
        }
        public ObservableCollection<RemoteCalculationInfo> Calculators
        {
            get
            {
                return _calculators;
            }
            set
            {
                _calculators = value;
                OnPropertyChanged("Calculators");
            }
        }

        public string SelectedStrategy
        {
            get
            {
                return _selectedStrategy;
            }
            set
            {
                _selectedStrategy = value;
                _client.GetStrategyParametersDescriptionAsync(_selectedStrategy);
                OnPropertyChanged("SelectedStrategy");
            }
        }

        public ObservableCollection<ParametersDescription> SelectedStrategyParameters
        {
            get
            {
                return _selectedStrategyParameters;
            }
            set
            {
                _selectedStrategyParameters = value;
                OnPropertyChanged("SelectedStrategyParameters");
            }
        }

        public string NewCalculationName
        {
            get
            {
                return _newCalculationName;
            }
            set
            {
                _newCalculationName = value;
                OnPropertyChanged("NewCalculationName");
            }
        }

        public RemoteCalculationInfo SelectedCalc
        {
            get
            {
                return _selectedCalc;
            }
            set
            {
                _selectedCalc = value;
                OnPropertyChanged("SelectedCalc");
            }
        }

        public TimePeriods SelectedPeriod
        {
            get
            {
                return _selectedPeriod;
            }
            set
            {
                _selectedPeriod = value;
                OnPropertyChanged("SelectedPeriod");
            }
        }

        public ActualizedInstrument SelectedInstrument
        {
            get
            {
                return _selectedInstrument;
            }
            set
            {
                _selectedInstrument = value;
                DateTo = SelectedInstrument.DateTo;
                DateFrom = SelectedInstrument.DateTo.AddMonths(-1);
                DisplayDateTo = SelectedInstrument.DateTo;
                DisplayDateFrom = SelectedInstrument.DateFrom.AddMonths(3);
                OnPropertyChanged("SelectedInstrument");
            }
        }

        public DateTime DateFrom
        {
            get
            {
                return _dateFrom;
            }
            set
            {
                _dateFrom = value;
                if (_dateFrom < SelectedInstrument.DateFrom.AddMonths(3))
                    _dateFrom = SelectedInstrument.DateFrom.AddMonths(3);
                if (_dateFrom > DateTo)
                    _dateFrom = DateTo;
                OnPropertyChanged("DateFrom");
            }
        }

        public DateTime DateTo
        {
            get
            {
                return _dateTo;
            }
            set
            {
                _dateTo = value;
                if (DateTo > SelectedInstrument.DateTo)
                    DateTo = SelectedInstrument.DateTo;
                OnPropertyChanged("DateTo");
            }
        }

        public DateTime DisplayDateFrom
        {
            get
            {
                return _displayDateFrom;
            }

            set
            {
                _displayDateFrom = value;
                OnPropertyChanged("DisplayDateFrom");
            }
        }

        public DateTime DisplayDateTo
        {
            get
            {
                return _displayDateTo;
            }

            set
            {
                _displayDateTo = value;
                OnPropertyChanged("DisplayDateTo");
            }
        }

        public bool CanAddCalc
        {
            get
            {
                return _canAddCalc;
            }
            set
            {
                _canAddCalc = value;
                OnPropertyChanged("CanAddCalc");
            }
        }

        public ObservableCollection<ResultDescription> Results
        {
            get
            {
                return _results;
            }
            set
            {
                _results = value;
                OnPropertyChanged("Results");
            }
        }
        public ResultDescription SelectedResult
        {
            get
            {
                return _selectedResult;
            }
            set
            {
                _selectedResult = value;
                OnPropertyChanged("SelectedResult");
                UpdateCommandBindings();
            }
        }

        public void UpdateCommandBindings()
        {
            RunCalculation.RaiseCanExecuteChanged();
            DeleteCalculation.RaiseCanExecuteChanged();
            AddNewCalculation.RaiseCanExecuteChanged();
            ClearNewCalculation.RaiseCanExecuteChanged();
            GetDetailedResultsCommand.RaiseCanExecuteChanged();
        }

        public vmMainPage()
        {
            SelectedContent = new MainPage();
            HtmlPage.Document.SetProperty("title", "Калькулятор");
            Calculators = new ObservableCollection<RemoteCalculationInfo>();
            _client.GetActualizedInstrumentsAsync();
            _client.GetActualizedInstrumentsCompleted += (sender, obj) =>
            {
                ActualizedInstruments = obj.Result;
            };
            _client.GetAvaliableStrategiesAsync();
            _client.GetAvaliableStrategiesCompleted += (sender, obj) =>
             {
                 AvaliableStrategies = obj.Result;
                 SelectedStrategy = AvaliableStrategies.First();
             };
            _client.GetRemoteCalculationsInfoCompleted += (sender, obj) =>
            {
                foreach (var rc in obj.Result)
                {
                    var oldrc = Calculators.SingleOrDefault(o => o.Id == rc.Id);
                    if (oldrc == null)
                        Calculators.Add(rc);
                    else
                    {
                        oldrc.FinishedOrdersCount = rc.FinishedOrdersCount;
                        oldrc.WaitingOrdersCount = rc.WaitingOrdersCount;
                        oldrc.IsDone = rc.IsDone;
                        oldrc.IsRunning = rc.IsRunning;
                        oldrc.IsWaiting = rc.IsWaiting;
                    }
                }
                foreach (var rc in Calculators.ToList())
                    if (!obj.Result.Any(o => o.Id == rc.Id))
                        Calculators.Remove(rc);
            };
            _client.GetRemoteCalculationsInfoAsync();
            _client.GetStrategyParametersDescriptionCompleted += (sender, obj) =>
            {
                SelectedStrategyParameters = new ObservableCollection<ParametersDescription>(
                    obj.Result.Select(o => 
                    new ParametersDescription() { Description = o }));
            };

            RunCalculation = new RelayCommand(o => RunCalc(), o => SelectedCalc != null && SelectedCalc.IsWaiting);
            DeleteCalculation = new RelayCommand(o => DeleteSelectedCalc(), o => SelectedCalc != null);
            AddNewCalculation = new RelayCommand(o => AddCalc(), o => CanAddCalc && !((NewCalculationName == null) || (SelectedStrategyParameters.Any(p => p.From == null || p.To == null || p.To < p.From))));
            ClearNewCalculation = new RelayCommand(o => ClearCalc());
            GoBackCommand = new RelayCommand(o => SelectedContent = new MainPage());
            GetDetailedResultsCommand = new RelayCommand(o => ExportSelectedResult(), o => SelectedResult != null);

            _timer = new DispatcherTimer();
            _timer.Interval = new TimeSpan(0, 0, 1);
            _timer.Tick += (sender, obj) => 
            {
                _client.GetRemoteCalculationsInfoAsync();
                OnPropertyChanged("CanAddCalc");
                UpdateCommandBindings();
            };
            _timer.Start();
        }

        public void UpdateCalculators()
        {
            _client.GetRemoteCalculationsInfoAsync();
        }

        public void DeleteSelectedCalc()
        {
            if (SelectedCalc != null)
            {
                _client.RemoveRemoteCalculationAsync(SelectedCalc.Id);
                Calculators.Remove(SelectedCalc);
            }
            else
            {
                MessageBox.Show("Выберите расчет для удаления!");
            }
        }

        public void AddCalc()
        {
            _client.AddRemoteCalculationCompleted += AddRemoteCalc;
            _client.AddRemoteCalculationAsync(NewCalculationName, SelectedStrategy);
        }

        private void CollectParams(int num = 0)
        {
            _collector[num] = (int)SelectedStrategyParameters[num].From - 1;
            while (_collector[num] < SelectedStrategyParameters[num].To)
            {
                _collector[num]++;
                var newVal = new float[SelectedStrategyParameters.Count];
                for (var i = 0; i < _collector.Count(); i++)
                    newVal[i] = _collector[i];
                if (num == _collector.Count() - 1)
                    _combinations.Add(newVal);
                if (num < _collector.Count() - 1)
                    CollectParams(num + 1);
            }
        }

        private void AddRemoteCalc(object sender, AddRemoteCalculationCompletedEventArgs e)
        {
            _client.AddRemoteCalculationCompleted -= AddRemoteCalc;
            CanAddCalc = false;
            ThreadPool.QueueUserWorkItem((obj) =>
            {
                lock (_lockObj)
                {
                    _combinations = new List<float[]>();
                    _collector = new int[SelectedStrategyParameters.Count];
                    for (var i = 0; i < _collector.Count(); i++)
                        _collector[i] = (int)SelectedStrategyParameters[i].From;
                    CollectParams();
                    foreach (var comb in _combinations)
                    {
                        _client.AddOrderToRemoteCalulationAsync(e.Result.Id, SelectedInstrument.Name, DateFrom, DateTo, SelectedPeriod, new ObservableCollection<float>(comb));
                    }         
                              
                    _canAddCalc = true;
                }
            });
        }

        public void ClearCalc()
        {
            _client.GetStrategyParametersDescriptionAsync(SelectedStrategy);
            NewCalculationName = "";
        }

        public void RunCalc()
        {
            if (SelectedCalc!=null)
                _client.StartRemoteCalculationAsync(SelectedCalc.Id);
        }

        public void StopCalc()
        {
            if (SelectedCalc != null)
                _client.StopRemoteCalculationAsync(SelectedCalc.Id);
        }

        public void OpenResults()
        {
            if (SelectedCalc != null)
            {
                _selectedCalcId = SelectedCalc.Id;
                SelectedContent = new Results();
                Results = new ObservableCollection<ResultDescription>();
                _client.GetFinishedOrdersForRemoteCalculationCompleted += GetResults;
                _client.GetFinishedOrdersForRemoteCalculationAsync(_selectedCalcId);
            }
        }

        private void GetResults(object sender, GetFinishedOrdersForRemoteCalculationCompletedEventArgs e)
        {
            _client.GetFinishedOrdersForRemoteCalculationCompleted -= GetResults;
            foreach (var res in e.Result)
            {
                if (res.Parameters != null)
                    Results.Add(new ResultDescription()
                    {
                        Id = res.Id,
                        DateFrom = res.DateFrom.ToShortDateString(),
                        DateTo = res.DateTo.ToShortDateString(),
                        InstrumentName = res.InstrumentName,
                        Period = PeriodsList[res.Period],
                        StrategyName = Calculators.Single(o => o.Id == _selectedCalcId).StrategyName,
                        Parameters = string.Join("-", res.Parameters),
                        Balance = res.TotalBalance
                    });
            }
            Results = new ObservableCollection<ResultDescription>(Results.OrderByDescending(o => o.Balance));
        }

        public void ExportSelectedResult()
        {

        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        public virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                var e = new PropertyChangedEventArgs(propertyName);
                handler(this, e);
            }
        }

        #endregion INotifyPropertyChanged Members
    }
}
