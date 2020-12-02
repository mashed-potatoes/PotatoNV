using PotatoNV_next.Utils;
using System;
using System.Diagnostics;
using System.Linq;
using System.Windows.Controls;

namespace PotatoNV_next.Controls
{
    public partial class AboutTab : UserControl
    {
        public AboutTab()
        {
            InitializeComponent();
            version.Text = string.Format(version.Text, GetVersion(3));
            fireLogo.Source = MediaConverter.ImageSourceFromBitmap(Properties.Resources.Fire.ToBitmap());
        }

        public static string GetVersion(int depth = 3)
        {
            return string.Join(".",
                    typeof(MainWindow).Assembly.GetName().Version
                    .ToString()
                    .Split('.')
                    .Take(depth));
        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Process.Start("https://github.com/mashed-potatoes/PotatoNV");
        }
    }
}
