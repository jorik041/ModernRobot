using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Text.RegularExpressions;
using ModernClient.ViewModels;

namespace ModernClient
{
    public partial class MainPage : UserControl
    {
        static bool AltGrIsPressed;

        public MainPage()
        {
            InitializeComponent();
        }

        void Numclient_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Alt)
            {
                AltGrIsPressed = false;
            }
        }

        void Numclient_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Alt)
            {
                AltGrIsPressed = true;
            }

            if (Keyboard.Modifiers == ModifierKeys.Shift || AltGrIsPressed == true)
            {
                e.Handled = true;
            }

            if (e.Handled == false && (e.Key < Key.D0 || e.Key > Key.D9))
            {
                if (e.Key < Key.NumPad0 || e.Key > Key.NumPad9)
                {
                    if (e.Key != Key.Back)
                    {
                        e.Handled = true;
                    }
                }
            }
        }


        private DateTime _lastClickTime;
        private int _lastSelectedIndex;

        private void ListBox_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if ((DateTime.Now - _lastClickTime).TotalMilliseconds<700)
                if (((ListBox)sender).SelectedIndex == _lastSelectedIndex)
                    ((vmMainPage)DataContext).OpenResults();
            _lastClickTime = DateTime.Now;
            _lastSelectedIndex = ((ListBox)sender).SelectedIndex;
        }
    }
}
