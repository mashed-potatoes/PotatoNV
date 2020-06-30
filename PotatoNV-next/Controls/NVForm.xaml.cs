using PotatoNV_next.Utils;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace PotatoNV_next.Controls
{
    public partial class NVForm : UserControl
    {
        private UsbController usbController;
        private Regex nvRegex = new Regex("^[a-zA-Z0-9]{16}$");
        private Bootloader[] bootloaders;

        public delegate void FormHandler(FormEventArgs formEventArgs);
        public event FormHandler OnFormSubmit;

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

            var random = new Random(Guid.NewGuid().GetHashCode());

            nvUnlockCode.Text = new string(Enumerable.Repeat("ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789", 16)
                .Select(s => s[random.Next(s.Length)]).ToArray());
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

            IsEnabled = false;

            var eventArgs = new FormEventArgs
            {
                TargetMode = IsSelectedDeviceInFastbootMode
                    ? UsbController.Device.DMode.Fastboot
                    : UsbController.Device.DMode.DownloadVCOM,
                Target = deviceList.SelectedItem.ToString(),
                BoardID = nvBidNumber.Text,
                UnlockCode = nvUnlockCode.Text,
                SerialNumber = nvSerialNumber.Text,
                DisableFBLOCK = disableFBLOCK.IsChecked.Value
            };

            if (!IsSelectedDeviceInFastbootMode)
            {
                eventArgs.Bootloader = bootloaders.First(x => x.Title == deviceBootloader.SelectedItem.ToString());
            }

            OnFormSubmit?.Invoke(eventArgs);
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
