using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.ComponentModel;
using System.Windows.Shapes;

namespace ModernClient.ViewModels
{
    public class ResultDescription : INotifyPropertyChanged
    {
        public string Parameters { get; set; }
        public string StrategyName { get; set; }
        public string InstrumentName { get; set; }
        public string DateFrom { get; set; }
        public string DateTo { get; set; }
        public string Period { get; set; }
        public Guid Id { get; set; }
        public float StopLoss { get; set; }

        private float _balance;
        public float Balance
        {
            get
            {
                return _balance;
            }
            set
            {
                _balance = value;
                OnPropertyChanged("Balance");
            }
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
