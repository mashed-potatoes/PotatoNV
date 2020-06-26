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
    public partial class AboutTab : UserControl
    {
        public AboutTab()
        {
            InitializeComponent();
        }

        private void DonateButton_ButtonClicked(object sender, EventArgs e)
        {
            MessageBox.Show("donate sir plos plos");
        }

        private void TelegramButton_ButtonClicked(object sender, EventArgs e)
        {
            MessageBox.Show("pro durov hak");
        }
    }
}
