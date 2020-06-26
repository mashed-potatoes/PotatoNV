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

namespace PotatoNV_next
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            Icon = MediaConverter.ImageSourceFromBitmap(Properties.Resources.Fire.ToBitmap());
            InitializeComponent();
        }
    }
}
