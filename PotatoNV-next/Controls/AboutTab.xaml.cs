using PotatoNV_next.Utils;
using System;
using System.Diagnostics;
using System.Windows.Controls;

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
            Log.Debug("Clicked to donate button!");
            Process.Start("https://mashed-potatoes.github.io/donate/?utm_source=potatonv&utm_medium=about-donate");
        }

        private void TelegramButton_ButtonClicked(object sender, EventArgs e)
        {
            Log.Debug("Clicked to Telegram button!");
            Process.Start("https://t.me/s/RePotato");
        }
    }
}
