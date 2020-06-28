using PotatoNV_next.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PotatoNV_next.Controls
{
    public partial class LogBox : UserControl
    {
        public bool ShowProgressBar
        {
            get { return (bool)GetValue(ShowProgressBarProperty); }
            set { SetValue(ShowProgressBarProperty, value); }
        }

        public static readonly DependencyProperty ShowProgressBarProperty = DependencyProperty.Register(
            "ShowProgressBar",
            typeof(bool),
            typeof(LogBox),
            new PropertyMetadata(false, OnPropertyChangedCallback)
        );

        private static void OnPropertyChangedCallback(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            (sender as LogBox).OnChanged();
        }

        protected virtual void OnChanged()
        {
            progressBarRowDefinition.Height = new GridLength(ShowProgressBar ? 16 : 0);
        }

        private void AppendLine(LogEventArgs e)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(() => logBox.AppendText(e.Message));
                return;
            }
            logBox.AppendText(e.Message);
        }

        public LogBox()
        {
            InitializeComponent();
            OnChanged();
#if DEBUG
            Log.PrintDebug = true;
#endif
            Log.Notify += AppendLine;
        }
    }
}
