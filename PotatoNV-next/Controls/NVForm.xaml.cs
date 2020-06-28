using PotatoNV_next.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
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

namespace PotatoNV_next.Controls
{
    public partial class NVForm : UserControl
    {
        private UsbController usbController;
        private Regex nvRegex = new Regex("^[a-zA-Z0-9]{16}$");
        private Bootloader[] bootloaders;

        public delegate void FormHandler(FormEventArgs formEventArgs);
        public static event FormHandler OnFormSubmit;

        private bool IsSelectedDeviceInFastbootMode;

        public NVForm()
        {
            InitializeComponent();

            usbController = new UsbController();
            usbController.Notify += HandleDevices;
            usbController.StartWorker();

            bootloaders = Bootloader.GetBootloaders();

            foreach (var bl in bootloaders)
            {
                deviceBootloader.Items.Add(bl.Title);
            }

            if (bootloaders.Length > 0)
            {
                deviceBootloader.SelectedIndex = 0;
            }
        }

        public class FormEventArgs : EventArgs
        {
            public UsbController.Device.DMode TargetMode { get; set; }
            public string Target { get; set; }
            public string BoardID { get; set; }
            public string UnlockCode { get; set; }
            public string SerialNumber { get; set; }
            public bool DisableFBLOCK { get; set; }
            public Bootloader Bootloader { get; set; } = null;
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

            IsEnabled = false;

            if (IsSelectedDeviceInFastbootMode)
            {
                OnFormSubmit?.Invoke(new FormEventArgs {
                    TargetMode = UsbController.Device.DMode.Fastboot,
                    Target = deviceList.SelectedItem.ToString(),
                    BoardID = nvBidNumber.Text,
                    UnlockCode = nvUnlockCode.Text,
                    SerialNumber = nvSerialNumber.Text,
                    DisableFBLOCK = disableFBLOCK.IsChecked.Value
                });

                return;
            }
        }

        private void NVForm_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            deviceList.IsEnabled = IsEnabled;
            deviceBootloader.IsEnabled = IsEnabled;
            nvBidNumber.IsEnabled = IsEnabled;
            nvSerialNumber.IsEnabled = IsEnabled;
            nvUnlockCode.IsEnabled = IsEnabled;
            disableFBLOCK.IsEnabled = IsEnabled;
            startButton.IsEnabled = IsEnabled;
        }

        private void DeviceList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (deviceList.SelectedIndex == -1)
            {
                IsSelectedDeviceInFastbootMode = false;
            }
            else
            {
                IsSelectedDeviceInFastbootMode = deviceList.SelectedItem.ToString().StartsWith("Fastboot");
            }

            deviceBootloader.IsEnabled = !IsSelectedDeviceInFastbootMode;
        }
    }
}
