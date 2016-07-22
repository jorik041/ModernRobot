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

namespace ModernClient.ViewModels
{
    public class vmMainPage : INotifyPropertyChanged
    {
        private ObservableCollection<ActualizedInstrument> _actualizedInstruments;
        private WCFCommunicatorClient _client = new WCFCommunicatorClient();

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

        public vmMainPage()
        {
            _client.GetActualizedInstrumentsAsync();
            _client.GetActualizedInstrumentsCompleted += (sender, obj) =>
            {
                ActualizedInstruments = obj.Result;
            };
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
