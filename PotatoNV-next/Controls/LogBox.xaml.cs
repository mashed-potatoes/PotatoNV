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
            new PropertyMetadata(false)
        );

        private void AppendLine(LogEventArgs e)
        {
            logBox.AppendText(e.Message);
        }

        public LogBox()
        {
            InitializeComponent();
#if DEBUG
            Log.PrintDebug = true;
#endif
            Log.AttachListener(AppendLine);
        }
    }
}
