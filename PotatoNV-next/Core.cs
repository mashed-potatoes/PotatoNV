using Potato.Fastboot;
using Potato.ImageFlasher;
using PotatoNV_next.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
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

        private void LogResponse(Fastboot.Response response)
        {
            Log.Debug($"response: {Encoding.UTF8.GetString(response.RawData)}");
        }

        private void FlashBootloader(Bootloader bootloader, string port)
        {
            var flasher = new ImageFlasher();

            Log.Info("Verifying images...");

            int asize = 0, dsize = 0;

            foreach (var image in bootloader.Images)
            {   
                if (!image.IsValid)
                {
                    throw new Exception($"Image `{image.Role}` is not valid!");
                }

                asize += image.Size;
            }

            Log.Debug($"Opening {port}...");

            flasher.Open(port);

            Log.Info($"Uploading {bootloader.Name}...");

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

            Log.SetProgressBar(false);
        }

        private void ReadInfo()
        {
            var serial = fb.GetSerialNumber();
            Log.Info($"Serial number: {serial}");

            var bsn = fb.Command("oem read_bsn");
            if (bsn.Status == Fastboot.Status.Okay)
            {
                Log.Info($"Board ID: {bsn.Payload}");
            }

            var model = fb.Command("oem get-product-model");
            Log.Info($"Model: {model.Payload}");

            var build = fb.Command("oem get-build-number");
            Log.Info($"Build number: {build.Payload.Replace(":", "")}");

            var fblock = fb.Command("oem lock-state info");
            var state = Regex.IsMatch(fblock.Payload, @"FB[\w: ]{1,}UNLOCKED");

            if (!state)
            {
                fblock = fb.Command("oem backdoor info");
                state = Regex.IsMatch(fblock.Payload, @"FB[\w: ]{1,}UNLOCKED");
            }

            Log.Info($"FBLOCK state: {(state ? "unlocked" : "locked")}");
            LogResponse(fblock);

            if (!state)
            {
                throw new Exception("*** FBLOCK is locked! ***");
            }

            var factoryKey = ReadFactoryKey();

            if (factoryKey != null)
            {
                Log.Info($"Unlock key: {factoryKey}");
            }

            var random = new Random(Guid.NewGuid().GetHashCode());

            args.UnlockCode = factoryKey ?? new string(Enumerable.Repeat("ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789", 16)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private void SetNVMEProp(string prop, byte[] value)
        {
            Log.Info($"Writing {prop}...");

            var cmd = new List<byte>();

            cmd.AddRange(Encoding.ASCII.GetBytes($"getvar:nve:{prop}@"));
            cmd.AddRange(value);

            var res = fb.Command(cmd.ToArray());

            LogResponse(res);

            if (!res.Payload.Contains("set nv ok"))
            {
                throw new Exception($"Failed to set: {res.Payload}");
            }
        }

        public static byte[] GetSHA256(string str)
        {
            using (var sha256 = SHA256.Create())
            {
                return sha256.ComputeHash(Encoding.ASCII.GetBytes(str));
            }
        }

        private void SetHWDogState(byte state)
        {
            foreach (var command in new[] { "hwdog certify set", "backdoor set" })
            {
                Log.Info($"Trying {command}...");
                var res = fb.Command($"oem {command} {state}");
                LogResponse(res);
                if (res.Status == Fastboot.Status.Okay || res.Payload.Contains("equal"))
                {
                    Log.Success($"{command}: success");
                    return;
                }
            }

            Log.Error("Failed to set FBLOCK state!");
        }

        private string ReadFactoryKey()
        {
            var res = fb.Command("getvar:nve:WVLOCK");
            var match = Regex.Match(res.Payload, @"\w{16}");

            return match.Success ? match.Value : null;
        }

        private void WriteNVME()
        {
            var fblockState = (byte)(args.DisableFBLOCK ? 0 : 1);

            try
            {
                SetNVMEProp("FBLOCK", new[] { fblockState });
            }
            catch (Exception ex)
            {
                Log.Error("Failed to set the FBLOCK, using the alternative method...");
                Log.Debug(ex.Message);
                SetHWDogState(fblockState);
            }

            try
            {
                SetNVMEProp("WVLOCK", Encoding.ASCII.GetBytes(args.UnlockCode));
                SetNVMEProp("USRKEY", GetSHA256(args.UnlockCode));
            }
            catch (Exception ex)
            {
                Log.Error("Failed to set the key.");
                Log.Debug(ex.Message);
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
                    FlashBootloader(args.Bootloader, args.Target.Split(':')[0]);

                    Log.Info("Waiting for any device...");
                    fb.Wait();
                }

                Log.Info("Connecting...");

                fb.Connect();
                ReadInfo();
                WriteNVME();

                Log.Info("Finalizing...");
                LogResponse(fb.Command($"oem unlock {args.UnlockCode}"));

                if (args.Reboot)
                {
                    Log.Info("Rebooting...");
                    fb.Command("reboot");
                }

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
