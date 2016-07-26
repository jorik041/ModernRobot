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

namespace ModernClient.ViewModels
{
    public class vmMainPage : INotifyPropertyChanged
    {
        private ObservableCollection<ActualizedInstrument> _actualizedInstruments;
        private WCFCommunicatorClient _client = new WCFCommunicatorClient();
        private ObservableCollection<string> _avaliableStrategies;
        private ObservableCollection<RemoteCalculationInfo> _calculators;

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

        public vmMainPage()
        {
            _client.GetActualizedInstrumentsAsync();
            _client.GetActualizedInstrumentsCompleted += (sender, obj) =>
            {
                ActualizedInstruments = obj.Result;
            };
            _client.GetAvaliableStrategiesAsync();
            _client.GetAvaliableStrategiesCompleted += (sender, obj) =>
             {
                 AvaliableStrategies = obj.Result;
             };
            _client.GetRemoteCalculationsInfoAsync();
            _client.GetRemoteCalculationsInfoCompleted += (sender, obj) =>
            {
                Calculators = obj.Result;
            };
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
