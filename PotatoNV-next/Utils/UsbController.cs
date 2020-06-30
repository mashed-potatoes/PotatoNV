using Potato.Fastboot;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Text.RegularExpressions;

namespace PotatoNV_next.Utils
{
    public class UsbController
    {
        public struct Device
        {
            public enum DMode
            {
                DownloadVCOM,
                Fastboot
            }

            public Device(DMode mode, string description)
            {
                Mode = mode;
                Description = description;
            }

            public DMode Mode { get; }
            public string Description { get; }
        }

        #region USB watcher
        private readonly BackgroundWorker usbWorker = new BackgroundWorker();
        private readonly Stopwatch watch = new Stopwatch();
        private long delta = 0;

        private void Watcher_EventArrived(object sender, EventArrivedEventArgs e)
        {
            if (watch.ElapsedMilliseconds - delta < 100)
            {
                return;
            }

            delta = watch.ElapsedMilliseconds;

            UpdateList();
        }

        private void UsbWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            UpdateList();
            using (var watcher = new ManagementEventWatcher())
            {
                var query = new WqlEventQuery("SELECT * FROM Win32_DeviceChangeEvent WHERE EventType = 2 OR EventType = 3");
                watcher.EventArrived += Watcher_EventArrived;
                watcher.Query = query;
                watcher.Start();
                watcher.WaitForNextEvent();
            }
        }

        public void StartWorker()
        {
            usbWorker.DoWork += UsbWorker_DoWork;
            watch.Start();
            usbWorker.RunWorkerAsync();
        }
        #endregion

        #region Device list
        private const int VID = 0x12D1, PID = 0x3609;
        private string[] cachedDevices = new string[] { };

        private Device[] GetDownloadVCOMDevices()
        {
            var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PnPEntity WHERE ClassGuid=\"{4d36e978-e325-11ce-bfc1-08002be10318}\"");
            var list = searcher.Get();
            var devices = new List<Device>();

            foreach (ManagementObject obj in list)
            {
                // Excepted format: USB\VID_12D1&PID_3609\6&127ABA2B&0&2
                var sdata = obj["DeviceID"].ToString().Split('\\');

                if (sdata.Length < 2 || sdata[1] != string.Format("VID_{0:X4}&PID_{1:X4}", VID, PID))
                {
                    continue;
                }

                var match = Regex.Match(obj["Name"].ToString(), @"COM\d+");

                if (match.Success)
                {
                    devices.Add(new Device(
                        Device.DMode.DownloadVCOM,
                        $"{match.Value}: {obj["Description"]}"
                    ));
                }
            }
            return devices.ToArray();
        }

        private Device[] GetFastbootDevices()
        {
            return Fastboot.GetDevices()
                .Select(x => new Device(Device.DMode.Fastboot, x))
                .ToArray();
        }
        #endregion

        #region Caller
        public delegate void DevicesHandler(Device[] devices);
        public event DevicesHandler Notify;

        private void UpdateList()
        {
            var list = new List<Device>();

            try
            {
                list.AddRange(GetDownloadVCOMDevices());
                list.AddRange(GetFastbootDevices());
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                Log.Debug(ex.StackTrace);
            }

            var sns = list.Select(x => x.Description).ToArray();

            if (Enumerable.SequenceEqual(sns, cachedDevices))
            {
                return;
            }

            cachedDevices = sns;

            Notify?.Invoke(list.ToArray());
        }
        #endregion
    }
}
