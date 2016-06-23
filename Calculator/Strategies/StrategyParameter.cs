using System.ComponentModel;

namespace Calculator.Strategies
{
    public class StrategyParameter : INotifyPropertyChanged
    {
        private float _value;
        public float Value
        {
            get
            {
                return _value;
            }
            set
            {
                _value = value;
                OnPropertyChanged("Value");
            }
        }
        public string Description { get; internal set; }

        public StrategyParameter(string parameterDescription)
        {
            Description = parameterDescription;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
