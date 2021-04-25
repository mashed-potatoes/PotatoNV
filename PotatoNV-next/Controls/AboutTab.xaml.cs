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
            var versionTag = $"v{Common.GetAssemblyVersion(typeof(MainWindow).Assembly)}\n[" +
                string.Join(", ", new (string AssemblyName, string Tag)[] {
                    ("Potato.Fastboot", "FB"),
                    ("Potato.ImageFlasher", "IF"),
                    ("LibUsbDotNet.LibUsbDotNet", "LD"),
                    ("libusb-1.0", "LU"),
                }.Select(x => $"{x.Tag} v{Common.GetAssemblyVersion($"{x.AssemblyName}.dll") ?? "??"}").ToArray()) + "]";

            version.Text = versionTag;
            fireLogo.Source = MediaConverter.ImageSourceFromBitmap(Properties.Resources.Fire.ToBitmap());
        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Process.Start("https://github.com/mashed-potatoes/PotatoNV");
        }
    }
}
