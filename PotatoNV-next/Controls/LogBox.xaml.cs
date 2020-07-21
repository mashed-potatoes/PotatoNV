using PotatoNV_next.Utils;
using System.IO;
using System.Windows;
using System.Windows.Controls;

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

        private void AppendLine(Log.LogEventArgs e)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(() => logBox.AppendText(e.Message));
                return;
            }
            logBox.AppendText(e.Message);
        }

        private void OnProgress(Log.ProgressEventArgs progressEventArgs)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(() => OnProgress(progressEventArgs));
                return;
            }

            ShowProgressBar = progressEventArgs.ShowBar;

            if (progressEventArgs.MaxValue.HasValue)
            {
                progressBar.Maximum = progressEventArgs.MaxValue.Value;
            }

            if (progressEventArgs.Value.HasValue)
            {
                progressBar.Value = progressEventArgs.Value.Value;
            }
        }

        public void Clear()
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(() => Clear());
                return;
            }

            logBox.Clear();
        }

        public LogBox()
        {
            InitializeComponent();
            OnChanged();
#if DEBUG
            Log.PrintDebug = File.Exists("print_debug");
#endif
            Log.Notify += AppendLine;
            Log.OnProgress += OnProgress;
        }
    }
}
