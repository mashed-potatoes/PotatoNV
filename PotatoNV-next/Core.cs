using Potato.Fastboot;
using Potato.ImageFlasher;
using PotatoNV_next.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace PotatoNV_next
{
    class Core
    {
        public delegate void RunWorkerCompletedHandler();
        public event RunWorkerCompletedHandler RunWorkerCompleted;

        private Fastboot fb;
        private Controls.NVForm.FormEventArgs args;

        private void FlashBootloader(Bootloader bootloader, string port)
        {
            var flasher = new ImageFlasher();

            Log.Info("Verifying images...");

            int asize = 0, dsize = 0;

            foreach (var image in bootloader.Images)
            {
                Log.Debug($"VrStat of {image.Role}: {image.IsValid}");
                
                if (!image.IsValid)
                {
                    throw new Exception($"Image `{image.Role}` is invalid!");
                }

                asize += image.Size;
            }

            Log.Success("Verification passed!");
            Log.Debug($"Opening {port}...");

            flasher.Open(port);

            Log.Info($"Uploading {bootloader.Name} bootloader");

            foreach (var image in bootloader.Images)
            {
                var size = image.Size;

                Log.Info($"- {image.Role}");

                flasher.Write(image.Path, (int)image.Address, x => {
                    Log.SetProgressBar(dsize + (int)(size / 100f * x), asize);
                });

                dsize += size;
            }

            flasher.Close();

            Log.Success("Bootloader uploaded");
            Log.SetProgressBar(false);
        }

        private void ReadInfo()
        {
            var serial = fb.GetSerialNumber();
            Log.Info($"- Serial number: {serial}");

            var bsn = fb.Command("oem read_bsn");
            Log.Info($"- Board ID: {bsn.Payload}");

            var model = fb.Command("oem get-product-model");
            Log.Info($"- Model: {model.Payload}");

            var build = fb.Command("oem get-build-number");
            Log.Info($"- Build number: {build.Payload.Replace(":", "")}");

            var regex = new Regex(@"FB[\w: ]{1,}UNLOCKED");
            var fblock = fb.Command("oem lock-state info");
            var state = regex.IsMatch(fblock.Payload);
            
            Log.Info($"- FBLOCK state: {(state ? "unlocked" : "locked")}");

            if (!state)
            {
                throw new Exception("FBLOCK is locked!");
            }
        }

        private void SetNVMEProp(string prop, byte[] value, string role = null)
        {
            Log.Info($"- Writing {role ?? prop}");

            var cmd = new List<byte>();

            cmd.AddRange(Encoding.ASCII.GetBytes($"getvar:nve:{prop}@"));
            cmd.AddRange(value);

            var res = fb.Command(cmd.ToArray());

            if (!res.Payload.Contains("set nv ok"))
            {
                throw new Exception($"Failed to set prop: {res.Payload}");
            }
        }

        private void SetNVMEProp(string prop, string value, string role = null)
        {
            SetNVMEProp(prop, Encoding.ASCII.GetBytes(value), role);
        }

        public static byte[] GetSHA256(string str)
        {
            using (var sha256 = SHA256.Create())
            {
                return sha256.ComputeHash(Encoding.ASCII.GetBytes(str));
            }
        }

        private void WriteNVME()
        {
            SetNVMEProp("FBLOCK", new[] { (byte)(args.DisableFBLOCK ? 0 : 1) }, "FBLOCK state");

            SetNVMEProp("USRKEY", GetSHA256(args.UnlockCode), "User key");

            if (!string.IsNullOrWhiteSpace(args.SerialNumber))
            {
                SetNVMEProp("SN", args.SerialNumber, "Serial number");
            }

            if (!string.IsNullOrWhiteSpace(args.BoardID))
            {
                SetNVMEProp("BOARDID", args.BoardID, "Board ID");
            }
        }

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            args = e.Argument as Controls.NVForm.FormEventArgs;
            fb = new Fastboot();

            try
            {
                if (args.TargetMode == UsbController.Device.DMode.DownloadVCOM)
                {
                    Log.Info("--> Flashing bootloader");
                    FlashBootloader(args.Bootloader, args.Target.Split(':')[0]);

                    Log.Info("Waiting for any device...");
                    fb.Wait();
                }

                Log.Info("--> Reading information");
                Log.Info("Connecting to fastboot device...");

                fb.Connect();
                ReadInfo();

                Log.Info("--> Updating NVME");
                WriteNVME();

                Log.Success("Update done!");
                Log.Info("Rebooting...");

                fb.Command("reboot");

                Log.Info($"Bootloader unlock code: {args.UnlockCode}");

                fb.Disconnect();
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                Log.Debug(ex.StackTrace);
            }
        }

        public void StartProcess(Controls.NVForm.FormEventArgs args)
        {
            var worker = new BackgroundWorker();
            worker.DoWork += Worker_DoWork;
            worker.RunWorkerCompleted += (sender, e) => RunWorkerCompleted();
            worker.RunWorkerAsync(args);
        }
    }
}
