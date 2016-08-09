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

namespace ModernClient.Helpers
{
    public class RelayCommand : ICommand
    {
        Func<object, bool> canExecute;
        Action<object> executeAction;
        bool canExecuteCache;
        public RelayCommand(Action<object> executeAction, Func<object, bool> canExecute)

        {
            this.executeAction = executeAction;
            this.canExecute = canExecute;
        }

        public RelayCommand(Action<object> executeAction)
        {
            this.executeAction = executeAction;
        }

        public void RaiseCanExecuteChanged()
        {
            if (CanExecuteChanged != null)
            {
                CanExecuteChanged(this, new EventArgs());
            }
        }

        #region ICommand Members

        public bool CanExecute(object parameter)
        {
            if (canExecute == null)
                return true;
            bool temp = canExecute(parameter);
            if (canExecuteCache != temp)
            {
                canExecuteCache = temp;
                if (CanExecuteChanged != null)
                {
                    CanExecuteChanged(this, new EventArgs());
                }
            }
            return canExecuteCache;
        }
        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            executeAction(parameter);
        }

        #endregion
    }
}
