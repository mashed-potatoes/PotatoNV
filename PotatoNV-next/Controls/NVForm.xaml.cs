using PotatoNV_next.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
    public partial class NVForm : UserControl
    {
        private UsbController usbController;
        private Regex nvRegex = new Regex("^[a-zA-Z0-9]{16}$");

        public NVForm()
        {
            InitializeComponent();

            usbController = new UsbController();
            usbController.AddListener(HandleDevices);
            usbController.StartWorker();
        }

        private void Assert(bool result, string message)
        {
            if (result)
            {
                return;
            }

            Log.Debug("Form check failed");
            Log.Error(message);
            MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);

            throw new Exception(message);
        }

        private void HandleDevices(UsbController.Device[] devices)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(() => HandleDevices(devices));
                return;
            }

            deviceList.Items.Clear();

            foreach (var device in devices)
            {
                deviceList.Items.Add(device.Mode == UsbController.Device.DMode.DownloadVCOM
                    ? device.Description
                    : $"Fastboot: {device.Description}");

                Log.Debug($"{device.Mode} mode: {device.Description}");
            }

            if (deviceList.SelectedIndex == -1 && devices.Length > 0)
            {
                deviceList.SelectedIndex = 0;
            }
        }

        private bool VerifyNVValue(string value, bool required = false)
        {
            if (value == string.Empty && !required)
            {
                return true;
            }

            return nvRegex.IsMatch(value);
        }
        
        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Assert(deviceList.SelectedIndex != -1, "No connected devices!\n\r" +
                        "Check connection and required drivers.");

                Assert(deviceBootloader.SelectedIndex != -1, "Couldn't find any valid bootloader!");

                Assert(VerifyNVValue(nvSerialNumber.Text), "Serial number is not valid.");

                Assert(VerifyNVValue(nvBidNumber.Text), "BoardID is not valid.");

                Assert(VerifyNVValue(nvUnlockCode.Text, true), "Unlock code is not valid.");
            }
            catch
            {
                return;
            }

            Log.Success("Form is valid, starting");
        }
    }
}
