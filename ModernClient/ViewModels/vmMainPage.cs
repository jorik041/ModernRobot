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
using System.Text;

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
        public RelayCommand SaveResultsCommand { get; set; }

        private ObservableCollection<ActualizedInstrument> _actualizedInstruments;
        private WCFCommunicatorClient _client = new WCFCommunicatorClient();
        private ObservableCollection<string> _avaliableStrategies;
        private ObservableCollection<RemoteCalculationInfo> _calculators;
        private RemoteCalculationInfo _selectedCalc;
        private string _selectedStrategy;
        private ObservableCollection<ParametersDescription> _selectedStrategyParameters;
        private string _newCalculationName;
        private TimePeriods _selectedPeriod;
        private ActualizedInstrument _selectedInstrument;
        private DateTime _dateFrom;
        private DateTime _dateTo;
        private DateTime _displayDateFrom;
        private DateTime _displayDateTo;
        private object _lockObj= new object();
        private DispatcherTimer _timer;
        private UserControl _selectedContent;
        private Guid _selectedCalcId;
        private ObservableCollection<ResultDescription> _results;
        private ResultDescription _selectedResult;
        private float _stopLossLow=100;
        private float _stopLossHigh=1000;
        private float _stopLossIncrement=50;
        private bool _useStopLoss;

        public float StopLossLow
        {
            get
            {
                return _stopLossLow;
            }
            set
            {
                _stopLossLow = value;
                OnPropertyChanged("StopLossLow");
            }
        }

        public float StopLossHigh
        {
            get
            {
                return _stopLossHigh;
            }
            set
            {
                _stopLossHigh = value;
                OnPropertyChanged("StopLossHigh");
            }
        }

        public float StopLossIncrement
        {
            get
            {
                return _stopLossIncrement;
            }
            set
            {
                _stopLossIncrement = value;
                OnPropertyChanged("StopLossIncrement");
            }
        }

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
                DeleteCalculation.RaiseCanExecuteChanged();
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

        public bool UseStopLoss
        {
            get
            {
                return _useStopLoss;
            }
            set
            {
                _useStopLoss = value;
                OnPropertyChanged("UseStopLoss");
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
            AddNewCalculation = new RelayCommand(o => AddCalc());
            ClearNewCalculation = new RelayCommand(o => ClearCalc());
            GoBackCommand = new RelayCommand(o => SelectedContent = new MainPage());
            GetDetailedResultsCommand = new RelayCommand(o => ExportSelectedResult(), o => SelectedResult != null);
            SaveResultsCommand = new RelayCommand(o => SaveResults(), o => Results.Count() > 0);

            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(250);
            _timer.Tick += (sender, obj) => 
            {
                _client.GetRemoteCalculationsInfoAsync();
                RunCalculation.RaiseCanExecuteChanged();
            };
            _timer.Start();
        }

        public void SaveResults()
        {
            var dlg = new SaveFileDialog
            {
                DefaultExt = ".csv",
                Filter = "Текст (.csv)|*.csv",
                DefaultFileName = "Список результатов"
            };
            if (dlg.ShowDialog() == true)
            {
                SelectedContent = new PleaseWait();
                using (var stream = dlg.OpenFile())
                {
                    var contents = new StringBuilder();
                    contents.AppendLine("Параметры;Стратегия;Инструмент;Период;Дата от;Дата до;Stop Loss;Баланс;Просадка;Коэф. выбора;");
                    foreach (var r in Results)
                        contents.AppendLine(string.Format("{0};{1};{2};{3};{4};{5};{6};{7};{8};{9}",
                            r.Parameters, r.StrategyName, r.InstrumentName, r.Period, 
                            r.DateFrom, r.DateTo, r.StopLoss, r.Balance, r.Gap, r.SelectCoeff));
                    using (var sw = new System.IO.StreamWriter(stream))
                    {
                        sw.Write(contents.ToString());
                        sw.Flush();
                        sw.Close();
                    }
                }
                SelectedContent = new Results();
            }
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
            if (StopLossIncrement<1 || StopLossLow > StopLossHigh || StopLossHigh<1 ||  
                (string.IsNullOrWhiteSpace(NewCalculationName) || 
                SelectedStrategyParameters.Any(p => p.From == null || p.To == null || 
                p.From == 0 || p.To == 0 || p.To < p.From)))
            {
                MessageBox.Show("Некорректные данные для расчета!");
                return;
            }
            _client.AddRemoteCalculationCompleted += OnAddRemoteCalc;
            _client.AddRemoteCalculationAsync(NewCalculationName, SelectedStrategy);
            SelectedContent = new PleaseWait();
        }

        private void OnAddRemoteCalc(object sender, AddRemoteCalculationCompletedEventArgs e)
        {
            _client.AddRemoteCalculationCompleted -= OnAddRemoteCalc;
            if (UseStopLoss)
            {
                for (var i = StopLossLow; i < StopLossHigh; i = i + StopLossIncrement)
                    _client.AddOrdersToRemoteCalulationAsync(e.Result.Id, SelectedInstrument.Name, DateFrom, DateTo, SelectedPeriod, new ObservableCollection<FromToValue>(SelectedStrategyParameters.Select(o => new FromToValue() { From = (float)o.From, To = (float)o.To })), i);
            }
            else
            {
                _client.AddOrdersToRemoteCalulationAsync(e.Result.Id, SelectedInstrument.Name, DateFrom, DateTo, SelectedPeriod, new ObservableCollection<FromToValue>(SelectedStrategyParameters.Select(o => new FromToValue() { From = (float)o.From, To = (float)o.To })), 0);
            }
            SelectedContent = new MainPage();
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
                SelectedContent = new PleaseWait();
                Results = new ObservableCollection<ResultDescription>();
                _client.GetFinishedOrdersForRemoteCalculationCompleted += GetResults;
                _client.GetFinishedOrdersForRemoteCalculationAsync(_selectedCalcId, 1000);
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
                        StopLoss = res.StopLoss,
                        Balance = res.TotalBalance,
                        Gap = res.Gap, 
                        SelectCoeff = res.TotalBalance * res.TotalBalance / res.Gap
                    });
            }
            Results = new ObservableCollection<ResultDescription>(Results.OrderByDescending(o => o.SelectCoeff));
            SelectedContent = new Results();
        }

        public void ExportSelectedResult()
        {
            var dlg = new SaveFileDialog
            {
                DefaultExt = ".csv",
                Filter = "Текст (.csv)|*.csv",
                DefaultFileName = SelectedResult.Parameters
            };
            if (dlg.ShowDialog() == true)
            {
                _client.GetFinishedOrderResultCompleted += GetFinishedResult;
                var saveFileContent = new Action<string> ((string contents) => 
                {
                    using (var stream = dlg.OpenFile())
                    {
                        using (var sw = new System.IO.StreamWriter(stream))
                        {
                            sw.Write(contents);
                            sw.Flush();
                            sw.Close();
                        }
                    }
                    SelectedContent = new Results();
                });
                SelectedContent = new PleaseWait();
                _client.GetFinishedOrderResultAsync(_selectedCalcId, SelectedResult.Id, saveFileContent);
            }
        }

        private string ParseLine(ObservableCollection<string> line, object balance)
        {
            line.Add(balance.ToString());
            return string.Join(";", line);
        }

        private void GetFinishedResult(object sender, GetFinishedOrderResultCompletedEventArgs e)
        {
            var contents = new StringBuilder();
            contents.AppendLine(ParseLine(e.Result.OutDataDescription, "Balance"));  
            for (var i=0; i < e.Result.OutData.Count(); i++)
            {
                contents.AppendLine(ParseLine(e.Result.OutData[i], e.Result.Balances[i]));
            }
            ((Action<string>)e.UserState).Invoke(contents.ToString());                   
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
